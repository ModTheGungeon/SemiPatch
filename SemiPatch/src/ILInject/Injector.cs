using System;
using System.Collections.Generic;
using ModTheGungeon;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace SemiPatch {
    public static class Injector {
        public static TypeDefinition VoidInjectionState = SemiPatch.SemiPatchModule.GetType("SemiPatch.InjectionState");
        public static TypeDefinition InjectionState = SemiPatch.SemiPatchModule.GetType("SemiPatch.InjectionState`1");
        public static TypeDefinition StringType;
        public static TypeDefinition ObjectType;
        public static GenericInstanceType StringToObjectDictionaryType;
        public static MethodReference StringToObjectDictionarySetItemMethod;
        public static MethodReference StringToObjectDictionaryGetItemMethod;

        public static Logger Logger = new Logger("Injector");

        static Injector() {
            StringType = SemiPatch.MscorlibModule.GetType("System.String");
            ObjectType = SemiPatch.MscorlibModule.GetType("System.Object");

            var dict_type = SemiPatch.MscorlibModule.GetType("System.Collections.Generic.IDictionary`2");
            MethodDefinition dict_set_item = null;
            MethodDefinition dict_get_item = null;
            for (var i = 0; i < dict_type.Methods.Count; i++) {
                var method = dict_type.Methods[i];

                if (method.Name == "set_Item") {
                    dict_set_item = method;
                } else if (method.Name == "get_Item") {
                    dict_get_item = method;
                }

                if (dict_set_item != null && dict_get_item != null) {
                    break;
                }
            }

            StringToObjectDictionaryType = new GenericInstanceType(dict_type);
            StringToObjectDictionaryType.GenericArguments.Add(StringType);
            StringToObjectDictionaryType.GenericArguments.Add(ObjectType);

            StringToObjectDictionarySetItemMethod = MakeDictionaryMethod(
                dict_set_item, StringToObjectDictionaryType
            );
            StringToObjectDictionaryGetItemMethod = MakeDictionaryMethod(
                dict_get_item, StringToObjectDictionaryType
            );
        }

        private static MethodReference MakeDictionaryMethod(MethodDefinition base_method, GenericInstanceType type) {
            var new_method = new MethodReference(
                base_method.Name,
                base_method.ReturnType,
                type
            );

            for (var i = 0; i < base_method.Parameters.Count; i++) {
                var param = base_method.Parameters[i];
                new_method.Parameters.Add(new ParameterDefinition(
                    param.Name,
                    param.Attributes,
                    type.Resolve().GenericParameters[i]
                ));
            }

            new_method.HasThis = base_method.HasThis;
            new_method.ExplicitThis = base_method.ExplicitThis;

            return new_method;
        }

        public static Type GetInjectionStateRuntimeType(Type return_type) {
            if (typeof(void).IsAssignableFrom(return_type)) return typeof(InjectionState);

            var state_type = typeof(InjectionState<>);
            var inst = state_type.MakeGenericType(return_type);

            return inst;
        }

        public static TypeReference GetInjectionStateType(ModuleDefinition module, TypeReference return_type) {
            if (return_type.IsSame(SemiPatch.VoidType)) return module.ImportReference(VoidInjectionState);

            var inst = new GenericInstanceType(module.ImportReference(InjectionState));
            inst.GenericArguments.Add(return_type);

            return inst;
        }

        public static MethodReference GetInjectionStateConstructor(ModuleDefinition module, TypeReference state_type) {
            var resolved = state_type.Resolve();

            MethodReference state_ctor = null;

            for (var i = 0; i < resolved.Methods.Count; i++) {
                var method = resolved.Methods[i];
                if (method.IsConstructor && method.Parameters.Count == 0) state_ctor = module.ImportReference(method);
            }

            // if not void injection state
            if (state_type is GenericInstanceType) {
                var generic_state_ctor = new MethodReference(state_ctor.Name, state_ctor.ReturnType, state_type);
                generic_state_ctor.HasThis = state_ctor.HasThis;
                generic_state_ctor.ExplicitThis = state_ctor.ExplicitThis;
                state_ctor = generic_state_ctor;
            }

            return module.ImportReference(state_ctor);
        }

        public static FieldReference GetInjectionStateOverrideReturnField(ModuleDefinition module, TypeReference state_type) {
            var resolved = state_type.Resolve();

            FieldReference state_ovr_field = null;

            for (var i = 0; i < resolved.Fields.Count; i++) {
                var field = resolved.Fields[i];
                if (field.Name == "_OverrideReturn") state_ovr_field = field;
            }

            // if not void injection state
            if (state_type is GenericInstanceType) {
                return module.ImportReference(new FieldReference(state_ovr_field.Name, state_ovr_field.FieldType, state_type));
            } else return module.ImportReference(state_ovr_field);
        }

        public static FieldReference GetInjectionStateLocalsField(ModuleDefinition module, TypeReference state_type) {
            var resolved = state_type.Resolve();

            FieldReference state_locals_field = null;

            for (var i = 0; i < resolved.Fields.Count; i++) {
                var field = resolved.Fields[i];
                if (field.Name == "_Locals") state_locals_field = field;
            }

            // if not void injection state
            if (state_type is GenericInstanceType) {
                return module.ImportReference(new FieldReference(state_locals_field.Name, state_locals_field.FieldType, state_type));
            } else return module.ImportReference(state_locals_field);
        }

        public static FieldReference GetInjectionStateHandlerPathField(ModuleDefinition module, TypeReference state_type) {
            var resolved = state_type.Resolve();

            FieldReference state_handler_path_field = null;

            for (var i = 0; i < resolved.Fields.Count; i++) {
                var field = resolved.Fields[i];
                if (field.Name == "_HandlerPath") state_handler_path_field = field;
            }

            // if not void injection state
            if (state_type is GenericInstanceType) {
                return module.ImportReference(new FieldReference(state_handler_path_field.Name, state_handler_path_field.FieldType, state_type));
            } else return module.ImportReference(state_handler_path_field);
        }

        public static FieldReference GetInjectionStateReturnValueField(ModuleDefinition module, TypeReference state_type) {
            if (!(state_type is GenericInstanceType)) return null;
            var resolved = state_type.Resolve();

            FieldReference state_ovr_field = null;

            for (var i = 0; i < resolved.Fields.Count; i++) {
                var field = resolved.Fields[i];
                if (field.Name == "_ReturnValue") state_ovr_field = field;
            }

            return module.ImportReference(new FieldReference(state_ovr_field.Name, state_ovr_field.FieldType, state_type));
        }

        public static void InsertInjectCall(MethodDefinition method, MethodDefinition inject_handler, IInjectionHandlerProxy inject_proxy, Instruction target_instr, InjectPosition pos, IList<CaptureLocalAttribute> local_captures, bool dynamic_method = false) {
            Logger.Debug($"Inserting injection handler '{inject_handler.ToPath()}' into method '{method.ToPath()}' {pos.ToString().ToLowerInvariant()} '{target_instr}' with {local_captures?.Count ?? 0} local capture(s).");
            var body = method.Body;
            var il = body.GetILProcessor();
            body.SimplifyMacros();

            var stack_delta = 0;
            for (var i = 0; i < body.Instructions.Count; i++) {
                var stack_check_instr = body.Instructions[i];

                if (stack_check_instr == target_instr) break;

                stack_delta += stack_check_instr.ComputeStackDelta();
            }

            // Building for debug will emit a seemingly pointless branch
            // that just jumps to the next instruction right before the ret,
            // this is so that you can inspect the return value in a debugger
            // however it makes injection behavior inconsistent between release &
            // debug builds so for now we will just nop it out altogether
            var prev_instr = target_instr.Previous;
            var has_debug_br = prev_instr != null && prev_instr.OpCode == OpCodes.Br && prev_instr.Operand == target_instr;
            if (has_debug_br) {
                prev_instr.OpCode = OpCodes.Nop;
            }

            var instr = il.Create(OpCodes.Nop);
            if (pos == InjectPosition.After) il.InsertAfter(target_instr, instr);
            else il.InsertBefore(target_instr, instr);

            var injection_state_type = GetInjectionStateType(method.Module, method.ReturnType);
            var injection_state_local = new VariableDefinition(injection_state_type);
            var injection_state_ctor = GetInjectionStateConstructor(method.Module, injection_state_type);
            var injection_state_ovr_field = GetInjectionStateOverrideReturnField(method.Module, injection_state_type);
            var injection_state_retval_field = GetInjectionStateReturnValueField(method.Module, injection_state_type);
            var injection_state_locals_field = GetInjectionStateLocalsField(method.Module, injection_state_type);
            var injection_state_handlerpath_field = GetInjectionStateHandlerPathField(method.Module, injection_state_type);

            body.Variables.Add(injection_state_local);
            il.InsertAfter(instr, instr = il.Create(
                OpCodes.Newobj, injection_state_ctor
            ));

            il.InsertAfter(instr, instr = il.Create(
                OpCodes.Dup
            ));

            il.InsertAfter(instr, instr = il.Create(
                OpCodes.Stloc, injection_state_local
            ));

            instr = inject_proxy.EmitAfterStateNewobj(il, instr, injection_state_local);

            il.InsertAfter(instr, instr = il.Create(
                OpCodes.Ldstr, inject_handler.ToPath().ToString()
            ));

            il.InsertAfter(instr, instr = il.Create(
                OpCodes.Stfld, injection_state_handlerpath_field
            ));

            if (local_captures != null && local_captures.Count > 0) {
                il.InsertAfter(instr, instr = il.Create(
                    OpCodes.Ldloc, injection_state_local
                ));

                il.InsertAfter(instr, instr = il.Create(
                    OpCodes.Ldfld, injection_state_locals_field
                ));

                for (var i = 0; i < local_captures.Count; i++) {
                    var capture = local_captures[i];

                    if (capture.Index < 0 || capture.Index >= method.Body.Variables.Count) {
                        throw new InvalidLocalIndexException(
                            inject_handler.ToPath(),
                            capture.Name,
                            capture.Index
                        );
                    }

                    var var_def = method.Body.Variables[capture.Index];

                    if (!capture.Type.IsSame(var_def.VariableType)) {
                        throw new InvalidLocalTypeException(
                            inject_handler.ToPath(),
                            capture.Name,
                            capture.Index,
                            capture.Type,
                            var_def.VariableType
                        );
                    }

                    if (i < local_captures.Count - 1) {
                        il.InsertAfter(instr, instr = il.Create(
                            OpCodes.Dup
                        ));
                    }

                    Logger.Debug($"Generating local capture (get) for capture '{capture.Name}'");

                    il.InsertAfter(instr, instr = il.Create(
                        OpCodes.Ldstr, capture.Name
                    ));

                    il.InsertAfter(instr, instr = il.Create(
                        OpCodes.Ldloc, var_def
                    ));

                    if (var_def.VariableType.IsValueType) {
                        il.InsertAfter(instr, instr = il.Create(
                            OpCodes.Box
                        ));
                    }

                    il.InsertAfter(instr, instr = il.Create(
                        OpCodes.Callvirt, StringToObjectDictionarySetItemMethod
                    ));
                }
            }

            instr = inject_proxy.EmitBeforeArgs(il, instr, method.DeclaringType, injection_state_local);

            for (var i = inject_proxy.SkipFirstParameter ? 1 : 0; i < method.Parameters.Count; i++) {
                instr = inject_proxy.EmitBeforeArg(il, instr, method.Parameters[i].ParameterType, i);
                il.InsertAfter(instr, instr = il.Create(
                    OpCodes.Ldarg, method.Parameters[i]
                ));
                instr = inject_proxy.EmitAfterArg(il, instr, method.Parameters[i].ParameterType, i);
            }


            instr = inject_proxy.EmitAfterArgs(il, instr);

            if (instr.Next != null && instr.Next.OpCode != OpCodes.Ret) {
                if (local_captures != null && local_captures.Count > 0) {
                    il.InsertAfter(instr, instr = il.Create(
                        OpCodes.Ldloc_1
                    ));

                    il.InsertAfter(instr, instr = il.Create(
                        OpCodes.Ldfld, injection_state_locals_field
                    ));

                    for (var i = 0; i < local_captures.Count; i++) {
                        var capture = local_captures[i];

                        var var_def = method.Body.Variables[capture.Index];

                        Logger.Debug($"Generating local capture (set) for capture '{capture.Name}'");

                        if (i < local_captures.Count - 1) {
                            il.InsertAfter(instr, instr = il.Create(
                                OpCodes.Dup
                            ));
                        }

                        il.InsertAfter(instr, instr = il.Create(
                            OpCodes.Ldstr, capture.Name
                        ));

                        il.InsertAfter(instr, instr = il.Create(
                            OpCodes.Callvirt, StringToObjectDictionaryGetItemMethod
                        ));

                        il.InsertAfter(instr, instr = il.Create(
                            OpCodes.Castclass, var_def.VariableType
                        ));

                        if (var_def.VariableType.IsValueType) {
                            il.InsertAfter(instr, instr = il.Create(
                                OpCodes.Unbox
                            ));
                        }

                        il.InsertAfter(instr, instr = il.Create(
                            OpCodes.Stloc, var_def
                        ));
                    }
                }

                il.InsertAfter(instr, instr = il.Create(
                    OpCodes.Ldfld,
                    injection_state_ovr_field
                ));

                il.InsertAfter(instr, instr = il.Create(
                    OpCodes.Brfalse,
                    instr.Next
                ));

                for (var i = 0; i < stack_delta; i++) {
                    il.InsertAfter(instr, instr = il.Create(
                        OpCodes.Pop
                    ));
                }

                // if injection state is not void
                if (injection_state_type is GenericInstanceType) {
                    il.InsertAfter(instr, instr = il.Create(
                        OpCodes.Ldloc, injection_state_local
                    ));

                    il.InsertAfter(instr, instr = il.Create(
                        OpCodes.Ldfld, injection_state_retval_field
                    ));
                }

                il.InsertAfter(instr, instr = il.Create(
                    OpCodes.Ret
                ));

                //// see emit_debug_br definition above for explanation on
                //// what this is for
                //if (emit_debug_br) {
                //    il.InsertBefore(instr, il.Create(OpCodes.Br, instr));
                //}
            }
            body.OptimizeMacros();
        }
    }
}
