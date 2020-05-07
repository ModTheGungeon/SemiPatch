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

        private Dictionary<InjectionSignature, DynamicMethodDefinition> _InjectHandlerDMDMap = new Dictionary<InjectionSignature, DynamicMethodDefinition>();
        private Dictionary<MethodPath, DynamicMethodDefinition> _InjectTargetDMDMap = new Dictionary<MethodPath, DynamicMethodDefinition>();
        private Dictionary<MethodPath, DynamicMethodDefinition> _PermanentTargetDMDMap = new Dictionary<MethodPath, DynamicMethodDefinition>();
        private Dictionary<Instruction, Instruction> _InjectInstructionMap = new Dictionary<Instruction, Instruction>();
        private Dictionary<InjectionSignature, DynamicMethodCallHandlerProxy> _InjectHandlerDMDCallProxyMap = new Dictionary<InjectionSignature, DynamicMethodCallHandlerProxy>();

        private List<IDetour> _Detours = new List<IDetour>();

        public RuntimeInjectionManager(System.Reflection.Assembly asm, ModuleDefinition mod) {
            _RunningModule = mod;
            _RunningAssembly = asm;
        }

        public DynamicMethodDefinition GetInjectionTargetDMD(string name, MethodPath target_path, ModuleDefinition target_module, System.Reflection.MethodBase method) {
            var method_path = method.ToPath();

            if (_InjectTargetDMDMap.TryGetValue(method_path, out DynamicMethodDefinition cached_dmd)) {
                return cached_dmd;
            }

            Logger.Debug($"Building DynamicMethodDefinition for method '{method_path}', target of at least one injection");

            var method_params = method.GetParameters();
            var param_length = method_params.Length;
            if (!method.IsStatic) param_length += 1;

            var param_types = new Type[param_length];
            if (!method.IsStatic) param_types[0] = method.DeclaringType;
            for (var i = 0; i < method_params.Length; i++) {
                param_types[method.IsStatic ? i : i + 1] = method_params[i].ParameterType;
            }

            var dmd = new DynamicMethodDefinition(name, (method as System.Reflection.MethodInfo)?.ReturnType ?? typeof(void), param_types);

            dmd.Definition.IsStatic = true;
            dmd.Definition.HasThis = false;
            dmd.Definition.ExplicitThis = false;

            var target_method = target_path.FindIn<MethodDefinition>(target_module);

            dmd.Definition.Body = target_method.Body.CloneBodyAndReimport(_RunningModule, dmd.Definition);

            for (var i = 0; i < dmd.Definition.Body.Instructions.Count; i++) {
                _InjectInstructionMap[target_method.Body.Instructions[i]] = dmd.Definition.Body.Instructions[i];
            }

            _InjectTargetDMDMap[target_path] = dmd;

            return dmd;
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

            handler_dmd.Definition.Body = handler_method.Body.CloneBodyAndReimport(
                _RunningModule,
                handler_dmd.Definition
            );

            return handler_dmd;
        }

        public void ProcessInjectionDifference(Relinker relinker, InjectionDifference diff, bool update_running_module = false) {
            Logger.Debug($"Processing injection difference for injection handler '{diff.HandlerPath}', target '{diff.TargetPath}'");

            if (diff is InjectionRemoved || diff is InjectionChanged) {
                if (diff is InjectionChanged) Logger.Debug($"Resetting changed injection '{diff.Signature}'");
                else Logger.Debug($"Disposing of removed injection '{diff.Signature}'");

                if (_InjectHandlerDMDMap.TryGetValue(diff.Signature, out DynamicMethodDefinition old_dmd)) {
                    old_dmd.Dispose();
                }
                _InjectHandlerDMDMap.Remove(diff.Signature);

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
            var preinject_dmd = GetInjectionTargetDMD(target_path.Signature.Name, target_path, diff.Target.Module, preinject_reflection);

            if (diff is InjectionRemoved) return;

            var handler_dmd = GetInjectionHandlerDMD(diff.Signature, diff.Handler, preinject_reflection);

            relinker.Relink(new Relinker.State(_RunningModule), handler_dmd.Definition);

            var target_instr = _InjectInstructionMap[diff.InjectionPoint];

            var handler = handler_dmd.Generate();

            var call_proxy = new DynamicMethodCallHandlerProxy(handler, !preinject_reflection.IsStatic);

            _InjectHandlerDMDCallProxyMap[diff.Signature] = call_proxy;

            Injector.InsertInjectCall(
                preinject_dmd.Definition,
                handler_dmd.Definition,
                call_proxy,
                target_instr,
                diff.Position,
                diff.LocalCaptures
            );
        }

        private void _CreateInjectionDMDThunk(MethodPath path, DynamicMethodDefinition dmd) {
            var target = path.FindIn<MethodDefinition>(_RunningModule);
            var support_attrs = new RDARSupport.SupportAttributeData(target.CustomAttributes);
            var inject = dmd.Generate();

            var thunk_path = target.ToPath();

            // if the method has an orig, that means we want to
            // redirect the orig to the rtinject method instead of
            // the target - meaning that like at static time,
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
            var stub = RuntimePatchManager.CreateCallStub(thunk, inject);
            _Detours.Add(new Hook(thunk, stub));
        }

        public void GenerateInjectionTargets() {
            Logger.Debug($"Patching injection targets");
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
