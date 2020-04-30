using System;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;

namespace SemiPatch.MonoMod {
    public class ExplicitOrigFactory {
        public static ModuleDefinition SemiPatchMonoModModule = ModuleDefinition.ReadModule(System.Reflection.Assembly.GetExecutingAssembly().Location);

        public static TypeReference ExplicitVoidOrig_n0 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitVoidOrig`1");
        public static TypeReference ExplicitVoidOrig_n1 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitVoidOrig`2");
        public static TypeReference ExplicitVoidOrig_n2 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitVoidOrig`3");
        public static TypeReference ExplicitVoidOrig_n3 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitVoidOrig`4");
        public static TypeReference ExplicitVoidOrig_n4 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitVoidOrig`5");
        public static TypeReference ExplicitVoidOrig_n5 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitVoidOrig`6");
        public static TypeReference ExplicitVoidOrig_n6 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitVoidOrig`7");
        public static TypeReference ExplicitVoidOrig_n7 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitVoidOrig`8");
        public static TypeReference ExplicitVoidOrig_n8 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitVoidOrig`9");
        public static TypeReference ExplicitVoidOrig_n9 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitVoidOrig`10");
        public static TypeReference ExplicitVoidOrig_n10 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitVoidOrig`11");
        public static TypeReference ExplicitVoidOrig_n11 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitVoidOrig`12");
        public static TypeReference ExplicitVoidOrig_n12 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitVoidOrig`13");
        public static TypeReference ExplicitVoidOrig_n13 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitVoidOrig`14");
        public static TypeReference ExplicitVoidOrig_n14 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitVoidOrig`15");
        public static TypeReference ExplicitVoidOrig_n15 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitVoidOrig`16");
        public static TypeReference ExplicitVoidOrig_n16 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitVoidOrig`17");
        public static TypeReference ExplicitVoidOrig_n17 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitVoidOrig`18");
        public static TypeReference ExplicitVoidOrig_n18 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitVoidOrig`19");
        public static TypeReference ExplicitVoidOrig_n19 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitVoidOrig`20");
        public static TypeReference ExplicitVoidOrig_n20 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitVoidOrig`21");
        public static TypeReference ExplicitVoidOrig_n21 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitVoidOrig`22");
        public static TypeReference ExplicitVoidOrig_n22 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitVoidOrig`23");
        public static TypeReference ExplicitVoidOrig_n23 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitVoidOrig`24");
        public static TypeReference ExplicitVoidOrig_n24 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitVoidOrig`25");
        public static TypeReference ExplicitVoidOrig_n25 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitVoidOrig`26");
        public static TypeReference ExplicitVoidOrig_n26 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitVoidOrig`27");
        public static TypeReference ExplicitVoidOrig_n27 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitVoidOrig`28");
        public static TypeReference ExplicitVoidOrig_n28 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitVoidOrig`29");
        public static TypeReference ExplicitVoidOrig_n29 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitVoidOrig`30");
        public static TypeReference ExplicitVoidOrig_n30 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitVoidOrig`31");

        public static TypeReference ExplicitOrig_n0 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitOrig`2");
        public static TypeReference ExplicitOrig_n1 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitOrig`3");
        public static TypeReference ExplicitOrig_n2 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitOrig`4");
        public static TypeReference ExplicitOrig_n3 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitOrig`5");
        public static TypeReference ExplicitOrig_n4 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitOrig`6");
        public static TypeReference ExplicitOrig_n5 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitOrig`7");
        public static TypeReference ExplicitOrig_n6 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitOrig`8");
        public static TypeReference ExplicitOrig_n7 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitOrig`9");
        public static TypeReference ExplicitOrig_n8 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitOrig`10");
        public static TypeReference ExplicitOrig_n9 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitOrig`11");
        public static TypeReference ExplicitOrig_n10 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitOrig`12");
        public static TypeReference ExplicitOrig_n11 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitOrig`13");
        public static TypeReference ExplicitOrig_n12 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitOrig`14");
        public static TypeReference ExplicitOrig_n13 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitOrig`15");
        public static TypeReference ExplicitOrig_n14 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitOrig`16");
        public static TypeReference ExplicitOrig_n15 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitOrig`17");
        public static TypeReference ExplicitOrig_n16 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitOrig`18");
        public static TypeReference ExplicitOrig_n17 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitOrig`19");
        public static TypeReference ExplicitOrig_n18 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitOrig`20");
        public static TypeReference ExplicitOrig_n19 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitOrig`21");
        public static TypeReference ExplicitOrig_n20 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitOrig`22");
        public static TypeReference ExplicitOrig_n21 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitOrig`23");
        public static TypeReference ExplicitOrig_n22 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitOrig`24");
        public static TypeReference ExplicitOrig_n23 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitOrig`25");
        public static TypeReference ExplicitOrig_n24 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitOrig`26");
        public static TypeReference ExplicitOrig_n25 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitOrig`27");
        public static TypeReference ExplicitOrig_n26 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitOrig`28");
        public static TypeReference ExplicitOrig_n27 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitOrig`29");
        public static TypeReference ExplicitOrig_n28 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitOrig`30");
        public static TypeReference ExplicitOrig_n29 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitOrig`31");
        public static TypeReference ExplicitOrig_n30 = SemiPatchMonoModModule.GetType("SemiPatch.MonoMod.ExplicitOrig`32");

