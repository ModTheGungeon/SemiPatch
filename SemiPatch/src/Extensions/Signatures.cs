using System;
using System.Text;
using Mono.Cecil;

namespace SemiPatch {
    public static partial class Extensions {
        public static string BuildSignature(this MethodReference method, bool skip_first_arg = false, string forced_name = null, string forced_first_arg = null, string forced_return_type = null) {
            var s = new StringBuilder();
            var name = forced_name ?? method.Name;
            if (method.Name != ".ctor") {
                if (forced_return_type != null) s.Append(forced_return_type);
                else s.Append(method.ReturnType.BuildSignature());
                s.Append(" ");
            } else {
                name = "<ctor>";
            }
            s.Append(name);
            var i = 0;
            if (method.HasGenericParameters) {
                s.Append("<");
                var generic_arg_count = method.GenericParameters.Count;
                i = 0;
                foreach (var generic_param in method.GenericParameters) {
                    s.Append(generic_param.BuildSignature());
                    if (i < generic_arg_count - 1) s.Append(", ");
                    i += 1;
                }
                s.Append(">");
            }
            s.Append("(");
            var param_count = method.Parameters.Count;
            if (forced_first_arg != null) {
                s.Append(forced_first_arg);
                if (param_count > 0) s.Append(", ");
            }
            i = 0;
            foreach (var param in method.Parameters) {
                if (i == 0 && skip_first_arg) {
                    i = 1;
                    continue;
                }
                s.Append(param.ParameterType.BuildSignature());
                //s.Append(" ");
                //s.Append("arg");
                //s.Append(i - (skip_first_arg ? 1 : 0));
                if (i < param_count - 1) s.Append(", ");
                i += 1;
            }

            s.Append(")");
            return s.ToString();
        }

        public static string BuildSignature(this System.Reflection.MethodBase method, bool skip_first_arg = false, string forced_name = null, string forced_first_arg = null) {
            var s = new StringBuilder();
            var name = forced_name ?? method.Name;
            if (method is System.Reflection.MethodInfo method_info) {
                s.Append(method_info.ReturnType.BuildSignature());
                s.Append(" ");
            } else {
                if (method.Name == ".ctor") {
                    name = "<ctor>";
                }
            }

            s.Append(name);
            var i = 0;
            if (method.IsGenericMethod) {
                var generic_params = method.GetGenericArguments();
                if (generic_params.Length > 0) {
                    s.Append("<");
                    var generic_arg_count = generic_params.Length;
                    i = 0;
                    foreach (var generic_param in generic_params) {
                        s.Append(generic_param.BuildSignature());
                        if (i < generic_arg_count - 1) s.Append(", ");
                        i += 1;
                    }
                    s.Append(">");
                }
            }
            s.Append("(");
            var parameters = method.GetParameters();
            var param_count = parameters.Length;
            if (forced_first_arg != null) {
                s.Append(forced_first_arg);
                if (param_count > 0) s.Append(", ");
            }
            i = 0;
            foreach (var param in parameters) {
                if (i == 0 && skip_first_arg) {
                    i = 1;
                    continue;
                }
                s.Append(param.ParameterType.BuildSignature());
                if (i < param_count - 1) s.Append(", ");
                i += 1;
            }

            s.Append(")");
            return s.ToString();
        }


        public static string BuildPrefixedSignature(this MethodReference method, bool skip_first_arg = false, string forced_name = null, string forced_first_arg = null) {
            return method.DeclaringType.PrefixSignature(method.BuildSignature(skip_first_arg, forced_name, forced_first_arg));
        }

        public static string BuildPropertySignatureFromSetter(this MethodReference method, string prop_name, bool skip_first_arg = false) {
            var s = new StringBuilder();
            s.Append(method.Parameters[skip_first_arg ? 1 : 0].ParameterType.BuildSignature());
            s.Append(" ");
            s.Append(prop_name);
            return s.ToString();
        }

        public static string BuildPropertySignatureFromGetter(this MethodReference method, string prop_name) {
            var s = new StringBuilder();
            s.Append(method.ReturnType.BuildSignature());
            s.Append(" ");
            s.Append(prop_name);
            return s.ToString();
        }

