using System;
using System.Collections.Generic;
using ModTheGungeon;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using BindingFlags = System.Reflection.BindingFlags;
using static SemiPatch.AssemblyDiff;

namespace SemiPatch {
    /// <summary>
    /// Powerful low level interface to <see cref="MonoMod.RuntimeDetour"/>, capable
    /// of generating and reloading methods at runtime. It is recommended to use
    /// the slightly higher level <see cref="RuntimeClient"/> type to perform
    /// loading and runtime reloading of SemiPatch patches.
    /// </summary>
    public class RuntimePatchManager : IDisposable {
        public static TypeDefinition RuntimeTypeHandleType = SemiPatch.MscorlibModule.GetType("System.RuntimeTypeHandle");
        public static TypeDefinition MethodInfoType = SemiPatch.MscorlibModule.GetType("System.Reflection.MethodInfo");
        public static TypeDefinition ObjectType = SemiPatch.MscorlibModule.GetType("System.Object");
        public static Logger Logger = new Logger("RuntimePatchManager");

        private Dictionary<MemberPath, IDetour> _MethodPatchMap = new Dictionary<MemberPath, IDetour>();
        private Dictionary<MemberPath, IDetour> _CallStubToOrigMap = new Dictionary<MemberPath, IDetour>();
        private ModuleDefinition _RunningModule;
        private System.Reflection.Assembly _RunningAssembly;
        private RuntimeInjectionManager _InjectionManager;
        public Relinker Relinker;

        public RuntimePatchManager(Relinker relinker, System.Reflection.Assembly asm, ModuleDefinition running_module) {
            _RunningAssembly = asm;
            _RunningModule = running_module;
            Relinker = relinker;
            _InjectionManager = new RuntimeInjectionManager(relinker, asm, running_module);
        }

        private void _ReplaceMethod(MethodDefinition patch_method, MethodPath target_path, System.Reflection.Assembly target_asm, ModuleDefinition target_module, bool update_running_module = false) {
            var target_method = (System.Reflection.MethodBase)target_path.FindIn(target_asm);

            var has_orig = (patch_method.Parameters.Count >= 1
                && (
                    OrigFactory.TypeIsGenericOrig(patch_method.Parameters[0].ParameterType)
                    || OrigFactory.TypeIsGenericVoidOrig(patch_method.Parameters[0].ParameterType)
                ));

            Relinker.Relink(new Relinker.State(patch_method.Module), patch_method);

            var method = _PreprocessPatchMethodForHooking(
                patch_method,
                target_path.FindIn(target_module).DeclaringType,
                has_orig
            );

            if (update_running_module) {
                var running_module_target_method = target_path.FindIn<MethodDefinition>(_RunningModule);
                running_module_target_method.Body = patch_method.Body;
            }

            var target_method_params = target_method.GetParameters();
            var dmd_params_length = target_method_params.Length;
            if (!target_method.IsStatic) dmd_params_length += 1;
            if (has_orig) dmd_params_length += 1;
            var dmd_types = new Type[dmd_params_length];
            if (has_orig) dmd_types[0] = method.Parameters[0].ParameterType.ToReflection();
            if (!target_method.IsStatic) dmd_types[has_orig ? 1 : 0] = target_method.DeclaringType;
            for (var k = 0; k < target_method_params.Length; k++) {
                dmd_types[k + (has_orig ? 1 : 0) + (target_method.IsStatic ? 0 : 1)] = target_method_params[k].ParameterType;
            }
            var dmd = new DynamicMethodDefinition(patch_method.Name, (target_method as System.Reflection.MethodInfo)?.ReturnType ?? typeof(void), dmd_types);
            var il = dmd.GetILProcessor();
            dmd.Definition.Body = method.Body.Clone(dmd.Definition);
            var dmd_method = dmd.Generate();

            var attrs = target_method.GetCustomAttributes(true);
            for (var k = 0; k < attrs.Length; k++) {
                var attr = attrs[k];

                if (typeof(RDARSupport.HasOriginalInAttribute).IsAssignableFrom(attr.GetType())) {
                    var orig_attr = (RDARSupport.HasOriginalInAttribute)attr;
                    var orig_sig = new Signature(target_method, forced_name: orig_attr.OrigName);
                    var methods = target_method.DeclaringType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                    System.Reflection.MethodInfo orig_method = null;

                    for (var l = 0; l < methods.Length; l++) {
                        var search_method = methods[l];
                        var search_method_sig = new Signature(search_method);
                        if (search_method_sig == orig_sig) {
                            orig_method = search_method;
                            break;
                        }
                    }

                    if (orig_method == null) {
                        throw new Exception($"Target method {new Signature(target_method)} has a static-patch artifact of a member orig method, but one could not be found with the signature '{orig_sig}'");
                    }

                    if (!_CallStubToOrigMap.ContainsKey(target_path)) {
                        Logger.Debug($"Inserted call stub in '{target_path}' to '{orig_sig}'.");
                        var stub = RDARPrimitive.CreateThunk(target_method, orig_method);
                        var orig_stub_detour = new Hook(target_method, stub);

                        _CallStubToOrigMap[target_path] = orig_stub_detour;

                        target_method = orig_method;
                    } else {
                        Logger.Debug($"Call stub in '{target_path}' to '{orig_sig}' already exists, not hooking.");
                    }
                    break;
                }
            }

            var hook = new Hook(target_method, dmd_method);
            _MethodPatchMap[patch_method.ToPath()] = hook;
        }

