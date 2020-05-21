using System;
using System.IO;
using System.Text;
using Mono.Cecil;

namespace SemiPatch {
    /// <summary>
    /// Abstract class representing an object with data about a single patch
    /// of a type member. See: <see cref="PatchFieldData"/>, <see cref="PatchMethodData"/>,
    /// <see cref="PatchPropertyData"/> for implementations.
    /// </summary>
    public abstract class PatchMemberData {
        /// <summary>
        /// The member object within the target assembly that this patch wants to change.
        /// Will not exist in the case of members tagged with the Insert attribute,
        /// as no corresponding target member will exist for them before being patched in.
        /// </summary>
        public IMemberDefinition TargetMember;

        /// <summary>
        /// The member object within the patch assembly. Will always exist.
        /// </summary>
        public IMemberDefinition PatchMember;

        /// <summary>
        /// Path of the member object that this patch is targetting. Will always
        /// exist, even if the object is an Insert patch and <see cref="TargetMember"/>
        /// is <code>null</code>.
        /// </summary>
        public MemberPath TargetPath;

        /// <summary>
        /// Path of the member object that represents this patch. Will always
        /// exist.
        /// </summary>
        public MemberPath PatchPath;

        /// <summary>
        /// Only used on methods. If <c>true</c>, the method that represents
        /// this patch takes an additional argument (in the first position) of type
        /// <c>Orig</c> or <c>VoidOrig</c>. See <see cref="ReceiveOriginalAttribute"/>
        /// to learn more about the attribute and its behavior.
        /// </summary>
        public bool ReceivesOriginal;

        /// <summary>
        /// If <c>true</c>, the member representing this patch is ignored,
        /// no matter if it has any other attributes or data. See <see cref="IgnoreAttribute"/>
        /// to learn more about the attribute and its behavior.
        /// </summary>
        public bool ExplicitlyIgnored;

        /// <summary>
        /// If not <c>null</c>, signifies that in the final executing
        /// product the member representing this patch should have the name
        /// specified in this field, as well as any references to the member
        /// must be renamed appropriately. See <see cref="TargetNameAttribute"/>
        /// to learn more about the attribute and its behavior.
        /// </summary>
        public string AliasedName;

        /// <summary>
        /// If <c>true</c>, for all intents and purposes this patch is
        /// ignored, no matter if it has any other attributes or data. Unlike
        /// the <see cref="ExplicitlyIgnored"/> field however, Proxy members
        /// must always refer to an existing member within the target type.
        /// See <see cref="ProxyAttribute"/> to learn more about the attribute
        /// and its behavior.
        /// </summary>
        public bool Proxy;

        /// <summary>
        /// Determines whether this patch represents inserting a member into the
        /// target type, not changing an existing one.
        /// </summary>
        /// <value><c>true</c> if <see cref="TargetMember"/> is <code>null</code>
        /// (<see cref="InsertAttribute"/> was used on the member); otherwise,
        /// <c>false</c>.</value>
        public bool IsInsert => TargetMember == null;

        /// <summary>
        /// The type of member that this object represents. Used only for
        /// hashing and equality comparison.
        /// </summary>
        public abstract MemberType MemberType { get; }

        public virtual bool EffectivelyIgnored => ExplicitlyIgnored || Proxy;

        protected PatchMemberData() { } 

        protected PatchMemberData(
            IMemberDefinition target, IMemberDefinition patch,
            MemberPath target_path, MemberPath patch_path,
            bool receives_original,
            bool explicitly_ignored,
            string aliased_name,
            bool proxy
        ) {
            TargetMember = target;
            PatchMember = patch;
            TargetPath = target_path;
            PatchPath = patch_path;

            ReceivesOriginal = receives_original;
            ExplicitlyIgnored = explicitly_ignored;
            AliasedName = aliased_name;
            Proxy = proxy;
        }

        protected PatchMemberData(
            IMemberDefinition patch,
            MemberPath target_path, MemberPath patch_path,
            bool receives_original,
            bool explicitly_ignored,
            string aliased_name,
            bool proxy
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
                s.Append($"Target {MemberType}: ").Append(TargetMember.FullName).Append("\n");
                s.Append(indent);
                s.Append("Patch ");
            }
            s.Append($"{MemberType}: ").Append(PatchMember.FullName);
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

        public virtual void Serialize(BinaryWriter writer) {
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
            x ^= PatchMember.CalculateHashCode();
            x ^= TargetMember?.CalculateHashCode() ?? 0;

            x ^= MemberType.GetHashCode();

            return x;
        }

        public bool Equals(PatchMemberData other) {
            return GetHashCode() == other.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj == null || !(obj is PatchMemberData)) return false;
            return Equals((PatchMemberData)obj);
        }

        public static bool operator ==(PatchMemberData a, PatchMemberData b) {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        public static bool operator !=(PatchMemberData a, PatchMemberData b) {
            if (a is null && b is null) return false;
            if (a is null || b is null) return true;
            return !a.Equals(b);
        }

        protected void DeserializeMemberBase<T>(
            string member_type_name,
            BinaryReader reader,
            Func<BinaryReader, MemberPath> read_path_func,
            Mono.Collections.Generic.Collection<T> target_members,
            Mono.Collections.Generic.Collection<T> patch_members
        ) where T : class, IMemberDefinition {
            var insert = reader.ReadBoolean();
            var target_path = read_path_func(reader);
            var patch_path = read_path_func(reader);
            var receives_original = reader.ReadBoolean();
            var explicitly_ignored = reader.ReadBoolean();
            var aliased_name = reader.ReadNullableString();
            var proxy = reader.ReadBoolean();

            IMemberDefinition patch = null;
            IMemberDefinition target = null;

            foreach (var member in patch_members) {
                var candidate_sig = Signature.FromInterface(member);
                if (candidate_sig == patch_path.Signature) {
                    patch = member;
                    break;
                }
            }
            if (patch == null) throw new PatchDataDeserializationException($"Failed to find patch {member_type_name} with signature '{patch_path}'");

            PatchMember = patch;
            TargetPath = target_path;
            PatchPath = patch_path;
            ReceivesOriginal = receives_original;
            ExplicitlyIgnored = explicitly_ignored;
            AliasedName = aliased_name;
            Proxy = proxy;

            if (insert) return;

            foreach (var member in target_members) {
                var candidate_sig = Signature.FromInterface(member);
                if (candidate_sig == target_path.Signature) {
                    target = member;
                    break;
                }
            }

            TargetMember = target;
        }
    }
}
