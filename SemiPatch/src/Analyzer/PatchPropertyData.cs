using System;
using System.IO;
using System.Text;
using Mono.Cecil;

namespace SemiPatch {
    public class PatchPropertyData : PatchMemberData<PropertyDefinition, PropertyPath> {
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

        public static PatchPropertyData Create(
            PropertyDefinition target, PropertyDefinition patch,
            PropertyPath target_path, PropertyPath patch_path,
            EndOfPositionalArguments end = default(EndOfPositionalArguments),
            bool receives_original = false,
            bool explicitly_ignored = false,
            string aliased_name = null,
            bool proxy = false
        ) {
            return new PatchPropertyData(
                target, patch,
                target_path, patch_path,
                receives_original: receives_original,
                explicitly_ignored: explicitly_ignored,
                aliased_name: aliased_name,
                proxy: proxy
            );
        }

        public override string MemberTypeName => "Property";

        public static PatchPropertyData Deserialize(TypeDefinition target_type, TypeDefinition patch_type, BinaryReader reader) {
            return Deserialize<PatchPropertyData, PropertyDefinition, PropertyPath>(
                "property",
                target_type,
                patch_type,
                reader,
                Create,
                (r) => r.ReadPropertyPath(),
                target_type.Properties,
                patch_type.Properties
            );
        }
    }
}
