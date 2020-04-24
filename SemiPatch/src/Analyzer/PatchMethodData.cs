using System;
using Mono.Cecil;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SemiPatch {
    /// <summary>
    /// Object containing data about a single method patch.
    /// See <see cref="PatchMemberData{,}"/> for elements available on all type member
    /// patches.
    /// </summary>
    public class PatchMethodData : PatchMemberData<MethodDefinition, MethodPath> {
        public PatchMethodData(
            MethodDefinition target, MethodDefinition patch,
            MethodPath target_path, MethodPath patch_path,
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

        public PatchMethodData (
            MethodDefinition patch,
            MethodPath target_path, MethodPath patch_path,
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

        public static PatchMethodData Create(
            MethodDefinition target, MethodDefinition patch,
            MethodPath target_path, MethodPath patch_path,
            EndOfPositionalArguments end = default(EndOfPositionalArguments),
            bool receives_original = false,
            bool explicitly_ignored = false,
            string aliased_name = null,
            bool proxy = false
        ) {
            return new PatchMethodData(
                target, patch,
                target_path, patch_path,
                receives_original: receives_original,
                explicitly_ignored: explicitly_ignored,
                aliased_name: aliased_name,
                proxy: proxy
            );
        }

        public override string MemberTypeName => "Method";

        public static PatchMethodData Deserialize(TypeDefinition target_type, TypeDefinition patch_type, BinaryReader reader) {
            return Deserialize<PatchMethodData, MethodDefinition, MethodPath>(
                "method",
                target_type,
                patch_type,
                reader,
                Create,
                (r) => r.ReadMethodPath(),
                target_type.Methods,
                patch_type.Methods
            );
        }
    }
}
