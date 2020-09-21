using System;
using System.IO;
using ModTheGungeon;
using Mono.Cecil;
using MonoMod;

namespace SemiPatch {
    /// <summary>
    /// Static SemiPatch client, capable of loading SPR modules and statically
    /// patching assemblies. For runtime patching and runtime reloading, see
    /// <see cref="RuntimeClient"/>;
    /// </summary>
    public class StaticClient : Client {
        public StaticPatcher Patcher;
        public Logger Logger;
        public MonoModder Modder;

        public StaticClient(ModuleDefinition target_module) : base(target_module) {
            Logger = new Logger($"{nameof(StaticClient)}({TargetModule.Name})");
            Modder = new MonoModder { Module = target_module };
            Patcher = new StaticPatcher(target_module);
        }

        public override AddModuleResult AddModule(ReloadableModule module) {
            Patcher.LoadPatch(module.PatchData, module.PatchModule);
            return AddModuleResult.Success;
        }

        public override CommitResult Commit() {
            Patcher.Patch();

            return CommitResult.Success;
        }

        public void WriteResult(Stream stream) {
            TargetModule.Write(stream);
        }

        public void WriteResult(string path) {
            TargetModule.Write(path);
        }

        public override void Dispose() {}
    }
}
