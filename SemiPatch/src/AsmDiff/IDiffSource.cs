using System;
using System.Collections.Generic;
using static SemiPatch.AssemblyDiff;

namespace SemiPatch {
    public interface IDiffSource {
        void ProduceDifference(IList<TypeDifference> diffs);
    }
}
