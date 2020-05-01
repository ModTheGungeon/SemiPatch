using System;
using System.IO;
using Mono.Cecil;

namespace SemiPatch {
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
