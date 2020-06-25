using System;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;

namespace SemiPatch {
    internal class OrigFactory {
        public static TypeReference VoidOrig_n0 = SemiPatch.SemiPatchModule.GetType("SemiPatch.VoidOrig");
        public static TypeReference VoidOrig_n1 = SemiPatch.SemiPatchModule.GetType("SemiPatch.VoidOrig`1");
        public static TypeReference VoidOrig_n2 = SemiPatch.SemiPatchModule.GetType("SemiPatch.VoidOrig`2");
        public static TypeReference VoidOrig_n3 = SemiPatch.SemiPatchModule.GetType("SemiPatch.VoidOrig`3");
        public static TypeReference VoidOrig_n4 = SemiPatch.SemiPatchModule.GetType("SemiPatch.VoidOrig`4");
        public static TypeReference VoidOrig_n5 = SemiPatch.SemiPatchModule.GetType("SemiPatch.VoidOrig`5");
        public static TypeReference VoidOrig_n6 = SemiPatch.SemiPatchModule.GetType("SemiPatch.VoidOrig`6");
        public static TypeReference VoidOrig_n7 = SemiPatch.SemiPatchModule.GetType("SemiPatch.VoidOrig`7");
        public static TypeReference VoidOrig_n8 = SemiPatch.SemiPatchModule.GetType("SemiPatch.VoidOrig`8");
        public static TypeReference VoidOrig_n9 = SemiPatch.SemiPatchModule.GetType("SemiPatch.VoidOrig`9");
        public static TypeReference VoidOrig_n10 = SemiPatch.SemiPatchModule.GetType("SemiPatch.VoidOrig`10");
        public static TypeReference VoidOrig_n11 = SemiPatch.SemiPatchModule.GetType("SemiPatch.VoidOrig`11");
        public static TypeReference VoidOrig_n12 = SemiPatch.SemiPatchModule.GetType("SemiPatch.VoidOrig`12");
        public static TypeReference VoidOrig_n13 = SemiPatch.SemiPatchModule.GetType("SemiPatch.VoidOrig`13");
        public static TypeReference VoidOrig_n14 = SemiPatch.SemiPatchModule.GetType("SemiPatch.VoidOrig`14");
        public static TypeReference VoidOrig_n15 = SemiPatch.SemiPatchModule.GetType("SemiPatch.VoidOrig`15");
        public static TypeReference VoidOrig_n16 = SemiPatch.SemiPatchModule.GetType("SemiPatch.VoidOrig`16");
        public static TypeReference VoidOrig_n17 = SemiPatch.SemiPatchModule.GetType("SemiPatch.VoidOrig`17");
        public static TypeReference VoidOrig_n18 = SemiPatch.SemiPatchModule.GetType("SemiPatch.VoidOrig`18");
        public static TypeReference VoidOrig_n19 = SemiPatch.SemiPatchModule.GetType("SemiPatch.VoidOrig`19");
        public static TypeReference VoidOrig_n20 = SemiPatch.SemiPatchModule.GetType("SemiPatch.VoidOrig`20");
        public static TypeReference VoidOrig_n21 = SemiPatch.SemiPatchModule.GetType("SemiPatch.VoidOrig`21");
        public static TypeReference VoidOrig_n22 = SemiPatch.SemiPatchModule.GetType("SemiPatch.VoidOrig`22");
        public static TypeReference VoidOrig_n23 = SemiPatch.SemiPatchModule.GetType("SemiPatch.VoidOrig`23");
        public static TypeReference VoidOrig_n24 = SemiPatch.SemiPatchModule.GetType("SemiPatch.VoidOrig`24");
        public static TypeReference VoidOrig_n25 = SemiPatch.SemiPatchModule.GetType("SemiPatch.VoidOrig`25");
        public static TypeReference VoidOrig_n26 = SemiPatch.SemiPatchModule.GetType("SemiPatch.VoidOrig`26");
        public static TypeReference VoidOrig_n27 = SemiPatch.SemiPatchModule.GetType("SemiPatch.VoidOrig`27");
        public static TypeReference VoidOrig_n28 = SemiPatch.SemiPatchModule.GetType("SemiPatch.VoidOrig`28");
        public static TypeReference VoidOrig_n29 = SemiPatch.SemiPatchModule.GetType("SemiPatch.VoidOrig`29");
        public static TypeReference VoidOrig_n30 = SemiPatch.SemiPatchModule.GetType("SemiPatch.VoidOrig`30");

        public static TypeReference Orig_n0 = SemiPatch.SemiPatchModule.GetType("SemiPatch.Orig`1");
        public static TypeReference Orig_n1 = SemiPatch.SemiPatchModule.GetType("SemiPatch.Orig`2");
        public static TypeReference Orig_n2 = SemiPatch.SemiPatchModule.GetType("SemiPatch.Orig`3");
        public static TypeReference Orig_n3 = SemiPatch.SemiPatchModule.GetType("SemiPatch.Orig`4");
        public static TypeReference Orig_n4 = SemiPatch.SemiPatchModule.GetType("SemiPatch.Orig`5");
        public static TypeReference Orig_n5 = SemiPatch.SemiPatchModule.GetType("SemiPatch.Orig`6");
        public static TypeReference Orig_n6 = SemiPatch.SemiPatchModule.GetType("SemiPatch.Orig`7");
        public static TypeReference Orig_n7 = SemiPatch.SemiPatchModule.GetType("SemiPatch.Orig`8");
        public static TypeReference Orig_n8 = SemiPatch.SemiPatchModule.GetType("SemiPatch.Orig`9");
        public static TypeReference Orig_n9 = SemiPatch.SemiPatchModule.GetType("SemiPatch.Orig`10");
        public static TypeReference Orig_n10 = SemiPatch.SemiPatchModule.GetType("SemiPatch.Orig`11");
        public static TypeReference Orig_n11 = SemiPatch.SemiPatchModule.GetType("SemiPatch.Orig`12");
        public static TypeReference Orig_n12 = SemiPatch.SemiPatchModule.GetType("SemiPatch.Orig`13");
        public static TypeReference Orig_n13 = SemiPatch.SemiPatchModule.GetType("SemiPatch.Orig`14");
        public static TypeReference Orig_n14 = SemiPatch.SemiPatchModule.GetType("SemiPatch.Orig`15");
        public static TypeReference Orig_n15 = SemiPatch.SemiPatchModule.GetType("SemiPatch.Orig`16");
        public static TypeReference Orig_n16 = SemiPatch.SemiPatchModule.GetType("SemiPatch.Orig`17");
        public static TypeReference Orig_n17 = SemiPatch.SemiPatchModule.GetType("SemiPatch.Orig`18");
        public static TypeReference Orig_n18 = SemiPatch.SemiPatchModule.GetType("SemiPatch.Orig`19");
        public static TypeReference Orig_n19 = SemiPatch.SemiPatchModule.GetType("SemiPatch.Orig`20");
        public static TypeReference Orig_n20 = SemiPatch.SemiPatchModule.GetType("SemiPatch.Orig`21");
        public static TypeReference Orig_n21 = SemiPatch.SemiPatchModule.GetType("SemiPatch.Orig`22");
        public static TypeReference Orig_n22 = SemiPatch.SemiPatchModule.GetType("SemiPatch.Orig`23");
        public static TypeReference Orig_n23 = SemiPatch.SemiPatchModule.GetType("SemiPatch.Orig`24");
        public static TypeReference Orig_n24 = SemiPatch.SemiPatchModule.GetType("SemiPatch.Orig`25");
        public static TypeReference Orig_n25 = SemiPatch.SemiPatchModule.GetType("SemiPatch.Orig`26");
        public static TypeReference Orig_n26 = SemiPatch.SemiPatchModule.GetType("SemiPatch.Orig`27");
        public static TypeReference Orig_n27 = SemiPatch.SemiPatchModule.GetType("SemiPatch.Orig`28");
        public static TypeReference Orig_n28 = SemiPatch.SemiPatchModule.GetType("SemiPatch.Orig`29");
        public static TypeReference Orig_n29 = SemiPatch.SemiPatchModule.GetType("SemiPatch.Orig`30");
        public static TypeReference Orig_n30 = SemiPatch.SemiPatchModule.GetType("SemiPatch.Orig`31");

