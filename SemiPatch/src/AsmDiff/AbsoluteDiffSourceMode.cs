using System;
namespace SemiPatch {
    /// <summary>
    /// Represents the mode of operation for "absolute diff sources"
    /// (<see cref="CILAbsoluteDiffSource"/>, <see cref="SemiPatchAbsoluteDiffSource"/>)
    /// , that is diff sources that take only one input and treat it as either added
    /// or removed in its entirety.
    /// </summary>
    public enum AbsoluteDiffSourceMode {
        AllRemoved,
        AllAdded
    }
}
