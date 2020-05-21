using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Utils;
using ModTheGungeon;
using static SemiPatch.AssemblyDiff;
using MonoMod.RuntimeDetour;

namespace SemiPatch {
    internal class RuntimeInjectionManager : IDisposable {
        public static Logger Logger = new Logger(nameof(RuntimeInjectionManager));

        private ModuleDefinition _RunningModule;
        private System.Reflection.Assembly _RunningAssembly;
        public Relinker Relinker;

        private Dictionary<InjectionSignature, DynamicMethodDefinition> _InjectHandlerDMDMap = new Dictionary<InjectionSignature, DynamicMethodDefinition>();
        private Dictionary<MethodPath, DynamicMethodDefinition> _InjectTargetDMDMap = new Dictionary<MethodPath, DynamicMethodDefinition>();
        private Dictionary<MethodPath, DynamicMethodDefinition> _PermanentTargetDMDMap = new Dictionary<MethodPath, DynamicMethodDefinition>();
        private Dictionary<Instruction, Instruction> _InjectInstructionMap = new Dictionary<Instruction, Instruction>();
        private Dictionary<InjectionSignature, DynamicMethodCallHandlerProxy> _InjectHandlerDMDCallProxyMap = new Dictionary<InjectionSignature, DynamicMethodCallHandlerProxy>();

        private Dictionary<InjectionSignature, InjectionDifference> _InjectDiffMap = new Dictionary<InjectionSignature, InjectionDifference>();
        private HashSet<MethodPath> _TargetsWithGrandfatheredStaticInjections = new HashSet<MethodPath>();

        private List<IDetour> _Detours = new List<IDetour>();

        public RuntimeInjectionManager(Relinker relinker, System.Reflection.Assembly asm, ModuleDefinition mod) {
            _RunningModule = mod;
            _RunningAssembly = asm;
            Relinker = relinker;
        }

        public DynamicMethodDefinition GetInjectionTargetDMD(Relinker relinker, string name, MethodPath target_path, ModuleDefinition target_module, System.Reflection.MethodBase target_method_reflection) {
            var method_path = target_method_reflection.ToPath();

            if (_InjectTargetDMDMap.TryGetValue(method_path, out DynamicMethodDefinition cached_dmd)) {
                return cached_dmd;
            }

            Logger.Debug($"Building DynamicMethodDefinition for method '{method_path}', target of at least one injection");

            var method_params = target_method_reflection.GetParameters();
            var param_length = method_params.Length;
            if (!target_method_reflection.IsStatic) param_length += 1;

            var param_types = new Type[param_length];
            if (!target_method_reflection.IsStatic) param_types[0] = target_method_reflection.DeclaringType;
            for (var i = 0; i < method_params.Length; i++) {
                param_types[target_method_reflection.IsStatic ? i : i + 1] = method_params[i].ParameterType;
            }

            var dmd = new DynamicMethodDefinition(name, (target_method_reflection as System.Reflection.MethodInfo)?.ReturnType ?? typeof(void), param_types);

            dmd.Definition.IsStatic = true;
            dmd.Definition.HasThis = false;
            dmd.Definition.ExplicitThis = false;

            var target_method = target_path.FindIn<MethodDefinition>(target_module);

            dmd.Definition.Body = target_method.Body.Clone(dmd.Definition, _RunningModule);

            for (var i = 0; i < dmd.Definition.Body.Instructions.Count; i++) {
                _InjectInstructionMap[target_method.Body.Instructions[i]] = dmd.Definition.Body.Instructions[i];
            }

            _InjectTargetDMDMap[target_path] = dmd;
            
            return dmd;
        }

