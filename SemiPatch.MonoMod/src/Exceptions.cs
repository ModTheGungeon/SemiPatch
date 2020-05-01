using System;
using Mono.Cecil;

namespace SemiPatch {
    public class InvalidTargetException : SemiPatchException {
        public ModuleDefinition ExpectedTarget;
        public ModuleDefinition ActualTarget;

        public InvalidTargetException(ModuleDefinition expected, ModuleDefinition actual)
        : base($"Invalid target in reloadable module: expected '{expected.Assembly.FullName}', but module was built against '{actual.Assembly.FullName}'.") {
            ExpectedTarget = expected;
            ActualTarget = actual;
        }
    }

    public class UnsupportedRDAROperationException : SemiPatchException {
        public UnsupportedRDAROperationException(AssemblyDiff.MemberDifference diff)
        : base($"RuntimeDetour-Assisted Reloading does not support applying this diff: {diff}") { }

        public UnsupportedRDAROperationException(AssemblyDiff.TypeDifference diff)
        : base($"RuntimeDetour-Assisted Reloading does not support applying this diff: {diff}") { }
    }
}
