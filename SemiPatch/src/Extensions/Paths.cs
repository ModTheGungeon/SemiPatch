using System;
using Mono.Cecil;

namespace SemiPatch {
    public static partial class Extensions {
        public static MethodPath ToPath(this MethodDefinition self, bool skip_first_arg = false, string forced_name = null) {
            return new MethodPath(self, skip_first_arg: skip_first_arg, forced_name: forced_name);
        }

        public static MethodPath ToPath(this System.Reflection.MethodBase self, bool skip_first_arg = false, string forced_name = null) {
            return new MethodPath(self, skip_first_arg: skip_first_arg, forced_name: forced_name);
        }

        public static PropertyPath ToPropertyPathFromGetter(this MethodDefinition self, string prop_name) {
            var sig = self.BuildPropertySignatureFromGetter(prop_name);
            return new PropertyPath(new Signature(sig, prop_name), self.DeclaringType);
        }

        public static PropertyPath ToPropertyPathFromSetter(this MethodDefinition self, string prop_name, bool skip_first_arg = false) {
            var sig = self.BuildPropertySignatureFromSetter(prop_name, skip_first_arg);
            return new PropertyPath(new Signature(sig, prop_name), self.DeclaringType);
        }

        public static FieldPath ToPath(this FieldDefinition self, string forced_name = null) {
            return new FieldPath(self, forced_name: forced_name);
        }

        public static FieldPath ToPath(this System.Reflection.FieldInfo self, string forced_name = null) {
            return new FieldPath(self, forced_name: forced_name);
        }

        public static PropertyPath ToPath(this PropertyDefinition self, string forced_name = null) {
            return new PropertyPath(self, forced_name: forced_name);
        }

        public static MemberPath ToPathGeneric<T>(this T member)
        where T : class, IMemberDefinition {
            if (member is MethodDefinition) return ToPath((MethodDefinition)(object)member);
            if (member is FieldDefinition) return ToPath((FieldDefinition)(object)member);
            if (member is PropertyDefinition) return ToPath((PropertyDefinition)(object)member);
            throw new InvalidOperationException($"Unsupported IMemberDefinition in ToPath: {member?.GetType().Name ?? "<null>"}");
        }

        public static TypePath ToPath(this TypeDefinition type) {
            return new TypePath(type);
        }

        public static TypePath ToPath(this Type type) {
            return new TypePath(type);
        }
    }
}
