using System;
using System.Collections.Generic;
using ModTheGungeon;
using Mono.Cecil;
using Mono.Cecil.Cil;
using static SemiPatch.AssemblyDiff;

namespace SemiPatch {
    /// <summary>
    /// Capable of producing an <see cref="AssemblyDiff"/> based on the actual
    /// contents of two modules. Additionally, <see cref="ExcludeTypesWithAttribute(TypeReference)"/>
    /// can be used to exclude types tagged with certain attributes from the
    /// algorithm, such as <see cref="PatchAttribute"/> (so that the more appropriate
    /// <see cref="SemiPatchDiffSource"/> can be used separately).
    /// </summary>
    public struct CILDiffSource : IDiffSource {
        public ModuleDefinition OldModule;
        public ModuleDefinition NewModule;
        public HashSet<string> ExcludedTypeAttributeSignatures;
        public static Logger Logger = new Logger(nameof(CILDiffSource));

        public CILDiffSource(ModuleDefinition old_mod, ModuleDefinition new_mod) {
            OldModule = old_mod;
            NewModule = new_mod;
            ExcludedTypeAttributeSignatures = new HashSet<string>();
        }

        public void DoubleSearchMembers<T>(
            MemberType member_type,
            IList<MemberDifference> diffs,
            IList<T> old_members,
            IList<T> new_members
        )
        where T : class, IMemberDefinition {
            var old_method_map = new Dictionary<Signature, T>();
            var new_method_map = new Dictionary<Signature, T>();
            var member_type_name = member_type.ToString();

            for (var i = 0; i < old_members.Count; i++) {
                var member = old_members[i];
                var sig = Signature.FromInterface(member);
                old_method_map[sig] = member;
            }

            for (var i = 0; i < new_members.Count; i++) {
                var member = new_members[i];
                var sig = Signature.FromInterface(member);
                new_method_map[sig] = member;
                if (old_method_map.TryGetValue(sig, out T old_member)) {
                    var old_hash = old_member.CalculateHashCode();
                    var new_hash = member.CalculateHashCode();

                    if (old_hash != new_hash) {
                        Logger.Debug($"{member_type_name} changed: {sig} (old hash: {old_hash}, new hash: {new_hash})");
                        var diff = new MemberChanged(member, member.ToPathGeneric());
                        diffs.Add(diff);
                    }
                } else {
                    Logger.Debug($"{member_type_name} added: {sig}");
                    var diff = new MemberAdded(member, member.ToPathGeneric());
                    diffs.Add(diff);
                }
            }

            for (var i = 0; i < old_members.Count; i++) {
                var member = old_members[i];
                var sig = Signature.FromInterface(member);
                if (!new_method_map.ContainsKey(sig)) {
                    Logger.Debug($"{member_type_name} removed: {sig}");
                    var diff = new MemberRemoved(member.ToPathGeneric());
                    diffs.Add(diff);
                }
            }
        }


        public TypeChanged CalculateTypeChange(TypeDefinition old_type, TypeDefinition new_type) {
            var change = new TypeChanged(old_type, new_type);
            DoubleSearchMembers(
                MemberType.Method,
                change.MemberDifferences,
                old_type.Methods,
                new_type.Methods
            );
            DoubleSearchMembers(
               MemberType.Field,
               change.MemberDifferences,
               old_type.Fields,
               new_type.Fields
           );
            DoubleSearchMembers(
                MemberType.Property,
                change.MemberDifferences,
                old_type.Properties,
                new_type.Properties
            );
            DoubleSearchTypes(change.NestedTypeDifferences, old_type.NestedTypes, new_type.NestedTypes);
            if (change.MemberDifferences.Count == 0 && change.NestedTypeDifferences.Count == 0) return null;
            return change;
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

        public void ProduceDifference(IList<TypeDifference> diffs) {
            DoubleSearchTypes(diffs, OldModule.Types, NewModule.Types);
        }
    }
}
