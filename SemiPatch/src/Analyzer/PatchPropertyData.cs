using System;
using System.IO;
using System.Text;
using Mono.Cecil;

namespace SemiPatch {
    /// <summary>
    /// Object containing data about a single field patch.
    /// See <see cref="PatchMemberData"/> for elements available on all type member
    /// patches.
    /// It is worth noting that while properties are recognized as a kind of member
    /// within .NET/Mono, they make use of method members for the getter and setter
    /// as well as optionally a field member for the backing field (in C#). Therefore,
    /// in essentially all cases this object will come alongside the respective
    /// patch data objects for the members it depends on.
    /// </summary>
    public class PatchPropertyData : PatchMemberData {
        protected PatchPropertyData() { }

        public PatchPropertyData(
            PropertyDefinition target, PropertyDefinition patch,
            PropertyPath target_path, PropertyPath patch_path,
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

        public PatchPropertyData(
            PropertyDefinition patch,
            PropertyPath target_path, PropertyPath patch_path,
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
        /// Typed version of <see cref="PatchMemberData.PatchMember"/>;
        /// </summary>
        public PropertyDefinition Target => (PropertyDefinition)TargetMember;
        /// <summary>
        /// Typed version of <see cref="PatchMemberData.PatchMember"/>;
        /// </summary>
        public PropertyDefinition Patch => (PropertyDefinition)PatchMember;

        public override MemberType MemberType => MemberType.Property;

        public static PatchPropertyData Deserialize(TypeDefinition target_type, TypeDefinition patch_type, BinaryReader reader) {
            var member = new PatchPropertyData();
            member.DeserializeMemberBase(
                "property",
                reader,
                (r) => r.ReadPropertyPath(),
                target_type.Properties,
                patch_type.Properties
            );
            return member;
        }
    }
}
