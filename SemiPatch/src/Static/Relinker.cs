using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Mono.Cecil.Cil;
using ModTheGungeon;

namespace SemiPatch {
    public class Relinker {
        public Dictionary<string, string> FieldRenameMap;
        public List<KeyValuePair<FieldDefinition, string>> FieldDefinitionRenames;
        public Dictionary<string, string> MethodRenameMap;
        public List<KeyValuePair<MethodDefinition, string>> MethodDefinitionRenames;
        public Logger Logger = new Logger("Relinker");

        public Relinker() {
            FieldRenameMap = new Dictionary<string, string>();
            FieldDefinitionRenames = new List<KeyValuePair<FieldDefinition, string>>();
            MethodRenameMap = new Dictionary<string, string>();
            MethodDefinitionRenames = new List<KeyValuePair<MethodDefinition, string>>();
        }

        public void QueueFieldRename(FieldDefinition field, string name) {
            var prefixed_sig = field.DeclaringType.PrefixSignature(field.BuildSignature());
            Logger.Debug($"Queued field rename: '{prefixed_sig}' to '{name}'");
            FieldRenameMap[prefixed_sig] = name;
            FieldDefinitionRenames.Add(new KeyValuePair<FieldDefinition, string>(field, name));
        }

        public void QueueMethodRename(MethodDefinition method, string name) {
            var prefixed_sig = method.DeclaringType.PrefixSignature(method.BuildSignature());
            Logger.Debug($"Queued method rename: '{prefixed_sig}' to '{name}'");
            MethodRenameMap[prefixed_sig] = name;
            MethodDefinitionRenames.Add(new KeyValuePair<MethodDefinition, string>(method, name));
        }

        protected string TryGetFieldName(string sig) {
            if (FieldRenameMap.TryGetValue(sig, out string name)) return name;
            return null;
        }

        protected string TryGetMethodName(string sig) {
            if (MethodRenameMap.TryGetValue(sig, out string name)) return name;
            return null;
        }

        public void Relink(MethodReference method) {
            Logger.Debug($"Relinking method {method.BuildSignature()}");

            var m = method.Resolve();

            if (m.Body == null) return;

            var il = m.Body.GetILProcessor();
            var instrs = m.Body.Instructions;

            m.Body.SimplifyMacros();
            for (var i = 0; i < instrs.Count; i++) {
                var instr = instrs[i];

                if (instr.Operand is MethodReference) {
                    var call_target = (MethodReference)instr.Operand;
                    string sig;
                    if (call_target is GenericInstanceMethod) {
                        var generic_method = ((GenericInstanceMethod)instr.Operand);
                        sig = call_target.DeclaringType.PrefixSignature(generic_method.Resolve().BuildSignature());
                    } else {
                        sig = call_target.DeclaringType.PrefixSignature(call_target.BuildSignature());
                    }
                    var new_name = TryGetMethodName(sig);
                    if (new_name != null) {
                        Logger.Debug($"Mapped method {sig} to name {new_name}");
                        var elem = call_target.GetElementMethod();
                        elem.Name = new_name;
                        instr.Operand = elem;

                    }
                } else if (instr.OpCode == OpCodes.Ldfld
                    || instr.OpCode == OpCodes.Ldflda
                    || instr.OpCode == OpCodes.Stfld
                    || instr.OpCode == OpCodes.Ldsfld
                    || instr.OpCode == OpCodes.Ldsflda
                    || instr.OpCode == OpCodes.Stsfld) {
                    var field = (FieldReference)instr.Operand;
                    var sig = field.DeclaringType.PrefixSignature(field.BuildSignature());
                    var new_name= TryGetFieldName(sig);
                    if (new_name != null) {
                        Logger.Debug($"Mapped field {field.BuildSignature()} to name {new_name}");
                        field.Name = new_name;
                    }
                }
            }
            m.Body.OptimizeMacros();
        }

        public void Relink(TypeDefinition type) {
            var types = type.NestedTypes;
            for (var i = 0; i < types.Count; i++) {
                Relink(types[i]);
            }

            Logger.Debug($"Relinking type {type.BuildSignature()}");

            for (var i = 0; i < type.Methods.Count; i++) {
                Relink(type.Methods[i]);
            }
        }

        public void FixDefinitions() {
            for (var i = 0; i < FieldDefinitionRenames.Count; i++) {
                var pair = FieldDefinitionRenames[i];
                pair.Key.Name = pair.Value;
            }

            for (var i = 0; i < MethodDefinitionRenames.Count; i++) {
                var pair = MethodDefinitionRenames[i];
                pair.Key.Name = pair.Value;
            }
        }

        public void Relink(ModuleDefinition module) {
            Logger.Debug($"Relinking module {module.Name}");
            var types = module.Types;
            for (var i = 0; i < types.Count; i++) {
                Relink(types[i]);
            }
        }
    }
}
