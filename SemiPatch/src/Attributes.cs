using System;
namespace SemiPatch {
    [AttributeUsage(AttributeTargets.Class)]
    public class PatchAttribute : Attribute {
        public PatchAttribute(Type type) {
            Type = type;
        }

        public Type Type;
    }

    [AttributeUsage(AttributeTargets.All)]
    public class InsertAttribute : Attribute {

    }

    [AttributeUsage(AttributeTargets.All)]
    public class IgnoreAttribute : Attribute {

    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    public class TargetNameAttribute : Attribute {
        public TargetNameAttribute(string name) { }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ReceiveOriginalAttribute : Attribute {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public class ProxyAttribute : Attribute {
    }

    [AttributeUsage(AttributeTargets.Constructor)]
    public class TreatLikeMethodAttribute : Attribute {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class GetterAttribute : Attribute {
        public GetterAttribute(string prop) { }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class SetterAttribute : Attribute {
        public SetterAttribute(string prop) { }
    }
}