        public static TypeReference ExplicitThisVoidOrig_n0 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisVoidOrig`1");
        public static TypeReference ExplicitThisVoidOrig_n1 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisVoidOrig`2");
        public static TypeReference ExplicitThisVoidOrig_n2 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisVoidOrig`3");
        public static TypeReference ExplicitThisVoidOrig_n3 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisVoidOrig`4");
        public static TypeReference ExplicitThisVoidOrig_n4 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisVoidOrig`5");
        public static TypeReference ExplicitThisVoidOrig_n5 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisVoidOrig`6");
        public static TypeReference ExplicitThisVoidOrig_n6 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisVoidOrig`7");
        public static TypeReference ExplicitThisVoidOrig_n7 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisVoidOrig`8");
        public static TypeReference ExplicitThisVoidOrig_n8 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisVoidOrig`9");
        public static TypeReference ExplicitThisVoidOrig_n9 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisVoidOrig`10");
        public static TypeReference ExplicitThisVoidOrig_n10 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisVoidOrig`11");
        public static TypeReference ExplicitThisVoidOrig_n11 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisVoidOrig`12");
        public static TypeReference ExplicitThisVoidOrig_n12 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisVoidOrig`13");
        public static TypeReference ExplicitThisVoidOrig_n13 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisVoidOrig`14");
        public static TypeReference ExplicitThisVoidOrig_n14 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisVoidOrig`15");
        public static TypeReference ExplicitThisVoidOrig_n15 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisVoidOrig`16");
        public static TypeReference ExplicitThisVoidOrig_n16 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisVoidOrig`17");
        public static TypeReference ExplicitThisVoidOrig_n17 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisVoidOrig`18");
        public static TypeReference ExplicitThisVoidOrig_n18 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisVoidOrig`19");
        public static TypeReference ExplicitThisVoidOrig_n19 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisVoidOrig`20");
        public static TypeReference ExplicitThisVoidOrig_n20 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisVoidOrig`21");
        public static TypeReference ExplicitThisVoidOrig_n21 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisVoidOrig`22");
        public static TypeReference ExplicitThisVoidOrig_n22 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisVoidOrig`23");
        public static TypeReference ExplicitThisVoidOrig_n23 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisVoidOrig`24");
        public static TypeReference ExplicitThisVoidOrig_n24 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisVoidOrig`25");
        public static TypeReference ExplicitThisVoidOrig_n25 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisVoidOrig`26");
        public static TypeReference ExplicitThisVoidOrig_n26 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisVoidOrig`27");
        public static TypeReference ExplicitThisVoidOrig_n27 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisVoidOrig`28");
        public static TypeReference ExplicitThisVoidOrig_n28 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisVoidOrig`29");
        public static TypeReference ExplicitThisVoidOrig_n29 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisVoidOrig`30");
        public static TypeReference ExplicitThisVoidOrig_n30 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisVoidOrig`31");

        public static TypeReference ExplicitThisOrig_n0 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisOrig`2");
        public static TypeReference ExplicitThisOrig_n1 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisOrig`3");
        public static TypeReference ExplicitThisOrig_n2 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisOrig`4");
        public static TypeReference ExplicitThisOrig_n3 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisOrig`5");
        public static TypeReference ExplicitThisOrig_n4 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisOrig`6");
        public static TypeReference ExplicitThisOrig_n5 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisOrig`7");
        public static TypeReference ExplicitThisOrig_n6 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisOrig`8");
        public static TypeReference ExplicitThisOrig_n7 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisOrig`9");
        public static TypeReference ExplicitThisOrig_n8 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisOrig`10");
        public static TypeReference ExplicitThisOrig_n9 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisOrig`11");
        public static TypeReference ExplicitThisOrig_n10 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisOrig`12");
        public static TypeReference ExplicitThisOrig_n11 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisOrig`13");
        public static TypeReference ExplicitThisOrig_n12 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisOrig`14");
        public static TypeReference ExplicitThisOrig_n13 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisOrig`15");
        public static TypeReference ExplicitThisOrig_n14 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisOrig`16");
        public static TypeReference ExplicitThisOrig_n15 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisOrig`17");
        public static TypeReference ExplicitThisOrig_n16 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisOrig`18");
        public static TypeReference ExplicitThisOrig_n17 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisOrig`19");
        public static TypeReference ExplicitThisOrig_n18 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisOrig`20");
        public static TypeReference ExplicitThisOrig_n19 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisOrig`21");
        public static TypeReference ExplicitThisOrig_n20 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisOrig`22");
        public static TypeReference ExplicitThisOrig_n21 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisOrig`23");
        public static TypeReference ExplicitThisOrig_n22 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisOrig`24");
        public static TypeReference ExplicitThisOrig_n23 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisOrig`25");
        public static TypeReference ExplicitThisOrig_n24 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisOrig`26");
        public static TypeReference ExplicitThisOrig_n25 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisOrig`27");
        public static TypeReference ExplicitThisOrig_n26 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisOrig`28");
        public static TypeReference ExplicitThisOrig_n27 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisOrig`29");
        public static TypeReference ExplicitThisOrig_n28 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisOrig`30");
        public static TypeReference ExplicitThisOrig_n29 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisOrig`31");
        public static TypeReference ExplicitThisOrig_n30 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitThisOrig`32");

