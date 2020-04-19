using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ModTheGungeon;

namespace SemiPatch {
    public partial struct AssemblyDiff {
        public IList<TypeDifference> TypeDifferences;

        public AssemblyDiff(IList<TypeDifference> diffs) {
            TypeDifferences = diffs;
        }
    }
}
