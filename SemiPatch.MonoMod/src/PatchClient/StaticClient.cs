using System;
using System.IO;
using ModTheGungeon;
using Mono.Cecil;
using MonoMod;

namespace SemiPatch {
    /// <summary>
    /// Static SemiPatch client, capable of loading SPR modules and statically
    /// patching assemblies. For runtime patching and runtime reloading, see
    /// <see cref="StaticClient"/>;
    /// </summary>
    public class StaticClient : Client {
        public enum PreloadResult {
            NoMMSGModule,
            Success
        }

        public enum CommitResult {
            Success
        }

        public StaticPatcher Patcher;
        public Logger Logger;
        public MonoModder Modder;

        public StaticClient(ModuleDefinition target_module) {
            TargetModule = target_module;
            Logger = new Logger($"{nameof(StaticClient)}({TargetModule.Name})");
            Modder = new MonoModder { Module = target_module };
            Patcher = new StaticPatcher(target_module);
        }

        public PreloadResult Preload(ReloadableModule module) {
            //var mmsg = module.MMSGModule;
            //if (mmsg == null) return PreloadResult.NoMMSGModule;

            //Modder.Mods.Add(mmsg);
            Patcher.LoadPatch(module.PatchData, module.PatchModule);
            return PreloadResult.Success;
        }

        public CommitResult Commit() {
            Patcher.Patch();
            //Modder.MapDependencies();
            //Modder.AutoPatch();

            return CommitResult.Success;
        }

        public void WriteResult(Stream stream) {
            TargetModule.Write(stream);
            //Modder.Write(stream);
        }

        public void WriteResult(string path) {
            TargetModule.Write(path);
            //Modder.Write(null, path);
        }

        public override void Dispose() {
            //Modder.Dispose();
        }
    }
}
