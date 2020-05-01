﻿using System;
using System.Collections.Generic;
using ModTheGungeon;
using Mono.Cecil;
using Mono.Cecil.Cil;
using static SemiPatch.AssemblyDiff;

namespace SemiPatch {
    public struct CILAbsoluteDiffSource : IDiffSource {
        public AbsoluteDiffSourceMode Mode;
        public ModuleDefinition Module;
        public HashSet<string> ExcludedTypeAttributeSignatures;
        public static Logger Logger = new Logger(nameof(CILAbsoluteDiffSource));

        public CILAbsoluteDiffSource(AbsoluteDiffSourceMode mode, ModuleDefinition mod) {
            Mode = mode;
            Module = mod;
            ExcludedTypeAttributeSignatures = new HashSet<string>();
        }

        public bool IsTypeExcluded(TypeDefinition type) {
            for (var i = 0; i < type.CustomAttributes.Count; i++) {
                var attr = type.CustomAttributes[i];
                if (attr.AttributeType.IsSame(SemiPatch.PatchAttribute)) return true;
                var prefixed_sig = attr.AttributeType.BuildPrefixedSignature();
                if (ExcludedTypeAttributeSignatures.Contains(prefixed_sig)) return true;
            }
            return false;
        }

        public void ExcludeTypesWithAttribute(TypeReference attr) {
            var prefixed_sig = attr.Resolve().BuildPrefixedSignature();
            Logger.Debug($"Excluded types with attribute: '{prefixed_sig}'");
            ExcludedTypeAttributeSignatures.Add(prefixed_sig);
        }

        public void ProduceDifference(IList<TypeDifference> diffs) {
            for (var i = 0; i < Module.Types.Count; i++) {
                var type = Module.Types[i];
                if (IsTypeExcluded(type)) continue;

                // if declaring type is removed, nested types are as well

                if (Mode == AbsoluteDiffSourceMode.AllAdded) {
                    diffs.Add(new TypeAdded(type));
                } else diffs.Add(new TypeRemoved(type));
            }
        }
    }
}
