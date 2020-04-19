using System;
using System.IO;
using System.Text;
using Mono.Cecil;

namespace SemiPatch {
    public class PatchFieldData {
        public FieldDefinition TargetField;
        public FieldDefinition PatchField;
        public string TargetSignature;
        public string PatchSignature;
        public bool IsInsert = false;
        public bool ExplicitlyIgnored = false;
        public string AliasedName = null;
        public bool Proxy;

        public PatchFieldData(string target_signature, string patch_signature, FieldDefinition patch_field, FieldDefinition target_field = null) {
            TargetSignature = target_signature;
            PatchSignature = patch_signature;
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
            if (TargetSignature != null) {
                s.Append("\n").Append(indent);
                s.Append("Target Signature: ").Append(TargetSignature);
            }
            s.Append("\n").Append(indent);
            s.Append("Patch Signature: ").Append(PatchSignature);
            if (ExplicitlyIgnored) {
                s.Append("\n").Append(indent);
                s.Append("Explicitly Ignored");
            }
            return s.ToString();
        }

        public void Serialize(BinaryWriter writer) {
            writer.Write(IsInsert);
            writer.Write(TargetSignature);
            writer.Write(PatchSignature);
            writer.Write(ExplicitlyIgnored);
            writer.WriteNullable(AliasedName);
            writer.Write(Proxy);
        }

        public static PatchFieldData Deserialize(TypeDefinition target_type, TypeDefinition patch_type, BinaryReader reader) {
            var insert = reader.ReadBoolean();
            var target_sig = reader.ReadString();
            var patch_sig = reader.ReadString();
            var explicitly_ignored = reader.ReadBoolean();
            var aliased_name = reader.ReadNullableString();
            var proxy = reader.ReadBoolean();

            FieldDefinition patch = null;
            FieldDefinition target = null;

            foreach (var field in patch_type.Fields) {
                var candidate_sig = field.BuildSignature();
                if (candidate_sig == patch_sig) {
                    patch = field;
                    break;
                }
            }
            if (patch == null) throw new Exception($"Deserialization error: Failed to find patch field in {patch_type.FullName} with signature '{patch_sig}'");

            if (explicitly_ignored || insert || proxy) return new PatchFieldData(target_sig, patch_sig, patch) { Proxy = proxy, ExplicitlyIgnored = explicitly_ignored };

            foreach (var field in target_type.Fields) {
                var candidate_sig = field.BuildSignature();
                if (candidate_sig == target_sig) {
                    target = field;
                    break;
                }
            }
            if (target == null) throw new Exception($"Deserialization error: Failed to find target field in {target_type.FullName} with signature '{target_sig}' - patch was not marked as Insert");

            return new PatchFieldData(target_sig, patch_sig, patch, target) { ExplicitlyIgnored = explicitly_ignored, AliasedName = aliased_name, Proxy = proxy };
        }
    }
}
