using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SemiPatch {
    public static partial class Extensions {
        public static bool IsSame(this TypeReference a, TypeReference b, bool exclude_generic_args = false) {
            if (ReferenceEquals(a, b)) return true;
            if (a == b) return true;

            if (!exclude_generic_args) {
                if (a is GenericInstanceType a_generic && b is GenericInstanceType b_generic) {
                    if (a_generic.GenericArguments.Count != b_generic.GenericArguments.Count) return false;
                    for (var i = 0; i < a_generic.GenericArguments.Count; i++) {
                        if (!IsSame(a_generic.GenericArguments[i], b_generic.GenericArguments[i])) return false;
                    }
                } else if (a is GenericInstanceType || b is GenericInstanceType) return false;
            }

            a = a.Resolve() ?? a;
            b = b.Resolve() ?? b;

            if (a.Name != b.Name) return false;
            if (a.Namespace != b.Namespace) return false;

            var scope_name = a.Scope.Name;
            if (scope_name == "mscorlib.dll") {
                // special case: mscorlib is weird, and e.g. on mono a ref to 2.0 will still resolve to 4.0
                // so we avoid doing any further checks
                return true;
            }

            if (a is TypeDefinition && b is TypeDefinition) {
                var ad = a as TypeDefinition;
                var bd = b as TypeDefinition;

                if (ad.Methods.Count != bd.Methods.Count) return false;
                if (ad.GenericParameters.Count != bd.GenericParameters.Count) return false;
                if (ad.Fields.Count != bd.Fields.Count) return false;
                if (ad.Properties.Count != bd.Properties.Count) return false;
                if (ad.CustomAttributes.Count != bd.CustomAttributes.Count) return false;
            } else if (a is TypeDefinition || b is TypeDefinition) return false;

            return a.Scope.IsSame(b.Scope);
        }

        public static bool IsSame(this IMetadataScope a, IMetadataScope b) {
            string a_full_name = null;
            string a_name = null;
            if (a is ModuleDefinition) {
                a_full_name = ((ModuleDefinition)a).Assembly.Name.FullName;
                a_name = ((ModuleDefinition)a).Assembly.Name.Name;
            } else if (a is AssemblyNameReference) {
                a_full_name = ((AssemblyNameReference)a).FullName;
                a_name = ((AssemblyNameReference)a).Name;
            }

            string b_full_name = null;
            string b_name = null;
            if (b is ModuleDefinition) {
                b_full_name = ((ModuleDefinition)b).Assembly.Name.FullName;
                b_name = ((ModuleDefinition)b).Assembly.Name.Name;
            } else if (b is AssemblyNameReference) {
                b_full_name = ((AssemblyNameReference)b).FullName;
                b_name = ((AssemblyNameReference)b).Name;
            }

            if (a_name == "mscorlib" && b_name == "mscorlib") {
                return true;
            }

            return a_full_name == b_full_name;
        }

        public static bool IsSame(this MethodReference a, MethodReference b, bool exclude_generic_args = false) {
            if (ReferenceEquals(a, b)) return true;
            if (a == b) return true;

            if (!exclude_generic_args) {
                if (a is GenericInstanceMethod a_generic && b is GenericInstanceMethod b_generic) {
                    if (a_generic.GenericArguments.Count != b_generic.GenericArguments.Count) return false;
                    for (var i = 0; i < a_generic.GenericArguments.Count; i++) {
                        if (!IsSame(a_generic.GenericArguments[i], b_generic.GenericArguments[i])) return false;
                    }
                } else if (a is GenericInstanceMethod || b is GenericInstanceMethod) return false;
            }

            a = a.Resolve() ?? a;
            b = b.Resolve() ?? b;

            if (a.Name != b.Name) return false;
            if (a.Parameters.Count != b.Parameters.Count) return false;
            if (!a.DeclaringType.IsSame(b.DeclaringType)) return false;
            if (!a.ReturnType.IsSame(b.ReturnType)) return false;
            for (var i = 0; i < a.Parameters.Count; i++) {
                if (!a.Parameters[i].IsSame(b.Parameters[i])) return false;
            }

            return true;
        }

        public static bool IsSame(this ParameterReference a, ParameterReference b) {
            if (ReferenceEquals(a, b)) return true;
            if (a == b) return true;

            a = a.Resolve() ?? a;
            b = b.Resolve() ?? b;

            if (a.Name != b.Name) return false;
            if (a.Index != b.Index) return false;
            if (!a.ParameterType.IsSame(b.ParameterType)) return false;

            return true;
        }

        public static int CalculateHashCode(this MethodBody body) {
            var x = body.Instructions.Count;
            for (var i = 0; i < body.Instructions.Count; i++) {
                var instr = body.Instructions[i];
                x ^= instr.ToString().GetHashCode();
            }
            return x;
        }

        public static int CalculateHashCode(this CustomAttribute attrib) {
            var x = attrib.Constructor.DeclaringType.PrefixSignature(attrib.Constructor.BuildSignature()).GetHashCode();
            for (var i = 0; i < attrib.ConstructorArguments.Count; i++) {
                var arg = attrib.ConstructorArguments[i];
                x ^= arg.ToString().GetHashCode();
            }
            for (var i = 0; i < attrib.Fields.Count; i++) {
                var field = attrib.Fields[i];
                x ^= field.Name.GetHashCode();
                x ^= field.Argument.Type.BuildSignature().GetHashCode();
                x ^= field.Argument.Value.ToString().GetHashCode();
            }
            for (var i = 0; i < attrib.Properties.Count; i++) {
                var prop = attrib.Properties[i];
                x ^= prop.Name.GetHashCode();
                x ^= prop.Argument.Type.BuildSignature().GetHashCode();
                x ^= prop.Argument.Value.ToString().GetHashCode();
            }
            return x;
        }

        public static int CalculateHashCode(this MethodDefinition method) {
            var x = method.Body.CalculateHashCode();
            for (var i = 0; i < method.CustomAttributes.Count; i++) {
                var attrib = method.CustomAttributes[i];
                x ^= attrib.CalculateHashCode();
            }
            x ^= (int)method.Attributes * 2663;
            x ^= (int)method.ImplAttributes * 6983;
            x ^= (int)method.SemanticsAttributes * 9811;

            // we don't hash RVA for two reasons
            // one is that we already hash the entire body, so rva is unnecessary
            // two is that changing surrounding members will actually affect
            // the rva - I'm not sure why, but I think it might have something
            // to do with the compiler inserting nops to pad the size of the
            // method to a specific byte boundary

            // this leads to false positive differences being detected inboth
            // SemiPatchDiffSource and CILDiffSource, so we just ignore it

            return x;
        }

        public static int CalculateHashCode(this FieldDefinition field) {
            var x = field.BuildSignature().GetHashCode();
            for (var i = 0; i < field.CustomAttributes.Count; i++) {
                var attrib = field.CustomAttributes[i];
                x ^= attrib.CalculateHashCode();
            }
            x ^= (int)field.Attributes * 2663;
            x ^= (int)field.RVA * 4547;
            return x;
        }

        public static int CalculateHashCode(this PropertyDefinition prop) {
            var x = prop.BuildSignature().GetHashCode();
            for (var i = 0; i < prop.CustomAttributes.Count; i++) {
                var attrib = prop.CustomAttributes[i];
                x ^= attrib.CalculateHashCode();
            }
            x ^= (int)prop.Attributes * 2663;

            if (prop.GetMethod != null) x ^= prop.GetMethod.CalculateHashCode();
            if (prop.SetMethod != null) x ^= prop.SetMethod.CalculateHashCode();

            return x;
        }

        public static int CalculateHashCode(this IMemberDefinition member) {
            if (member is MethodDefinition) return CalculateHashCode((MethodDefinition)member);
            if (member is FieldDefinition) return CalculateHashCode((FieldDefinition)member);
            if (member is PropertyDefinition) return CalculateHashCode((PropertyDefinition)member);
            throw new InvalidOperationException($"Unsupported IMemberDefinition in CalculateHashCode: {member?.GetType().Name ?? "<null>"}");
        }
    }
}
