using System;
namespace SemiPatch {
    public class UnsupportedRDAROperationException : SemiPatchException {
        public UnsupportedRDAROperationException(AssemblyDiff.MemberDifference diff)
        : base($"RuntimeDetour-Assisted Reloading does not support applying this diff: {diff}") { }

        public UnsupportedRDAROperationException(AssemblyDiff.TypeDifference diff)
        : base($"RuntimeDetour-Assisted Reloading does not support applying this diff: {diff}") { }
    }
}
