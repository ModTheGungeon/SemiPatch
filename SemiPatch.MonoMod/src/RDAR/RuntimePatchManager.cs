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

namespace SemiPatch.MonoMod {
    public class RuntimePatchManager {
        public static TypeDefinition RuntimeTypeHandleType = SemiPatch.MscorlibModule.GetType("System.RuntimeTypeHandle");
        public static TypeDefinition TypeType = SemiPatch.MscorlibModule.GetType("System.Type");
        public static TypeDefinition MethodInfoType = SemiPatch.MscorlibModule.GetType("System.Reflection.MethodInfo");
        public static TypeDefinition RuntimeReflectionExtensionsType = SemiPatch.MscorlibModule.GetType("System.Reflection.RuntimeReflectionExtensions");
        public static TypeDefinition DelegateType = SemiPatch.MscorlibModule.GetType("System.Delegate");
        public static TypeDefinition ObjectType = SemiPatch.MscorlibModule.GetType("System.Object");
        public static MethodDefinition GetMethodInfoMethod;
        public static MethodDefinition GetTypeFromHandleMethod;
        public static MethodDefinition CreateDelegateMethod;
        public static Logger Logger = new Logger("RuntimePatchManager");

        static RuntimePatchManager() {
            for (var i = 0; i < TypeType.Methods.Count; i++) {
                var method = TypeType.Methods[i];

                if (method.Name == "GetTypeFromHandle"
                    && method.Parameters.Count == 1
                    && method.Parameters[0].ParameterType.IsSame(RuntimeTypeHandleType)
                ) {
                    GetTypeFromHandleMethod = method;
                    break;
                }
            }

            for (var i = 0; i < RuntimeReflectionExtensionsType.Methods.Count; i++) {
                var method = RuntimeReflectionExtensionsType.Methods[i];

                if (method.Name == "GetMethodInfo"
                    && method.Parameters.Count == 1
                    && method.Parameters[0].ParameterType.IsSame(DelegateType)
                ) {
                    GetMethodInfoMethod = method;
                    break;
                }
            }

            for (var i = 0; i < DelegateType.Methods.Count; i++) {
                var method = DelegateType.Methods[i];

                if (method.Name == "CreateDelegate"
                    && method.Parameters.Count == 3
                    && method.Parameters[0].ParameterType.IsSame(TypeType)
                    && method.Parameters[1].ParameterType.IsSame(ObjectType)
                    && method.Parameters[2].ParameterType.IsSame(MethodInfoType)
                ) {
                    CreateDelegateMethod = method;
                    break;
                }
            }
        }

        private List<IDetour> _Detours = new List<IDetour>();
        private ModuleDefinition _RunningModule;
        private System.Reflection.Assembly _RunningAssembly;

        public RuntimePatchManager(System.Reflection.Assembly asm, ModuleDefinition running_module) {
            _RunningAssembly = asm;
            _RunningModule = running_module;
        }

