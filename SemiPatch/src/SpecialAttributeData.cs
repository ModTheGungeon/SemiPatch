using System;
using Mono.Collections.Generic;
using Mono.Cecil;
using System.Collections.Generic;

namespace SemiPatch {
    internal struct SpecialAttributeData {
        public TypeDefinition PatchType;
        public bool Insert;
        public bool Ignore;
        public string AliasedName;
        public bool ReceiveOriginal;
        public bool Proxy;
        public bool TreatConstructorLikeMethod;
        public string PropertyGetter;
        public string PropertySetter;
        public bool IsPropertyMethod => PropertyGetter != null || PropertySetter != null;
        public InjectAttribute InjectData;
        public IList<CaptureLocalAttribute> LocalCaptures;

        public SpecialAttributeData(Collection<CustomAttribute> attrs) {
            PatchType = null;
            Insert = false;
            Ignore = false;
            AliasedName = null;
            ReceiveOriginal = false;
            Proxy = false;
            TreatConstructorLikeMethod = false;
            PropertyGetter = null;
            PropertySetter = null;
            InjectData = null;
            LocalCaptures = null;

            foreach (var attr in attrs) {
                if (attr.AttributeType.IsSame(SemiPatch.PatchAttribute)) {
                    PatchType = (attr.ConstructorArguments[0].Value as TypeReference).Resolve();
                } else if (attr.AttributeType.IsSame(SemiPatch.InsertAttribute)) {
                    Insert = true;
                } else if (attr.AttributeType.IsSame(SemiPatch.IgnoreAttribute)) {
                    Ignore = true;
                } else if (attr.AttributeType.IsSame(SemiPatch.TargetNameAttribute)) {
                    AliasedName = attr.ConstructorArguments[0].Value as string;
                } else if (attr.AttributeType.IsSame(SemiPatch.ReceiveOriginalAttribute)) {
                    ReceiveOriginal = true;
                } else if (attr.AttributeType.IsSame(SemiPatch.ProxyAttribute)) {
                    Proxy = true;
                } else if (attr.AttributeType.IsSame(SemiPatch.TreatLikeMethodAttribute)) {
                    TreatConstructorLikeMethod = true;
                } else if (attr.AttributeType.IsSame(SemiPatch.GetterAttribute)) {
                    PropertyGetter = attr.ConstructorArguments[0].Value as string;
                } else if (attr.AttributeType.IsSame(SemiPatch.SetterAttribute)) {
                    PropertySetter = attr.ConstructorArguments[0].Value as string;
                } else if (attr.AttributeType.IsSame(SemiPatch.InjectAttribute)) {
                    InjectData = InjectAttribute.MakeFromCecil(attr.ConstructorArguments);
                } else if (attr.AttributeType.IsSame(SemiPatch.CaptureLocalAttribute)) {
                    if (LocalCaptures == null) LocalCaptures = new List<CaptureLocalAttribute>();
                    LocalCaptures.Add(CaptureLocalAttribute.MakeFromCecil(attr.ConstructorArguments));
                }
            }
        }

        public string GetNameIfAliased(string name) => AliasedName ?? name;
    }
}