        protected virtual void _ProcessMethodDifference(MemberDifference diff, bool update_running_module = false) {
            Logger.Debug($"Processing method difference for target '{diff.TargetPath}'");

            var patch_path = ((MethodDefinition)diff.Member).ToPath();
            if (_MethodPatchMap.TryGetValue(patch_path, out IDetour old_detour)) {
                Logger.Debug($"Disposing of existing patch hook for '{patch_path}' targetting '{diff.TargetPath}'.");
                old_detour.Dispose();
                _MethodPatchMap.Remove(patch_path);
            }

            if (diff is MemberAdded) {
                throw new UnsupportedRDAROperationException(diff);
            } else if (diff is MemberRemoved) {
                throw new UnsupportedRDAROperationException(diff);
            } else if (diff is MemberChanged change_diff) {
                _ReplaceMethod(
                    change_diff.Member as MethodDefinition,
                    change_diff.TargetPath as MethodPath,
                    _RunningAssembly,
                    _RunningModule,
                    update_running_module
                );
            } else {
                throw new UnsupportedRDAROperationException(diff);
            }
        }

        protected virtual void _ProcessFieldDifference(MemberDifference diff, bool update_running_module = false) {
            Logger.Debug($"Processing field difference for target '{diff.TargetPath}'");

            throw new UnsupportedRDAROperationException(diff);
        }

        protected virtual void _ProcessPropertyDifference(MemberDifference diff, bool update_running_module = false) {
            Logger.Debug($"Processing property difference for target '{diff.TargetPath}'");

            throw new UnsupportedRDAROperationException(diff);
        }

        private void _ProcessMemberDifference(MemberDifference diff, bool update_running_module = false) {
            if (diff.Type == MemberType.Method) {
                _ProcessMethodDifference(diff, update_running_module);
            } else if (diff.Type == MemberType.Field) {
                _ProcessFieldDifference(diff, update_running_module);
            } else if (diff.Type == MemberType.Property) {
                _ProcessPropertyDifference(diff, update_running_module);
            } else {
                throw new UnsupportedRDAROperationException(diff);
            }
        }

        protected virtual void _ProcessTypeAdded(TypeAdded diff, bool update_running_module = false) {
            throw new UnsupportedRDAROperationException(diff);
        }

        protected virtual void _ProcessTypeRemoved(TypeRemoved diff, bool update_running_module = false) {
            throw new UnsupportedRDAROperationException(diff);
        }

        protected virtual void _ProcessTypeChanged(TypeChanged diff, bool update_running_module = false) {
            // injections should be processed before methods can
            // to retain proper ordering of receiveoriginal patches +
            // injections (i.e. orig will be hooked by the injection stub
            // first and then it will be hooked by the patch hook)
            for (var i = 0; i < diff.InjectionDifferences.Count; i++) {
                _InjectionManager.ProcessInjectionDifference(diff.InjectionDifferences[i], update_running_module);
            }

            for (var i = 0; i < diff.MemberDifferences.Count; i++) {
                _ProcessMemberDifference(diff.MemberDifferences[i], update_running_module);
            }

            for (var i = 0; i < diff.NestedTypeDifferences.Count; i++) {
                _ProcessTypeDifference(diff.NestedTypeDifferences[i], update_running_module);
            }
        }

        private void _ProcessTypeDifference(TypeDifference diff, bool update_running_module = false) {
            Logger.Debug($"Processing type difference {diff.ToString()}");

            if (diff is TypeAdded) {
                _ProcessTypeAdded((TypeAdded)diff, update_running_module);
            } else if (diff is TypeRemoved) {
                _ProcessTypeRemoved((TypeRemoved)diff, update_running_module);
            } else if (diff is TypeChanged) {
                _ProcessTypeChanged((TypeChanged)diff, update_running_module);
            } else {
                throw new UnsupportedRDAROperationException(diff);
            }
        }

        protected bool _IsTypeGeneric(TypeDefinition type) {
            // types nested within generic types copy the generic parameters
            // (e.g. Class<T>.NestedClass is actually compiled as Class`1.NestedClass<T>)
            return type.GenericParameters.Count > 0;
        }

        protected bool _IsMethodGeneric(MethodDefinition method) {
            // we don't support neither generic type members nor generic members
            // themselves
            return _IsTypeGeneric(method.DeclaringType) || method.GenericParameters.Count > 0;
        }