        private void _EnsureStaticInjectionsLoaded(MethodPath target_path, ModuleDefinition target_module) {
            if (_TargetsWithGrandfatheredStaticInjections.Contains(target_path)) return;

            var running_method = target_path.FindIn<MethodDefinition>(_RunningModule);
            // if static patched, this can contain data about static injections
            //
            var target_method = target_path.FindIn<MethodDefinition>(target_module);

            Logger.Debug($"Looking for static injections in '{target_path}'... ({running_method.CustomAttributes.Count} attributes)");
            var attrs = new RDARSupport.SupportAttributeData(running_method.CustomAttributes);
            if (attrs.StaticInjectionHandlers != null) {
                Logger.Debug($"Target has static injections - copying");
                for (var i = 0; i < attrs.StaticInjectionHandlers.Count; i++) {
                    var inj = attrs.StaticInjectionHandlers[i];

                    var path = new MethodPath(
                        new Signature(inj.HandlerSignature, inj.HandlerName),
                        running_method.DeclaringType
                    );

                    var sig = new InjectionSignature(inj.Signature);
                    var static_handler_method = path.FindIn<MethodDefinition>(_RunningModule);

                    var semipatch_attrs = new SpecialAttributeData(static_handler_method.CustomAttributes);

                    Logger.Debug($"Grandfathering static injection: '{sig}'");

                    _InjectDiffMap[sig] = new InjectionAdded(
                        target_method,
                        target_method.ToPath(),
                        static_handler_method,
                        path,
                        target_method.Body.Instructions[inj.InstructionIndex],
                        semipatch_attrs.LocalCaptures,
                        inj.Position
                    );
                }
            }
            _TargetsWithGrandfatheredStaticInjections.Add(target_path);
        }

        public DynamicMethodDefinition GetInjectionHandlerDMD(InjectionSignature sig, MethodDefinition handler_method, System.Reflection.MethodBase preinject_target_method) {
            if (_InjectHandlerDMDMap.TryGetValue(sig, out DynamicMethodDefinition cached_handler)) {
                return cached_handler;
            }

            Logger.Debug($"Building DynamicMethodDefinition for injection handler '{sig}'");

            var preinject_params = preinject_target_method.GetParameters();
            var handler_param_types = new Type[preinject_params.Length + (handler_method.IsStatic ? 1 : 2)];

            if (!handler_method.IsStatic) handler_param_types[0] = preinject_target_method.GetThisParamType();

            var state_type = Injector.GetInjectionStateRuntimeType((preinject_target_method as System.Reflection.MethodInfo)?.ReturnType ?? typeof(void));
            handler_param_types[handler_method.IsStatic ? 0 : 1] = state_type;

            for (var i = 0; i < preinject_params.Length; i++) {
                handler_param_types[i + (handler_method.IsStatic ? 1 : 2)] =
                    preinject_params[i].ParameterType;
            }

            var handler_dmd = new DynamicMethodDefinition(
                handler_method.Name,
                typeof(void),
                handler_param_types
            );

            _InjectHandlerDMDMap[sig] = handler_dmd;

            handler_dmd.Definition.IsStatic = true;
            handler_dmd.Definition.HasThis = false;
            handler_dmd.Definition.ExplicitThis = true;

            handler_dmd.Definition.Body = handler_method.Body.Clone(
                handler_dmd.Definition,
                _RunningModule
            );

            return handler_dmd;
        }

        private void _InstallInjection(InjectionSignature sig, MethodDefinition handler_method, Instruction injection_point, InjectPosition pos, DynamicMethodDefinition preinject_dmd, System.Reflection.MethodBase preinject_reflection, IList<CaptureLocalAttribute> captures) {
            var handler_dmd = GetInjectionHandlerDMD(sig, handler_method, preinject_reflection);

            Relinker.Relink(new Relinker.State(_RunningModule), handler_dmd.Definition);

            var target_instr = _InjectInstructionMap[injection_point];

            var handler = handler_dmd.Generate();

            var call_proxy = new DynamicMethodCallHandlerProxy(handler, !preinject_reflection.IsStatic);

            _InjectHandlerDMDCallProxyMap[sig] = call_proxy;

            Injector.InsertInjectCall(
                preinject_dmd.Definition,
                handler_dmd.Definition,
                call_proxy,
                target_instr,
                pos,
                captures
            );
        }

