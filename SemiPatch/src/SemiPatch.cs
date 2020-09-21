using System;
using System.Collections.Generic;
using Mono.Cecil;

namespace SemiPatch {
    /// <summary>
    /// Represents the kind of a .NET type member.
    /// </summary>
    public enum MemberType {
        Method,
        Field,
        Property
    }

    internal static class SemiPatch {
        public static ModuleDefinition SemiPatchModule;
        public static TypeDefinition PatchAttribute;
        public static TypeDefinition InsertAttribute;
        public static TypeDefinition IgnoreAttribute;
        public static TypeDefinition TargetNameAttribute;
        public static TypeDefinition ReceiveOriginalAttribute;
        public static TypeDefinition ProxyAttribute;
        public static TypeDefinition TreatLikeMethodAttribute;
        public static TypeDefinition GetMethodAttribute;
        public static TypeDefinition SetMethodAttribute;
        public static TypeDefinition InjectAttribute;
        public static TypeDefinition CaptureLocalAttribute;
        public static TypeDefinition OrigType;

        public static TypeDefinition VoidInjectionStateType;
        public static TypeDefinition InjectionStateType;

        public static FieldDefinition VoidInjectionStateOverrideReturnField;
        public static FieldDefinition InjectionStateOverrideReturnField;
        public static FieldDefinition InjectionStateReturnValueField;



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
            GetMethodAttribute = SemiPatchModule.GetType("SemiPatch.GetMethodAttribute");
            SetMethodAttribute = SemiPatchModule.GetType("SemiPatch.SetMethodAttribute");
            InjectAttribute = SemiPatchModule.GetType("SemiPatch.InjectAttribute");
            CaptureLocalAttribute = SemiPatchModule.GetType("SemiPatch.CaptureLocalAttribute");
            OrigType = SemiPatchModule.GetType("SemiPatch.Orig");

            VoidInjectionStateType = SemiPatchModule.GetType("SemiPatch.InjectionState");
            InjectionStateType = SemiPatchModule.GetType("SemiPatch.InjectionState`1");

            for (var i = 0; i < VoidInjectionStateType.Fields.Count; i++) {
                var field = VoidInjectionStateType.Fields[i];
                if (field.Name == "_OverrideReturn") {
                    VoidInjectionStateOverrideReturnField = field;
                    break;
                }
            }

            for (var i = 0; i < InjectionStateType.Fields.Count; i++) {
                var field = InjectionStateType.Fields[i];
                if (field.Name == "_OverrideReturn") {
                    InjectionStateOverrideReturnField = field;
                } else if (field.Name == "_ReturnValue") {
                    InjectionStateReturnValueField = field;
                }

                if (InjectionStateReturnValueField != null && InjectionStateOverrideReturnField != null) {
                    break;
                }
            }
        }
    }
}
