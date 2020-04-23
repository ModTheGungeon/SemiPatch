using System;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;

namespace SemiPatch {
    public class OrigFactory {
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

        public static TypeReference OrigGenericTypeForMethod(MethodReference method, bool skip_first_arg = false) {
            var param_count = method.Parameters.Count;
            if (skip_first_arg) param_count -= 1;
            if (method.ReturnType.IsSame(SemiPatch.VoidType)) {
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
            return type.IsSame(Orig_n0) || type.IsSame(Orig_n1) || type.IsSame(Orig_n2) || type.IsSame(Orig_n3) || type.IsSame(Orig_n4) || type.IsSame(Orig_n5) || type.IsSame(Orig_n6) || type.IsSame(Orig_n7) || type.IsSame(Orig_n8) || type.IsSame(Orig_n9) || type.IsSame(Orig_n10) || type.IsSame(Orig_n11) || type.IsSame(Orig_n12) || type.IsSame(Orig_n13) || type.IsSame(Orig_n14) || type.IsSame(Orig_n15) || type.IsSame(Orig_n16) || type.IsSame(Orig_n17) || type.IsSame(Orig_n18) || type.IsSame(Orig_n19) || type.IsSame(Orig_n20) || type.IsSame(Orig_n21) || type.IsSame(Orig_n22) || type.IsSame(Orig_n23) || type.IsSame(Orig_n24) || type.IsSame(Orig_n25) || type.IsSame(Orig_n26) || type.IsSame(Orig_n27) || type.IsSame(Orig_n28) || type.IsSame(Orig_n29) || type.IsSame(Orig_n30);
        }

        public static bool TypeIsGenericVoidOrig(TypeReference type) {
            return type.IsSame(VoidOrig_n0) || type.IsSame(VoidOrig_n1) || type.IsSame(VoidOrig_n2) || type.IsSame(VoidOrig_n3) || type.IsSame(VoidOrig_n4) || type.IsSame(VoidOrig_n5) || type.IsSame(VoidOrig_n6) || type.IsSame(VoidOrig_n7) || type.IsSame(VoidOrig_n8) || type.IsSame(VoidOrig_n9) || type.IsSame(VoidOrig_n10) || type.IsSame(VoidOrig_n11) || type.IsSame(VoidOrig_n12) || type.IsSame(VoidOrig_n13) || type.IsSame(VoidOrig_n14) || type.IsSame(VoidOrig_n15) || type.IsSame(VoidOrig_n16) || type.IsSame(VoidOrig_n17) || type.IsSame(VoidOrig_n18) || type.IsSame(VoidOrig_n19) || type.IsSame(VoidOrig_n20) || type.IsSame(VoidOrig_n21) || type.IsSame(VoidOrig_n22) || type.IsSame(VoidOrig_n23) || type.IsSame(VoidOrig_n24) || type.IsSame(VoidOrig_n25) || type.IsSame(VoidOrig_n26) || type.IsSame(VoidOrig_n27) || type.IsSame(VoidOrig_n28) || type.IsSame(VoidOrig_n29) || type.IsSame(VoidOrig_n30);
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
            if (!orig.IsGenericInstance) throw new ArgumentException("Argument must be an Orig or VoidOrig TypeReference", nameof(orig));
            var inst = (GenericInstanceType)orig;
            var is_void = TypeIsGenericVoidOrig(inst.ElementType);
            TypeReference return_type = null;
            if (!TypeIsGenericOrig(inst.ElementType) && !is_void) throw new ArgumentException("Argument must be an Orig or VoidOrig TypeReference", nameof(orig));

            var n = inst.GenericArguments.Count;
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
                s.Append(" ");
                s.Append("arg");
                s.Append(i);
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
            return new Signature(s.ToString());
        }
    }
}