        public static TypeReference GetBaseOrigType(int param_count, bool is_void) {
            if (is_void) {
                // VoidOrig
                if (param_count == 0) {
                    return VoidOrig_n0;
                } else if (param_count == 1) {
                    return VoidOrig_n1;
                } else if (param_count == 2) {
                    return VoidOrig_n2;
                } else if (param_count == 3) {
                    return VoidOrig_n3;
                } else if (param_count == 4) {
                    return VoidOrig_n4;
                } else if (param_count == 5) {
                    return VoidOrig_n5;
                } else if (param_count == 6) {
                    return VoidOrig_n6;
                } else if (param_count == 7) {
                    return VoidOrig_n7;
                } else if (param_count == 8) {
                    return VoidOrig_n8;
                } else if (param_count == 9) {
                    return VoidOrig_n9;
                } else if (param_count == 10) {
                    return VoidOrig_n10;
                } else if (param_count == 11) {
                    return VoidOrig_n11;
                } else if (param_count == 12) {
                    return VoidOrig_n12;
                } else if (param_count == 13) {
                    return VoidOrig_n13;
                } else if (param_count == 14) {
                    return VoidOrig_n14;
                } else if (param_count == 15) {
                    return VoidOrig_n15;
                } else if (param_count == 16) {
                    return VoidOrig_n16;
                } else if (param_count == 17) {
                    return VoidOrig_n17;
                } else if (param_count == 18) {
                    return VoidOrig_n18;
                } else if (param_count == 19) {
                    return VoidOrig_n19;
                } else if (param_count == 20) {
                    return VoidOrig_n20;
                } else if (param_count == 21) {
                    return VoidOrig_n21;
                } else if (param_count == 22) {
                    return VoidOrig_n22;
                } else if (param_count == 23) {
                    return VoidOrig_n23;
                } else if (param_count == 24) {
                    return VoidOrig_n24;
                } else if (param_count == 25) {
                    return VoidOrig_n25;
                } else if (param_count == 26) {
                    return VoidOrig_n26;
                } else if (param_count == 27) {
                    return VoidOrig_n27;
                } else if (param_count == 28) {
                    return VoidOrig_n28;
                } else if (param_count == 29) {
                    return VoidOrig_n29;
                } else if (param_count == 30) {
                    return VoidOrig_n30;
                } else {
                    throw new InvalidOperationException("SemiPatch cannot create delegates for methods with over 30 arguments.");
                }
            } else {
                // Orig
                if (param_count == 0) {
                    return Orig_n0;
                } else if (param_count == 1) {
                    return Orig_n1;
                } else if (param_count == 2) {
                    return Orig_n2;
                } else if (param_count == 3) {
                    return Orig_n3;
                } else if (param_count == 4) {
                    return Orig_n4;
                } else if (param_count == 5) {
                    return Orig_n5;
                } else if (param_count == 6) {
                    return Orig_n6;
                } else if (param_count == 7) {
                    return Orig_n7;
                } else if (param_count == 8) {
                    return Orig_n8;
                } else if (param_count == 9) {
                    return Orig_n9;
                } else if (param_count == 10) {
                    return Orig_n10;
                } else if (param_count == 11) {
                    return Orig_n11;
                } else if (param_count == 12) {
                    return Orig_n12;
                } else if (param_count == 13) {
                    return Orig_n13;
                } else if (param_count == 14) {
                    return Orig_n14;
                } else if (param_count == 15) {
                    return Orig_n15;
                } else if (param_count == 16) {
                    return Orig_n16;
                } else if (param_count == 17) {
                    return Orig_n17;
                } else if (param_count == 18) {
                    return Orig_n18;
                } else if (param_count == 19) {
                    return Orig_n19;
                } else if (param_count == 20) {
                    return Orig_n20;
                } else if (param_count == 21) {
                    return Orig_n21;
                } else if (param_count == 22) {
                    return Orig_n22;
                } else if (param_count == 23) {
                    return Orig_n23;
                } else if (param_count == 24) {
                    return Orig_n24;
                } else if (param_count == 25) {
                    return Orig_n25;
                } else if (param_count == 26) {
                    return Orig_n26;
                } else if (param_count == 27) {
                    return Orig_n27;
                } else if (param_count == 28) {
                    return Orig_n28;
                } else if (param_count == 29) {
                    return Orig_n29;
                } else if (param_count == 30) {
                    return Orig_n30;
                } else {
                    throw new InvalidOperationException("SemiPatch cannot create delegates for methods with over 30 arguments.");
                }
            }
        }

        public static TypeReference OrigGenericTypeForMethod(MethodReference method, bool skip_first_arg = false) {
            var param_count = method.Parameters.Count;
            if (skip_first_arg) param_count -= 1;
            return GetBaseOrigType(param_count, method.ReturnType.IsSame(SemiPatch.VoidType));
        }

        public static GenericInstanceType GetInstancedOrigType(ModuleDefinition module, int param_count, bool is_void, TypeReference return_type, params TypeReference[] param_types) {
            var type = GetBaseOrigType(param_count, is_void);
            var inst = new GenericInstanceType(module.ImportReference(type));
            for (var i = 0; i < param_types.Length; i++) {
                inst.GenericArguments.Add(param_types[i]);
            }

            if (!is_void) {
                inst.GenericArguments.Add(return_type);
            }
            return inst;
        }