        public static string BuildSignature(this PropertyReference prop, string forced_name = null, bool include_get_set = false) {
            var s = new StringBuilder();
            s.Append(prop.PropertyType.BuildSignature());
            s.Append(" ");
            s.Append(forced_name ?? prop.Name);
            if (include_get_set) {
                s.Append(" {");
                var resolved = prop.Resolve();
                if (resolved.GetMethod != null && resolved.SetMethod != null) {
                    s.Append(" get; set;");
                } else if (resolved.GetMethod != null) {
                    s.Append(" get;");
                } else if (resolved.SetMethod != null) {
                    s.Append(" set;");
                }
                s.Append(" }");
            }

            return s.ToString();
        }

        public static string BuildSignature(this TypeReference type) {
            if (type.Namespace == "System") {
                if (type.Name == "Void") return "void";
                if (type.Name == "String") return "string";
                if (type.Name == "Int32") return "int";
                if (type.Name == "UInt32") return "uint";
                if (type.Name == "Int64") return "long";
                if (type.Name == "UInt64") return "ulong";
                if (type.Name == "Int16") return "short";
                if (type.Name == "UInt16") return "ushort";
                if (type.Name == "Char") return "char";
                if (type.Name == "Boolean") return "bool";
            }

            var s = new StringBuilder();
            var name = type.FullName;
            var grave_accent_index = name.IndexOf('`');
            if (grave_accent_index > -1) {
                name = name.Substring(0, grave_accent_index);
            }
            s.Append(name);
            if (type is GenericInstanceType) {
                var inst = (GenericInstanceType)type;
                if (inst.HasGenericArguments) {
                    s.Append("<");
                    for (var i = 0; i < inst.GenericArguments.Count; i++) {
                        var generic_param = inst.GenericArguments[i];
                        s.Append(generic_param.BuildSignature());
                        if (i < inst.GenericArguments.Count - 1) s.Append(", ");
                    }
                    s.Append(">");
                }
            } else {
                if (type.HasGenericParameters) {
                    s.Append("<");
                    for (var i = 0; i < type.GenericParameters.Count; i++) {
                        var generic_param = type.GenericParameters[i];
                        s.Append(generic_param.BuildSignature());
                        if (i < type.GenericParameters.Count - 1) s.Append(", ");
                    }
                    s.Append(">");
                }
            }
            return s.ToString();
        }

        public static string BuildSignature(this Type type) {
            if (type.Namespace == "System") {
                if (type.Name == "Void") return "void";
                if (type.Name == "String") return "string";
                if (type.Name == "Int32") return "int";
                if (type.Name == "UInt32") return "uint";
                if (type.Name == "Int64") return "long";
                if (type.Name == "UInt64") return "ulong";
                if (type.Name == "Int16") return "short";
                if (type.Name == "UInt16") return "ushort";
                if (type.Name == "Char") return "char";
                if (type.Name == "Boolean") return "bool";
            }

            var s = new StringBuilder();
            var name = type.FullName ?? type.Name;
            var grave_accent_index = name.IndexOf('`');
            if (grave_accent_index > -1) {
                name = name.Substring(0, grave_accent_index);
            }
            s.Append(name);
            var generic_params = type.GetGenericArguments();
            if (generic_params.Length > 0) {
                s.Append("<");
                for (var i = 0; i < generic_params.Length; i++) {
                    var generic_param = generic_params[i];
                    s.Append(generic_param.BuildSignature());
                    if (i < generic_params.Length - 1) s.Append(", ");
                }
                s.Append(">");
            }
            return s.ToString();
        }

        public static string BuildPrefixedSignature(this TypeReference type) {
            if (type.DeclaringType == null) return type.BuildSignature();
            return type.DeclaringType.PrefixSignature(type.BuildSignature());
        }

        public static string BuildSignature(this FieldReference field, string forced_name = null) {
            var s = new StringBuilder();
            s.Append(field.FieldType.BuildSignature());
            s.Append(" ");
            s.Append(forced_name ?? field.Name);
            return s.ToString();
        }

        public static string BuildSignature(this System.Reflection.FieldInfo field, string forced_name = null) {
            var s = new StringBuilder();
            s.Append(field.FieldType.BuildSignature());
            s.Append(" ");
            s.Append(forced_name ?? field.Name);
            return s.ToString();
        }

        public static string BuildPrefixedSignature(this FieldReference field) {
            return field.DeclaringType.PrefixSignature(field.BuildSignature());
        }

        public static string PrefixSignature(this TypeReference type, string sig) {
            return $"[{type.BuildSignature()}] {sig}";
        }
    }
}
