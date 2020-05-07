using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Utils;

namespace SemiPatch {
    public static class Extensions {
        public static MethodBody CloneBodyAndReimport(this MethodBody body, ModuleDefinition module, MethodDefinition new_owner) {
            var new_body = body.Clone(new_owner);
            for (var i = 0; i < body.Variables.Count; i++) {
                new_body.Variables[i].VariableType = module.ImportReference(body.Variables[i].VariableType);
            }

            for (var i = 0; i < body.Instructions.Count; i++) {
                var new_instr = new_body.Instructions[i];
                var old_instr = body.Instructions[i];

                if (old_instr.Operand is IMetadataTokenProvider mref) new_instr.Operand = module.ImportReference(mref);
                if (old_instr.Operand is ParameterReference) {
                    var param = (ParameterReference)old_instr.Operand;
                    new_instr.Operand = new_owner.Parameters[param.Index];
                }
            }

            return new_body;
        }
    }
}
