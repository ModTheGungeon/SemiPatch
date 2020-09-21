using System;
using System.IO;
using Mono.Cecil;

namespace SemiPatch {
    /// <summary>
    /// Base type of a SemiPatch client capable of loading <see cref="ReloadableModule"/>s
    /// and using them for patching and assembly manipulation.
    /// </summary>
    public abstract class Client : IDisposable {
        public enum AddModuleResult {
            Success,
            NoChanges,
            IncompatibleWithClient,
        }

        public enum CommitResult {
            Success
        }

        public ModuleDefinition TargetModule;
        public DefaultAssemblyResolver AssemblyResolver = new DefaultAssemblyResolver();

        public Client(ModuleDefinition target_module) {
            TargetModule = target_module;
        }

        public abstract AddModuleResult AddModule(ReloadableModule mod);
        public abstract CommitResult Commit();

        public ReloadableModule Load(Stream stream) {
            var rm = ReloadableModule.Read(stream, TargetModule, AssemblyResolver);
            if (rm.PatchData.TargetModule.Assembly.FullName != TargetModule.Assembly.FullName) {
                throw new InvalidTargetException(TargetModule, rm.PatchData.TargetModule);
            }
            return rm;
        }

        public ReloadableModule Load(string file_path) {
            return ReloadableModule.Read(file_path, TargetModule, AssemblyResolver);
        }

        public abstract void Dispose();

        public void AddAssemblySearchDirectory(string path) {
            AssemblyResolver.AddSearchDirectory(path);
        }
    }
}
