using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MonoMod.Utils;
using System.Collections.Generic;
using ModTheGungeon;

namespace SemiPatch {
    public static class RDARPrimitive {
        public static TypeDefinition TypeType = SemiPatch.MscorlibModule.GetType("System.Type");
        public static TypeDefinition DelegateType = SemiPatch.MscorlibModule.GetType("System.Delegate");
        public static TypeDefinition RuntimeReflectionExtensionsType = SemiPatch.MscorlibModule.GetType("System.Reflection.RuntimeReflectionExtensions");
        public static MethodDefinition GetMethodInfoMethod =
            RuntimeReflectionExtensionsType.GetMethodDef("System.Reflection.MethodInfo GetMethodInfo(System.Delegate)");
        public static MethodDefinition GetTypeFromHandleMethod =
            TypeType.GetMethodDef("System.Type GetTypeFromHandle(System.RuntimeTypeHandle)");
        public static MethodDefinition CreateDelegateMethod =
            DelegateType.GetMethodDef("System.Delegate CreateDelegate(System.Type, System.Object, System.Reflection.MethodInfo)");

        private static Logger _Logger = new Logger("RDARPrimitive");

        public static System.Reflection.MethodInfo CreateThunk(System.Reflection.MethodBase source, System.Reflection.MethodBase target) {
            var patched_params = source.GetParameters();
            var stub_param_length = patched_params.Length;
            if (!source.IsStatic) stub_param_length += 1;

            var param_types = new Type[stub_param_length];
            if (!source.IsStatic) param_types[0] = source.DeclaringType;
            for (var i = 0; i < patched_params.Length; i++) {
                param_types[source.IsStatic ? i : i + 1] = patched_params[i].ParameterType;
            }
            var dmd = new DynamicMethodDefinition(source.Name, (source as System.Reflection.MethodInfo)?.ReturnType ?? typeof(void), param_types);

            var body = dmd.Definition.Body;
            var il = body.GetILProcessor();

            dmd.Definition.IsStatic = true;
            dmd.Definition.HasThis = false;
            dmd.Definition.ExplicitThis = false;

            for (var i = 0; i < stub_param_length; i++) {
                il.Append(il.Create(OpCodes.Ldarg, dmd.Definition.Parameters[i]));
            }

            if (target.IsVirtual) {
                il.Append(il.Create(OpCodes.Callvirt, target));
            } else il.Append(il.Create(OpCodes.Call, target));

            il.Append(il.Create(OpCodes.Ret));

            body.OptimizeMacros();

            var gen = dmd.Generate();
            return gen;
        }

