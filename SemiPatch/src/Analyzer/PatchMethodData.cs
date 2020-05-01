using System;
using Mono.Cecil;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SemiPatch {
    /// <summary>
    /// Object containing data about a single method patch.
    /// See <see cref="PatchMemberData"/> for elements available on all type member
    /// patches.
    /// </summary>
    public class PatchMethodData : PatchMemberData {
        private PatchMethodData() { }

        public PatchMethodData(
            MethodDefinition target, MethodDefinition patch,
            MethodPath target_path, MethodPath patch_path,
            EndOfPositionalArguments end = default(EndOfPositionalArguments),
            bool receives_original = false,
            bool explicitly_ignored = false,
            string aliased_name = null,
            bool proxy = false,
            bool rejected_default_ctor = false
        ) : base(
            target, patch,
            target_path, patch_path,
            receives_original: receives_original,
            explicitly_ignored: explicitly_ignored,
            aliased_name: aliased_name,
            proxy: proxy
        ) { FalseDefaultConstructor = rejected_default_ctor; }

        public PatchMethodData (
            MethodDefinition patch,
            MethodPath target_path, MethodPath patch_path,
            EndOfPositionalArguments end = default(EndOfPositionalArguments),
            bool receives_original = false,
            bool explicitly_ignored = false,
            string aliased_name = null,
            bool proxy = false,
            bool rejected_default_ctor = false
        ) : this(
            null, patch,
            target_path, patch_path,
            receives_original: receives_original,
            explicitly_ignored: explicitly_ignored,
            aliased_name: aliased_name,
            proxy: proxy,
            rejected_default_ctor: rejected_default_ctor
        ) { }

        /// <summary>
        /// If <c>true</c>, this data represents an empty, untagged, default
        /// parameterless constructor in the patch class that does not actually
        /// exist within the target class. This field is used for example in
        /// <see cref="Relinker"/> to reject attempts to construct objects that
        /// don't have a default constructor.
        /// </summary>
        public bool FalseDefaultConstructor = false;

        /// <summary>
        /// Typed version of <see cref="PatchMemberData.TargetMember"/>;
        /// </summary>
        public MethodDefinition Target => (MethodDefinition)TargetMember;
        /// <summary>
        /// Typed version of <see cref="PatchMemberData.PatchMember"/>;
        /// </summary>
        public MethodDefinition Patch => (MethodDefinition)PatchMember;

        public override MemberType MemberType => MemberType.Method;

        public override void Serialize(BinaryWriter writer) {
            base.Serialize(writer);
            writer.Write(FalseDefaultConstructor);
        }

        public static PatchMethodData Deserialize(TypeDefinition target_type, TypeDefinition patch_type, BinaryReader reader) {
            var member = new PatchMethodData();
            member.DeserializeMemberBase(
                "method",
                reader,
                (r) => r.ReadMethodPath(),
                target_type.Methods,
                patch_type.Methods
            );
            member.FalseDefaultConstructor = reader.ReadBoolean();
            return member;
        }
    }
}
