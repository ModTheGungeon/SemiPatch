using System;
using System.IO;
using Mono.Cecil;

namespace SemiPatch {
    /// <summary>
    /// Base type of a SemiPatch client capable of loading <see cref="ReloadableModule"/>s
    /// and using them for patching and assembly manipulation.
    /// </summary>
    public abstract class Client : IDisposable {
        public ModuleDefinition TargetModule;

        public ReloadableModule Load(Stream stream) {
            var rm = ReloadableModule.Read(stream, TargetModule);
            if (rm.PatchData.TargetModule.Assembly.FullName != TargetModule.Assembly.FullName) {
                throw new InvalidTargetException(TargetModule, rm.PatchData.TargetModule);
            }
            return rm;
        }

        public ReloadableModule Load(string file_path) {
            return ReloadableModule.Read(file_path, TargetModule);
        }

        public abstract void Dispose();
    }
}
