using System;
using System.Collections.Generic;
using System.Text;
using ModTheGungeon;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MonoMod.Utils;

namespace SemiPatch {
    public class StaticPatcher {
        public static ModuleDefinition MscorlibModule;
        public static TypeReference StringType;
        public static TypeReference IntType;
        public static TypeReference InjectPositionType;
        public static MethodDefinition RDARSupportNameAliasedFromAttributeConstructor;
        public static MethodDefinition RDARSupportHasOriginalInAttributeConstructor;
        public static MethodDefinition RDARSupportHasPreinjectInAttributeConstructor;
        public static MethodDefinition RDARSupportStaticallyInjectedAttributeConstructor;
        public static MethodDefinition RDARSupportStaticInjectionHandlerAttributeConstructor;

        static StaticPatcher() {
            MscorlibModule = ModuleDefinition.ReadModule(typeof(string).Assembly.Location);
            StringType = MscorlibModule.GetType("System.String");
            IntType = MscorlibModule.GetType("System.Int32");
            InjectPositionType = SemiPatch.SemiPatchModule.GetType("SemiPatch.InjectPosition");

            RDARSupportNameAliasedFromAttributeConstructor = RDARSupport.RDARSupport.RDARSupportNameAliasedFromAttribute.Methods[0];
            RDARSupportHasOriginalInAttributeConstructor = RDARSupport.RDARSupport.RDARSupportHasOriginalInAttribute.Methods[0];
            RDARSupportHasPreinjectInAttributeConstructor = RDARSupport.RDARSupport.RDARSupportHasPreinjectInAttribute.Methods[0];
            RDARSupportStaticallyInjectedAttributeConstructor = RDARSupport.RDARSupport.RDARSupportStaticallyInjectedAttribute.Methods[0];
            RDARSupportStaticInjectionHandlerAttributeConstructor = RDARSupport.RDARSupport.RDARSupportStaticInjectionHandlerAttribute.Methods[0];
        }

        private Relinker _Relinker;
        private IList<PatchData> _Patches = new List<PatchData>();
        private IList<ModuleDefinition> _PatchModules = new List<ModuleDefinition>();
        public static Logger Logger = new Logger(nameof(StaticPatcher));
        public ModuleDefinition TargetModule;

        private IDictionary<MethodPath, string> _OrigNameMap = new Dictionary<MethodPath, string>();
        public IDictionary<MethodPath, MethodDefinition> _OrigSourceMap = new Dictionary<MethodPath, MethodDefinition>();
        private IDictionary<MethodPath, string> _PreinjectNameMap = new Dictionary<MethodPath, string>();
        private IDictionary<MethodPath, IDictionary<Instruction, Instruction>> _InjectInstructionMap = new Dictionary<MethodPath, IDictionary<Instruction, Instruction>>();

        public StaticPatcher(ModuleDefinition target_module) {
            _Relinker = new Relinker();
            _Relinker.LaxOnInserts = true;
            TargetModule = target_module;
        }

        public void LoadPatch(PatchData p, ModuleDefinition module) {
            _Patches.Add(p);
            _PatchModules.Add(module);
        }

        private void _ProcessPatch(PatchTypeData type, PatchFieldData field) {
            if (field.ExplicitlyIgnored) {
                Logger.Debug($"Ignored field: '{field.PatchPath}'");
                return;
            }
            if (field.Proxy) Logger.Debug($"Proxied field: '{field.PatchPath}'");

            if (!field.EffectivelyIgnored) {
                Logger.Debug($"Patching field: '{field.Patch.ToPath()}', target: '{field.Target?.ToPath().ToString() ?? "<none>"}'");
                FieldDefinition target_field;

                if (field.IsInsert) {
                    target_field = new FieldDefinition(
                        field.Patch.Name,
                        field.Patch.Attributes,
                        type.TargetType.Module.ImportReference(field.Patch.FieldType)
                    ) {
                        HasDefault = field.Patch.HasDefault,
                        DeclaringType = type.TargetType
                    };

                    if (field.Patch.HasConstant) {
                        target_field.Constant = field.Patch.Constant.ImportUntyped(TargetModule);
                    }
                    type.TargetType.Fields.Add(target_field);
                } else target_field = field.Target;

                for (var i = 0; i < field.Patch.CustomAttributes.Count; i++) {
                    target_field.CustomAttributes.Add(field.Patch.CustomAttributes[i].Clone(TargetModule));
                }
            }

            _Relinker.Map(field.PatchPath, Relinker.MemberEntry.FromPatchData(
                type.TargetType.Module,
                type,
                field
            ));
        }