        public static MethodReference GetInvokeMethod(TypeReference type) {
            if (!TypeIsGenericOrig(type) && !TypeIsGenericVoidOrig(type)) throw new ArgumentException("Argument must be an Orig or VoidOrig TypeReference", nameof(type));
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

        public static TypeReference OrigTypeForMethod(ModuleDefinition module, MethodReference method, bool skip_first_arg = false) {
            var type = OrigGenericTypeForMethod(method, skip_first_arg);
            var inst = new GenericInstanceType(module.ImportReference(type));
            for (var i = skip_first_arg ? 1 : 0; i < method.Parameters.Count; i++) {
                inst.GenericArguments.Add(method.Parameters[i].ParameterType);
            }

            if (!method.ReturnType.IsSame(SemiPatch.VoidType)) {
                inst.GenericArguments.Add(method.ReturnType);
            }
            return inst;
        }

        public static bool TypeIsGenericOrig(TypeReference type) {
            return type.IsSame(Orig_n0, exclude_generic_args: true) || type.IsSame(Orig_n1, exclude_generic_args: true) || type.IsSame(Orig_n2, exclude_generic_args: true) || type.IsSame(Orig_n3, exclude_generic_args: true) || type.IsSame(Orig_n4, exclude_generic_args: true) || type.IsSame(Orig_n5, exclude_generic_args: true) || type.IsSame(Orig_n6, exclude_generic_args: true) || type.IsSame(Orig_n7, exclude_generic_args: true) || type.IsSame(Orig_n8, exclude_generic_args: true) || type.IsSame(Orig_n9, exclude_generic_args: true) || type.IsSame(Orig_n10, exclude_generic_args: true) || type.IsSame(Orig_n11, exclude_generic_args: true) || type.IsSame(Orig_n12, exclude_generic_args: true) || type.IsSame(Orig_n13, exclude_generic_args: true) || type.IsSame(Orig_n14, exclude_generic_args: true) || type.IsSame(Orig_n15, exclude_generic_args: true) || type.IsSame(Orig_n16, exclude_generic_args: true) || type.IsSame(Orig_n17, exclude_generic_args: true) || type.IsSame(Orig_n18, exclude_generic_args: true) || type.IsSame(Orig_n19, exclude_generic_args: true) || type.IsSame(Orig_n20, exclude_generic_args: true) || type.IsSame(Orig_n21, exclude_generic_args: true) || type.IsSame(Orig_n22, exclude_generic_args: true) || type.IsSame(Orig_n23, exclude_generic_args: true) || type.IsSame(Orig_n24, exclude_generic_args: true) || type.IsSame(Orig_n25, exclude_generic_args: true) || type.IsSame(Orig_n26, exclude_generic_args: true) || type.IsSame(Orig_n27, exclude_generic_args: true) || type.IsSame(Orig_n28, exclude_generic_args: true) || type.IsSame(Orig_n29, exclude_generic_args: true) || type.IsSame(Orig_n30, exclude_generic_args: true); 
        }

        public static bool TypeIsGenericVoidOrig(TypeReference type) {
            return type.IsSame(VoidOrig_n0, exclude_generic_args: true) || type.IsSame(VoidOrig_n1, exclude_generic_args: true) || type.IsSame(VoidOrig_n2, exclude_generic_args: true) || type.IsSame(VoidOrig_n3, exclude_generic_args: true) || type.IsSame(VoidOrig_n4, exclude_generic_args: true) || type.IsSame(VoidOrig_n5, exclude_generic_args: true) || type.IsSame(VoidOrig_n6, exclude_generic_args: true) || type.IsSame(VoidOrig_n7, exclude_generic_args: true) || type.IsSame(VoidOrig_n8, exclude_generic_args: true) || type.IsSame(VoidOrig_n9, exclude_generic_args: true) || type.IsSame(VoidOrig_n10, exclude_generic_args: true) || type.IsSame(VoidOrig_n11, exclude_generic_args: true) || type.IsSame(VoidOrig_n12, exclude_generic_args: true) || type.IsSame(VoidOrig_n13, exclude_generic_args: true) || type.IsSame(VoidOrig_n14, exclude_generic_args: true) || type.IsSame(VoidOrig_n15, exclude_generic_args: true) || type.IsSame(VoidOrig_n16, exclude_generic_args: true) || type.IsSame(VoidOrig_n17, exclude_generic_args: true) || type.IsSame(VoidOrig_n18, exclude_generic_args: true) || type.IsSame(VoidOrig_n19, exclude_generic_args: true) || type.IsSame(VoidOrig_n20, exclude_generic_args: true) || type.IsSame(VoidOrig_n21, exclude_generic_args: true) || type.IsSame(VoidOrig_n22, exclude_generic_args: true) || type.IsSame(VoidOrig_n23, exclude_generic_args: true) || type.IsSame(VoidOrig_n24, exclude_generic_args: true) || type.IsSame(VoidOrig_n25, exclude_generic_args: true) || type.IsSame(VoidOrig_n26, exclude_generic_args: true) || type.IsSame(VoidOrig_n27, exclude_generic_args: true) || type.IsSame(VoidOrig_n28, exclude_generic_args: true) || type.IsSame(VoidOrig_n29, exclude_generic_args: true) || type.IsSame(VoidOrig_n30, exclude_generic_args: true);
        }

        public static int GetParameterCount(TypeReference type) {
            if (type is GenericInstanceType generic_type) {
                if (TypeIsGenericOrig(type)) return generic_type.GenericArguments.Count - 1;
                if (TypeIsGenericVoidOrig(type)) return generic_type.GenericArguments.Count;
            } else {
                if (TypeIsGenericOrig(type)) return type.GenericParameters.Count - 1;
                if (TypeIsGenericVoidOrig(type)) return type.GenericParameters.Count;
            }
            throw new ArgumentException("Type must be generic Orig or VoidOrig", nameof(type));
        }

        public static MethodReference NativePointerConstructorForOrigType(ModuleDefinition module, TypeReference type) {
            if (!TypeIsGenericOrig(type) && !TypeIsGenericVoidOrig(type)) throw new ArgumentException("Argument must be an Orig or VoidOrig TypeReference", nameof(type));
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


        public static Signature GetMethodSignatureFromOrig(TypeReference orig, string name, Collection<GenericParameter> generic_param_source = null) {
            int n = 0;
            GenericInstanceType inst = null;
            bool is_void = false; 

            if (orig is GenericInstanceType) {
                inst = (GenericInstanceType)orig;
                is_void = TypeIsGenericVoidOrig(inst.ElementType);
                n = inst.GenericArguments.Count;
            } else {
                is_void = true;
            }

            TypeReference return_type = null;
            if (inst != null && !TypeIsGenericOrig(inst.ElementType)) throw new ArgumentException("Argument must be an Orig or VoidOrig TypeReference", nameof(orig));

            if (!is_void) {
                n -= 1;
                return_type = inst.GenericArguments[inst.GenericArguments.Count - 1];
            }

            var s = new StringBuilder();
            s.Append(is_void ? "void" : return_type.BuildSignature());
            s.Append(" ");
            s.Append(name);
            if (generic_param_source != null && generic_param_source.Count > 0) {
                s.Append("<");
                for (var i = 0; i < generic_param_source.Count; i++) {
                    s.Append(generic_param_source[i].BuildSignature());
                    if (i < generic_param_source.Count - 1) s.Append(", ");
                }
                s.Append(">");
            } else {
                var has_generic_args = false;
                for (var i = 0; i < n; i++) {
                    var arg = inst.GenericArguments[i];
                    if (arg.IsGenericParameter) {
                        if (!has_generic_args) {
                            s.Append("<");
                        } else {
                            s.Append(", ");
                        }
                        has_generic_args = true;

                        s.Append(arg.BuildSignature());
                    }
                }
                if (has_generic_args) s.Append(">");
            }
            s.Append("(");

            for (var i = 0; i < n; i++) {
                var arg = inst.GenericArguments[i];
                s.Append(arg.BuildSignature());
                //s.Append(" ");
                //s.Append("arg");
                //s.Append(i);
                if (i < n - 1) s.Append(", ");
            }
            //var param_count = param_count;
            //if (forced_first_arg != null) {
            //    s.Append(forced_first_arg);
            //    if (param_count > 0) s.Append(", ");
            //}
            //i = 0;
            //foreach (var param in method.Parameters) {
            //    if (i == 0 && skip_first_arg) {
            //        i = 1;
            //        continue;
            //    }
            //    s.Append(param.ParameterType.ToString());
            //    s.Append(" ");
            //    s.Append(param.Name);
            //    if (i < param_count - 1) s.Append(", ");
            //    i += 1;
            //}

            s.Append(")");
            return new Signature(s.ToString(), orig.Name);
        }

        public static Type GetReflectionType(bool is_void, int param_count) {
            if (is_void) {
                if (param_count == 0) return typeof(VoidOrig);
                else if (param_count == 1) return typeof(VoidOrig<>);
                else if (param_count == 2) return typeof(VoidOrig<,>);
                else if (param_count == 3) return typeof(VoidOrig<,,>);
                else if (param_count == 4) return typeof(VoidOrig<,,,>);
                else if (param_count == 5) return typeof(VoidOrig<,,,,>);
                else if (param_count == 6) return typeof(VoidOrig<,,,,,>);
                else if (param_count == 7) return typeof(VoidOrig<,,,,,,>);
                else if (param_count == 8) return typeof(VoidOrig<,,,,,,,>);
                else if (param_count == 9) return typeof(VoidOrig<,,,,,,,,>);
                else if (param_count == 10) return typeof(VoidOrig<,,,,,,,,,>);
                else if (param_count == 11) return typeof(VoidOrig<,,,,,,,,,,>);
                else if (param_count == 12) return typeof(VoidOrig<,,,,,,,,,,,>);
                else if (param_count == 13) return typeof(VoidOrig<,,,,,,,,,,,,>);
                else if (param_count == 14) return typeof(VoidOrig<,,,,,,,,,,,,,>);
                else if (param_count == 15) return typeof(VoidOrig<,,,,,,,,,,,,,,>);
                else if (param_count == 16) return typeof(VoidOrig<,,,,,,,,,,,,,,,>);
                else if (param_count == 17) return typeof(VoidOrig<,,,,,,,,,,,,,,,,>);
                else if (param_count == 18) return typeof(VoidOrig<,,,,,,,,,,,,,,,,,>);
                else if (param_count == 19) return typeof(VoidOrig<,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 20) return typeof(VoidOrig<,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 21) return typeof(VoidOrig<,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 22) return typeof(VoidOrig<,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 23) return typeof(VoidOrig<,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 24) return typeof(VoidOrig<,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 25) return typeof(VoidOrig<,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 26) return typeof(VoidOrig<,,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 27) return typeof(VoidOrig<,,,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 28) return typeof(VoidOrig<,,,,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 29) return typeof(VoidOrig<,,,,,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 30) return typeof(VoidOrig<,,,,,,,,,,,,,,,,,,,,,,,,,,,,,>);
            } else {
                if (param_count == 0) return typeof(Orig<>);
                else if (param_count == 1) return typeof(Orig<,>);
                else if (param_count == 2) return typeof(Orig<,,>);
                else if (param_count == 3) return typeof(Orig<,,,>);
                else if (param_count == 4) return typeof(Orig<,,,,>);
                else if (param_count == 5) return typeof(Orig<,,,,,>);
                else if (param_count == 6) return typeof(Orig<,,,,,,>);
                else if (param_count == 7) return typeof(Orig<,,,,,,,>);
                else if (param_count == 8) return typeof(Orig<,,,,,,,,>);
                else if (param_count == 9) return typeof(Orig<,,,,,,,,,>);
                else if (param_count == 10) return typeof(Orig<,,,,,,,,,,>);
                else if (param_count == 11) return typeof(Orig<,,,,,,,,,,,>);
                else if (param_count == 12) return typeof(Orig<,,,,,,,,,,,,>);
                else if (param_count == 13) return typeof(Orig<,,,,,,,,,,,,,>);
                else if (param_count == 14) return typeof(Orig<,,,,,,,,,,,,,,>);
                else if (param_count == 15) return typeof(Orig<,,,,,,,,,,,,,,,>);
                else if (param_count == 16) return typeof(Orig<,,,,,,,,,,,,,,,,>);
                else if (param_count == 17) return typeof(Orig<,,,,,,,,,,,,,,,,,>);
                else if (param_count == 18) return typeof(Orig<,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 19) return typeof(Orig<,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 20) return typeof(Orig<,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 21) return typeof(Orig<,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 22) return typeof(Orig<,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 23) return typeof(Orig<,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 24) return typeof(Orig<,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 25) return typeof(Orig<,,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 26) return typeof(Orig<,,,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 27) return typeof(Orig<,,,,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 28) return typeof(Orig<,,,,,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 29) return typeof(Orig<,,,,,,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 30) return typeof(Orig<,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,>);
            }

            throw new InvalidOperationException($"SemiPatch cannot create delegates for methods with over 30 arguments ({param_count} > 30).");
        }

        public static TypeReference GetBaseExplicitThisOrigType(int param_count, bool is_void = false) {
            if (is_void) {
                // ExplicitThisVoidOrig
                if (param_count == 0) {
                    return ExplicitThisVoidOrig_n0;
                } else if (param_count == 1) {
                    return ExplicitThisVoidOrig_n1;
                } else if (param_count == 2) {
                    return ExplicitThisVoidOrig_n2;
                } else if (param_count == 3) {
                    return ExplicitThisVoidOrig_n3;
                } else if (param_count == 4) {
                    return ExplicitThisVoidOrig_n4;
                } else if (param_count == 5) {
                    return ExplicitThisVoidOrig_n5;
                } else if (param_count == 6) {
                    return ExplicitThisVoidOrig_n6;
                } else if (param_count == 7) {
                    return ExplicitThisVoidOrig_n7;
                } else if (param_count == 8) {
                    return ExplicitThisVoidOrig_n8;
                } else if (param_count == 9) {
                    return ExplicitThisVoidOrig_n9;
                } else if (param_count == 10) {
                    return ExplicitThisVoidOrig_n10;
                } else if (param_count == 11) {
                    return ExplicitThisVoidOrig_n11;
                } else if (param_count == 12) {
                    return ExplicitThisVoidOrig_n12;
                } else if (param_count == 13) {
                    return ExplicitThisVoidOrig_n13;
                } else if (param_count == 14) {
                    return ExplicitThisVoidOrig_n14;
                } else if (param_count == 15) {
                    return ExplicitThisVoidOrig_n15;
                } else if (param_count == 16) {
                    return ExplicitThisVoidOrig_n16;
                } else if (param_count == 17) {
                    return ExplicitThisVoidOrig_n17;
                } else if (param_count == 18) {
                    return ExplicitThisVoidOrig_n18;
                } else if (param_count == 19) {
                    return ExplicitThisVoidOrig_n19;
                } else if (param_count == 20) {
                    return ExplicitThisVoidOrig_n20;
                } else if (param_count == 21) {
                    return ExplicitThisVoidOrig_n21;
                } else if (param_count == 22) {
                    return ExplicitThisVoidOrig_n22;
                } else if (param_count == 23) {
                    return ExplicitThisVoidOrig_n23;
                } else if (param_count == 24) {
                    return ExplicitThisVoidOrig_n24;
                } else if (param_count == 25) {
                    return ExplicitThisVoidOrig_n25;
                } else if (param_count == 26) {
                    return ExplicitThisVoidOrig_n26;
                } else if (param_count == 27) {
                    return ExplicitThisVoidOrig_n27;
                } else if (param_count == 28) {
                    return ExplicitThisVoidOrig_n28;
                } else if (param_count == 29) {
                    return ExplicitThisVoidOrig_n29;
                } else if (param_count == 30) {
                    return ExplicitThisVoidOrig_n30;
                } else {
                    throw new InvalidOperationException($"SemiPatch cannot create delegates for methods with over 30 arguments ({param_count} > 30).");
                }
            } else {
                // ExplicitThisOrig
                if (param_count == 0) {
                    return ExplicitThisOrig_n0;
                } else if (param_count == 1) {
                    return ExplicitThisOrig_n1;
                } else if (param_count == 2) {
                    return ExplicitThisOrig_n2;
                } else if (param_count == 3) {
                    return ExplicitThisOrig_n3;
                } else if (param_count == 4) {
                    return ExplicitThisOrig_n4;
                } else if (param_count == 5) {
                    return ExplicitThisOrig_n5;
                } else if (param_count == 6) {
                    return ExplicitThisOrig_n6;
                } else if (param_count == 7) {
                    return ExplicitThisOrig_n7;
                } else if (param_count == 8) {
                    return ExplicitThisOrig_n8;
                } else if (param_count == 9) {
                    return ExplicitThisOrig_n9;
                } else if (param_count == 10) {
                    return ExplicitThisOrig_n10;
                } else if (param_count == 11) {
                    return ExplicitThisOrig_n11;
                } else if (param_count == 12) {
                    return ExplicitThisOrig_n12;
                } else if (param_count == 13) {
                    return ExplicitThisOrig_n13;
                } else if (param_count == 14) {
                    return ExplicitThisOrig_n14;
                } else if (param_count == 15) {
                    return ExplicitThisOrig_n15;
                } else if (param_count == 16) {
                    return ExplicitThisOrig_n16;
                } else if (param_count == 17) {
                    return ExplicitThisOrig_n17;
                } else if (param_count == 18) {
                    return ExplicitThisOrig_n18;
                } else if (param_count == 19) {
                    return ExplicitThisOrig_n19;
                } else if (param_count == 20) {
                    return ExplicitThisOrig_n20;
                } else if (param_count == 21) {
                    return ExplicitThisOrig_n21;
                } else if (param_count == 22) {
                    return ExplicitThisOrig_n22;
                } else if (param_count == 23) {
                    return ExplicitThisOrig_n23;
                } else if (param_count == 24) {
                    return ExplicitThisOrig_n24;
                } else if (param_count == 25) {
                    return ExplicitThisOrig_n25;
                } else if (param_count == 26) {
                    return ExplicitThisOrig_n26;
                } else if (param_count == 27) {
                    return ExplicitThisOrig_n27;
                } else if (param_count == 28) {
                    return ExplicitThisOrig_n28;
                } else if (param_count == 29) {
                    return ExplicitThisOrig_n29;
                } else if (param_count == 30) {
                    return ExplicitThisOrig_n30;
                } else {
                    throw new InvalidOperationException($"SemiPatch cannot create delegates for methods with over 30 arguments ({param_count} > 30).");
                }
            }
        }

        public static GenericInstanceType GetInstancedExplicitThisOrigType(ModuleDefinition module, int param_count, bool is_void, TypeReference return_type, params TypeReference[] param_types) {
            var type = GetBaseExplicitThisOrigType(param_count, is_void);
            var inst = new GenericInstanceType(module.ImportReference(type));
            for (var i = 0; i < param_types.Length; i++) {
                inst.GenericArguments.Add(param_types[i]);
            }

            if (!is_void) {
                inst.GenericArguments.Add(return_type);
            }
            return inst;
        }

        public static TypeReference ExplicitThisOrigTypeForOrig(ModuleDefinition module, TypeReference instance_type, GenericInstanceType orig_type) {
            var is_void = OrigFactory.TypeIsGenericVoidOrig(orig_type);
            if (!OrigFactory.TypeIsGenericOrig(orig_type) && !is_void) {
                throw new ArgumentException("Type must be generic Orig or VoidOrig", nameof(orig_type));
            }

            var param_count = OrigFactory.GetParameterCount(orig_type);

            var type = GetBaseExplicitThisOrigType(param_count, is_void);
            var inst = new GenericInstanceType(module.ImportReference(type));
            inst.GenericArguments.Add(instance_type);
            for (var i = 0; i < param_count; i++) {
                var orig_arg = orig_type.GenericArguments[i];
                inst.GenericArguments.Add(orig_arg);
            }

            return inst;
        }

        public static bool TypeIsGenericExplicitThisOrig(TypeReference type) {
            return type.IsSame(ExplicitThisOrig_n0, exclude_generic_args: true) || type.IsSame(ExplicitThisOrig_n1, exclude_generic_args: true) || type.IsSame(ExplicitThisOrig_n2, exclude_generic_args: true) || type.IsSame(ExplicitThisOrig_n3, exclude_generic_args: true) || type.IsSame(ExplicitThisOrig_n4, exclude_generic_args: true) || type.IsSame(ExplicitThisOrig_n5, exclude_generic_args: true) || type.IsSame(ExplicitThisOrig_n6, exclude_generic_args: true) || type.IsSame(ExplicitThisOrig_n7, exclude_generic_args: true) || type.IsSame(ExplicitThisOrig_n8, exclude_generic_args: true) || type.IsSame(ExplicitThisOrig_n9, exclude_generic_args: true) || type.IsSame(ExplicitThisOrig_n10, exclude_generic_args: true) || type.IsSame(ExplicitThisOrig_n11, exclude_generic_args: true) || type.IsSame(ExplicitThisOrig_n12, exclude_generic_args: true) || type.IsSame(ExplicitThisOrig_n13, exclude_generic_args: true) || type.IsSame(ExplicitThisOrig_n14, exclude_generic_args: true) || type.IsSame(ExplicitThisOrig_n15, exclude_generic_args: true) || type.IsSame(ExplicitThisOrig_n16, exclude_generic_args: true) || type.IsSame(ExplicitThisOrig_n17, exclude_generic_args: true) || type.IsSame(ExplicitThisOrig_n18, exclude_generic_args: true) || type.IsSame(ExplicitThisOrig_n19, exclude_generic_args: true) || type.IsSame(ExplicitThisOrig_n20, exclude_generic_args: true) || type.IsSame(ExplicitThisOrig_n21, exclude_generic_args: true) || type.IsSame(ExplicitThisOrig_n22, exclude_generic_args: true) || type.IsSame(ExplicitThisOrig_n23, exclude_generic_args: true) || type.IsSame(ExplicitThisOrig_n24, exclude_generic_args: true) || type.IsSame(ExplicitThisOrig_n25, exclude_generic_args: true) || type.IsSame(ExplicitThisOrig_n26, exclude_generic_args: true) || type.IsSame(ExplicitThisOrig_n27, exclude_generic_args: true) || type.IsSame(ExplicitThisOrig_n28, exclude_generic_args: true) || type.IsSame(ExplicitThisOrig_n29, exclude_generic_args: true) || type.IsSame(ExplicitThisOrig_n30, exclude_generic_args: true);
        }

        public static bool TypeIsGenericExplicitThisVoidOrig(TypeReference type) {
            return type.IsSame(ExplicitThisVoidOrig_n0, exclude_generic_args: true) || type.IsSame(ExplicitThisVoidOrig_n1, exclude_generic_args: true) || type.IsSame(ExplicitThisVoidOrig_n2, exclude_generic_args: true) || type.IsSame(ExplicitThisVoidOrig_n3, exclude_generic_args: true) || type.IsSame(ExplicitThisVoidOrig_n4, exclude_generic_args: true) || type.IsSame(ExplicitThisVoidOrig_n5, exclude_generic_args: true) || type.IsSame(ExplicitThisVoidOrig_n6, exclude_generic_args: true) || type.IsSame(ExplicitThisVoidOrig_n7, exclude_generic_args: true) || type.IsSame(ExplicitThisVoidOrig_n8, exclude_generic_args: true) || type.IsSame(ExplicitThisVoidOrig_n9, exclude_generic_args: true) || type.IsSame(ExplicitThisVoidOrig_n10, exclude_generic_args: true) || type.IsSame(ExplicitThisVoidOrig_n11, exclude_generic_args: true) || type.IsSame(ExplicitThisVoidOrig_n12, exclude_generic_args: true) || type.IsSame(ExplicitThisVoidOrig_n13, exclude_generic_args: true) || type.IsSame(ExplicitThisVoidOrig_n14, exclude_generic_args: true) || type.IsSame(ExplicitThisVoidOrig_n15, exclude_generic_args: true) || type.IsSame(ExplicitThisVoidOrig_n16, exclude_generic_args: true) || type.IsSame(ExplicitThisVoidOrig_n17, exclude_generic_args: true) || type.IsSame(ExplicitThisVoidOrig_n18, exclude_generic_args: true) || type.IsSame(ExplicitThisVoidOrig_n19, exclude_generic_args: true) || type.IsSame(ExplicitThisVoidOrig_n20, exclude_generic_args: true) || type.IsSame(ExplicitThisVoidOrig_n21, exclude_generic_args: true) || type.IsSame(ExplicitThisVoidOrig_n22, exclude_generic_args: true) || type.IsSame(ExplicitThisVoidOrig_n23, exclude_generic_args: true) || type.IsSame(ExplicitThisVoidOrig_n24, exclude_generic_args: true) || type.IsSame(ExplicitThisVoidOrig_n25, exclude_generic_args: true) || type.IsSame(ExplicitThisVoidOrig_n26, exclude_generic_args: true) || type.IsSame(ExplicitThisVoidOrig_n27, exclude_generic_args: true) || type.IsSame(ExplicitThisVoidOrig_n28, exclude_generic_args: true) || type.IsSame(ExplicitThisVoidOrig_n29, exclude_generic_args: true) || type.IsSame(ExplicitThisVoidOrig_n30, exclude_generic_args: true);
        }

        public static MethodReference GetExplicitThisInvokeMethod(TypeReference type) {
            if (!TypeIsGenericExplicitThisOrig(type) && !TypeIsGenericExplicitThisVoidOrig(type)) throw new ArgumentException("Argument must be an ExplicitThisOrig or ExplicitThisVoidOrig TypeReference", nameof(type));

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

        public static MethodReference NativePointerConstructorForExplicitThisOrigType(ModuleDefinition module, TypeReference type) {
            if (!TypeIsGenericExplicitThisOrig(type) && !TypeIsGenericExplicitThisVoidOrig(type)) throw new ArgumentException("Argument must be an ExplicitThisOrig or ExplicitThisVoidOrig TypeReference", nameof(type));
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

        public static Type GetExplicitThisReflectionType(bool is_void, int param_count) {
            if (is_void) {
                if (param_count == 0) return typeof(ExplicitThisVoidOrig<>);
                else if (param_count == 1) return typeof(ExplicitThisVoidOrig<,>);
                else if (param_count == 2) return typeof(ExplicitThisVoidOrig<,,>);
                else if (param_count == 3) return typeof(ExplicitThisVoidOrig<,,,>);
                else if (param_count == 4) return typeof(ExplicitThisVoidOrig<,,,,>);
                else if (param_count == 5) return typeof(ExplicitThisVoidOrig<,,,,,>);
                else if (param_count == 6) return typeof(ExplicitThisVoidOrig<,,,,,,>);
                else if (param_count == 7) return typeof(ExplicitThisVoidOrig<,,,,,,,>);
                else if (param_count == 8) return typeof(ExplicitThisVoidOrig<,,,,,,,,>);
                else if (param_count == 9) return typeof(ExplicitThisVoidOrig<,,,,,,,,,>);
                else if (param_count == 10) return typeof(ExplicitThisVoidOrig<,,,,,,,,,,>);
                else if (param_count == 11) return typeof(ExplicitThisVoidOrig<,,,,,,,,,,,>);
                else if (param_count == 12) return typeof(ExplicitThisVoidOrig<,,,,,,,,,,,,>);
                else if (param_count == 13) return typeof(ExplicitThisVoidOrig<,,,,,,,,,,,,,>);
                else if (param_count == 14) return typeof(ExplicitThisVoidOrig<,,,,,,,,,,,,,,>);
                else if (param_count == 15) return typeof(ExplicitThisVoidOrig<,,,,,,,,,,,,,,,>);
                else if (param_count == 16) return typeof(ExplicitThisVoidOrig<,,,,,,,,,,,,,,,,>);
                else if (param_count == 17) return typeof(ExplicitThisVoidOrig<,,,,,,,,,,,,,,,,,>);
                else if (param_count == 18) return typeof(ExplicitThisVoidOrig<,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 19) return typeof(ExplicitThisVoidOrig<,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 20) return typeof(ExplicitThisVoidOrig<,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 21) return typeof(ExplicitThisVoidOrig<,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 22) return typeof(ExplicitThisVoidOrig<,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 23) return typeof(ExplicitThisVoidOrig<,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 24) return typeof(ExplicitThisVoidOrig<,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 25) return typeof(ExplicitThisVoidOrig<,,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 26) return typeof(ExplicitThisVoidOrig<,,,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 27) return typeof(ExplicitThisVoidOrig<,,,,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 28) return typeof(ExplicitThisVoidOrig<,,,,,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 29) return typeof(ExplicitThisVoidOrig<,,,,,,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 30) return typeof(ExplicitThisVoidOrig<,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,>);
            } else {
                if (param_count == 0) return typeof(ExplicitThisOrig<,>);
                else if (param_count == 1) return typeof(ExplicitThisOrig<,,>);
                else if (param_count == 2) return typeof(ExplicitThisOrig<,,,>);
                else if (param_count == 3) return typeof(ExplicitThisOrig<,,,,>);
                else if (param_count == 4) return typeof(ExplicitThisOrig<,,,,,>);
                else if (param_count == 5) return typeof(ExplicitThisOrig<,,,,,,>);
                else if (param_count == 6) return typeof(ExplicitThisOrig<,,,,,,,>);
                else if (param_count == 7) return typeof(ExplicitThisOrig<,,,,,,,,>);
                else if (param_count == 8) return typeof(ExplicitThisOrig<,,,,,,,,,>);
                else if (param_count == 9) return typeof(ExplicitThisOrig<,,,,,,,,,,>);
                else if (param_count == 10) return typeof(ExplicitThisOrig<,,,,,,,,,,,>);
                else if (param_count == 11) return typeof(ExplicitThisOrig<,,,,,,,,,,,,>);
                else if (param_count == 12) return typeof(ExplicitThisOrig<,,,,,,,,,,,,,>);
                else if (param_count == 13) return typeof(ExplicitThisOrig<,,,,,,,,,,,,,,>);
                else if (param_count == 14) return typeof(ExplicitThisOrig<,,,,,,,,,,,,,,,>);
                else if (param_count == 15) return typeof(ExplicitThisOrig<,,,,,,,,,,,,,,,,>);
                else if (param_count == 16) return typeof(ExplicitThisOrig<,,,,,,,,,,,,,,,,,>);
                else if (param_count == 17) return typeof(ExplicitThisOrig<,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 18) return typeof(ExplicitThisOrig<,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 19) return typeof(ExplicitThisOrig<,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 20) return typeof(ExplicitThisOrig<,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 21) return typeof(ExplicitThisOrig<,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 22) return typeof(ExplicitThisOrig<,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 23) return typeof(ExplicitThisOrig<,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 24) return typeof(ExplicitThisOrig<,,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 25) return typeof(ExplicitThisOrig<,,,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 26) return typeof(ExplicitThisOrig<,,,,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 27) return typeof(ExplicitThisOrig<,,,,,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 28) return typeof(ExplicitThisOrig<,,,,,,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 29) return typeof(ExplicitThisOrig<,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 30) return typeof(ExplicitThisOrig<,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,>);
            }

            throw new InvalidOperationException($"SemiPatch cannot create delegates for methods with over 30 arguments ({param_count} > 30).");
        }
    }
}
