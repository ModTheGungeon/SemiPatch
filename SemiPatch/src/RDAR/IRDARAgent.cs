using System;
namespace SemiPatch.RDAR {
    public interface IRDARAgent {
        AssemblyDiff ProduceDifference();
    }
}
