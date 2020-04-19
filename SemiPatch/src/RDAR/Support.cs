using System;
namespace SemiPatch.RDAR.Support {
    public class NameAliasedFromAttribute : Attribute {
        public NameAliasedFromAttribute(string name) { }
    }

    public class HasOriginalInAttribute : Attribute {
        public HasOriginalInAttribute(string name) { }
    }
}
