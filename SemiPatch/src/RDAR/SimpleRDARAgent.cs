using System;
using System.Collections.Generic;
using ModTheGungeon;
using Mono.Cecil;
using Mono.Cecil.Cil;
using static SemiPatch.AssemblyDiff;

namespace SemiPatch.RDAR {
    public class SimpleRDARAgent : IRDARAgent {
        public ModuleDefinition OldModule;
        public ModuleDefinition NewModule;
        public HashSet<string> ExcludedTypeAttributeSignatures;
        public static Logger Logger = new Logger(nameof(SimpleRDARAgent));

        public SimpleRDARAgent(ModuleDefinition old_mod, ModuleDefinition new_mod) {
            OldModule = old_mod;
            NewModule = new_mod;
            ExcludedTypeAttributeSignatures = new HashSet<string>();
        }

        public int CalculateMethodBodyHashCode(MethodBody body) {
            var x = body.Instructions.Count;
            for (var i = 0; i < body.Instructions.Count; i++) {
                var instr = body.Instructions[i];
                x ^= instr.ToString().GetHashCode();
            }
            return x;
        }

        public int CalculateCustomAttributeHashCode(CustomAttribute attrib) {
            var x = attrib.Constructor.DeclaringType.PrefixSignature(attrib.Constructor.BuildSignature()).GetHashCode();
            for (var i = 0; i < attrib.ConstructorArguments.Count; i++) {
                var arg = attrib.ConstructorArguments[i];
                x ^= arg.ToString().GetHashCode();
            }
            for (var i = 0; i < attrib.Fields.Count; i++) {
                var field = attrib.Fields[i];
                x ^= field.Name.GetHashCode();
                x ^= field.Argument.Type.BuildSignature().GetHashCode();
                x ^= field.Argument.Value.ToString().GetHashCode();
            }
            for (var i = 0; i < attrib.Properties.Count; i++) {
                var prop = attrib.Properties[i];
                x ^= prop.Name.GetHashCode();
                x ^= prop.Argument.Type.BuildSignature().GetHashCode();
                x ^= prop.Argument.Value.ToString().GetHashCode();
            }
            return x;
        }

        public int CalculateMethodHashCode(MethodDefinition method) {
            var x = CalculateMethodBodyHashCode(method.Body);
            for (var i = 0; i < method.CustomAttributes.Count; i++) {
                var attrib = method.CustomAttributes[i];
                x ^= CalculateCustomAttributeHashCode(attrib);
            }
            x ^= (int)method.Attributes * 2663;
            x ^= (int)method.RVA * 4547;
            x ^= (int)method.ImplAttributes * 6983;
            x ^= (int)method.SemanticsAttributes * 9811;

            return x;
        }

        public int CalculateFieldHashCode(FieldDefinition field) {
            var x = field.BuildSignature().GetHashCode();
            for (var i = 0; i < field.CustomAttributes.Count; i++) {
                var attrib = field.CustomAttributes[i];
                x ^= CalculateCustomAttributeHashCode(attrib);
            }
            x ^= (int)field.Attributes * 2663;
            x ^= (int)field.RVA * 4547;
            return x;
        }

        public int CalculatePropertyHashCode(PropertyDefinition prop) {
            var x = prop.BuildSignature().GetHashCode();
            for (var i = 0; i < prop.CustomAttributes.Count; i++) {
                var attrib = prop.CustomAttributes[i];
                x ^= CalculateCustomAttributeHashCode(attrib);
            }
            x ^= (int)prop.Attributes * 2663;

            if (prop.GetMethod != null) x ^= CalculateMethodHashCode(prop.GetMethod);
            if (prop.SetMethod != null) x ^= CalculateMethodHashCode(prop.SetMethod);

            return x;
        }