        private void _ProcessPatch(PatchTypeData type, PatchPropertyData prop) {
            if (prop.ExplicitlyIgnored) {
                Logger.Debug($"Ignored property: '{prop.PatchPath}'");
                return;
            }
            if (prop.Proxy) Logger.Debug($"Proxied property: '{prop.PatchPath}'");

            if (!prop.EffectivelyIgnored) {
                Logger.Debug($"Patching property: '{prop.Patch.ToPath()}', target: '{prop.Target?.ToPath().ToString() ?? "<none>"}'");
                PropertyDefinition target_prop;

                if (prop.IsInsert) {
                    target_prop = new PropertyDefinition(
                        prop.Patch.Name,
                        prop.Patch.Attributes,
                        type.TargetType.Module.ImportReference(prop.Patch.PropertyType)
                    ) {
                        DeclaringType = type.TargetType
                    };

                    if (prop.Patch.HasConstant) {
                        target_prop.Constant = prop.Patch.Constant.ImportUntyped(TargetModule);
                    }

                    if (prop.Patch.GetMethod != null) {
                        target_prop.GetMethod = prop.Patch.GetMethod.ToPath().WithDeclaringType(type.TargetType).FindIn<MethodDefinition>(type.TargetType.Module);
                    }

                    if (prop.Patch.SetMethod != null) {
                        target_prop.SetMethod = prop.Patch.SetMethod.ToPath().WithDeclaringType(type.TargetType).FindIn<MethodDefinition>(type.TargetType.Module);
                    }

                    type.TargetType.Properties.Add(target_prop);
                } else target_prop = prop.Target;

                for (var i = 0; i < prop.Patch.CustomAttributes.Count; i++) {
                    target_prop.CustomAttributes.Add(prop.Patch.CustomAttributes[i].Clone(TargetModule));
                }
            }
            
            _Relinker.Map(prop.PatchPath, Relinker.MemberEntry.FromPatchData(
                type.TargetType.Module,
                type,
                prop
            ));

        }

        public string _MapOrigForMethod(PatchMethodData method) {
            return _MapOrigForMethod((MethodPath)method.TargetPath, method.Target.Name);
        }

        public string _MapOrigForMethod(MethodPath target_path, string target_name) {
            if (_OrigNameMap.TryGetValue(target_path, out string name)) return name;

            var s = new StringBuilder();
            s.Append("$SEMIPATCH$ORIG$$");
            s.Append(target_name);
            return _OrigNameMap[target_path] = s.ToString();
        }

        private Instruction _CopyInstruction(Instruction instr, MethodDefinition target, int arg_offset = 0) {
            var new_instr = Instruction.Create(OpCodes.Nop);
            new_instr.OpCode = instr.OpCode;
            if (instr.Operand is IMetadataTokenProvider) {
                new_instr.Operand = TargetModule.ImportReference((IMetadataTokenProvider)instr.Operand);
            } else if (instr.Operand is VariableDefinition) {
                new_instr.Operand = target.Body.Variables[((VariableDefinition)instr.Operand).Index];
            } else if (instr.Operand is ParameterDefinition) {
                new_instr.Operand = target.Parameters[((ParameterDefinition)instr.Operand).Index - arg_offset];
            } else new_instr.Operand = instr.Operand;
            return new_instr;
        }

