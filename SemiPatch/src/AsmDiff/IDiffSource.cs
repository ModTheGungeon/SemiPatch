using System;
namespace SemiPatch.RDAR {
    public interface IDiffSource {
        AssemblyDiff ProduceDifference();
    }
}
