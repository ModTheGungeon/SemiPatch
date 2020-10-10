using System;
using System.IO;
using System.Collections.Generic;
using Mono.Cecil;

namespace SemiPatch {
    public class OverridableAssemblyResolver : IAssemblyResolver {
        public readonly IAssemblyResolver BackingResolver;
        public readonly Dictionary<string, AssemblyDefinition> Overrides;
        public readonly Dictionary<string, Stream> StreamOverrides;

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters reader_params) {
            var str_name = name.FullName;
            if (Overrides.TryGetValue(str_name, out AssemblyDefinition asm)) return asm;
            if (StreamOverrides.TryGetValue(str_name, out Stream stream)) return AssemblyDefinition.ReadAssembly(stream, reader_params);
            return BackingResolver.Resolve(name, reader_params);
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name) {
            var str_name = name.FullName;
            if (Overrides.TryGetValue(str_name, out AssemblyDefinition asm)) return asm;
            if (StreamOverrides.TryGetValue(str_name, out Stream stream)) return AssemblyDefinition.ReadAssembly(stream);
            return BackingResolver.Resolve(name);
        }

        public OverridableAssemblyResolver(IAssemblyResolver backing_resolver) {
            BackingResolver = backing_resolver;
            Overrides = new Dictionary<string, AssemblyDefinition>();
        }

        public void AddOverride(AssemblyDefinition asm) {
            Overrides[asm.FullName] = asm;
        }

        public void AddStreamOverride(string full_name, Stream stream) {
            StreamOverrides[full_name] = stream;
        }

        public bool HasOverride(AssemblyDefinition asm) {
            return Overrides.ContainsKey(asm.FullName) || StreamOverrides.ContainsKey(asm.FullName);
        }

        public void Dispose() {
            BackingResolver.Dispose();
        }
    }
}
