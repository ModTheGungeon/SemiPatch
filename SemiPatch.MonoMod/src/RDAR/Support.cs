using System;
using Mono.Cecil;

namespace SemiPatch.RDAR.Support {
    public class NameAliasedFromAttribute : Attribute {
        public NameAliasedFromAttribute(string name) { }
    }

    public class HasOriginalInAttribute : Attribute {
        public string OrigName;

        public HasOriginalInAttribute(string name) { OrigName = name; }
    }

    public static class RDARSupport {
        public static ModuleDefinition SemiPatchMonoModModule;
        public static TypeDefinition RDARSupportNameAliasedFromAttribute;
        public static TypeDefinition RDARSupportHasOriginalInAttribute;

        static RDARSupport() {
            SemiPatchMonoModModule = ModuleDefinition.ReadModule(System.Reflection.Assembly.GetExecutingAssembly().Location);
            RDARSupportNameAliasedFromAttribute = SemiPatchMonoModModule.GetType("SemiPatch.RDAR.Support.NameAliasedFromAttribute");
            RDARSupportHasOriginalInAttribute = SemiPatchMonoModModule.GetType("SemiPatch.RDAR.Support.HasOriginalInAttribute");
        }
    }
}
