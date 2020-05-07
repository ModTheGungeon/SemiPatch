using System;
using Mono.Cecil;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SemiPatch {
    /// <summary>
    /// Object containing data about a single patch of a type. See:
    /// <see cref="PatchMemberData{,}"/> for information about patches of type
    /// members.
    /// </summary>
    public class PatchTypeData {
        /// <summary>
        /// The type object within the target assembly that this patch wants
        /// to change. Unlike members, this will never be <code>null</code>,
        /// as it's not possible (and illogical) to use the <code>Insert</code>
        /// attribute on patch types (you can simply add the new type).
        /// </summary>
        public TypeDefinition TargetType;

        /// <summary>
        /// The type object within the patch assembly that contains members
        /// outlining how to patch the target type.
        /// </summary>
        public TypeDefinition PatchType;

        /// <summary>
        /// Fully qualified name of the assembly that <see cref="PatchType"/>
        /// comes from.
        /// </summary>
        public string PatchModuleName;

        /// <summary>
        /// List of objects representing method patches.
        /// </summary>
        public IList<PatchMethodData> Methods;

        /// <summary>
        /// List of objects representing field patches.
        /// </summary>
        public IList<PatchFieldData> Fields;

        /// <summary>
        /// List of objects representing property patches.
        /// </summary>
        public IList<PatchPropertyData> Properties;

        /// <summary>
        /// List of objects representing method injections.
        /// </summary>
        public IList<PatchInjectData> Injections;

        public PatchTypeData(TypeDefinition target, TypeDefinition patch) {
            TargetType = target;
            PatchType = patch;
            PatchModuleName = patch.Module.Assembly.FullName;
            Methods = new List<PatchMethodData>();
            Fields = new List<PatchFieldData>();
            Properties = new List<PatchPropertyData>();
            Injections = new List<PatchInjectData>();
        }

        public string ToString(string indent) {
            var s = new StringBuilder();
            s.Append(indent);
            s.Append("Target Type: ").Append(TargetType.FullName).Append("\n");
            s.Append(indent);
            s.Append("Patch Type: ").Append(PatchType.FullName).Append("\n");
            s.Append(indent);
            s.Append("Methods:");
            s.Append("\n");
            for (var i = 0; i < Methods.Count; i++) {
                s.Append(Methods[i].ToString(indent + "\t"));
                if (i < Methods.Count - 1) s.Append("\n\n");
            }
            s.Append("\n").Append(indent);
            s.Append("Fields:");
            s.Append("\n");
            for (var i = 0; i < Fields.Count; i++) {
                s.Append(Fields[i].ToString(indent + "\t"));
                if (i < Fields.Count - 1) s.Append("\n\n");
            }
            s.Append("\n").Append(indent);
            s.Append("Properties:");
            s.Append("\n");
            for (var i = 0; i < Properties.Count; i++) {
                s.Append(Properties[i].ToString(indent + "\t"));
                if (i < Properties.Count - 1) s.Append("\n\n");
            }
            s.Append("\n").Append(indent);
            s.Append("Injections:");
            s.Append("\n");
            for (var i = 0; i < Injections.Count; i++) {
                s.Append(Injections[i].ToString(indent + "\t"));
                if (i < Injections.Count - 1) s.Append("\n\n");
            }
            return s.ToString();
        }

        public override string ToString() => ToString("");

        public void Serialize(BinaryWriter writer) {
            writer.Write(TargetType.FullName);
            writer.Write(PatchType.FullName);
            writer.Write(PatchModuleName);
            writer.Write(Methods.Count);
            foreach (var method in Methods) method.Serialize(writer);
            writer.Write(Fields.Count);
            foreach (var field in Fields) field.Serialize(writer);
            writer.Write(Properties.Count);
            foreach (var prop in Properties) prop.Serialize(writer);
            writer.Write(Injections.Count);
            foreach (var inject in Injections) inject.Serialize(writer);
        }

        public static PatchTypeData Deserialize(ModuleDefinition target_module, IDictionary<string, ModuleDefinition> patch_module_map, BinaryReader reader) {
            var target_type_name = reader.ReadString();
            var patch_type_name = reader.ReadString();
            var patch_module_name = reader.ReadString();
            ModuleDefinition patch_module;
            if (!patch_module_map.TryGetValue(patch_module_name, out patch_module)) {
                throw new Exception($"Deserialization error: Failed to acquire patch module '{patch_module_name}' while deserializing type '{patch_type_name}'");
            }

            TypeDefinition target_type = null;
            TypeDefinition patch_type = null;

            foreach (var type in target_module.Types) {
                if (type.FullName == target_type_name) {
                    target_type = type;
                    break;
                }
            }

            if (target_type == null) throw new Exception($"Deserialization error: Failed to find type '{target_type_name}' in target module '{target_module.Name}'");

            foreach (var type in patch_module.Types) {
                if (type.FullName == patch_type_name) {
                    patch_type = type;
                    break;
                }
            }

            if (patch_type == null) throw new Exception($"Deserialization error: Failed to find type '{patch_type_name}' in patch module '{patch_module.Name}'");

            var data = new PatchTypeData(target_type, patch_type);

            var method_count = reader.ReadInt32();

            for (var i = 0; i < method_count; i++) {
                var method = PatchMethodData.Deserialize(target_type, patch_type, reader);
                data.Methods.Add(method);
            }

            var field_count = reader.ReadInt32();

            for (var i = 0; i < field_count; i++) {
                var field = PatchFieldData.Deserialize(target_type, patch_type, reader);
                data.Fields.Add(field);
            }

            var prop_count = reader.ReadInt32();

            for (var i = 0; i < prop_count; i++) {
                var prop = PatchPropertyData.Deserialize(target_type, patch_type, reader);
                data.Properties.Add(prop);
            }

            var inject_count = reader.ReadInt32();

            for (var i = 0; i < inject_count; i++) {
                var inject = PatchInjectData.Deserialize(target_type, patch_type, reader);
                data.Injections.Add(inject);
            }


            return data;
        }

        public override int GetHashCode() {
            var x = TargetType.BuildPrefixedSignature().GetHashCode();
            x ^= PatchType.BuildPrefixedSignature().GetHashCode();
            x ^= PatchModuleName.GetHashCode();
            x ^= Methods.Count;
            for (var i = 0; i < Methods.Count; i++) {
                x ^= Methods[i].GetHashCode();
            }
            x ^= Fields.Count;
            for (var i = 0; i < Fields.Count; i++) {
                x ^= Fields[i].GetHashCode();
            }
            x ^= Properties.Count;
            for (var i = 0; i < Properties.Count; i++) {
                x ^= Properties[i].GetHashCode();
            }
            x ^= Injections.Count;
            for (var i = 0; i < Properties.Count; i++) {
                x ^= Injections[i].GetHashCode();
            }
            return x;
        }

        public bool Equals(PatchTypeData other) {
            return GetHashCode() == other.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj == null || !(obj is PatchTypeData)) return false;
            return Equals((PatchTypeData)obj);
        }

        public static bool operator ==(PatchTypeData a, PatchTypeData b) {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        public static bool operator !=(PatchTypeData a, PatchTypeData b) {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return !a.Equals(b);
        }
    }
}
