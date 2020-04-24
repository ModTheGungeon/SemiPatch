using System;
namespace SemiPatch {
    public interface IDiffSource {
        AssemblyDiff ProduceDifference();
    }
}
