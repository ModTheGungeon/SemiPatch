using System;
using System.Collections.Generic;
using static SemiPatch.AssemblyDiff;

namespace SemiPatch {
    /// <summary>
    /// Interface that represents an object that is capable of producing
    /// a list of <see cref="TypeDifference"/>s. For examples see
    /// <see cref="CILDiffSource"/> or <see cref="SemiPatchDiffSource"/>.
    /// </summary>
    public interface IDiffSource {
        void ProduceDifference(IList<TypeDifference> diffs);
    }
}
