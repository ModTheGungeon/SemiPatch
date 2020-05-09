using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SemiPatch {
    public class PatchInjectData {
        public MethodDefinition Target;
        public MethodPath TargetPath;
        public MethodDefinition Handler;
        public MethodPath HandlerPath;
        public int BodyIndex;
        public Instruction InjectionPoint;
        public IList<CaptureLocalAttribute> LocalCaptures;
        public InjectPosition Position;
        public string HandlerAliasedName;

        public PatchInjectData(MethodDefinition target, MethodDefinition handler, int body_index, IList<CaptureLocalAttribute> captures = null, string handler_alias_name = null) {
            Target = target;
            TargetPath = target.ToPath();
            Handler = handler;
            HandlerPath = handler.ToPath();
            BodyIndex = body_index;
            if (BodyIndex == Target.Body.Instructions.Count) {
                Position = InjectPosition.After;
                InjectionPoint = Target.Body.Instructions[BodyIndex - 1];
            } else {
                Position = InjectPosition.Before;
                InjectionPoint = Target.Body.Instructions[BodyIndex];
            }
            LocalCaptures = captures;
            HandlerAliasedName = handler_alias_name;
        }

        public string BuildInjectionSignature() {
            var s = new StringBuilder();
            s.Append(HandlerPath);
            s.Append(" -> ");
            s.Append(TargetPath);
            return s.ToString();
        }

        public string ToString(string indent) {
            var s = new StringBuilder();
            s.Append(indent);
            s.Append("Target Method: ").Append(TargetPath).Append("\n");
            s.Append(indent);
            s.Append("Injection Handler: ").Append(HandlerPath).Append("\n");
            s.Append(indent);
            s.Append("Injection Point: ").Append(InjectionPoint).Append("\n");
            s.Append("\n");

            if (LocalCaptures != null) {
                s.Append(indent);
                s.Append("Captured Locals: ");
                s.Append("\n");

                var lc_indent = indent + "\t";
                for (var i = 0; i < LocalCaptures.Count; i++) {
                    var capture = LocalCaptures[i];
                    s.Append(lc_indent);
                    s.Append($"- '{capture.Name}' : {capture.Type.BuildSignature()} @ {capture.Index}");
                    s.Append("\n");
                }
            }
            return s.ToString();
        }

        public override string ToString() => ToString("");

        public void Serialize(BinaryWriter writer) {
            writer.Write(TargetPath);
            writer.Write(HandlerPath);
            writer.Write(BodyIndex);
            if (LocalCaptures == null) writer.Write(0);
            else {
                writer.Write(LocalCaptures.Count);
                for (var i = 0; i < LocalCaptures.Count; i++) {
                    var capture = LocalCaptures[i];
                    writer.Write(capture.Name);
                    writer.Write(capture.Type.Module.Assembly.FullName);
                    writer.Write(capture.Type.FullName);
                    writer.Write(capture.Index);
                }
            }
            writer.WriteNullable(HandlerAliasedName);
        }

        public static PatchInjectData Deserialize(TypeDefinition target_type, TypeDefinition patch_type, BinaryReader reader) {
            var target_path = reader.ReadMethodPath();
            var handler_path = reader.ReadMethodPath();
            var body_index = reader.ReadInt32();
            var local_capture_count = reader.ReadInt32();
            var captures = new List<CaptureLocalAttribute>();
            for (var i = 0; i < local_capture_count; i++) {
                var name = reader.ReadString();
                var type_asm_name = reader.ReadString();
                var type_full_name = reader.ReadString();
                var index = reader.ReadInt32();

                var type = GlobalModuleLoader.FindType(patch_type.Module, type_full_name);
                if (type == null) type = GlobalModuleLoader.FindType(target_type.Module, type_full_name);
                if (type == null) {
                    if (type == null) throw new PatchDataDeserializationException($"Failed to find type '{type_full_name}' for local capture '{name}' of injection handler '{handler_path}'");
                }

                captures.Add(new CaptureLocalAttribute(index, type, name));
            }
            var handler_aliased_name = reader.ReadNullableString();

            MethodDefinition target = null;
            MethodDefinition handler = null;

            for (var i = 0; i < target_type.Methods.Count; i++) {
                var method = target_type.Methods[i];
                var candidate_sig = Signature.FromInterface(method);
                if (candidate_sig == target_path.Signature) {
                    target = method;
                    break;
                }
            }

            if (target == null) throw new PatchDataDeserializationException($"Failed to find injection target method with signature '{target_path}'");

            for (var i = 0; i < patch_type.Methods.Count; i++) {
                var method = patch_type.Methods[i];
                var candidate_sig = Signature.FromInterface(method);
                if (candidate_sig == handler_path.Signature) {
                    handler = method;
                    break;
                }
            }

            if (handler == null) throw new PatchDataDeserializationException($"Failed to find injection target handler with signature '{handler_path}'");

            var inject = new PatchInjectData(target, handler, body_index, captures, handler_aliased_name);
            return inject;
        }

        public override int GetHashCode() {
            var x = Target.BuildPrefixedSignature().GetHashCode();
            x ^= Handler.BuildPrefixedSignature().GetHashCode();
            x ^= BodyIndex * 98867;
            x ^= (int)Position * 79943;
            if (LocalCaptures != null) {
                for (var i = 0; i < LocalCaptures.Count; i++) {
                    x ^= LocalCaptures[i].GetHashCode();
                }
            }
            x ^= Target.CalculateHashCode();
            x ^= Handler.CalculateHashCode();
            return x;
        }

        public bool Equals(PatchInjectData other) {
            return GetHashCode() == other.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj == null || !(obj is PatchInjectData)) return false;
            return Equals((PatchInjectData)obj);
        }

        public static bool operator ==(PatchInjectData a, PatchInjectData b) {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        public static bool operator !=(PatchInjectData a, PatchInjectData b) {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return !a.Equals(b);
        }
    }
}