        private void _HandleReceiveOriginalMethod(PatchMethodData method, MethodDefinition target, IList<VariableDefinition> original_target_vars) {
            var orig_name = _MapOrigForMethod(method);
            Logger.Debug($"Creating orig method: '{orig_name}'");

            var orig_def = new MethodDefinition(orig_name, target.Attributes, target.ReturnType);
            for (var i = 0; i < target.GenericParameters.Count; i++) {
                var patch_param = target.GenericParameters[i];
                var orig_param = new GenericParameter(patch_param.Name, orig_def);
                orig_def.GenericParameters.Add(orig_param);
            }
            for (var i = 0; i < target.Parameters.Count; i++) {
                orig_def.Parameters.Add(target.Parameters[i].Clone(TargetModule));
            }
            orig_def.Body = method.Target.Body.Clone(orig_def);
            orig_def.Body.Variables.Clear();
            for (var i = 0; i < original_target_vars.Count; i++) {
                var var_def = original_target_vars[i];
                orig_def.Body.Variables.Add(new VariableDefinition(var_def.VariableType));
            }
            //var il = orig_def.Body.GetILProcessor();
            //il.Append(Instruction.Create(OpCodes.Ret));
            target.DeclaringType.Methods.Add(orig_def);
            orig_def.DeclaringType = target.DeclaringType;
            _OrigSourceMap[orig_def.ToPath()] = target;

            var orig = orig_def.Module.ImportReference(OrigFactory.OrigTypeForMethod(orig_def.Module, orig_def));

            Logger.Debug($"Orig method: '{orig_def}', orig type: '{orig}'");

            var patch = method.Patch;
            patch.Body.SimplifyMacros();
            var instr_map = new Dictionary<Instruction, Instruction>();
            target.Body.Instructions.Clear();
            for (var i = 0; i < patch.Body.Instructions.Count; i++) {
                var instr = patch.Body.Instructions[i];
                var new_instr = Instruction.Create(OpCodes.Nop);
                new_instr.OpCode = instr.OpCode;
                if (instr.Operand is IMetadataTokenProvider op) {
                    new_instr.Operand = TargetModule.ImportReference(op);
                } else new_instr.Operand = instr.Operand;
                target.Body.Instructions.Add(new_instr);

                instr_map[instr] = new_instr;
            }

            for (var i = 0; i < target.Body.Instructions.Count; i++) {
                var instr = target.Body.Instructions[i];
                if (instr.Operand is Instruction op_instr) {
                    instr.Operand = instr_map[op_instr];
                }
            }

            if (method.ReceivesOriginal) {
                VariableDefinition orig_delegate_local = null;
                Logger.Debug($"Rewriting Orig/VoidOrig Invokes for ReceiveOriginal patch method");

                var il = target.Body.GetILProcessor();

                for (var i = 0; i < il.Body.Instructions.Count; i++) {
                    var instr = il.Body.Instructions[i];

                    if (instr.OpCode == OpCodes.Callvirt) {
                        var call_target = (MethodReference)instr.Operand;
                        if (call_target.DeclaringType.IsSame(orig) && call_target.Name == "Invoke") {
                            Logger.Debug($"Attempting orig optimization from IL_{instr.Offset.ToString("x4")}");

                            Instruction orig_ldarg_instr = null;
                            var success = false;
                            var stack_count = 0;
                            var prev = instr.Previous;
                            while (prev != null) {
                                stack_count += prev.ComputeStackDelta();

                                if (stack_count == orig_def.Parameters.Count + 1) {
                                    if (prev.OpCode == OpCodes.Ldarg) {
                                        var param = (ParameterReference)prev.Operand;
                                        if (param == patch.Parameters[0]) {
                                            orig_ldarg_instr = prev;
                                            success = true;
                                            break;
                                        }
                                    } else {
                                        break;
                                    }
                                }
                                prev = prev.Previous;
                            }

                            if (success) {
                                Logger.Debug($"Optimization successful, orig passed at: {orig_ldarg_instr}");

                                if (orig_def.IsStatic) {
                                    orig_ldarg_instr.OpCode = OpCodes.Nop;
                                    orig_ldarg_instr.Operand = null;
                                } else {
                                    orig_ldarg_instr.OpCode = OpCodes.Ldarg_0;
                                    orig_ldarg_instr.Operand = null;
                                }

                                MethodReference orig_ref = orig_def;
                                if (orig_def.DeclaringType.HasGenericParameters) {
                                    Logger.Debug($"Fixing declaring type on optimized direct call");
                                    var generic_type = new GenericInstanceType(orig_ref.DeclaringType);
                                    for (var j = 0; j < orig_ref.DeclaringType.GenericParameters.Count; j++) {
                                        generic_type.GenericArguments.Add(orig_ref.DeclaringType.GenericParameters[j]);
                                    }
                                    var orig_target_type = orig_def.Module.ImportReference(generic_type);
                                    orig_ref = orig_def.MakeReference();
                                    orig_ref.DeclaringType = orig_target_type;
                                }

                                instr.OpCode = orig_def.IsVirtual ? OpCodes.Callvirt : OpCodes.Call;
                                instr.Operand = orig_ref;
                            } else {
                                Logger.Debug($"Optimization unsuccessful");
                            }
                        }
                    }
                }

                for (var i = 0; i < il.Body.Instructions.Count; i++) {
                    var instr = il.Body.Instructions[i];

                    if (instr.OpCode == OpCodes.Ldarg) {
                        var param = (ParameterReference)instr.Operand;
                        if (param != patch.Parameters[0]) {
                            instr.Operand = target.Parameters[((ParameterReference)instr.Operand).Index - 1];
                            continue;
                        }
                        Instruction new_instr;

                        if (orig_delegate_local == null) {
                            orig_delegate_local = new VariableDefinition(orig);
                            target.Body.Variables.Add(orig_delegate_local);
                            if (target.IsStatic) {
                                il.Replace(instr, new_instr = il.Create(OpCodes.Ldnull));
                            } else {
                                il.Replace(instr, new_instr = il.Create(OpCodes.Ldarg_0));
                            }
                            Instruction ldftn_instr;

                            MethodReference orig_def_spec = orig_def;
                            if (orig_def.HasGenericParameters) {
                                var orig_def_spec_generic = new GenericInstanceMethod(orig_def);
                                for (var j = 0; j < target.GenericParameters.Count; j++) {
                                    orig_def_spec_generic.GenericArguments.Add(target.GenericParameters[j]);
                                }
                                orig_def_spec = orig_def_spec_generic;
                            }
                            il.InsertAfter(new_instr, ldftn_instr = il.Create(OpCodes.Ldftn, orig_def_spec));
                            i += 1;
                            var ctor = orig_def.Module.ImportReference(OrigFactory.NativePointerConstructorForOrigType(orig_def.Module, orig));
                            Instruction newobj_instr;
                            il.InsertAfter(ldftn_instr, newobj_instr = il.Create(OpCodes.Newobj, ctor));
                            i += 1;
                            Instruction stloc_instr;
                            il.InsertAfter(newobj_instr, stloc_instr = il.Create(OpCodes.Stloc, orig_delegate_local));
                            i += 1;
                            il.InsertAfter(stloc_instr, il.Create(OpCodes.Ldloc, orig_delegate_local));
                            i += 1;
                        } else {
                            il.Replace(instr, il.Create(OpCodes.Ldloc, orig_delegate_local));
                        }
                    }
                }

                var attr = new CustomAttribute(TargetModule.ImportReference(RDARSupportHasOriginalInAttributeConstructor));
                attr.ConstructorArguments.Add(new CustomAttributeArgument(StringType, orig_def.Name));
                target.CustomAttributes.Add(attr);

                //Logger.Debug($"Removing Orig/VoidOrig parameter");
                //target.Parameters.RemoveAt(0);
                target.Body.OptimizeMacros();
                patch.Body.OptimizeMacros();
            }
        }

