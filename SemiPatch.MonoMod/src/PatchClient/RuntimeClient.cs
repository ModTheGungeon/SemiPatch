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

        public struct QueuedReloadableModule {
            public ReloadableModule Module;
            public AssemblyDiff Diff;

            public QueuedReloadableModule(ReloadableModule mod, AssemblyDiff diff) {
                Module = mod;
                Diff = diff;
            }
        }

        public readonly RuntimePatchManager PatchManager;
        public ModuleDefinition RunningModule;
        public Relinker Relinker;

        private IDictionary<string, ReloadableModule> _ModuleMap;
        private IDictionary<string, QueuedReloadableModule> _QueuedModuleMap;

        public Logger Logger;

        public RuntimeClient(System.Reflection.Assembly asm, ModuleDefinition target_module, ModuleDefinition running_module)
        : base(target_module) {
            RunningModule = running_module;

            Relinker = new Relinker();
            PatchManager = new RuntimePatchManager(Relinker, asm, running_module);

            Logger = new Logger($"{nameof(RuntimeClient)}({TargetModule.Name})");

            _ModuleMap = new Dictionary<string, ReloadableModule>();
            _QueuedModuleMap = new Dictionary<string, QueuedReloadableModule>();
        }

        public override AddModuleResult AddModule(ReloadableModule mod) {
            Logger.Debug($"Adding module: {mod.Identifier}");

            ReloadableModule prev_mod = null;
            _ModuleMap.TryGetValue(mod.Identifier, out prev_mod);

            var diff = ReloadableModule.Compare(prev_mod, mod);

            if (!diff.HasChanges) return AddModuleResult.NoChanges;
            if (!PatchManager.CanPatchAtRuntime(diff)) return AddModuleResult.IncompatibleWithClient;

            _QueuedModuleMap[mod.Identifier] = new QueuedReloadableModule(mod, diff);

            return AddModuleResult.Success;
        }

        public override CommitResult Commit() {
            Logger.Debug($"Committing modules");

            // reset relinker only
            Relinker.Clear();

            // re-register modules that haven't changed in relinker
            foreach (var kv in _ModuleMap) {
                if (_QueuedModuleMap.TryGetValue(kv.Key, out QueuedReloadableModule prev_mod)) {
                    continue;
                }

                var mod = kv.Value;
                Relinker.LoadRelinkMapFrom(mod.PatchData, RunningModule);
            }

            foreach (var kv in _QueuedModuleMap) {
                ReloadableModule old_mod = null;
                _ModuleMap.TryGetValue(kv.Key, out old_mod);

                var new_mod = kv.Value;
                Relinker.LoadRelinkMapFrom(new_mod.Module.PatchData, RunningModule);
                PatchManager.ProcessDifference(new_mod.Diff, update_running_module: false);
                
                _ModuleMap[kv.Key] = new_mod.Module;
            }

            _QueuedModuleMap.Clear();

            PatchManager.FinalizeProcessing();

            return CommitResult.Success;
        }

        public override void Dispose() {
            PatchManager.Dispose();
        }
    }
}
