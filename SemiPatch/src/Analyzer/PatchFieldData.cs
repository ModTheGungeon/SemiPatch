using System;
using System.IO;
using System.Text;
using Mono.Cecil;

namespace SemiPatch {
    /// <summary>
    /// Object containing data about a single field patch.
    /// See <see cref="PatchMemberData"/> for elements available on all type member
    /// patches.
    /// </summary>
    public class PatchFieldData : PatchMemberData {
        protected PatchFieldData() { }

        public PatchFieldData(
            FieldDefinition target, FieldDefinition patch,
            FieldPath target_path, FieldPath patch_path,
            EndOfPositionalArguments end = default(EndOfPositionalArguments),
            bool receives_original = false,
            bool explicitly_ignored = false,
            string aliased_name = null,
            bool proxy = false
        ) : base(
            target, patch,
            target_path, patch_path,
            receives_original: receives_original,
            explicitly_ignored: explicitly_ignored,
            aliased_name: aliased_name,
            proxy: proxy
        ) { }

        public PatchFieldData(
            FieldDefinition patch,
            FieldPath target_path, FieldPath patch_path,
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
        ) { }

        /// <summary>
        /// Typed version of <see cref="PatchMemberData.TargetMember"/>;
        /// </summary>
        public FieldDefinition Target => (FieldDefinition)TargetMember;
        /// <summary>
        /// Typed version of <see cref="PatchMemberData.PatchMember"/>;
        /// </summary>
        public FieldDefinition Patch => (FieldDefinition)PatchMember;

        public override MemberType MemberType => MemberType.Field;

        public static PatchFieldData Deserialize(TypeDefinition target_type, TypeDefinition patch_type, BinaryReader reader) {
            var member = new PatchFieldData();
            member.DeserializeMemberBase(
                "field",
                reader,
                (r) => r.ReadFieldPath(),
                target_type.Fields,
                patch_type.Fields
            );
            return member;
        }
    }
}
