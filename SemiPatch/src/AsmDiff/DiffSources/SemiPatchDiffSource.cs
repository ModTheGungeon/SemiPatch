using System;
using System.Collections.Generic;
using ModTheGungeon;
using Mono.Cecil;
using static SemiPatch.AssemblyDiff;

namespace SemiPatch {
    /// <summary>
    /// Produces a difference between the after-patch state of two SemiPatch
    /// metadata objects (<see cref="PatchData"/>). In other words, it produces
    /// an <see cref="AssemblyDiff"/> that represents the steps needed to take
    /// to go from the state of an assembly patched using the old metadata to the
    /// state that it would be in if it was patched using the new metadata.
    /// </summary>
    public struct SemiPatchDiffSource : IDiffSource {
        public PatchData OldPatchData;
        public PatchData NewPatchData;
        public static Logger Logger = new Logger(nameof(SemiPatchDiffSource));

        public SemiPatchDiffSource(PatchData old_data, PatchData new_data) {
            OldPatchData = old_data;
            NewPatchData = new_data;
        }

        public void DoubleSearchMembers<T> (
            MemberType member_type,
            IList<MemberDifference> diffs,
            IList<T> old_members,
            IList<T> new_members
        ) where T : PatchMemberData {
            var member_type_name = member_type.ToString();
            var old_member_map = new Dictionary<MemberPath, T>();
            var new_member_map = new Dictionary<MemberPath, T>();

            for (var i = 0; i < old_members.Count; i++) {
                var member = old_members[i];
                if (member is PatchMethodData) {
                    if (((PatchMethodData)(object)member).FalseDefaultConstructor) continue;
                }

                old_member_map[member.TargetPath] = member;
            }

            for (var i = 0; i < new_members.Count; i++) {
                var member = new_members[i];
                if (member is PatchMethodData) {
                    if (((PatchMethodData)(object)member).FalseDefaultConstructor) continue;
                }

                new_member_map[member.TargetPath] = member;

                if (old_member_map.TryGetValue(member.TargetPath, out T old_member)) {
                    if (member != old_member) {
                        Logger.Debug($"{member_type_name} changed (patch changed): {member.TargetPath} patched in {member.PatchPath}");
                        diffs.Add(new MemberChanged(member.PatchMember, member.TargetPath));
                    }
                } else {
                    if (member.IsInsert) {
                        Logger.Debug($"{member_type_name} added (insert patch added: {member.TargetPath} patched (added) in {member.PatchPath}");
                        diffs.Add(new MemberAdded(member.PatchMember, member.TargetPath));
                    } else {
                        Logger.Debug($"{member_type_name} changed (patch added): {member.TargetPath} patched in {member.PatchPath}");
                        diffs.Add(new MemberChanged(member.PatchMember, member.TargetPath));
                    }
                }
            }

            for (var i = 0; i < old_members.Count; i++) {
                var member = old_members[i];
                if (member is PatchMethodData) {
                    if (((PatchMethodData)(object)member).FalseDefaultConstructor) continue;
                }

                if (!old_member_map.ContainsKey(member.TargetPath)) {
                    if (member.IsInsert) {
                        Logger.Debug($"{member_type_name} removed (insert patch removed): {member.TargetPath} patched (removed) in {member.PatchPath}");
                        diffs.Add(new MemberRemoved(member.TargetPath));
                    } else {
                        Logger.Debug($"{member_type_name} changed (patch removed): {member.TargetPath} patched in {member.PatchPath}");
                        diffs.Add(new MemberChanged(member.PatchMember, member.TargetPath));
                    }
                }
            }
        }

        public TypeChanged CalculateTypeChange(PatchTypeData old_type, PatchTypeData new_type) {
            var change = new TypeChanged(old_type.TargetType.Resolve(), old_type.TargetType.Resolve());
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
            if (change.MemberDifferences.Count == 0 && change.NestedTypeDifferences.Count == 0) return null;
            return change;
        }

        public TypeChanged GetFullPatchTypeChange(PatchTypeData type_data, bool is_unpatch = false) {
            var change = new TypeChanged(type_data.TargetType.Resolve(), is_unpatch ? type_data.TargetType.Resolve() : type_data.PatchType);
            for (var i = 0; i < type_data.Methods.Count; i++) {
                var method_data = type_data.Methods[i];
                change.MemberDifferences.Add(new MemberChanged(is_unpatch ? method_data.TargetMember : method_data.PatchMember, method_data.TargetPath));
            }
            return change;
        }

        public void DoubleSearchTypes(IList<TypeDifference> diffs, IList<PatchTypeData> old_types, IList<PatchTypeData> new_types) {
            var old_type_map = new Dictionary<string, PatchTypeData>();
            var new_type_map = new Dictionary<string, PatchTypeData>();

            for (var i = 0; i < old_types.Count; i++) {
                var type = old_types[i];
                old_type_map[type.TargetType.BuildPrefixedSignature()] = type;
            }

            for (var i = 0; i < new_types.Count; i++) {
                var type = new_types[i];
                var sig = type.TargetType.BuildPrefixedSignature();
                new_type_map[sig] = type;

                if (old_type_map.TryGetValue(sig, out PatchTypeData old_type)) {
                    var change = CalculateTypeChange(old_type, type);
                    if (change != null) {
                        Logger.Debug($"Type changed (patch changed): target {sig} patched in {type.PatchType.BuildPrefixedSignature()}");
                        diffs.Add(change);
                    }
                } else {
                    Logger.Debug($"Type changed (patch added): target {sig} patched in {type.PatchType.BuildPrefixedSignature()}");
                    diffs.Add(GetFullPatchTypeChange(type));
                }
            }

            for (var i = 0; i < old_types.Count; i++) {
                var type = old_types[i];
                var sig = type.TargetType.BuildPrefixedSignature();

                if (!new_type_map.ContainsKey(sig)) {
                    Logger.Debug($"Type changed (patch removed): target {sig} patched in {type.PatchType.BuildPrefixedSignature()}");
                    diffs.Add(GetFullPatchTypeChange(type, is_unpatch: true));
                }
            }
        }

        public void ProduceDifference(IList<TypeDifference> diffs) {
            DoubleSearchTypes(diffs, OldPatchData.Types, NewPatchData.Types);
        }
    }
}