        private void _ProcessPatch(PatchTypeData type, PatchMethodData method) {
            if (method.ExplicitlyIgnored) {
                Logger.Debug($"Ignored method: '{method.PatchPath}'");
                return;
            }

            if (method.FalseDefaultConstructor) Logger.Debug($"Ignored method (false-default-ctor): '{method.PatchPath}'");
            if (method.Proxy) Logger.Debug($"Proxied method: '{method.PatchPath}'");

            if (!method.EffectivelyIgnored) { 
                Logger.Debug($"Patching method: '{method.Patch.ToPath()}', target: '{method.Target?.ToPath().ToString() ?? "<none>"}'");
                MethodDefinition target_method; 

                if (method.IsInsert) {
                    target_method = new MethodDefinition(
                        method.Patch.Name,
                        method.Patch.Attributes,
                        type.TargetType.Module.ImportReference(method.Patch.ReturnType)
                    ) {
                        DeclaringType = type.TargetType,
                        HasThis = method.Patch.HasThis,
                        ExplicitThis = method.Patch.ExplicitThis,
                        DebugInformation = method.Patch.DebugInformation,
                        ImplAttributes = method.Patch.ImplAttributes,
                        SemanticsAttributes = method.Patch.SemanticsAttributes,
                    };

                    for (var i = 0; i < method.Patch.Parameters.Count; i++) {
                        var param = method.Patch.Parameters[i];
                        var new_param = new ParameterDefinition(
                            param.Name,
                            param.Attributes,
                            type.TargetType.Module.ImportReference(param.ParameterType)
                        );
                        target_method.Parameters.Add(new_param);
                    }

                    for (var i = 0; i < method.Patch.GenericParameters.Count; i++) {
                        var param = method.Patch.GenericParameters[i];
                        var new_param = new GenericParameter(
                            param.Name,
                            target_method
                        );
                        target_method.GenericParameters.Add(new_param);
                    }

                    type.TargetType.Methods.Add(target_method);
                } else {
                    target_method = method.Target;
                }

                for (var i = 0; i < method.Patch.CustomAttributes.Count; i++) {
                    target_method.CustomAttributes.Add(method.Patch.CustomAttributes[i].Clone(TargetModule));
                }

                var original_target_variables = new VariableDefinition[target_method.Body.Variables.Count];
                for (var i = 0; i < target_method.Body.Variables.Count; i++) {
                    original_target_variables[i] = target_method.Body.Variables[i];
                }

                target_method.Body.Variables.Clear();
                for (var i = 0; i < method.Patch.Body.Variables.Count; i++) {
                    var var_def = method.Patch.Body.Variables[i];
                    target_method.Body.Variables.Add(
                        new VariableDefinition(TargetModule.ImportReference(var_def.VariableType))
                    );
                }

                if (method.ReceivesOriginal) {
                    _HandleReceiveOriginalMethod(method, target_method, original_target_variables);
                } else {
                    target_method.Body = method.Patch.Body.Clone(target_method, TargetModule);
                }
            }

            _Relinker.Map(method.PatchPath, Relinker.MemberEntry.FromPatchData(
                type.TargetType.Module,
                type,
                method
            ));


        }