        protected virtual bool _CanPatchTypeAtRuntime(TypeDifference type_diff) {
            if (type_diff.OldType != null && _IsTypeGeneric(type_diff.OldType)) {
                return false;
            }
            if (!(type_diff is TypeChanged change)) throw new UnsupportedRDAROperationException(type_diff);

            for (var i = 0; i < change.MemberDifferences.Count; i++) {
                var member = change.MemberDifferences[i];
                if (member.Type == MemberType.Method) {
                    if (!(member is MemberChanged)) {
                        return false;
                    }
                    if (_IsMethodGeneric((MethodDefinition)member.Member)) return false;
                } else return false;
            }

            for (var i = 0; i < change.NestedTypeDifferences.Count; i++) {
                var nested_type = change.NestedTypeDifferences[i];
                if (nested_type is TypeChanged nested_type_changed) {
                    if (!_CanPatchTypeAtRuntime(nested_type_changed)) {
                        return false;
                    }
                } else return false;
            }

            return true;
        }

        public bool CanPatchAtRuntime(AssemblyDiff diff) {
            for (var i = 0; i < diff.TypeDifferences.Count; i++) {
                var type = diff.TypeDifferences[i];

                if (!_CanPatchTypeAtRuntime(type)) return false;
            }

            return true;
        }

        public void ProcessDifference(AssemblyDiff diff, bool update_running_module = false) {
            Logger.Debug($"Processing assembly difference");
            for (var i = 0; i < diff.TypeDifferences.Count; i++) {
                _ProcessTypeDifference(diff.TypeDifferences[i], update_running_module);
            }
        }

        public void FinalizeProcessing() {
            _InjectionManager.GenerateInjectionTargets();
        }

        public void Dispose() {
            foreach (var kv in _MethodPatchMap) {
                kv.Value.Dispose();
            }
            foreach (var kv in _CallStubToOrigMap) {
                kv.Value.Dispose();
            }
            _InjectionManager.Dispose();
        }

        private void _RewriteMethodAsHookTarget(MethodDefinition method, TypeReference instance_type, bool has_orig) {
            // for public static methods we're set
            // order is (orig,...args) and orig doesn't take an instance type

            // for instance methods however, some work has to be done

            if (method.HasThis && !method.ExplicitThis) {
                var module = method.Module;
                ParameterDefinition orig_param = null;
                TypeReference explicit_orig_type = null;

                Logger.Debug($"Method before rewriting: {method.BuildSignature()}");
                Logger.Debug($"IL before rewriting:");
                for (var i = 0; i < method.Body.Instructions.Count; i++) {
                    Logger.Debug(method.Body.Instructions[i]);
                }

                if (has_orig) {
                    orig_param = method.Parameters[0];
                    var orig_type = (GenericInstanceType)orig_param.ParameterType;
                    explicit_orig_type = OrigFactory.ExplicitThisOrigTypeForOrig(
                        module,
                        instance_type, // DeclaringType of target method
                        orig_type
                    );

                    RDARPrimitive.RewriteOrigToExplicitThisOrig(
                       method,
                       explicit_orig_type,
                       method.Parameters[0]
                   );
                }

                // orig is optional so it's fine if we don't have it
                // if it's not used

                var self_param = new ParameterDefinition("$SEMIPATCH$self", ParameterAttributes.None, method.DeclaringType);

                method.HasThis = false;
                method.IsStatic = true;

                if (has_orig) {
                    // new order: (orig,self,...args)
                    method.Parameters.Insert(1, self_param);

                    // change out orig with ExplicitThisOrig
                    method.Parameters[0] = new ParameterDefinition(
                        orig_param.Name,
                        orig_param.Attributes,
                        explicit_orig_type
                    );

                    // here we will swap references to the two args in the IL
                    // (old this/0 -> new self/1, old orig/1 -> new orig/0)
                    // there is no need to change any other ldargs as we haven't
                    // actually changed the amount of REAL parameters - 
                    // we've technically changed the amount of parameters,
                    // but we've basically just incorporated the 'this' argument
                    // as an explicit argument so ldargs remain the same except for
                    // 1&2
                    for (var i = 0; i < method.Body.Instructions.Count; i++) {
                        var instr = method.Body.Instructions[i];

                        // we don't ever rewind the loop so this is fine
                        // (note the else if!)
                        if (instr.OpCode == OpCodes.Ldarg_0) instr.OpCode = OpCodes.Ldarg_1;
                        else if (instr.OpCode == OpCodes.Ldarg_1) instr.OpCode = OpCodes.Ldarg_0;
                    }
                } else {
                    // new order: (self,...args)
                    method.Parameters.Insert(0, self_param);
                }

                Logger.Debug($"Method after rewriting: {method.BuildSignature()}");
                Logger.Debug($"IL after rewriting:");
                for (var i = 0; i < method.Body.Instructions.Count; i++) {
                    Logger.Debug(method.Body.Instructions[i]);
                }

            }
        }

        private MethodDefinition _PreprocessPatchMethodForHooking(MethodDefinition method, TypeReference instance_type, bool has_orig, bool preserve_method_definition = false) {
            if (preserve_method_definition) method = method.Clone();

            _RewriteMethodAsHookTarget(method, instance_type, has_orig);

            return method;
        }
    }
}
