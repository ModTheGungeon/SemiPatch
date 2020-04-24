using System;
using System.IO;
using System.Text;
using Mono.Cecil;

namespace SemiPatch {
    public abstract class PatchMemberData<MemberDefinitionType, PathType>
    where MemberDefinitionType : class, IMemberDefinition
    where PathType : MemberPath<MemberDefinitionType> {
        public delegate T PatchMemberDataConstructor<T, TMemberDefinitionType, TPathType>(
             TMemberDefinitionType target,
             TMemberDefinitionType patch,
             TPathType target_type, TPathType patch_type,
             EndOfPositionalArguments end,
             bool receives_original, bool explicitly_ignored,
             string aliased_name, bool proxy
        ) where TMemberDefinitionType : class, IMemberDefinition
          where TPathType : MemberPath<TMemberDefinitionType>;

        public MemberDefinitionType Target;
        public MemberDefinitionType Patch;
        public PathType TargetPath;
        public PathType PatchPath;
        public bool ReceivesOriginal;
        public bool ExplicitlyIgnored;
        public string AliasedName;
        public bool Proxy;

        public bool IsInsert => Target == null;
        public abstract string MemberTypeName { get; }

        protected PatchMemberData(
            MemberDefinitionType target, MemberDefinitionType patch,
            PathType target_path, PathType patch_path,
            EndOfPositionalArguments end = default(EndOfPositionalArguments),
            bool receives_original = false,
            bool explicitly_ignored = false,
            string aliased_name = null,
            bool proxy = false
        ) {
            Target = target;
            Patch = patch;
            TargetPath = target_path;
            PatchPath = patch_path;

            ReceivesOriginal = receives_original;
            ExplicitlyIgnored = explicitly_ignored;
            AliasedName = aliased_name;
            Proxy = proxy;
        }

        protected PatchMemberData(
            MemberDefinitionType patch,
            PathType target_path, PathType patch_path,
            EndOfPositionalArguments end = default(EndOfPositionalArguments),
            bool receives_original = false,
            bool explicitly_ignored = false,
            string aliased_name = null,
            bool proxy = false
        ) : this(
            null, patch,
            target_path, patch_path,
            receives_original: receives_original,
            explicitly_ignored: explicitly_ignored,
            aliased_name: aliased_name,
            proxy: proxy
        ) {}

        public string ToString(string indent) {
            var s = new StringBuilder();
            s.Append(indent);
            if (IsInsert) {
                s.Append("Inserted ");
            } else {
                s.Append($"Target {MemberTypeName}: ").Append(Target.FullName).Append("\n");
                s.Append(indent);
                s.Append("Patch ");
            }
            s.Append($"{MemberTypeName}: ").Append(Patch.FullName);
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

        public override string ToString() {
            return ToString("");
        }

        public void Serialize(BinaryWriter writer) {
            writer.Write(IsInsert);
            writer.Write(TargetPath);
            writer.Write(PatchPath);
            writer.Write(ReceivesOriginal);
            writer.Write(ExplicitlyIgnored);
            writer.WriteNullable(AliasedName);
            writer.Write(Proxy);
        }

        public override int GetHashCode() {
            var x = TargetPath?.GetHashCode() ?? 21409;
            x ^= PatchPath.GetHashCode();
            x ^= IsInsert ? 38911 : 3007;
            x ^= ReceivesOriginal ? 31861 : 6157;
            x ^= ExplicitlyIgnored ? 52211 : 8773;
            x ^= AliasedName?.GetHashCode() ?? 26207;
            x ^= Proxy ? 73699 : 136061;
            x ^= Patch.CalculateHashCode();
            x ^= Target?.CalculateHashCode() ?? 0;

            x ^= MemberTypeName.GetHashCode();

            return x;
        }

        public bool Equals(PatchMemberData<MemberDefinitionType, PathType> other) {
            return GetHashCode() == other.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj == null || !(obj is PatchMemberData<MemberDefinitionType, PathType>)) return false;
            return Equals((PatchMemberData<MemberDefinitionType, PathType>)obj);
        }

        public static bool operator ==(PatchMemberData<MemberDefinitionType, PathType> a, PatchMemberData<MemberDefinitionType, PathType> b) {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        public static bool operator !=(PatchMemberData<MemberDefinitionType, PathType> a, PatchMemberData<MemberDefinitionType, PathType> b) {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return !a.Equals(b);
        }

        public static T Deserialize<T, TMemberDefinitionType, TPathType>(
            string member_type_name,
            TypeReference target_type,
            TypeReference patch_type,
            BinaryReader reader,
            PatchMemberDataConstructor<T, TMemberDefinitionType, TPathType> ctor,
            Func<BinaryReader, TPathType> read_path_func,
            Mono.Collections.Generic.Collection<TMemberDefinitionType> target_members,
            Mono.Collections.Generic.Collection<TMemberDefinitionType> patch_members
        )
        where T : PatchMemberData<MemberDefinitionType, PathType>
        where TMemberDefinitionType : class, IMemberDefinition
        where TPathType : MemberPath<TMemberDefinitionType> {
            var insert = reader.ReadBoolean();
            var target_path = read_path_func(reader);
            var patch_path = read_path_func(reader);
            var receives_original = reader.ReadBoolean();
            var explicitly_ignored = reader.ReadBoolean();
            var aliased_name = reader.ReadNullableString();
            var proxy = reader.ReadBoolean();

            TMemberDefinitionType patch = null;
            TMemberDefinitionType target = null;

            foreach (var member in patch_members) {
                var candidate_sig = Signature.FromInterface(member);
                if (candidate_sig == patch_path.Signature) {
                    patch = member;
                    break;
                }
            }
            if (patch == null) throw new Exception($"Deserialization error: Failed to find patch {member_type_name} in {patch_type.FullName} with signature '{patch_path}'");

            if (insert) return ctor(
                null, patch, target_path, patch_path,
                default(EndOfPositionalArguments),
                receives_original, explicitly_ignored, aliased_name, proxy
            );

            foreach (var member in target_members) {
                var candidate_sig = Signature.FromInterface(member);
                if (candidate_sig == target_path.Signature) {
                    target = member;
                    break;
                }
            }
            if (target == null) throw new Exception($"Deserialization error: Failed to find target {member_type_name} in {target_type.FullName} with signature '{target_path}' - patch was not marked as Insert");

            return ctor(
                target, patch, target_path, patch_path,
                default(EndOfPositionalArguments),
                receives_original, explicitly_ignored, aliased_name, proxy
            );
        }
    }
}
