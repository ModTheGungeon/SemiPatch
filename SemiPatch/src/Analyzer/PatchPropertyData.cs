using System;
using System.IO;
using System.Text;
using Mono.Cecil;

namespace SemiPatch {
    public class PatchPropertyData {
        // these are always inserts
        // property patching is done through get_ and set_ methods

        public string TargetSignature;
        public string PatchSignature;
        public PropertyReference PatchProperty;
        public string AliasedName;
        public bool ExplicitlyIgnored;
        public bool Proxy;

        public PatchPropertyData(string target_sig, string patch_sig, PropertyReference patch) {
            TargetSignature = target_sig;
            PatchSignature = patch_sig;
            PatchProperty = patch;
        }

        public string ToString(string indent) {
            var s = new StringBuilder();
            s.Append(indent);
            s.Append("Patch Property: ").Append(PatchProperty.FullName);
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
            writer.Write(TargetSignature);
            writer.Write(PatchSignature);
            writer.Write(ExplicitlyIgnored);
            writer.WriteNullable(AliasedName);
            writer.Write(Proxy);
        }

        public static PatchPropertyData Deserialize(TypeDefinition target_type, TypeDefinition patch_type, BinaryReader reader) {
            var target_sig = reader.ReadString();
            var patch_sig = reader.ReadString();
            var explicitly_ignored = reader.ReadBoolean();
            var aliased_name = reader.ReadNullableString();
            var proxy = reader.ReadBoolean();

            PropertyDefinition patch = null;

            foreach (var prop in patch_type.Properties) {
                var candidate_sig = prop.BuildSignature();
                if (candidate_sig == patch_sig) {
                    patch = prop;
                    break;
                }
            }
            if (patch == null) throw new Exception($"Deserialization error: Failed to find patch property in {patch_type.FullName} with signature '{patch_sig}'");

            return new PatchPropertyData(target_sig, patch_sig, patch) { ExplicitlyIgnored = explicitly_ignored, AliasedName = aliased_name, Proxy = proxy };
        }
    }
}