        public void ProcessInjectionDifference(InjectionDifference diff, bool update_running_module = false) {
            _EnsureStaticInjectionsLoaded(diff.TargetPath, diff.Target.Module);
            Logger.Debug($"Processing injection difference for injection handler '{diff.HandlerPath}', target '{diff.TargetPath}'");

            if (diff is InjectionRemoved || diff is InjectionChanged) {
                if (diff is InjectionChanged) Logger.Debug($"Resetting changed injection '{diff.Signature}'");
                else Logger.Debug($"Disposing of removed injection '{diff.Signature}'");

                if (_InjectHandlerDMDMap.TryGetValue(diff.Signature, out DynamicMethodDefinition old_dmd)) {
                    old_dmd.Dispose();
                }
                _InjectHandlerDMDMap.Remove(diff.Signature);

                _InjectDiffMap.Remove(diff.Signature);

                if (_InjectHandlerDMDCallProxyMap.TryGetValue(diff.Signature, out DynamicMethodCallHandlerProxy old_proxy)) {
                    old_proxy.Dispose();
                }
                _InjectHandlerDMDCallProxyMap.Remove(diff.Signature);
            } else Logger.Debug($"Registering new injection '{diff.Signature}'");

            var running_target = diff.TargetPath.FindIn<MethodDefinition>(_RunningModule);
            var support_attrs = new RDARSupport.SupportAttributeData(running_target.CustomAttributes);
            if (diff is InjectionRemoved && support_attrs.PreinjectName == null) return;

            var target_path = diff.TargetPath;
            var preinject_path = target_path;

            if (support_attrs.PreinjectName != null) {
                preinject_path = preinject_path.WithSignature(new Signature(
                    diff.Target, forced_name: support_attrs.PreinjectName
                ));
            }

            var preinject_reflection = preinject_path.FindIn(_RunningAssembly) as System.Reflection.MethodBase;
            var preinject_dmd = GetInjectionTargetDMD(Relinker, target_path.Signature.Name, target_path, diff.Target.Module, preinject_reflection);

            if (diff is InjectionRemoved) return;

            _InjectDiffMap[diff.Signature] = diff;
        }

        private void _CreateInjectionDMDThunk(MethodPath path, DynamicMethodDefinition dmd) {
            var target = path.FindIn<MethodDefinition>(_RunningModule);
            var support_attrs = new RDARSupport.SupportAttributeData(target.CustomAttributes);
            var inject = dmd.Generate();

            var thunk_path = target.ToPath();

            // if the method has an orig, that means we want to
            // redirect the orig to the rtinject method instead of
            // the target - meaning that like at public static time,
            // first the patch will be ran and if orig is ran within
            // the target patch then the method with injection
            // handlers is ran (tl;dr patching, including with
            // receiveoriginal, has precedence over injections)
            if (support_attrs.OrigName != null) {
                thunk_path = target.ToPath().WithSignature(new Signature(
                    target, forced_name: support_attrs.PreinjectName
                ));
            }

            Logger.Debug($"Creating thunk in '{thunk_path}' for injection handler dynamic method '{inject}'");

            var thunk = thunk_path.FindIn(_RunningAssembly) as System.Reflection.MethodBase;
            var stub = RDARPrimitive.CreateThunk(thunk, inject);
            _Detours.Add(new Hook(thunk, stub));
        }

        public void GenerateInjectionTargets() {
            Logger.Debug($"Patching injection targets");

            foreach (var kv in _InjectDiffMap) {
                var sig = kv.Key;
                var diff = kv.Value;

                Logger.Debug($"Post-process (inject) for '{sig}'");

                var preinject_dmd = _InjectTargetDMDMap[diff.Target.ToPath()];
                var preinject_reflection = diff.Target.ToPath().FindIn(_RunningAssembly) as System.Reflection.MethodBase;

                _InstallInjection(
                    diff.Signature, diff.Handler,
                    diff.InjectionPoint, diff.Position,
                    preinject_dmd, preinject_reflection,
                    diff.LocalCaptures
                );
            }

            foreach (var kv in _InjectTargetDMDMap) {
                _CreateInjectionDMDThunk(kv.Key, kv.Value);
            }
        }

        public void RevertInjectionTargets() {
            Logger.Debug($"Reverting patches on injection targets");
            for (var i = 0; i < _Detours.Count; i++) {
                _Detours[i].Dispose();
            }
            _Detours.Clear();

            foreach (var kv in _InjectTargetDMDMap) {
                if (!_PermanentTargetDMDMap.ContainsKey(kv.Key)) {
                    kv.Value.Dispose();
                }
            }
            _TargetsWithGrandfatheredStaticInjections.Clear();
            _InjectTargetDMDMap.Clear();
            _InjectInstructionMap.Clear();
        }

        public void Dispose() {
            RevertInjectionTargets();
            foreach (var kv in _InjectHandlerDMDMap) {
                kv.Value.Dispose();
            }
            foreach (var kv in _InjectHandlerDMDCallProxyMap) {
                kv.Value.Dispose();
            }
        }
    }
}