        public static void RewriteOrigToExplicitThisOrig(MethodDefinition method, TypeReference explicit_orig_type, ParameterDefinition orig_param) {
            var body = method.Body;
            var orig_type = orig_param.ParameterType;
            var il = body.GetILProcessor();

            body.SimplifyMacros();

            var orig_param_count = OrigFactory.GetParameterCount(orig_type);

            // instructions will be compared by reference
            // but that's okay because it's not like the collection
            // is just gonna change between the next two loops
            // because we don't actually change the instance of
            // Instruction in the optimizing loop below, we can actually
            // get away with this
            var optimized_orig_ldarg_offs_set = new HashSet<Instruction>();

            for (var i = 0; i < il.Body.Instructions.Count; i++) {
                var instr = il.Body.Instructions[i];

                if (instr.OpCode == OpCodes.Callvirt) {
                    var call_target = (MethodReference)instr.Operand;
                    if (call_target.DeclaringType.IsSame(orig_type) && call_target.Name == "Invoke") {
                        var invoke_instr = instr;
                        _Logger.Debug($"Attempting explicit orig rewrite optimization from IL_{instr.Offset.ToString("x4")}");

                        Instruction orig_ldarg_instr = null;
                        var success = false;
                        var stack_count = 0;
                        var prev = instr.Previous;
                        while (prev != null) {
                            stack_count += prev.ComputeStackDelta();

                            if (stack_count == orig_param_count + 1) {
                                if (prev.OpCode == OpCodes.Ldarg) {
                                    var param = (ParameterReference)prev.Operand;
                                    if (param == orig_param) {
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
                            _Logger.Debug($"Optimization successful, orig passed at: {orig_ldarg_instr}");
                            // we add the instance as the first argument
                            // and then swap the invoke method for the explicit orig one
                            // later on in the caller of this method we will
                            // swap the positions of the two arguments, fix the IL
                            // and change the actual type of the orig arg

                            optimized_orig_ldarg_offs_set.Add(orig_ldarg_instr);
                            il.InsertAfter(orig_ldarg_instr, il.Create(OpCodes.Ldarg_0));
                            var new_invoke_method = method.Module.ImportReference(
                                OrigFactory.GetExplicitThisInvokeMethod(explicit_orig_type)
                            );
                            invoke_instr.Operand = new_invoke_method;
                        } else {
                            _Logger.Debug($"Optimization unsuccessful");
                        }
                    }
                }
            }

            VariableDefinition explicit_orig_delegate_local = null;

            for (var i = 0; i < il.Body.Instructions.Count; i++) {
                var instr = il.Body.Instructions[i];

                if (instr.OpCode == OpCodes.Ldarg) {
                    var param = (ParameterReference)instr.Operand;
                    if (param != orig_param) continue;

                    if (optimized_orig_ldarg_offs_set.Contains(instr)) {
                        _Logger.Debug($"Orig ldarg at IL_{instr.Offset.ToString("x4")} is part of prior optimization, skipping");
                        continue;
                    }


                    _Logger.Debug($"Spotted unoptimized orig ldarg at IL_{instr.Offset.ToString("x4")}");

                    Instruction new_instr;

                    if (explicit_orig_delegate_local == null) {
                        _Logger.Debug($"Explicit orig delegate local doesn't exist yet, creating");


                        explicit_orig_delegate_local = new VariableDefinition(explicit_orig_type);
                        body.Variables.Add(explicit_orig_delegate_local);

                        Instruction prev_instr;

                        // we work backwards here
                        // the IL should look like this in the end:
                        /*
                            IL_0058:  ldtoken class MainClass/TestDelegateA`3<int32,string,int32>
                            IL_005d:  call class [mscorlib]System.Type class [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
                            IL_0062:  ldarg.0 
                            IL_0063:  ldarg.1 
                            IL_0064:  call class [mscorlib]System.Reflection.MethodInfo class [mscorlib]System.Reflection.RuntimeReflectionExtensions::GetMethodInfo(class [mscorlib]System.Delegate)
                            IL_0069:  call class [mscorlib]System.Delegate class [mscorlib]System.Delegate::CreateDelegate(class [mscorlib]System.Type, object, class [mscorlib]System.Reflection.MethodInfo)
                        */
                        // we are currently at the ldarg.1
                        // so first, we will work our way inserting backwards the ldarg.0
                        // and the type object push

                        // can't forget about the offset! (and the index)

                        il.InsertBefore(instr, prev_instr = il.Create(OpCodes.Ldarg_0));
                        i += 1;

                        il.InsertBefore(prev_instr, prev_instr = il.Create(OpCodes.Call, GetTypeFromHandleMethod));
                        i += 1;

                        // the purpose is to create an Orig/VoidOrig out of an
                        // ExplicitThisOrig/ExplicitThisVoidOrig, therefore as the type
                        // of the new delegate we push the non-explicit orig type
                        il.InsertBefore(prev_instr, prev_instr = il.Create(OpCodes.Ldtoken, orig_type));
                        i += 1;

                        // finally, we are done with the part before ldarg.1
                        // now we have to do the part after ldarg.1

                        il.InsertAfter(instr, new_instr = il.Create(OpCodes.Call, GetMethodInfoMethod));
                        i += 1;

                        il.InsertAfter(new_instr, new_instr = il.Create(OpCodes.Call, CreateDelegateMethod));
                        i += 1;

                        // now we should have a very fresh new Orig/VoidOrig
                        // on the stack

                        il.InsertAfter(new_instr, new_instr = il.Create(OpCodes.Stloc, explicit_orig_delegate_local));
                        i += 1;

                        il.InsertAfter(new_instr, new_instr = il.Create(OpCodes.Ldloc, explicit_orig_delegate_local));
                        i += 1;
                    } else {
                        il.Replace(instr, new_instr = il.Create(OpCodes.Ldloc, explicit_orig_delegate_local));
                    }
                }
            }
            body.OptimizeMacros();
        }
    }

}
