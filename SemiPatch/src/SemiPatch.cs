using System;
using Mono.Cecil;

namespace SemiPatch {
    public enum MemberType {
        Method,
        Field,
        Property
    }

    public class SemiPatch {


        public static ModuleDefinition SemiPatchModule;
        public static TypeDefinition PatchAttribute;
        public static TypeDefinition InsertAttribute;
        public static TypeDefinition IgnoreAttribute;
        public static TypeDefinition TargetNameAttribute;
        public static TypeDefinition ReceiveOriginalAttribute;
        public static TypeDefinition ProxyAttribute;
        public static TypeDefinition TreatLikeMethodAttribute;
        public static TypeDefinition GetterAttribute;
        public static TypeDefinition SetterAttribute;
        public static TypeDefinition OrigType;
        public static TypeDefinition PatchControlType;



        public static ModuleDefinition MscorlibModule;
        public static TypeReference VoidType;

        static SemiPatch() {
            MscorlibModule = ModuleDefinition.ReadModule(typeof(string).Assembly.Location);
            VoidType = MscorlibModule.GetType("System.Void");

            SemiPatchModule = ModuleDefinition.ReadModule(System.Reflection.Assembly.GetExecutingAssembly().Location);
            PatchAttribute = SemiPatchModule.GetType("SemiPatch.PatchAttribute");
            InsertAttribute = SemiPatchModule.GetType("SemiPatch.InsertAttribute");
            IgnoreAttribute = SemiPatchModule.GetType("SemiPatch.IgnoreAttribute");
            TargetNameAttribute = SemiPatchModule.GetType("SemiPatch.TargetNameAttribute");
            ReceiveOriginalAttribute = SemiPatchModule.GetType("SemiPatch.ReceiveOriginalAttribute");
            ProxyAttribute = SemiPatchModule.GetType("SemiPatch.ProxyAttribute");
            TreatLikeMethodAttribute = SemiPatchModule.GetType("SemiPatch.TreatLikeMethodAttribute");
            GetterAttribute = SemiPatchModule.GetType("SemiPatch.GetterAttribute");
            SetterAttribute = SemiPatchModule.GetType("SemiPatch.SetterAttribute");
            OrigType = SemiPatchModule.GetType("SemiPatch.Orig");
            PatchControlType = SemiPatchModule.GetType("SemiPatch.PatchControl");
        }
    }
}