        public void DoubleSearchMethods(IList<MemberDifference> diffs, Mono.Collections.Generic.Collection<MethodDefinition> old_methods, Mono.Collections.Generic.Collection<MethodDefinition> new_methods) {
            var old_method_map = new Dictionary<string, int>();
            var new_method_map = new Dictionary<string, int>();

            for (var i = 0; i < old_methods.Count; i++) {
                var method = old_methods[i];
                old_method_map[method.BuildSignature()] = i;
            }

            for (var i = 0; i < new_methods.Count; i++) {
                var method = new_methods[i];
                var sig = method.BuildSignature();
                new_method_map[sig] = i;
                if (old_method_map.TryGetValue(sig, out int old_method_idx)) {
                    var old_method = old_methods[old_method_idx];
                    var old_hash = CalculateMethodHashCode(old_method);
                    var new_hash = CalculateMethodHashCode(method);

                    if (old_hash != new_hash) {
                        Logger.Debug($"Method changed: {sig} (old hash: {old_hash}, new hash: {new_hash})");
                        var diff = new MethodChanged(method, method.ToPath());
                        diffs.Add(diff);
                    }
                } else {
                    Logger.Debug($"Method added: {sig}");
                    var diff = new MethodAdded(method, method.ToPath());
                    diffs.Add(diff);
                }
            }

            for (var i = 0; i < old_methods.Count; i++) {
                var method = old_methods[i];
                var sig = method.BuildSignature();
                if (!new_method_map.ContainsKey(sig)) {
                    Logger.Debug($"Method removed: {sig}");
                    var diff = new MethodRemoved(method, method.ToPath());
                    diffs.Add(diff);
                }
            }
        }

        public void DoubleSearchFields(IList<MemberDifference> diffs, Mono.Collections.Generic.Collection<FieldDefinition> old_fields, Mono.Collections.Generic.Collection<FieldDefinition> new_fields) {
            var old_field_map = new Dictionary<string, int>();
            var new_field_map = new Dictionary<string, int>();

            for (var i = 0; i < old_fields.Count; i++) {
                var field = old_fields[i];
                old_field_map[field.BuildSignature()] = i;
            }

            for (var i = 0; i < new_fields.Count; i++) {
                var field = new_fields[i];
                var sig = field.BuildSignature();
                new_field_map[sig] = i;
                if (old_field_map.TryGetValue(sig, out int old_field_idx)) {
                    var old_field = old_fields[old_field_idx];
                    var old_hash = CalculateFieldHashCode(old_field);
                    var new_hash = CalculateFieldHashCode(field);

                    if (old_hash != new_hash) {
                        Logger.Debug($"Field changed: {sig} (old hash: {old_hash}, new hash: {new_hash})");
                        var diff = new FieldChanged(field, field.ToPath());
                        diffs.Add(diff);
                    }
                } else {
                    Logger.Debug($"Field added: {sig}");
                    var diff = new FieldAdded(field, field.ToPath());
                    diffs.Add(diff);
                }
            }

            for (var i = 0; i < old_fields.Count; i++) {
                var field = old_fields[i];
                var sig = field.BuildSignature();
                if (!new_field_map.ContainsKey(sig)) {
                    Logger.Debug($"Field removed: {sig}");
                    var diff = new FieldRemoved(field, field.ToPath());
                    diffs.Add(diff);
                }
            }
        }

        public void DoubleSearchProperties(IList<MemberDifference> diffs, Mono.Collections.Generic.Collection<PropertyDefinition> old_props, Mono.Collections.Generic.Collection<PropertyDefinition> new_props) {
            var old_prop_map = new Dictionary<string, int>();
            var new_prop_map = new Dictionary<string, int>();

            for (var i = 0; i < old_props.Count; i++) {
                var prop = old_props[i];
                old_prop_map[prop.BuildSignature()] = i;
            }

            for (var i = 0; i < new_props.Count; i++) {
                var prop = new_props[i];
                var sig = prop.BuildSignature();
                new_prop_map[sig] = i;
                if (old_prop_map.TryGetValue(sig, out int old_prop_idx)) {
                    var old_prop = old_props[old_prop_idx];
                    var old_hash = CalculatePropertyHashCode(old_prop);
                    var new_hash = CalculatePropertyHashCode(prop);

                    if (old_hash != new_hash) {
                        Logger.Debug($"Property changed: {sig} (old hash: {old_hash}, new hash: {new_hash})");
                        var diff = new PropertyChanged(prop, prop.ToPath());
                        diffs.Add(diff);
                    }
                } else {
                    Logger.Debug($"Property added: {sig}");
                    var diff = new PropertyAdded(prop, prop.ToPath());
                    diffs.Add(diff);
                }
            }

            for (var i = 0; i < old_props.Count; i++) {
                var prop = old_props[i];
                var sig = prop.BuildSignature();
                if (!new_prop_map.ContainsKey(sig)) {
                    Logger.Debug($"Property removed: {sig}");
                    var diff = new PropertyRemoved(prop, prop.ToPath());
                    diffs.Add(diff);
                }
            }
        }


