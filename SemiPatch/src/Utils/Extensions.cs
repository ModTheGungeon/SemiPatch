using System;
using System.IO;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SemiPatch {
    /// <summary>
    /// Extension methods used all over the place in SemiPatch.
    /// TODO: document all these?
    /// </summary>
    public static class Extensions {
        public static bool IsSame(this TypeReference a, TypeReference b) {
            a = a.Resolve() ?? a;
            b = b.Resolve() ?? b;

            if (ReferenceEquals(a, b)) return true;
            if (a == b) return true;

            if (a.Name != b.Name) return false;
            if (a.Namespace != b.Namespace) return false;

            if (a is TypeDefinition && b is TypeDefinition) {
                var ad = a as TypeDefinition;
                var bd = b as TypeDefinition;

                if (ad.Methods.Count != bd.Methods.Count) return false;
                if (ad.GenericParameters.Count != bd.GenericParameters.Count) return false;
                if (ad.Fields.Count != bd.Fields.Count) return false;
                if (ad.Properties.Count != bd.Properties.Count) return false;
                if (ad.CustomAttributes.Count != bd.CustomAttributes.Count) return false;
            }

            return a.Scope.IsSame(b.Scope);
        }

        public static bool IsSame(this IMetadataScope a, IMetadataScope b) {
            string a_name = null;
            if (a is ModuleDefinition) {
                a_name = ((ModuleDefinition)a).Assembly.Name.FullName;
            } else if (a is AssemblyNameReference) a_name = ((AssemblyNameReference)a).FullName;

            string b_name = null;
            if (b is ModuleDefinition) {
                b_name = ((ModuleDefinition)b).Assembly.Name.FullName;
            } else if (b is AssemblyNameReference) b_name = ((AssemblyNameReference)b).FullName;

            return a_name == b_name;
        }

        public static string BuildSignature(this MethodReference method, bool skip_first_arg = false, string forced_name = null, string forced_first_arg = null) {
            var s = new StringBuilder();
            var name = forced_name ?? method.Name;
            if (method.Name != ".ctor") {
                s.Append(method.ReturnType.BuildSignature());
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
                s.Append(" ");
                s.Append("arg");
                s.Append(i - (skip_first_arg ? 1 : 0));
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
            var name = type.Name;
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

        public static string BuildPrefixedSignature(this FieldReference field) {
            return field.DeclaringType.PrefixSignature(field.BuildSignature());
        }

        public static string PrefixSignature(this TypeReference type, string sig) {
            return $"[{type.BuildSignature()}] {sig}";
        }

        // Based on https://github.com/jbevain/cecil/blob/b2958c0d8473aa6eaa6b3bd7ad19d5f643a97804/Mono.Cecil.Cil/CodeWriter.cs#L435
        // Modified to use publicly accessible fields and types
        public static int ComputeStackDelta(this Instruction instruction) {
            var stack_size = 0;
            switch (instruction.OpCode.FlowControl) {
            case FlowControl.Call: {
                    var method = (IMethodSignature)instruction.Operand;
                    // pop 'this' argument
                    if ((method.HasThis && !method.ExplicitThis) && instruction.OpCode.Code != Code.Newobj)
                        stack_size--;
                    // pop normal arguments
                    if (method.HasParameters)
                        stack_size -= method.Parameters.Count;
                    // pop function pointer
                    if (instruction.OpCode.Code == Code.Calli)
                        stack_size--;
                    // push return value
                    if (method.ReturnType.MetadataType != MetadataType.Void || instruction.OpCode.Code == Code.Newobj)
                        stack_size++;
                    break;
                }
            default:
                ComputePopDelta(instruction.OpCode.StackBehaviourPop, ref stack_size);
                ComputePushDelta(instruction.OpCode.StackBehaviourPush, ref stack_size);
                break;
            }
            return stack_size;
        }

        static void ComputePopDelta(StackBehaviour pop_behavior, ref int stack_size) {
            switch (pop_behavior) {
            case StackBehaviour.Popi:
            case StackBehaviour.Popref:
            case StackBehaviour.Pop1:
                stack_size--;
                break;
            case StackBehaviour.Pop1_pop1:
            case StackBehaviour.Popi_pop1:
            case StackBehaviour.Popi_popi:
            case StackBehaviour.Popi_popi8:
            case StackBehaviour.Popi_popr4:
            case StackBehaviour.Popi_popr8:
            case StackBehaviour.Popref_pop1:
            case StackBehaviour.Popref_popi:
                stack_size -= 2;
                break;
            case StackBehaviour.Popi_popi_popi:
            case StackBehaviour.Popref_popi_popi:
            case StackBehaviour.Popref_popi_popi8:
            case StackBehaviour.Popref_popi_popr4:
            case StackBehaviour.Popref_popi_popr8:
            case StackBehaviour.Popref_popi_popref:
                stack_size -= 3;
                break;
            case StackBehaviour.PopAll:
                stack_size = 0;
                break;
            }
        }

        static void ComputePushDelta(StackBehaviour push_behaviour, ref int stack_size) {
            switch (push_behaviour) {
            case StackBehaviour.Push1:
            case StackBehaviour.Pushi:
            case StackBehaviour.Pushi8:
            case StackBehaviour.Pushr4:
            case StackBehaviour.Pushr8:
            case StackBehaviour.Pushref:
                stack_size++;
                break;
            case StackBehaviour.Push1_push1:
                stack_size += 2;
                break;
            }
        }

        public static void WriteNullable(this BinaryWriter writer, string obj) {
            if (obj == null) writer.Write((byte)0);
            else {
                writer.Write((byte)1); 
                writer.Write(obj);
            }
        }

        public static string ReadNullableString(this BinaryReader reader) {
            if (reader.ReadByte() == (byte)0) {
                return null;
            }
            return reader.ReadString();
        }

        public static void Write(this BinaryWriter writer, MemberPath path) {
            path.Serialize(writer);
        }

        public static MethodPath ReadMethodPath(this BinaryReader reader) {
            return MethodPath.Deserialize(reader);
        }

        public static FieldPath ReadFieldPath(this BinaryReader reader) {
            return FieldPath.Deserialize(reader);
        }

        public static PropertyPath ReadPropertyPath(this BinaryReader reader) {
            return PropertyPath.Deserialize(reader);
        }

        public static MethodReference MakeReference(this MethodDefinition self) {
            var new_inst = new MethodReference(self.Name, self.ReturnType, self.DeclaringType);
            for (var i = 0; i < self.Parameters.Count; i++) {
                var param = self.Parameters[i];
                new_inst.Parameters.Add(new ParameterDefinition(
                    param.Name,
                    param.Attributes,
                    param.ParameterType
                ));
            }
            for (var i = 0; i < self.GenericParameters.Count; i++) {
                var param = self.GenericParameters[i];
                new_inst.GenericParameters.Add(new GenericParameter(param.Name, new_inst));
            }
            new_inst.HasThis = self.HasThis;
            new_inst.ExplicitThis = self.ExplicitThis;
            return new_inst;
        }

        public static MethodPath ToPath(this MethodDefinition self, bool skip_first_arg = false, string forced_name = null) {
            return new MethodPath(self, skip_first_arg: skip_first_arg, forced_name: forced_name);
        }

        public static PropertyPath ToPropertyPathFromGetter(this MethodDefinition self, string prop_name) {
            var sig = self.BuildPropertySignatureFromGetter(prop_name);
            return new PropertyPath(new Signature(sig), self.DeclaringType);
        }

        public static PropertyPath ToPropertyPathFromSetter(this MethodDefinition self, string prop_name, bool skip_first_arg = false) {
            var sig = self.BuildPropertySignatureFromSetter(prop_name, skip_first_arg);
            return new PropertyPath(new Signature(sig), self.DeclaringType);
        }

        public static FieldPath ToPath(this FieldDefinition self, string forced_name = null) {
            return new FieldPath(self, forced_name: forced_name);
        }

        public static PropertyPath ToPath(this PropertyDefinition self, string forced_name = null) {
            return new PropertyPath(self, forced_name: forced_name);
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
            x ^= (int)method.RVA * 4547;
            x ^= (int)method.ImplAttributes * 6983;
            x ^= (int)method.SemanticsAttributes * 9811;

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

        public static PathType ToPath<MemberDefinitionType, PathType>(this MemberDefinitionType member)
        where MemberDefinitionType : class, IMemberDefinition
        where PathType : MemberPath<MemberDefinitionType>{
            if (member is MethodDefinition) return (PathType)(object)ToPath((MethodDefinition)(object)member);
            if (member is FieldDefinition) return (PathType)(object)ToPath((FieldDefinition)(object)member);
            if (member is PropertyDefinition) return (PathType)(object)ToPath((PropertyDefinition)(object)member);
            throw new InvalidOperationException($"Unsupported IMemberDefinition in ToPath: {member?.GetType().Name ?? "<null>"}");
        }

        public static MemberPath ToPathInterface(this IMemberDefinition member) {
            if (member is MethodDefinition) return ToPath((MethodDefinition)(object)member);
            if (member is FieldDefinition) return ToPath((FieldDefinition)(object)member);
            if (member is PropertyDefinition) return ToPath((PropertyDefinition)(object)member);
            throw new InvalidOperationException($"Unsupported IMemberDefinition in ToPathInterface: {member?.GetType().Name ?? "<null>"}");
        }

        public static TypePath ToPath(this TypeDefinition type) {
            return new TypePath(type);
        }
    }
}
