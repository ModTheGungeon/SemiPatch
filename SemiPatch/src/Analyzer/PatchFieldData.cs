using System;
using System.IO;
using System.Text;
using Mono.Cecil;

namespace SemiPatch {
    public class PatchFieldData {
        public FieldDefinition TargetField;
        public FieldDefinition PatchField;
        public FieldPath TargetPath;
        public FieldPath PatchPath;
        public bool IsInsert = false;
        public bool ExplicitlyIgnored = false;
        public string AliasedName = null;
        public bool Proxy;

        public PatchFieldData(FieldPath target_path, FieldPath patch_path, FieldDefinition patch_field, FieldDefinition target_field = null) {
            TargetPath = target_path;
            PatchPath = patch_path;
            TargetField = target_field;
            PatchField = patch_field;
        }

        public string ToString(string indent) {
            var s = new StringBuilder();
            s.Append(indent);
            if (IsInsert) {
                s.Append("Inserted ");
            } else if (Proxy) {
                s.Append("Proxy ");
            } else if (!ExplicitlyIgnored) {
                s.Append("Target Field: ").Append(TargetField.FullName).Append("\n");
                s.Append(indent);
                s.Append("Patch ");
            }
            s.Append("Field: ").Append(PatchField.FullName);
            if (AliasedName != null) {
                s.Append("\n").Append(indent);
                s.Append("Target Name: ").Append(AliasedName);
            }
            if (TargetPath != null) {
                s.Append("\n").Append(indent);
                s.Append("Target Signature: ").Append(TargetPath);
            }
            s.Append("\n").Append(indent);
            s.Append("Patch Signature: ").Append(PatchPath);
            if (ExplicitlyIgnored) {
                s.Append("\n").Append(indent);
                s.Append("Explicitly Ignored");
            }
            return s.ToString();
        }

        public void Serialize(BinaryWriter writer) {
            writer.Write(IsInsert);
            writer.Write(TargetPath);
            writer.Write(PatchPath);
            writer.Write(ExplicitlyIgnored);
            writer.WriteNullable(AliasedName);
            writer.Write(Proxy);
        }

        public static PatchFieldData Deserialize(TypeDefinition target_type, TypeDefinition patch_type, BinaryReader reader) {
            var insert = reader.ReadBoolean();
            var target_path = reader.ReadFieldPath();
            var patch_path = reader.ReadFieldPath();
            var explicitly_ignored = reader.ReadBoolean();
            var aliased_name = reader.ReadNullableString();
            var proxy = reader.ReadBoolean();

            FieldDefinition patch = null;
            FieldDefinition target = null;

            foreach (var field in patch_type.Fields) {
                var candidate_sig = new Signature(field);
                if (candidate_sig == patch_path.Signature) {
                    patch = field;
                    break;
                }
            }
            if (patch == null) throw new Exception($"Deserialization error: Failed to find patch field in {patch_type.FullName} with signature '{patch_path}'");

            if (explicitly_ignored || insert || proxy) return new PatchFieldData(target_path, patch_path, patch) { Proxy = proxy, ExplicitlyIgnored = explicitly_ignored };

            foreach (var field in target_type.Fields) {
                var candidate_sig = new Signature(field);
                if (candidate_sig == target_path.Signature) {
                    target = field;
                    break;
                }
            }
            if (target == null) throw new Exception($"Deserialization error: Failed to find target field in {target_type.FullName} with signature '{target_path}' - patch was not marked as Insert");

            return new PatchFieldData(target_path, patch_path, patch, target) { ExplicitlyIgnored = explicitly_ignored, AliasedName = aliased_name, Proxy = proxy };
        }

        public override int GetHashCode() {
            var x = TargetPath?.GetHashCode() ?? 1047294;
            x ^= PatchPath.GetHashCode();
            x ^= IsInsert ? 54497 : 102013;
            x ^= ExplicitlyIgnored ? 60679 : 49639;
            x ^= AliasedName?.GetHashCode() ?? 101467;
            x ^= Proxy ? 31567 : 67021;

            return x;
        }

        public bool Equals(PatchFieldData other) {
            return GetHashCode() == other.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj == null || !(obj is PatchFieldData)) return false;
            return Equals((PatchFieldData)obj);
        }

        public static bool operator ==(PatchFieldData a, PatchFieldData b) {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        public static bool operator !=(PatchFieldData a, PatchFieldData b) {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return !a.Equals(b);
        }
    }
}