        //private Instruction _GetMappedInstruction(MethodPath target_path, MethodDefinition target, Instruction instr) {
        //    if (!_InjectInstructionMap.TryGetValue(target_path, out IDictionary<Instruction, Instruction> instr_map)) {
        //        _InjectInstructionMap[target_path] = instr_map = new Dictionary<Instruction, Instruction>();
        //        for (var i = 0; i < target.Body.Instructions.Count; i++) {
        //            instr_map[target.Body.Instructions[i]]
        //        }
        //    }


        //}

        public string _MapPreinjectForInjection(PatchInjectData method) {
            return _MapPreinjectForInjection(method.TargetPath, method.Target.Name);
        }

        public string _MapPreinjectForInjection(MethodPath target_path, string target_name) {
            if (_PreinjectNameMap.TryGetValue(target_path, out string name)) return name;

            var s = new StringBuilder();
            s.Append("$SEMIPATCH$PREINJECT$$");
            s.Append(target_name);
            return _PreinjectNameMap[target_path] = s.ToString();
        }


        private void _ProcessPatch(PatchTypeData type, PatchInjectData inject) {
            Logger.Debug($"Injecting: '{inject.HandlerPath}', target: '{inject.TargetPath}'");

            var preinject_name = _MapPreinjectForInjection(inject);
            MethodDefinition preinject_def;

            var rdar_attrs = new RDARSupport.SupportAttributeData(inject.Target.CustomAttributes);
            if (!rdar_attrs.IsStaticallyInjected) {
                preinject_def = new MethodDefinition(preinject_name, inject.Target.Attributes, inject.Target.ReturnType);
                for (var i = 0; i < inject.Target.GenericParameters.Count; i++) {
                    var patch_param = inject.Target.GenericParameters[i];
                    var orig_param = new GenericParameter(patch_param.Name, preinject_def);
                    preinject_def.GenericParameters.Add(orig_param);
                }
                for (var i = 0; i < inject.Target.Parameters.Count; i++) {
                    preinject_def.Parameters.Add(inject.Target.Parameters[i].Clone(TargetModule));
                }
                preinject_def.Body = inject.Target.Body.Clone(preinject_def);
                type.TargetType.Methods.Add(preinject_def);

                var attr = new CustomAttribute(TargetModule.ImportReference(RDARSupportHasPreinjectInAttributeConstructor));
                attr.ConstructorArguments.Add(new CustomAttributeArgument(StringType, preinject_def.Name));
                inject.Target.CustomAttributes.Add(attr);

                var injected_attr = new CustomAttribute(TargetModule.ImportReference(RDARSupportStaticallyInjectedAttributeConstructor));
                inject.Target.CustomAttributes.Add(injected_attr);
            } else {
                preinject_def = new MethodPath(
                    new Signature(inject.Target, forced_name: preinject_name),
                    inject.Target.DeclaringType
                ).FindIn<MethodDefinition>(TargetModule);
            }

            var handler_path = inject.HandlerPath.WithDeclaringType(type.TargetType);
            if (inject.HandlerAliasedName != null) {
                handler_path = inject.HandlerPath.WithSignature(new Signature(inject.Handler, forced_name: inject.HandlerAliasedName));
            }

            var rdar_handler_attr = new CustomAttribute(TargetModule.ImportReference(
                RDARSupportStaticInjectionHandlerAttributeConstructor)
            );
            var injection_sig = new InjectionSignature(inject.Handler.ToPath(), inject.Target.ToPath());
            rdar_handler_attr.ConstructorArguments.Add(new CustomAttributeArgument(StringType, injection_sig.ToString()));
            rdar_handler_attr.ConstructorArguments.Add(new CustomAttributeArgument(StringType, inject.Handler.Name));
            rdar_handler_attr.ConstructorArguments.Add(new CustomAttributeArgument(StringType, new Signature(inject.Handler).ToString()));
            rdar_handler_attr.ConstructorArguments.Add(new CustomAttributeArgument(IntType, inject.BodyIndex));
            rdar_handler_attr.ConstructorArguments.Add(new CustomAttributeArgument(InjectPositionType, inject.Position));
            inject.Target.CustomAttributes.Add(rdar_handler_attr);

            _ProcessPatch(
                type,
                new PatchMethodData(
                    patch: inject.Handler,
                    target_path: handler_path,
                    patch_path: inject.HandlerPath
                )
            );

            Injector.InsertInjectCall(
                inject.Target,
                inject.Handler,
                new DirectMethodCallHandlerProxy(inject.Handler),
                inject.InjectionPoint,
                inject.Position,
                inject.LocalCaptures
            );
        }