        public static TypeReference GetExplicitOrigTypeReference(int param_count, bool is_void = false) {
            if (is_void) {
                // ExplicitVoidOrig
                if (param_count == 0) {
                    return ExplicitVoidOrig_n0;
                } else if (param_count == 1) {
                    return ExplicitVoidOrig_n1;
                } else if (param_count == 2) {
                    return ExplicitVoidOrig_n2;
                } else if (param_count == 3) {
                    return ExplicitVoidOrig_n3;
                } else if (param_count == 4) {
                    return ExplicitVoidOrig_n4;
                } else if (param_count == 5) {
                    return ExplicitVoidOrig_n5;
                } else if (param_count == 6) {
                    return ExplicitVoidOrig_n6;
                } else if (param_count == 7) {
                    return ExplicitVoidOrig_n7;
                } else if (param_count == 8) {
                    return ExplicitVoidOrig_n8;
                } else if (param_count == 9) {
                    return ExplicitVoidOrig_n9;
                } else if (param_count == 10) {
                    return ExplicitVoidOrig_n10;
                } else if (param_count == 11) {
                    return ExplicitVoidOrig_n11;
                } else if (param_count == 12) {
                    return ExplicitVoidOrig_n12;
                } else if (param_count == 13) {
                    return ExplicitVoidOrig_n13;
                } else if (param_count == 14) {
                    return ExplicitVoidOrig_n14;
                } else if (param_count == 15) {
                    return ExplicitVoidOrig_n15;
                } else if (param_count == 16) {
                    return ExplicitVoidOrig_n16;
                } else if (param_count == 17) {
                    return ExplicitVoidOrig_n17;
                } else if (param_count == 18) {
                    return ExplicitVoidOrig_n18;
                } else if (param_count == 19) {
                    return ExplicitVoidOrig_n19;
                } else if (param_count == 20) {
                    return ExplicitVoidOrig_n20;
                } else if (param_count == 21) {
                    return ExplicitVoidOrig_n21;
                } else if (param_count == 22) {
                    return ExplicitVoidOrig_n22;
                } else if (param_count == 23) {
                    return ExplicitVoidOrig_n23;
                } else if (param_count == 24) {
                    return ExplicitVoidOrig_n24;
                } else if (param_count == 25) {
                    return ExplicitVoidOrig_n25;
                } else if (param_count == 26) {
                    return ExplicitVoidOrig_n26;
                } else if (param_count == 27) {
                    return ExplicitVoidOrig_n27;
                } else if (param_count == 28) {
                    return ExplicitVoidOrig_n28;
                } else if (param_count == 29) {
                    return ExplicitVoidOrig_n29;
                } else if (param_count == 30) {
                    return ExplicitVoidOrig_n30;
                } else {
                    throw new InvalidOperationException($"SemiPatch cannot create delegates for methods with over 30 arguments ({param_count} > 30).");
                }
            } else {
                // ExplicitOrig
                if (param_count == 0) {
                    return ExplicitOrig_n0;
                } else if (param_count == 1) {
                    return ExplicitOrig_n1;
                } else if (param_count == 2) {
                    return ExplicitOrig_n2;
                } else if (param_count == 3) {
                    return ExplicitOrig_n3;
                } else if (param_count == 4) {
                    return ExplicitOrig_n4;
                } else if (param_count == 5) {
                    return ExplicitOrig_n5;
                } else if (param_count == 6) {
                    return ExplicitOrig_n6;
                } else if (param_count == 7) {
                    return ExplicitOrig_n7;
                } else if (param_count == 8) {
                    return ExplicitOrig_n8;
                } else if (param_count == 9) {
                    return ExplicitOrig_n9;
                } else if (param_count == 10) {
                    return ExplicitOrig_n10;
                } else if (param_count == 11) {
                    return ExplicitOrig_n11;
                } else if (param_count == 12) {
                    return ExplicitOrig_n12;
                } else if (param_count == 13) {
                    return ExplicitOrig_n13;
                } else if (param_count == 14) {
                    return ExplicitOrig_n14;
                } else if (param_count == 15) {
                    return ExplicitOrig_n15;
                } else if (param_count == 16) {
                    return ExplicitOrig_n16;
                } else if (param_count == 17) {
                    return ExplicitOrig_n17;
                } else if (param_count == 18) {
                    return ExplicitOrig_n18;
                } else if (param_count == 19) {
                    return ExplicitOrig_n19;
                } else if (param_count == 20) {
                    return ExplicitOrig_n20;
                } else if (param_count == 21) {
                    return ExplicitOrig_n21;
                } else if (param_count == 22) {
                    return ExplicitOrig_n22;
                } else if (param_count == 23) {
                    return ExplicitOrig_n23;
                } else if (param_count == 24) {
                    return ExplicitOrig_n24;
                } else if (param_count == 25) {
                    return ExplicitOrig_n25;
                } else if (param_count == 26) {
                    return ExplicitOrig_n26;
                } else if (param_count == 27) {
                    return ExplicitOrig_n27;
                } else if (param_count == 28) {
                    return ExplicitOrig_n28;
                } else if (param_count == 29) {
                    return ExplicitOrig_n29;
                } else if (param_count == 30) {
                    return ExplicitOrig_n30;
                } else {
                    throw new InvalidOperationException($"SemiPatch cannot create delegates for methods with over 30 arguments ({param_count} > 30).");
                }
            }
        }

