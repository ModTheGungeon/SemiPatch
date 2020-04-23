using System;
using Mono.Cecil;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SemiPatch {
    public class PatchMethodData {
        public MethodDefinition TargetMethod;
        public MethodDefinition PatchMethod;
        public MethodPath TargetPath;
        public MethodPath PatchPath;
        public bool ReceivesOriginal;
        public bool ExplicitlyIgnored;
        public string AliasedName;
        public bool Proxy;

        public PatchMethodData(MethodDefinition target, MethodDefinition patch, MethodPath target_path, MethodPath patch_path = null, bool receives_original = false, bool explicitly_ignored = false, string target_name = null) {
            TargetMethod = target;
            PatchMethod = patch;
            TargetPath = target_path;
            PatchPath = patch_path ?? target_path;
            ReceivesOriginal = receives_original;
            ExplicitlyIgnored = explicitly_ignored;
            AliasedName = target_name;
        }

        public PatchMethodData(MethodDefinition source, MethodPath target_path, MethodPath patch_path = null, bool receives_original = false, bool explicitly_ignored = false, string target_name = null) {
            PatchMethod = source;
            TargetPath = target_path;
            PatchPath = patch_path ?? target_path;
            ReceivesOriginal = receives_original;
            ExplicitlyIgnored = explicitly_ignored;
            AliasedName = target_name;
        }

        public bool IsInsert => TargetMethod == null;

        public string ToString(string indent) {
            var s = new StringBuilder();
            s.Append(indent);
            if (IsInsert) {
                s.Append("Inserted ");
            } else {
                s.Append("Target Method: ").Append(TargetMethod.FullName).Append("\n");
                s.Append(indent);
                s.Append("Patch ");
            }
            s.Append("Method: ").Append(PatchMethod.FullName);
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
            if (ReceivesOriginal) {
                s.Append("\n").Append(indent);
                s.Append("Receives Original");
            }
            if (ExplicitlyIgnored) {
                s.Append("\n").Append(indent);
                s.Append("Explicitly Ignored");
            }
            if (Proxy) {
                s.Append("\n").Append(indent);
                s.Append("Proxy");
            }
            return s.ToString();
        }

        public override string ToString() => ToString("");

        public void Serialize(BinaryWriter writer) {
            writer.Write(IsInsert);
            writer.Write(TargetPath);
            writer.Write(PatchPath);
            writer.Write(ReceivesOriginal);
            writer.Write(ExplicitlyIgnored);
            writer.WriteNullable(AliasedName);
            writer.Write(Proxy);
        }

        public static PatchMethodData Deserialize(TypeDefinition target_type, TypeDefinition patch_type, BinaryReader reader) {
            var insert = reader.ReadBoolean();
            var target_path = reader.ReadMethodPath();
            var patch_path = reader.ReadMethodPath();
            var receives_original = reader.ReadBoolean();
            var explicitly_ignored = reader.ReadBoolean();
            var aliased_name = reader.ReadNullableString();
            var proxy = reader.ReadBoolean();

            MethodDefinition patch = null;
            MethodDefinition target = null;

            foreach (var method in patch_type.Methods) {
                var candidate_sig = new Signature(method);
                if (candidate_sig == patch_path.Signature) {
                    patch = method;
                    break;
                }
            }
            if (patch == null) throw new Exception($"Deserialization error: Failed to find patch method in {patch_type.FullName} with signature '{patch_path}'");

            if (insert) return new PatchMethodData(patch, target_path) { Proxy = proxy };

            foreach (var method in target_type.Methods) {
                var candidate_sig = new Signature(method);
                if (candidate_sig == target_path.Signature) {
                    target = method;
                    break;
                }
            }
            if (target == null) throw new Exception($"Deserialization error: Failed to find target method in {target_type.FullName} with signature '{target_path}' - patch was not marked as Insert");

            return new PatchMethodData(target, patch, target_path, patch_path, receives_original, explicitly_ignored, aliased_name) { Proxy = proxy };
        }

        public override int GetHashCode() {
            var x = TargetPath?.GetHashCode() ?? 21409;
            x ^= PatchPath.GetHashCode();
            x ^= IsInsert ? 38911 : 3007;
            x ^= ReceivesOriginal ? 31861: 6157;
            x ^= ExplicitlyIgnored ? 52211 : 8773;
            x ^= AliasedName?.GetHashCode() ?? 26207;
            x ^= Proxy ? 73699 : 136061;

            return x;
        }

        public bool Equals(PatchMethodData other) {
            return GetHashCode() == other.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj == null || !(obj is PatchMethodData)) return false;
            return Equals((PatchMethodData)obj);
        }

        public static bool operator ==(PatchMethodData a, PatchMethodData b) {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        public static bool operator !=(PatchMethodData a, PatchMethodData b) {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return !a.Equals(b);
        }
    }
}
