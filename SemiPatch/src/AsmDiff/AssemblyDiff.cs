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
    /// to another.
    /// </summary>
    public partial struct AssemblyDiff {
        public IList<TypeDifference> TypeDifferences;

        public static AssemblyDiff Empty => new AssemblyDiff { TypeDifferences = new List<TypeDifference>() };

        public bool HasChanges => TypeDifferences != null && TypeDifferences.Count > 0;

        public AssemblyDiff(params IDiffSource[] sources) {
            TypeDifferences = new List<TypeDifference>();

            for (var i = 0; i < sources.Length; i++) {
                sources[i].ProduceDifference(TypeDifferences);
            }
        }
    }
}