        public static TypeReference ExplicitOrigTypeForOrig(ModuleDefinition module, TypeReference instance_type, GenericInstanceType orig_type) {
            var is_void = OrigFactory.TypeIsGenericVoidOrig(orig_type);
            if (!OrigFactory.TypeIsGenericOrig(orig_type) && !is_void) {
                throw new ArgumentException("Type must be generic Orig or VoidOrig", nameof(orig_type));
            }

            var param_count = OrigFactory.GetParameterCount(orig_type);

            var type = GetExplicitOrigTypeReference(param_count, is_void);
            var inst = new GenericInstanceType(module.ImportReference(type));
            inst.GenericArguments.Add(instance_type);
            for (var i = 0; i < param_count; i++) {
                var orig_arg = orig_type.GenericArguments[i];
                inst.GenericArguments.Add(orig_arg);
            }

            return inst;
        }

        public static bool TypeIsGenericExplicitOrig(TypeReference type) {
            return type.IsSame(ExplicitOrig_n0) || type.IsSame(ExplicitOrig_n1) || type.IsSame(ExplicitOrig_n2) || type.IsSame(ExplicitOrig_n3) || type.IsSame(ExplicitOrig_n4) || type.IsSame(ExplicitOrig_n5) || type.IsSame(ExplicitOrig_n6) || type.IsSame(ExplicitOrig_n7) || type.IsSame(ExplicitOrig_n8) || type.IsSame(ExplicitOrig_n9) || type.IsSame(ExplicitOrig_n10) || type.IsSame(ExplicitOrig_n11) || type.IsSame(ExplicitOrig_n12) || type.IsSame(ExplicitOrig_n13) || type.IsSame(ExplicitOrig_n14) || type.IsSame(ExplicitOrig_n15) || type.IsSame(ExplicitOrig_n16) || type.IsSame(ExplicitOrig_n17) || type.IsSame(ExplicitOrig_n18) || type.IsSame(ExplicitOrig_n19) || type.IsSame(ExplicitOrig_n20) || type.IsSame(ExplicitOrig_n21) || type.IsSame(ExplicitOrig_n22) || type.IsSame(ExplicitOrig_n23) || type.IsSame(ExplicitOrig_n24) || type.IsSame(ExplicitOrig_n25) || type.IsSame(ExplicitOrig_n26) || type.IsSame(ExplicitOrig_n27) || type.IsSame(ExplicitOrig_n28) || type.IsSame(ExplicitOrig_n29) || type.IsSame(ExplicitOrig_n30);
        }