        private void _ProcessPatch(PatchTypeData type) {
            Logger.Debug($"Patching type: '{type.PatchType.ToPath()}', target: '{type.TargetType.ToPath()}'");

            for (var i = 0; i < type.Injections.Count; i++) {
                _ProcessPatch(type, type.Injections[i]);
            }

            for (var i = 0; i < type.Fields.Count; i++) {
                _ProcessPatch(type, type.Fields[i]);
            }

            for (var i = 0; i < type.Methods.Count; i++) {
                _ProcessPatch(type, type.Methods[i]);
            }

            for (var i = 0; i < type.Properties.Count; i++) {
                _ProcessPatch(type, type.Properties[i]);
            }

            for (var i = 0; i < type.PatchType.CustomAttributes.Count; i++) {
                type.TargetType.CustomAttributes.Add(type.PatchType.CustomAttributes[i].Clone(TargetModule));
            }

            _Relinker.Map(type.PatchType.ToPath(), Relinker.TypeEntry.FromPatchData(
                type.TargetType.Module,
                type
            ));
        }

        private void _ProcessPatch(PatchData patch) {
            for (var i = 0; i < patch.Types.Count; i++) {
                _ProcessPatch(patch.Types[i]);
            }
        }

        private void _MergeModule(ModuleDefinition mod) {
            for (var i = 0; i < mod.Types.Count; i++) {
                var type = mod.Types[i];
                if (_IsExcludedFromMerging(type)) continue;
                Logger.Debug($"Merging type: {type.BuildSignature()}");
                type.Clone(TargetModule, _IsExcludedFromMerging);
            }
        }

        private bool _IsExcludedFromMerging(TypeDefinition type) {
            if (type.Namespace == "" && type.FullName == "<Module>") {
                return true;
            }
            for (var i = 0; i < type.CustomAttributes.Count; i++) {
                var attr = type.CustomAttributes[i];
                if (attr.AttributeType.IsSame(SemiPatch.PatchAttribute)) return true;
            }
            return false;
        }

        public void Patch() {
            for (var i = 0; i < _Patches.Count; i++) {
                _ProcessPatch(_Patches[i]);
            }
            for (var i = 0; i < _PatchModules.Count; i++) {
                _MergeModule(_PatchModules[i]);
            }
            _Relinker.Relink(TargetModule);
        }
    }
}