        public TypeChanged CalculateTypeChange(TypeDefinition old_type, TypeDefinition new_type) {
            var change = new TypeChanged(old_type, new_type);
            DoubleSearchMethods(change.MemberDifferences, old_type.Methods, new_type.Methods);
            DoubleSearchFields(change.MemberDifferences, old_type.Fields, new_type.Fields);
            DoubleSearchProperties(change.MemberDifferences, old_type.Properties, new_type.Properties);
            DoubleSearchTypes(change.NestedTypeDifferences, old_type.NestedTypes, new_type.NestedTypes);
            if (change.MemberDifferences.Count == 0 && change.NestedTypeDifferences.Count == 0) return null;
            return change;
        }

        public bool IsTypeExcluded(TypeDefinition type) {
            for (var i = 0; i < type.CustomAttributes.Count; i++) {
                var attr = type.CustomAttributes[i];
                var prefixed_sig = attr.AttributeType.BuildPrefixedSignature();
                if (ExcludedTypeAttributeSignatures.Contains(prefixed_sig)) return true;
            }
            return false;
        }

        public void DoubleSearchTypes(IList<TypeDifference> diffs, Mono.Collections.Generic.Collection<TypeDefinition> old_types, Mono.Collections.Generic.Collection<TypeDefinition> new_types) {
            var old_type_map = new Dictionary<string, int>();
            var new_type_map = new Dictionary<string, int>();

            for (var i = 0; i < old_types.Count; i++) {
                var type = old_types[i];
                if (IsTypeExcluded(type)) continue;
                old_type_map[type.BuildSignature()] = i;
            }

            for (var i = 0; i < new_types.Count; i++) {
                var type = new_types[i];
                if (IsTypeExcluded(type)) continue;

                var sig = type.BuildSignature();
                new_type_map[sig] = i;
                if (old_type_map.TryGetValue(sig, out int old_type_idx)) {
                    var old_type = old_types[old_type_idx];
                    var diff = CalculateTypeChange(old_type, type);
                    if (diff != null) {
                        Logger.Debug($"Type changed: {sig} - {diff.MemberDifferences.Count} member diff(s), {diff.NestedTypeDifferences.Count} nested type diff(s)");
                        diffs.Add(diff);
                    }
                } else {
                    Logger.Debug($"Type added: {sig}");
                    var diff = new TypeAdded(type);
                    diffs.Add(diff);
                }
            }

            for (var i = 0; i < old_types.Count; i++) {
                var type = old_types[i];
                if (IsTypeExcluded(type)) continue;

                var sig = type.BuildSignature();
                if (!new_type_map.ContainsKey(sig)) {
                    Logger.Debug($"Type removed: {sig}");
                    var diff = new TypeRemoved(type);
                    diffs.Add(diff);
                }
            }
        }

        public void ExcludeTypesWithAttribute(TypeReference attr) {
            var prefixed_sig = attr.Resolve().BuildPrefixedSignature();
            Logger.Debug($"Excluded types with attribute: '{prefixed_sig}'");
            ExcludedTypeAttributeSignatures.Add(prefixed_sig);
        }

        public AssemblyDiff ProduceDifference() {
            var diffs = new List<TypeDifference>();
            DoubleSearchTypes(diffs, OldModule.Types, NewModule.Types);
            return new AssemblyDiff(diffs);
        }
    }
}
