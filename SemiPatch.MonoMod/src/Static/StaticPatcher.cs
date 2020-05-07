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
        public static MethodDefinition RDARSupportNameAliasedFromAttributeConstructor;
        public static MethodDefinition RDARSupportHasOriginalInAttributeConstructor;
        public static MethodDefinition RDARSupportHasPreinjectInAttributeConstructor;
        public static MethodDefinition RDARSupportStaticallyInjectedAttributeConstructor;

        static StaticPatcher() {
            MscorlibModule = ModuleDefinition.ReadModule(typeof(string).Assembly.Location);
            StringType = MscorlibModule.GetType("System.String");
            RDARSupportNameAliasedFromAttributeConstructor = RDARSupport.RDARSupport.RDARSupportNameAliasedFromAttribute.Methods[0];
            RDARSupportHasOriginalInAttributeConstructor = RDARSupport.RDARSupport.RDARSupportHasOriginalInAttribute.Methods[0];
            RDARSupportHasPreinjectInAttributeConstructor = RDARSupport.RDARSupport.RDARSupportHasPreinjectInAttribute.Methods[0];
            RDARSupportStaticallyInjectedAttributeConstructor = RDARSupport.RDARSupport.RDARSupportStaticallyInjectedAttribute.Methods[0];
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
            if (field.ExplicitlyIgnored) Logger.Debug($"Ignored field: '{field.PatchPath}'");

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
                    target_field.Constant = _ImportUntyped(field.Patch.Constant);
                }
                type.TargetType.Fields.Add(target_field);
            } else target_field = field.Target;

            for (var i = 0; i < field.Patch.CustomAttributes.Count; i++) {
                target_field.CustomAttributes.Add(_Clone(field.Patch.CustomAttributes[i]));
            }

            _Relinker.Map(field.PatchPath, Relinker.MemberEntry.FromPatchData(
                type.TargetType.Module,
                type,
                field
            ));
        }

        private void _ProcessPatch(PatchTypeData type, PatchPropertyData prop) {
            if (prop.ExplicitlyIgnored) Logger.Debug($"Ignored property: '{prop.PatchPath}'");

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
                    target_prop.Constant = _ImportUntyped(prop.Patch.Constant);
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
                target_prop.CustomAttributes.Add(_Clone(prop.Patch.CustomAttributes[i]));
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
                orig_def.Parameters.Add(_Clone(target.Parameters[i]));
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
            if (method.ExplicitlyIgnored) Logger.Debug($"Ignored method: '{method.PatchPath}'");

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
                target_method.CustomAttributes.Add(_Clone(method.Patch.CustomAttributes[i]));
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
                target_method.Body = method.Patch.Body.CloneBodyAndReimport(TargetModule, target_method);
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
            var preinject_def = new MethodDefinition(preinject_name, inject.Target.Attributes, inject.Target.ReturnType);
            for (var i = 0; i < inject.Target.GenericParameters.Count; i++) {
                var patch_param = inject.Target.GenericParameters[i];
                var orig_param = new GenericParameter(patch_param.Name, preinject_def);
                preinject_def.GenericParameters.Add(orig_param);
            }
            for (var i = 0; i < inject.Target.Parameters.Count; i++) {
                preinject_def.Parameters.Add(_Clone(inject.Target.Parameters[i]));
            }
            preinject_def.Body = inject.Target.Body.Clone(preinject_def);
            type.TargetType.Methods.Add(preinject_def);

            var attr = new CustomAttribute(TargetModule.ImportReference(RDARSupportHasPreinjectInAttributeConstructor));
            attr.ConstructorArguments.Add(new CustomAttributeArgument(StringType, preinject_def.Name));
            inject.Target.CustomAttributes.Add(attr);

            var rdar_attrs = new RDARSupport.SupportAttributeData(inject.Target.CustomAttributes);
            if (!rdar_attrs.IsStaticallyInjected) {
                var injected_attr = new CustomAttribute(TargetModule.ImportReference(RDARSupportStaticallyInjectedAttributeConstructor));
                inject.Target.CustomAttributes.Add(injected_attr);
            }

            var handler_path = inject.HandlerPath.WithDeclaringType(type.TargetType);
            if (inject.HandlerAliasedName != null) {
                handler_path = inject.HandlerPath.WithSignature(new Signature(inject.Handler, forced_name: inject.HandlerAliasedName));
            }

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
                type.TargetType.CustomAttributes.Add(_Clone(type.PatchType.CustomAttributes[i]));
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

        private object _ImportUntyped(object obj) {
            if (obj is IMetadataTokenProvider) return (object)TargetModule.ImportReference((IMetadataTokenProvider)obj);
            return obj;
        }

        private EventDefinition _Clone(TypeDefinition decl_type, EventDefinition ev) {
            var new_event = new EventDefinition(
                ev.Name,
                ev.Attributes,
                TargetModule.ImportReference(ev.EventType)
            ) {
                AddMethod = ev.AddMethod?.ToPath()?.WithDeclaringType(decl_type)?.FindIn<MethodDefinition>(TargetModule),
                RemoveMethod = ev.RemoveMethod?.ToPath()?.WithDeclaringType(decl_type)?.FindIn<MethodDefinition>(TargetModule),
                InvokeMethod = ev.InvokeMethod?.ToPath()?.WithDeclaringType(decl_type)?.FindIn<MethodDefinition>(TargetModule)
            };


            for (var i = 0; i < ev.OtherMethods.Count; i++) {
                var other_method = ev.OtherMethods[i];
                new_event.OtherMethods.Add(
                    other_method.ToPath().WithDeclaringType(decl_type).FindIn<MethodDefinition>(TargetModule)
                );
            }

            for (var i = 0; i < ev.CustomAttributes.Count; i++) {
                var attr = ev.CustomAttributes[i];
                var new_attr = new CustomAttribute(
                    TargetModule.ImportReference(attr.Constructor),
                    attr.GetBlob()
                );
                new_event.CustomAttributes.Add(new_attr);
            }

            return new_event;
        }

        private PropertyDefinition _Clone(TypeDefinition decl_type, PropertyDefinition prop) {
            var new_prop = new PropertyDefinition(
                prop.Name,
                prop.Attributes,
                TargetModule.ImportReference(prop.PropertyType)
            ) {
                HasDefault = prop.HasDefault,
                GetMethod = prop.GetMethod?.ToPath()?.WithDeclaringType(decl_type)?.FindIn<MethodDefinition>(TargetModule),
                SetMethod = prop.SetMethod?.ToPath()?.WithDeclaringType(decl_type)?.FindIn<MethodDefinition>(TargetModule)
            };

            if (prop.HasConstant) {
                new_prop.Constant = _ImportUntyped(prop.Constant);
            }

            for (var i = 0; i < prop.OtherMethods.Count; i++) {
                var other_method = prop.OtherMethods[i];
                new_prop.OtherMethods.Add(
                    other_method.ToPath().WithDeclaringType(decl_type).FindIn<MethodDefinition>(TargetModule)
                );
            }

            for (var i = 0; i < prop.CustomAttributes.Count; i++) {
                var attr = prop.CustomAttributes[i];
                var new_attr = new CustomAttribute(
                    TargetModule.ImportReference(attr.Constructor),
                    attr.GetBlob()
                );
                new_prop.CustomAttributes.Add(new_attr);
            }

            return new_prop;
        }

        private FieldDefinition _Clone(TypeDefinition decl_type, FieldDefinition field) {
            var new_field = new FieldDefinition(
                field.Name,
                field.Attributes,
                TargetModule.ImportReference(field.FieldType)
            ) {
                InitialValue = field.InitialValue,
                HasDefault = field.HasDefault
            };

            if (field.HasConstant) {
                field.Constant = _ImportUntyped(field.Constant);
            }

            for (var i = 0; i < field.CustomAttributes.Count; i++) {
                var attr = field.CustomAttributes[i];
                var new_attr = new CustomAttribute(
                    TargetModule.ImportReference(attr.Constructor),
                    attr.GetBlob()
                );
                new_field.CustomAttributes.Add(new_attr);
            }

            return new_field;
        }

        private GenericParameter _Clone(IGenericParameterProvider owner, GenericParameter param) {
            var new_param = new GenericParameter(
                param.Name,
                owner
            );
            for (var i = 0; i < param.Constraints.Count; i++) {
                new_param.Constraints.Add(TargetModule.ImportReference(param.Constraints[i]));
            }
            return new_param;
        }

        private CustomAttributeArgument _Clone(CustomAttributeArgument arg) {
            return new CustomAttributeArgument(
                TargetModule.ImportReference(arg.Type),
                _ImportUntyped(arg.Value)
            );
        }

        private CustomAttributeNamedArgument _Clone(CustomAttributeNamedArgument arg) {
            return new CustomAttributeNamedArgument(
                arg.Name,
                _Clone(arg.Argument)
            );
        }

        private CustomAttribute _Clone(CustomAttribute attr) {
            var new_attr = new CustomAttribute(
                TargetModule.ImportReference(attr.Constructor)
            );

            for (var i = 0; i < attr.ConstructorArguments.Count; i++) {
                new_attr.ConstructorArguments.Add(_Clone(attr.ConstructorArguments[i]));
            }

            for (var i = 0; i < attr.Fields.Count; i++) {
                new_attr.Fields.Add(_Clone(attr.Fields[i]));
            }

            for (var i = 0; i < attr.Properties.Count; i++) {
                new_attr.Properties.Add(_Clone(attr.Properties[i]));
            }
            return new_attr;
        }

        private ParameterDefinition _Clone(ParameterDefinition param) {
            var new_param = new ParameterDefinition(
                param.Name,
                param.Attributes,
                TargetModule.ImportReference(param.ParameterType)
            ) { HasDefault = param.HasDefault };
            if (param.HasConstant) {
                new_param.Constant = _ImportUntyped(param.Constant);
            }
            return new_param;
        }

        private MethodDefinition _Clone(TypeDefinition decl_type, MethodDefinition method) {
            var new_method = new MethodDefinition(
                method.Name,
                method.Attributes,
                TargetModule.ImportReference(method.ReturnType)
            ) {
                ImplAttributes = method.ImplAttributes,
                SemanticsAttributes = method.SemanticsAttributes,
                CallingConvention = method.CallingConvention,
                AggressiveInlining = method.AggressiveInlining,
                DebugInformation = method.DebugInformation,
                ExplicitThis = method.ExplicitThis,
                HasThis = method.HasThis,
                NoOptimization = method.NoOptimization
            };

            for (var i = 0; i < method.CustomAttributes.Count; i++) {
                var attr = method.CustomAttributes[i];
                var new_attr = new CustomAttribute(
                    TargetModule.ImportReference(attr.Constructor),
                    attr.GetBlob()
                );
                new_method.CustomAttributes.Add(new_attr);
            }

            for (var i = 0; i < method.Parameters.Count; i++) {
                new_method.Parameters.Add(_Clone(method.Parameters[i]));
            }

            for (var i = 0; i < method.GenericParameters.Count; i++) {
                var param = method.GenericParameters[i];
                var new_param = new GenericParameter(
                    param.Name,
                    new_method
                );
                new_method.GenericParameters.Add(new_param);
            }

            new_method.Body = method.Body.CloneBodyAndReimport(TargetModule, new_method);

            return new_method;
        }

        private InterfaceImplementation _Clone(InterfaceImplementation impl) {
            var new_impl = new InterfaceImplementation(
                TargetModule.ImportReference(impl.InterfaceType)
            );
            for (var i = 0; i < impl.CustomAttributes.Count; i++) {
                new_impl.CustomAttributes.Add(_Clone(impl.CustomAttributes[i]));
            }
            return new_impl;
        }

        private TypeDefinition _Clone(TypeDefinition type) {
            var new_type = new TypeDefinition(
                type.Namespace,
                type.Name,
                type.Attributes,
                type.BaseType != null ? TargetModule.ImportReference(type.BaseType) : null
            );


            for (var i = 0; i < type.Fields.Count; i++) {
                new_type.Fields.Add(_Clone(new_type, type.Fields[i]));
            }

            // since properties and events depend on existing methods,
            // methods have to be copied first so that they can be resolved

            for (var i = 0; i < type.Methods.Count; i++) {
                new_type.Methods.Add(_Clone(new_type, type.Methods[i]));
            }

            for (var i = 0; i < type.Properties.Count; i++) {
                new_type.Properties.Add(_Clone(new_type, type.Properties[i]));
            }

            for (var i = 0; i < type.Events.Count; i++) {
                new_type.Events.Add(_Clone(new_type, type.Events[i]));
            }

            for (var i = 0; i < type.GenericParameters.Count; i++) {
                new_type.GenericParameters.Add(_Clone(new_type, type.GenericParameters[i]));
            }

            for (var i = 0; i < type.CustomAttributes.Count; i++) {
                new_type.CustomAttributes.Add(_Clone(type.CustomAttributes[i]));
            }

            for (var i = 0; i < type.Interfaces.Count; i++) {
                new_type.Interfaces.Add(_Clone(type.Interfaces[i]));
            }

            for (var i = 0; i < type.NestedTypes.Count; i++) {
                var nested_type = type.NestedTypes[i];
                if (_IsExcludedFromMerging(nested_type)) continue;
                Logger.Debug($"Merging type: {nested_type.BuildSignature()}");
                new_type.NestedTypes.Add(_Clone(nested_type));
            }

            return new_type;
        }

        private void _MergeModule(ModuleDefinition mod) {
            for (var i = 0; i < mod.Types.Count; i++) {
                var type = mod.Types[i];
                if (_IsExcludedFromMerging(type)) continue;
                Logger.Debug($"Merging type: {type.BuildSignature()}");
                TargetModule.Types.Add(_Clone(type));
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
