using System;
using Mono.Cecil;

namespace SemiPatch {
    public static partial class Extensions {
        public static FieldReference GetFieldRef(this TypeDefinition type, string sig_str) {
            FieldDefinition field = null;
            for (var i = 0; i < type.Fields.Count; i++) {
                var search_field = type.Fields[i];
                if (new Signature(search_field) == sig_str) {
                    field = search_field;
                    break;
                }
            }
            if (type is GenericInstanceType) {
                var generic_field = new FieldReference(field.Name, field.FieldType, type);
                return generic_field;
            }
            if (field == null) throw new Exception($"QuickCecil: failed to locate field '{sig_str}'");
            return field;
        }

        public static FieldDefinition GetFieldDef(this TypeDefinition type, string sig_str) {
            return (FieldDefinition)GetFieldRef(type, sig_str);
        }

        public static MethodReference GetMethodRef(this TypeReference type, string sig_str) {
            MethodDefinition method = null;
            var r = type.Resolve();
            for (var i = 0; i < r.Methods.Count; i++) {
                var search_method = r.Methods[i];
                if (new Signature(search_method) == sig_str) {
                    method = search_method;
                    break;
                }
            }
            if (type is GenericInstanceType) {
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
            if (method == null) throw new Exception($"QuickCecil: failed to locate method '{sig_str}'");
            return method;
        }

        public static MethodDefinition GetMethodDef(this TypeDefinition type, string sig_str) {
            return (MethodDefinition)GetMethodRef(type, sig_str);
        }

        public static PropertyReference GetPropertyRef(this TypeReference type, string sig_str) {
            var r = type.Resolve();
            for (var i = 0; i < r.Properties.Count; i++) {
                if (new Signature(r.Properties[i]) == sig_str) return r.Properties[i];
            }
            throw new Exception($"QuickCecil: failed to locate property '{sig_str}'");
        }

        public static PropertyDefinition GetPropertyDef(this TypeDefinition type, string sig_str) {
            return (PropertyDefinition)GetPropertyRef(type, sig_str);
        }

        public static GenericInstanceMethod MakeGeneric(this MethodDefinition method, params TypeReference[] type_params) {
            var inst = new GenericInstanceMethod(method);
            for (var i = 0; i < method.GenericParameters.Count; i++) {
                inst.GenericArguments.Add(type_params[i]);
            }
            return inst;
        }

        public static GenericInstanceType MakeGeneric(this TypeDefinition type, params TypeReference[] type_params) {
            var inst = new GenericInstanceType(type);
            for (var i = 0; i < type.GenericParameters.Count; i++) {
                inst.GenericArguments.Add(type_params[i]);
            }
            return inst;
        }
    }
}
