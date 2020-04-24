using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ModTheGungeon;

namespace SemiPatch {
    /// <summary>
    /// Representation of a difference between assemblies. This may be used
    /// for purposes other than a direct diff between two DLLs - its main purpose
    /// is to represent the steps required to go from one state of an assembly
    /// to another. For example, <see cref="SemiPatchDiffSource"/> produces
    /// <code>AssemblyDiff</code> to represent how an assembly should be patched
    /// based on two iterations of metadata objects, unrelated to how the actual
    /// patches look in bytecode.
    /// </summary>
    public partial struct AssemblyDiff {
        public IList<TypeDifference> TypeDifferences;

        public AssemblyDiff(IList<TypeDifference> diffs) {
            TypeDifferences = diffs;
        }
    }
}
