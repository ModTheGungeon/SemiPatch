using System;
using Mono.Cecil;

namespace SemiPatch {
    public class ExperimentalRuntimePatchManager : RuntimePatchManager {
        public ExperimentalRuntimePatchManager(System.Reflection.Assembly asm, ModuleDefinition mod)
            : base(asm, mod) { }

        protected override bool _CanPatchTypeAtRuntime(AssemblyDiff.TypeDifference type_diff) {
            return true;
        }

        protected override void _ProcessMethodDifference(Relinker relinker, AssemblyDiff.MemberDifference diff, bool update_running_module = false) {
            base._ProcessMethodDifference(relinker, diff, update_running_module);
        }
    }
}
