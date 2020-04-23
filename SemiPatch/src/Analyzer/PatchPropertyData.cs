using System;
using System.IO;
using System.Text;
using Mono.Cecil;

namespace SemiPatch {
    public class PatchPropertyData {
        // these are always inserts, ignores or proxies
        // property patching is done through get_ and set_ methods

        public PropertyPath TargetPath;
        public PropertyPath PatchPath;
        public PropertyReference PatchProperty;
        public string AliasedName;
        public bool ExplicitlyIgnored;
        public bool Proxy;

        public PatchPropertyData(PropertyPath target_path, PropertyPath patch_path, PropertyReference patch) {
            TargetPath = target_path;
            PatchPath = patch_path;
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
            writer.Write(TargetPath);
            writer.Write(PatchPath);
            writer.Write(ExplicitlyIgnored);
            writer.WriteNullable(AliasedName);
            writer.Write(Proxy);
        }

        public static PatchPropertyData Deserialize(TypeDefinition target_type, TypeDefinition patch_type, BinaryReader reader) {
            var target_path = reader.ReadPropertyPath();
            var patch_path = reader.ReadPropertyPath();
            var explicitly_ignored = reader.ReadBoolean();
            var aliased_name = reader.ReadNullableString();
            var proxy = reader.ReadBoolean();

            PropertyDefinition patch = null;

            foreach (var prop in patch_type.Properties) {
                var candidate_sig = new Signature(prop);
                if (candidate_sig == patch_path.Signature) {
                    patch = prop;
                    break;
                }
            }
            if (patch == null) throw new Exception($"Deserialization error: Failed to find patch property in {patch_type.FullName} with signature '{patch_path}'");

            return new PatchPropertyData(target_path, patch_path, patch) { ExplicitlyIgnored = explicitly_ignored, AliasedName = aliased_name, Proxy = proxy };
        }

        public override int GetHashCode() {
            var x = TargetPath?.GetHashCode() ?? 58033;
            x ^= PatchPath.GetHashCode();
            x ^= ExplicitlyIgnored ? 68269 : 49373;
            x ^= AliasedName?.GetHashCode() ?? 57181;
            x ^= Proxy ? 24539 : 14111;

            return x;
        }

        public bool Equals(PatchPropertyData other) {
            return GetHashCode() == other.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj == null || !(obj is PatchPropertyData)) return false;
            return Equals((PatchPropertyData)obj);
        }

        public static bool operator ==(PatchPropertyData a, PatchPropertyData b) {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        public static bool operator !=(PatchPropertyData a, PatchPropertyData b) {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return !a.Equals(b);
        }
    }
}
