using System;
using Mono.Cecil;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SemiPatch {
    public class PatchMethodData {
        public MethodDefinition TargetMethod;
        public MethodDefinition PatchMethod;
        public string TargetSignature;
        public string PatchSignature;
        public bool ReceivesOriginal;
        public bool ExplicitlyIgnored;
        public string AliasedName;
        public bool Proxy;

        public PatchMethodData(MethodDefinition target, MethodDefinition patch, string target_sig, string patch_sig = null, bool receives_original = false, bool explicitly_ignored = false, string target_name = null) {
            TargetMethod = target;
            PatchMethod = patch;
            TargetSignature = target_sig;
            PatchSignature = patch_sig ?? target_sig;
            ReceivesOriginal = receives_original;
            ExplicitlyIgnored = explicitly_ignored;
            AliasedName = target_name;
        }

        public PatchMethodData(MethodDefinition source, string target_sig, string patch_sig = null, bool receives_original = false, bool explicitly_ignored = false, string target_name = null) {
            PatchMethod = source;
            TargetSignature = target_sig;
            PatchSignature = patch_sig ?? target_sig;
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
            if (TargetSignature != null) {
                s.Append("\n").Append(indent);
                s.Append("Target Signature: ").Append(TargetSignature);
            }
            s.Append("\n").Append(indent);
            s.Append("Patch Signature: ").Append(PatchSignature);
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
            writer.Write(TargetSignature);
            writer.Write(PatchSignature);
            writer.Write(ReceivesOriginal);
            writer.Write(ExplicitlyIgnored);
            writer.WriteNullable(AliasedName);
            writer.Write(Proxy);
        }

        public static PatchMethodData Deserialize(TypeDefinition target_type, TypeDefinition patch_type, BinaryReader reader) {
            var insert = reader.ReadBoolean();
            var target_sig = reader.ReadString();
            var patch_sig = reader.ReadString();
            var receives_original = reader.ReadBoolean();
            var explicitly_ignored = reader.ReadBoolean();
            var aliased_name = reader.ReadNullableString();
            var proxy = reader.ReadBoolean();

            MethodDefinition patch = null;
            MethodDefinition target = null;

            foreach (var method in patch_type.Methods) {
                var candidate_sig = method.BuildSignature();
                if (candidate_sig == patch_sig) {
                    patch = method;
                    break;
                }
            }
            if (patch == null) throw new Exception($"Deserialization error: Failed to find patch method in {patch_type.FullName} with signature '{patch_sig}'");

            if (insert) return new PatchMethodData(patch, target_sig) { Proxy = proxy };

            foreach (var method in target_type.Methods) {
                var candidate_sig = method.BuildSignature();
                if (candidate_sig == target_sig) {
                    target = method;
                    break;
                }
            }
            if (target == null) throw new Exception($"Deserialization error: Failed to find target method in {target_type.FullName} with signature '{target_sig}' - patch was not marked as Insert");

            return new PatchMethodData(target, patch, target_sig, patch_sig, receives_original, explicitly_ignored, aliased_name) { Proxy = proxy };
        }
    }
}
