using System;
using System.Collections.Generic;
using ModTheGungeon;
using Mono.Cecil;

namespace SemiPatch.MonoMod {
    public class SingleTargetClient {
        public enum Result {
            Success,
            RequiresStaticPatching,
            NoChanges
        }

        public struct IdentifiedReloadableModule {
            public string Identifier;
            public ReloadableModule Module;
        }

        public readonly RuntimePatchManager PatchManager;
        public readonly ModuleDefinition TargetModule;
        public readonly ModuleDefinition RunningModule;
        public Relinker Relinker;

        public Logger Logger;

        public SingleTargetClient(System.Reflection.Assembly asm, ModuleDefinition target_module, ModuleDefinition running_module) {
            PatchManager = new RuntimePatchManager(asm, running_module);
            TargetModule = target_module;
            RunningModule = running_module;
            Logger = new Logger($"SingleTargetClient({TargetModule.Name})");
        }

        public void Reset() {
            Relinker = new Relinker();
        }

        public void Preload(ReloadableModule module) {
            Relinker.LoadRelinkMapFrom(module.PatchData, RunningModule);
        }

        public Result Process(ReloadableModule old_module, ReloadableModule new_module) {
            var diff = ReloadableModule.Compare(old_module, new_module);
            if (!diff.HasChanges) return Result.NoChanges;
            if (!PatchManager.CanPatchAtRuntime(diff)) return Result.RequiresStaticPatching;

            PatchManager.ProcessDifference(Relinker, diff, update_running_module: false);
            return Result.Success;
        }

        //public void CommitUpdate() {
        //    foreach (var sched_rm in _ScheduledReloadableModules) {
        //        ReloadableModule loaded_rm;

        //        if (_LoadedReloadableModules.TryGetValue(sched_rm.Key, out loaded_rm)) {

        //        } else {
        //            var source = new 
        //        }
        //    }
        //    _PatchManager.ProcessDifference
        //}

    }
}
