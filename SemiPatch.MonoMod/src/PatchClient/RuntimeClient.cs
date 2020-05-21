using System;
using System.Collections.Generic;
using System.IO;
using ModTheGungeon;
using Mono.Cecil;

namespace SemiPatch {
    /// <summary>
    /// Runtime SemiPatch client, capable of loading SPR modules and reloading them
    /// during runtime using RDAR. For public static patching, see <see cref="StaticClient"/>;
    /// </summary>
    public class RuntimeClient : Client {
        public enum Result {
            Success,
            RequiresStaticPatching,
            NoChanges
        }

        public readonly RuntimePatchManager PatchManager;
        public ModuleDefinition RunningModule;
        public Relinker Relinker;

        public Logger Logger;

        public RuntimeClient(System.Reflection.Assembly asm, ModuleDefinition target_module, ModuleDefinition running_module) {
            TargetModule = target_module;
            RunningModule = running_module;
            Relinker = new Relinker();
            PatchManager = new RuntimePatchManager(Relinker, asm, running_module);
            Logger = new Logger($"{nameof(RuntimeClient)}({TargetModule.Name})");
        }

        public void BeginProcessing() {
            Logger.Debug("Reset");
            Relinker.Clear();
            PatchManager.ResetPatches();
        }

        public Result Process(ReloadableModule old_module, ReloadableModule new_module) {
            Logger.Debug($"Processing: {old_module} -> {new_module}");

            Relinker.LoadRelinkMapFrom(new_module.PatchData, RunningModule);

            var diff = ReloadableModule.Compare(old_module, new_module);
            if (!diff.HasChanges) return Result.NoChanges;
            if (!PatchManager.CanPatchAtRuntime(diff)) return Result.RequiresStaticPatching;

            PatchManager.ProcessDifference(diff, update_running_module: false);
            return Result.Success;
        }

        public void FinishProcessing() {
            PatchManager.FinalizeProcessing();
        }

        public override void Dispose() {
            PatchManager.Dispose();
        }
    }
}
