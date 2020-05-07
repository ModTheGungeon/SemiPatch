using System;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;

namespace SemiPatch {
    internal class ExplicitOrigFactory {
        public static TypeReference ExplicitVoidOrig_n0 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitVoidOrig`1");
        public static TypeReference ExplicitVoidOrig_n1 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitVoidOrig`2");
        public static TypeReference ExplicitVoidOrig_n2 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitVoidOrig`3");
        public static TypeReference ExplicitVoidOrig_n3 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitVoidOrig`4");
        public static TypeReference ExplicitVoidOrig_n4 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitVoidOrig`5");
        public static TypeReference ExplicitVoidOrig_n5 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitVoidOrig`6");
        public static TypeReference ExplicitVoidOrig_n6 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitVoidOrig`7");
        public static TypeReference ExplicitVoidOrig_n7 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitVoidOrig`8");
        public static TypeReference ExplicitVoidOrig_n8 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitVoidOrig`9");
        public static TypeReference ExplicitVoidOrig_n9 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitVoidOrig`10");
        public static TypeReference ExplicitVoidOrig_n10 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitVoidOrig`11");
        public static TypeReference ExplicitVoidOrig_n11 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitVoidOrig`12");
        public static TypeReference ExplicitVoidOrig_n12 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitVoidOrig`13");
        public static TypeReference ExplicitVoidOrig_n13 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitVoidOrig`14");
        public static TypeReference ExplicitVoidOrig_n14 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitVoidOrig`15");
        public static TypeReference ExplicitVoidOrig_n15 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitVoidOrig`16");
        public static TypeReference ExplicitVoidOrig_n16 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitVoidOrig`17");
        public static TypeReference ExplicitVoidOrig_n17 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitVoidOrig`18");
        public static TypeReference ExplicitVoidOrig_n18 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitVoidOrig`19");
        public static TypeReference ExplicitVoidOrig_n19 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitVoidOrig`20");
        public static TypeReference ExplicitVoidOrig_n20 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitVoidOrig`21");
        public static TypeReference ExplicitVoidOrig_n21 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitVoidOrig`22");
        public static TypeReference ExplicitVoidOrig_n22 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitVoidOrig`23");
        public static TypeReference ExplicitVoidOrig_n23 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitVoidOrig`24");
        public static TypeReference ExplicitVoidOrig_n24 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitVoidOrig`25");
        public static TypeReference ExplicitVoidOrig_n25 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitVoidOrig`26");
        public static TypeReference ExplicitVoidOrig_n26 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitVoidOrig`27");
        public static TypeReference ExplicitVoidOrig_n27 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitVoidOrig`28");
        public static TypeReference ExplicitVoidOrig_n28 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitVoidOrig`29");
        public static TypeReference ExplicitVoidOrig_n29 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitVoidOrig`30");
        public static TypeReference ExplicitVoidOrig_n30 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitVoidOrig`31");

        public static TypeReference ExplicitOrig_n0 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitOrig`2");
        public static TypeReference ExplicitOrig_n1 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitOrig`3");
        public static TypeReference ExplicitOrig_n2 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitOrig`4");
        public static TypeReference ExplicitOrig_n3 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitOrig`5");
        public static TypeReference ExplicitOrig_n4 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitOrig`6");
        public static TypeReference ExplicitOrig_n5 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitOrig`7");
        public static TypeReference ExplicitOrig_n6 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitOrig`8");
        public static TypeReference ExplicitOrig_n7 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitOrig`9");
        public static TypeReference ExplicitOrig_n8 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitOrig`10");
        public static TypeReference ExplicitOrig_n9 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitOrig`11");
        public static TypeReference ExplicitOrig_n10 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitOrig`12");
        public static TypeReference ExplicitOrig_n11 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitOrig`13");
        public static TypeReference ExplicitOrig_n12 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitOrig`14");
        public static TypeReference ExplicitOrig_n13 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitOrig`15");
        public static TypeReference ExplicitOrig_n14 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitOrig`16");
        public static TypeReference ExplicitOrig_n15 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitOrig`17");
        public static TypeReference ExplicitOrig_n16 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitOrig`18");
        public static TypeReference ExplicitOrig_n17 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitOrig`19");
        public static TypeReference ExplicitOrig_n18 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitOrig`20");
        public static TypeReference ExplicitOrig_n19 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitOrig`21");
        public static TypeReference ExplicitOrig_n20 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitOrig`22");
        public static TypeReference ExplicitOrig_n21 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitOrig`23");
        public static TypeReference ExplicitOrig_n22 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitOrig`24");
        public static TypeReference ExplicitOrig_n23 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitOrig`25");
        public static TypeReference ExplicitOrig_n24 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitOrig`26");
        public static TypeReference ExplicitOrig_n25 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitOrig`27");
        public static TypeReference ExplicitOrig_n26 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitOrig`28");
        public static TypeReference ExplicitOrig_n27 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitOrig`29");
        public static TypeReference ExplicitOrig_n28 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitOrig`30");
        public static TypeReference ExplicitOrig_n29 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitOrig`31");
        public static TypeReference ExplicitOrig_n30 = SemiPatch.SemiPatchModule.GetType("SemiPatch.ExplicitOrig`32");

