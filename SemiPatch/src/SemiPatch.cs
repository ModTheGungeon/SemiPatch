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
        public static TypeDefinition GetterAttribute;
        public static TypeDefinition SetterAttribute;
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

        private static Dictionary<string, ModuleDefinition> _CachedModules = new Dictionary<string, ModuleDefinition>();
        private static Dictionary<string, System.Reflection.Assembly> _CachedAssemblies = new Dictionary<string, System.Reflection.Assembly>();


        private static ModuleDefinition _GetModule(string fully_qualified_name) {
            if (_CachedModules.TryGetValue(fully_qualified_name, out ModuleDefinition mod)) {
                return mod;
            }

            var asm = System.Reflection.Assembly.ReflectionOnlyLoad(fully_qualified_name);
            if (asm == null) return null;
            return _CachedModules[fully_qualified_name] = ModuleDefinition.ReadModule(asm.Location);
        }

        public static TypeDefinition FindType(IList<TypeDefinition> types, string full_name) {
            for (var i = 0; i < types.Count; i++) {
                var type = types[i];
                if (type.FullName == full_name) return type;
                var found_type = FindType(type.NestedTypes, full_name);
                if (found_type != null) return found_type;
            }
            return null;
        }

        public static TypeDefinition FindType(ModuleDefinition module, string full_name) {
            for (var i = 0; i < module.AssemblyReferences.Count; i++) {
                var dep_module_name = module.AssemblyReferences[i].FullName;
                var type = FindType(_GetModule(dep_module_name), full_name);
                if (type != null) return type;
            }

            return FindType(module.Types, full_name);
        }

        private static System.Reflection.Assembly _GetAssembly(string fully_qualified_name) {
            if (_CachedAssemblies.TryGetValue(fully_qualified_name, out System.Reflection.Assembly asm)) {
                return asm;
            }

            var loaded_asm = System.Reflection.Assembly.Load(fully_qualified_name);
            if (loaded_asm == null) return null;
            return _CachedAssemblies[fully_qualified_name] = loaded_asm;
        }

        public static Type FindType(IList<Type> types, string full_name) {
            for (var i = 0; i < types.Count; i++) {
                var type = types[i];
                if (type.FullName == full_name) return type;
                var nested_types = type.GetNestedTypes();
                var found_type = FindType(nested_types, full_name);
                if (found_type != null) return found_type;
            }
            return null;
        }

        public static Type FindType(System.Reflection.Assembly asm, string full_name) {
            var refs = asm.GetReferencedAssemblies();
            for (var i = 0; i < refs.Length; i++) {
                var dep_module_name = refs[i].FullName;
                var type = FindType(_GetAssembly(dep_module_name), full_name);
                if (type != null) return type;
            }

            return FindType(asm.GetTypes(), full_name);
        }

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