        private void _ReplaceMethod(Relinker relinker, MethodDefinition patch_method, MethodPath target_path, System.Reflection.Assembly target_asm, ModuleDefinition target_module, bool update_running_module = false) {
            var target_method = (System.Reflection.MethodBase)target_path.FindIn(target_asm);
            Console.WriteLine($"TARGET METHOD: {target_method}");

            var has_orig = (patch_method.Parameters.Count >= 1
                && (
                    OrigFactory.TypeIsGenericOrig(patch_method.Parameters[0].ParameterType)
                    || OrigFactory.TypeIsGenericVoidOrig(patch_method.Parameters[0].ParameterType)
                ));

            relinker.Relink(new Relinker.State(patch_method.Module), patch_method);

            var method = _PreprocessPatchMethodForHooking(
                patch_method,
                target_path.FindIn(target_module).DeclaringType,
                has_orig
            );

            if (update_running_module) {
                var running_module_target_method = target_path.FindIn(_RunningModule);
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
            Console.WriteLine(dmd.Definition);
            var dmd_method = dmd.Generate();
            Console.WriteLine(dmd_method);

            var attrs = target_method.GetCustomAttributes(true);
            for (var k = 0; k < attrs.Length; k++) {
                var attr = attrs[k];

                if (typeof(RDAR.Support.HasOriginalInAttribute).IsAssignableFrom(attr.GetType())) {
                    var orig_attr = (RDAR.Support.HasOriginalInAttribute)attr;
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

                    Console.WriteLine($"target: {target_method}");
                    Console.WriteLine($"orig: {orig_method}");

                    var stub = _CreateOrigStub(target_method, orig_method);
                    var orig_stub_detour = new NativeDetour(target_method, stub);

                    Console.WriteLine($"stubbed {target_method}: {stub} {stub.Attributes}");
                    _Detours.Add(orig_stub_detour);

                    target_method = orig_method;
                    break;
                }
            }

            var hook = new Hook(target_method, dmd_method);
            _Detours.Add(hook);
        }

        private void _ProcessMethodDifference(Relinker relinker, MemberDifference<MethodDefinition, MethodPath> diff, bool update_running_module = false) {
            if (diff is MemberAdded<MethodDefinition, MethodPath>) {
                throw new UnsupportedRDAROperationException(diff);
            } else if (diff is MemberRemoved<MethodDefinition, MethodPath>) {
                throw new UnsupportedRDAROperationException(diff);
            } else if (diff is MemberChanged<MethodDefinition, MethodPath> change_diff) {
                _ReplaceMethod(
                    relinker,
                    change_diff.Member,
                    change_diff.TargetPath,
                    _RunningAssembly,
                    _RunningModule,
                    update_running_module
                );
            } else {
                throw new UnsupportedRDAROperationException(diff);
            }
        }

        private void _ProcessFieldDifference(Relinker relinker, MemberDifference<FieldDefinition, FieldPath> diff, bool update_running_module = false) {
            throw new UnsupportedRDAROperationException(diff);
        }

        private void _ProcessPropertyDifference(Relinker relinker, MemberDifference<PropertyDefinition, PropertyPath> diff, bool update_running_module = false) {
            throw new UnsupportedRDAROperationException(diff);
        }

        private void _ProcessMemberDifference(Relinker relinker, MemberDifference diff, bool update_running_module = false) {
            if (diff.MemberType == MemberType.Method) {
                _ProcessMethodDifference(relinker, (MemberDifference<MethodDefinition, MethodPath>)diff, update_running_module);
            } else if (diff.MemberType == MemberType.Field) {
                _ProcessFieldDifference(relinker, (MemberDifference<FieldDefinition, FieldPath>)diff, update_running_module);
            } else if (diff.MemberType == MemberType.Property) {
                _ProcessPropertyDifference(relinker, (MemberDifference<PropertyDefinition, PropertyPath>)diff, update_running_module);
            } else {
                throw new UnsupportedRDAROperationException(diff);
            }
        }

        private void _ProcessTypeDifference(Relinker relinker, TypeDifference diff, bool update_running_module = false) {
            if (diff is TypeAdded) {
                throw new UnsupportedRDAROperationException(diff);
            }

            if (diff is TypeRemoved) {
                throw new UnsupportedRDAROperationException(diff);
            }

            var change = (TypeChanged)diff;
            for (var i = 0; i < change.MemberDifferences.Count; i++) {
                _ProcessMemberDifference(relinker, change.MemberDifferences[i], update_running_module);
            }

            for (var i = 0; i < change.NestedTypeDifferences.Count; i++) {
                _ProcessTypeDifference(relinker, change.NestedTypeDifferences[i], update_running_module);
            }
        }

        public void ProcessDifference(Relinker relinker, AssemblyDiff diff, bool update_running_module = false) {
            for (var i = 0; i < diff.TypeDifferences.Count; i++) {
                _ProcessTypeDifference(relinker, diff.TypeDifferences[i], update_running_module);
            }
        }

        public void ResetDetours() {
            for (var i = 0; i < _Detours.Count; i++) {
                _Detours[i].Dispose();
            }

            _Detours.Clear();
        }

        private static void _RewriteOrigToExplicitOrig(MethodDefinition method, TypeReference explicit_orig_type, ParameterDefinition orig_param) {
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
                        Logger.Debug($"Attempting explicit orig rewrite optimization from IL_{instr.Offset.ToString("x4")}");

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
                            Logger.Debug($"Optimization successful, orig passed at: {orig_ldarg_instr}");
                            // we add the instance as the first argument
                            // and then swap the invoke method for the explicit orig one
                            // later on in the caller of this method we will
                            // swap the positions of the two arguments, fix the IL
                            // and change the actual type of the orig arg

                            optimized_orig_ldarg_offs_set.Add(orig_ldarg_instr);
                            il.InsertAfter(orig_ldarg_instr, il.Create(OpCodes.Ldarg_0));
                            var new_invoke_method = method.Module.ImportReference(
                                ExplicitOrigFactory.GetInvokeMethod(explicit_orig_type)
                            );
                            invoke_instr.Operand = new_invoke_method;
                        } else {
                            Logger.Debug($"Optimization unsuccessful");
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
                        Logger.Debug($"Orig ldarg at IL_{instr.Offset.ToString("x4")} is part of prior optimization, skipping");
                        continue;
                    }


                    Logger.Debug($"Spotted unoptimized orig ldarg at IL_{instr.Offset.ToString("x4")}");

                    Instruction new_instr;

                    if (explicit_orig_delegate_local == null) {
                        Logger.Debug($"Explicit orig delegate local doesn't exist yet, creating");


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
                        // ExplicitOrig/ExplicitVoidOrig, therefore as the type
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

        private static void _RewriteMethodAsHookTarget(MethodDefinition method, TypeReference instance_type, bool has_orig) {
            // for static methods we're set
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
                    explicit_orig_type = ExplicitOrigFactory.ExplicitOrigTypeForOrig(
                        module,
                        instance_type, // DeclaringType of target method
                        orig_type
                    );
                    _RewriteOrigToExplicitOrig(
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

                    // change out orig with explicitorig
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

        private static MethodDefinition _PreprocessPatchMethodForHooking(MethodDefinition method, TypeReference instance_type, bool has_orig, bool preserve_method_definition = false) {
            if (preserve_method_definition) method = method.Clone();

            _RewriteMethodAsHookTarget(method, instance_type, has_orig);

            return method;
        }

        private static System.Reflection.MethodInfo _CreateOrigStub(System.Reflection.MethodBase patched, System.Reflection.MethodBase orig) {
            var patched_params = patched.GetParameters();
            var stub_param_length = patched_params.Length;
            if (!patched.IsStatic) stub_param_length += 1;

            var param_types = new Type[stub_param_length];
            if (!patched.IsStatic) param_types[0] = patched.DeclaringType;
            for (var i = 0; i < patched_params.Length; i++) {
                param_types[patched.IsStatic ? i : i + 1] = patched_params[i].ParameterType;
            }
            var dmd = new DynamicMethodDefinition(patched.Name, (patched as System.Reflection.MethodInfo)?.ReturnType ?? typeof(void), param_types);

            var body = dmd.Definition.Body;
            var il = body.GetILProcessor();

            dmd.Definition.IsStatic = true;
            dmd.Definition.HasThis = false;
            dmd.Definition.ExplicitThis = false;

            for (var i = 0; i < stub_param_length; i++) {
                il.Append(il.Create(OpCodes.Ldarg, dmd.Definition.Parameters[i]));
            }

            if (orig.IsVirtual) {
                il.Append(il.Create(OpCodes.Callvirt, orig));
            } else il.Append(il.Create(OpCodes.Call, orig));

            il.Append(il.Create(OpCodes.Ret));

            body.OptimizeMacros();

            for (var i = 0; i < body.Instructions.Count; i++) {
                Console.WriteLine(body.Instructions[i]);
            }

            return dmd.Generate();
        }
    }
}