        public static TypeReference GetBaseExplicitOrigType(int param_count, bool is_void = false) {
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

        public static GenericInstanceType GetInstancedExplicitOrigType(ModuleDefinition module, int param_count, bool is_void, TypeReference return_type, params TypeReference[] param_types) {
            var type = GetBaseExplicitOrigType(param_count, is_void);
            var inst = new GenericInstanceType(module.ImportReference(type));
            for (var i = 0; i < param_types.Length; i++) {
                inst.GenericArguments.Add(param_types[i]);
            }

            if (!is_void) {
                inst.GenericArguments.Add(return_type);
            }
            return inst;
        }

        public static TypeReference ExplicitOrigTypeForOrig(ModuleDefinition module, TypeReference instance_type, GenericInstanceType orig_type) {
            var is_void = OrigFactory.TypeIsGenericVoidOrig(orig_type);
            if (!OrigFactory.TypeIsGenericOrig(orig_type) && !is_void) {
                throw new ArgumentException("Type must be generic Orig or VoidOrig", nameof(orig_type));
            }

            var param_count = OrigFactory.GetParameterCount(orig_type);

            var type = GetBaseExplicitOrigType(param_count, is_void);
            var inst = new GenericInstanceType(module.ImportReference(type));
            inst.GenericArguments.Add(instance_type);
            for (var i = 0; i < param_count; i++) {
                var orig_arg = orig_type.GenericArguments[i];
                inst.GenericArguments.Add(orig_arg);
            }

            return inst;
        }

        public static bool TypeIsGenericExplicitOrig(TypeReference type) {
            return type.IsSame(ExplicitOrig_n0, exclude_generic_args: true) || type.IsSame(ExplicitOrig_n1, exclude_generic_args: true) || type.IsSame(ExplicitOrig_n2, exclude_generic_args: true) || type.IsSame(ExplicitOrig_n3, exclude_generic_args: true) || type.IsSame(ExplicitOrig_n4, exclude_generic_args: true) || type.IsSame(ExplicitOrig_n5, exclude_generic_args: true) || type.IsSame(ExplicitOrig_n6, exclude_generic_args: true) || type.IsSame(ExplicitOrig_n7, exclude_generic_args: true) || type.IsSame(ExplicitOrig_n8, exclude_generic_args: true) || type.IsSame(ExplicitOrig_n9, exclude_generic_args: true) || type.IsSame(ExplicitOrig_n10, exclude_generic_args: true) || type.IsSame(ExplicitOrig_n11, exclude_generic_args: true) || type.IsSame(ExplicitOrig_n12, exclude_generic_args: true) || type.IsSame(ExplicitOrig_n13, exclude_generic_args: true) || type.IsSame(ExplicitOrig_n14, exclude_generic_args: true) || type.IsSame(ExplicitOrig_n15, exclude_generic_args: true) || type.IsSame(ExplicitOrig_n16, exclude_generic_args: true) || type.IsSame(ExplicitOrig_n17, exclude_generic_args: true) || type.IsSame(ExplicitOrig_n18, exclude_generic_args: true) || type.IsSame(ExplicitOrig_n19, exclude_generic_args: true) || type.IsSame(ExplicitOrig_n20, exclude_generic_args: true) || type.IsSame(ExplicitOrig_n21, exclude_generic_args: true) || type.IsSame(ExplicitOrig_n22, exclude_generic_args: true) || type.IsSame(ExplicitOrig_n23, exclude_generic_args: true) || type.IsSame(ExplicitOrig_n24, exclude_generic_args: true) || type.IsSame(ExplicitOrig_n25, exclude_generic_args: true) || type.IsSame(ExplicitOrig_n26, exclude_generic_args: true) || type.IsSame(ExplicitOrig_n27, exclude_generic_args: true) || type.IsSame(ExplicitOrig_n28, exclude_generic_args: true) || type.IsSame(ExplicitOrig_n29, exclude_generic_args: true) || type.IsSame(ExplicitOrig_n30, exclude_generic_args: true);
        }

        public static bool TypeIsGenericExplicitVoidOrig(TypeReference type) {
            return type.IsSame(ExplicitVoidOrig_n0, exclude_generic_args: true) || type.IsSame(ExplicitVoidOrig_n1, exclude_generic_args: true) || type.IsSame(ExplicitVoidOrig_n2, exclude_generic_args: true) || type.IsSame(ExplicitVoidOrig_n3, exclude_generic_args: true) || type.IsSame(ExplicitVoidOrig_n4, exclude_generic_args: true) || type.IsSame(ExplicitVoidOrig_n5, exclude_generic_args: true) || type.IsSame(ExplicitVoidOrig_n6, exclude_generic_args: true) || type.IsSame(ExplicitVoidOrig_n7, exclude_generic_args: true) || type.IsSame(ExplicitVoidOrig_n8, exclude_generic_args: true) || type.IsSame(ExplicitVoidOrig_n9, exclude_generic_args: true) || type.IsSame(ExplicitVoidOrig_n10, exclude_generic_args: true) || type.IsSame(ExplicitVoidOrig_n11, exclude_generic_args: true) || type.IsSame(ExplicitVoidOrig_n12, exclude_generic_args: true) || type.IsSame(ExplicitVoidOrig_n13, exclude_generic_args: true) || type.IsSame(ExplicitVoidOrig_n14, exclude_generic_args: true) || type.IsSame(ExplicitVoidOrig_n15, exclude_generic_args: true) || type.IsSame(ExplicitVoidOrig_n16, exclude_generic_args: true) || type.IsSame(ExplicitVoidOrig_n17, exclude_generic_args: true) || type.IsSame(ExplicitVoidOrig_n18, exclude_generic_args: true) || type.IsSame(ExplicitVoidOrig_n19, exclude_generic_args: true) || type.IsSame(ExplicitVoidOrig_n20, exclude_generic_args: true) || type.IsSame(ExplicitVoidOrig_n21, exclude_generic_args: true) || type.IsSame(ExplicitVoidOrig_n22, exclude_generic_args: true) || type.IsSame(ExplicitVoidOrig_n23, exclude_generic_args: true) || type.IsSame(ExplicitVoidOrig_n24, exclude_generic_args: true) || type.IsSame(ExplicitVoidOrig_n25, exclude_generic_args: true) || type.IsSame(ExplicitVoidOrig_n26, exclude_generic_args: true) || type.IsSame(ExplicitVoidOrig_n27, exclude_generic_args: true) || type.IsSame(ExplicitVoidOrig_n28, exclude_generic_args: true) || type.IsSame(ExplicitVoidOrig_n29, exclude_generic_args: true) || type.IsSame(ExplicitVoidOrig_n30, exclude_generic_args: true);
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

        public static Type GetReflectionType(bool is_void, int param_count) {
            if (is_void) {
                if (param_count == 0) return typeof(ExplicitVoidOrig<>);
                else if (param_count == 1) return typeof(ExplicitVoidOrig<,>);
                else if (param_count == 2) return typeof(ExplicitVoidOrig<,,>);
                else if (param_count == 3) return typeof(ExplicitVoidOrig<,,,>);
                else if (param_count == 4) return typeof(ExplicitVoidOrig<,,,,>);
                else if (param_count == 5) return typeof(ExplicitVoidOrig<,,,,,>);
                else if (param_count == 6) return typeof(ExplicitVoidOrig<,,,,,,>);
                else if (param_count == 7) return typeof(ExplicitVoidOrig<,,,,,,,>);
                else if (param_count == 8) return typeof(ExplicitVoidOrig<,,,,,,,,>);
                else if (param_count == 9) return typeof(ExplicitVoidOrig<,,,,,,,,,>);
                else if (param_count == 10) return typeof(ExplicitVoidOrig<,,,,,,,,,,>);
                else if (param_count == 11) return typeof(ExplicitVoidOrig<,,,,,,,,,,,>);
                else if (param_count == 12) return typeof(ExplicitVoidOrig<,,,,,,,,,,,,>);
                else if (param_count == 13) return typeof(ExplicitVoidOrig<,,,,,,,,,,,,,>);
                else if (param_count == 14) return typeof(ExplicitVoidOrig<,,,,,,,,,,,,,,>);
                else if (param_count == 15) return typeof(ExplicitVoidOrig<,,,,,,,,,,,,,,,>);
                else if (param_count == 16) return typeof(ExplicitVoidOrig<,,,,,,,,,,,,,,,,>);
                else if (param_count == 17) return typeof(ExplicitVoidOrig<,,,,,,,,,,,,,,,,,>);
                else if (param_count == 18) return typeof(ExplicitVoidOrig<,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 19) return typeof(ExplicitVoidOrig<,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 20) return typeof(ExplicitVoidOrig<,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 21) return typeof(ExplicitVoidOrig<,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 22) return typeof(ExplicitVoidOrig<,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 23) return typeof(ExplicitVoidOrig<,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 24) return typeof(ExplicitVoidOrig<,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 25) return typeof(ExplicitVoidOrig<,,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 26) return typeof(ExplicitVoidOrig<,,,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 27) return typeof(ExplicitVoidOrig<,,,,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 28) return typeof(ExplicitVoidOrig<,,,,,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 29) return typeof(ExplicitVoidOrig<,,,,,,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 30) return typeof(ExplicitVoidOrig<,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,>);
            } else {
                if (param_count == 0) return typeof(ExplicitOrig<,>);
                else if (param_count == 1) return typeof(ExplicitOrig<,,>);
                else if (param_count == 2) return typeof(ExplicitOrig<,,,>);
                else if (param_count == 3) return typeof(ExplicitOrig<,,,,>);
                else if (param_count == 4) return typeof(ExplicitOrig<,,,,,>);
                else if (param_count == 5) return typeof(ExplicitOrig<,,,,,,>);
                else if (param_count == 6) return typeof(ExplicitOrig<,,,,,,,>);
                else if (param_count == 7) return typeof(ExplicitOrig<,,,,,,,,>);
                else if (param_count == 8) return typeof(ExplicitOrig<,,,,,,,,,>);
                else if (param_count == 9) return typeof(ExplicitOrig<,,,,,,,,,,>);
                else if (param_count == 10) return typeof(ExplicitOrig<,,,,,,,,,,,>);
                else if (param_count == 11) return typeof(ExplicitOrig<,,,,,,,,,,,,>);
                else if (param_count == 12) return typeof(ExplicitOrig<,,,,,,,,,,,,,>);
                else if (param_count == 13) return typeof(ExplicitOrig<,,,,,,,,,,,,,,>);
                else if (param_count == 14) return typeof(ExplicitOrig<,,,,,,,,,,,,,,,>);
                else if (param_count == 15) return typeof(ExplicitOrig<,,,,,,,,,,,,,,,,>);
                else if (param_count == 16) return typeof(ExplicitOrig<,,,,,,,,,,,,,,,,,>);
                else if (param_count == 17) return typeof(ExplicitOrig<,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 18) return typeof(ExplicitOrig<,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 19) return typeof(ExplicitOrig<,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 20) return typeof(ExplicitOrig<,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 21) return typeof(ExplicitOrig<,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 22) return typeof(ExplicitOrig<,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 23) return typeof(ExplicitOrig<,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 24) return typeof(ExplicitOrig<,,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 25) return typeof(ExplicitOrig<,,,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 26) return typeof(ExplicitOrig<,,,,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 27) return typeof(ExplicitOrig<,,,,,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 28) return typeof(ExplicitOrig<,,,,,,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 29) return typeof(ExplicitOrig<,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,>);
                else if (param_count == 30) return typeof(ExplicitOrig<,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,>);
            }

            throw new InvalidOperationException($"SemiPatch cannot create delegates for methods with over 30 arguments ({param_count} > 30).");
        }
    }
}