        public static bool TypeIsGenericExplicitVoidOrig(TypeReference type) {
            return type.IsSame(ExplicitVoidOrig_n0) || type.IsSame(ExplicitVoidOrig_n1) || type.IsSame(ExplicitVoidOrig_n2) || type.IsSame(ExplicitVoidOrig_n3) || type.IsSame(ExplicitVoidOrig_n4) || type.IsSame(ExplicitVoidOrig_n5) || type.IsSame(ExplicitVoidOrig_n6) || type.IsSame(ExplicitVoidOrig_n7) || type.IsSame(ExplicitVoidOrig_n8) || type.IsSame(ExplicitVoidOrig_n9) || type.IsSame(ExplicitVoidOrig_n10) || type.IsSame(ExplicitVoidOrig_n11) || type.IsSame(ExplicitVoidOrig_n12) || type.IsSame(ExplicitVoidOrig_n13) || type.IsSame(ExplicitVoidOrig_n14) || type.IsSame(ExplicitVoidOrig_n15) || type.IsSame(ExplicitVoidOrig_n16) || type.IsSame(ExplicitVoidOrig_n17) || type.IsSame(ExplicitVoidOrig_n18) || type.IsSame(ExplicitVoidOrig_n19) || type.IsSame(ExplicitVoidOrig_n20) || type.IsSame(ExplicitVoidOrig_n21) || type.IsSame(ExplicitVoidOrig_n22) || type.IsSame(ExplicitVoidOrig_n23) || type.IsSame(ExplicitVoidOrig_n24) || type.IsSame(ExplicitVoidOrig_n25) || type.IsSame(ExplicitVoidOrig_n26) || type.IsSame(ExplicitVoidOrig_n27) || type.IsSame(ExplicitVoidOrig_n28) || type.IsSame(ExplicitVoidOrig_n29) || type.IsSame(ExplicitVoidOrig_n30);
        }

        public static MethodReference GetInvokeMethod(TypeReference type) {
            if (!TypeIsGenericExplicitOrig(type) && !TypeIsGenericExplicitVoidOrig(type)) throw new ArgumentException("Argument must be an ExplicitOrig or ExplicitVoidOrig TypeReference", nameof(type));

            var resolved = type.Resolve();
            for (var i = 0; i < resolved.Methods.Count; i++) {
                var method = resolved.Methods[i];

                if (method.Name == "Invoke") {
                    var generic_method = new MethodReference(method.Name, method.ReturnType, type);
                    for (var j = 0; j < method.Parameters.Count; j++) {
                        var param = method.Parameters[j];
                        generic_method.Parameters.Add(new ParameterDefinition(
                            param.Name,
                            param.Attributes,
                            param.ParameterType
                        ));
                    }
                    generic_method.HasThis = method.HasThis;
                    generic_method.ExplicitThis = method.ExplicitThis;

                    return generic_method;
                }
            }

            throw new Exception("unreachable");
        }

        public static MethodReference NativePointerConstructorForExplicitOrigType(ModuleDefinition module, TypeReference type) {
            if (!TypeIsGenericExplicitOrig(type) && !TypeIsGenericExplicitVoidOrig(type)) throw new ArgumentException("Argument must be an ExplicitOrig or ExplicitVoidOrig TypeReference", nameof(type));
            var resolved = type.Resolve();
            for (var i = 0; i < resolved.Methods.Count; i++) {
                var method = resolved.Methods[i];

                if (method.IsConstructor && method.Parameters.Count == 2) {
                    var generic_method = new MethodReference(method.Name, method.ReturnType, type);
                    for (var j = 0; j < method.Parameters.Count; j++) {
                        var param = method.Parameters[j];
                        generic_method.Parameters.Add(new ParameterDefinition(
                            param.Name,
                            param.Attributes,
                            param.ParameterType
                        ));
                    }
                    generic_method.HasThis = method.HasThis;
                    generic_method.ExplicitThis = method.ExplicitThis;

                    return generic_method;
                }
            }

            throw new Exception("unreachable");
        }
    }
}
