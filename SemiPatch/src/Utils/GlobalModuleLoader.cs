using System;
using System.Collections.Generic;
using Mono.Cecil;

namespace SemiPatch {
    public static class GlobalModuleLoader {
        private static Dictionary<string, ModuleDefinition> _CachedModules = new Dictionary<string, ModuleDefinition>();
        private static Dictionary<string, System.Reflection.Assembly> _CachedAssemblies = new Dictionary<string, System.Reflection.Assembly>();

        public static ModuleDefinition GetModule(string fully_qualified_name) {
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

        public static TypeDefinition FindType(ModuleDefinition module, string full_name, HashSet<string> visited_modules = null) {
            if (visited_modules == null) visited_modules = new HashSet<string>();
            for (var i = 0; i < module.AssemblyReferences.Count; i++) {
                var dep_module_name = module.AssemblyReferences[i].FullName;
                if (visited_modules.Contains(dep_module_name)) continue;
                visited_modules.Add(dep_module_name);
                var type = FindType(GetModule(dep_module_name), full_name, visited_modules);
                if (type != null) return type;
            }

            return FindType(module.Types, full_name);
        }

        public static System.Reflection.Assembly GetAssembly(string fully_qualified_name) {
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
                var type = FindType(GetAssembly(dep_module_name), full_name);
                if (type != null) return type;
            }

            return FindType(asm.GetTypes(), full_name);
        }
    }
}
