using System;
using System.Collections.Generic;
using ModTheGungeon;
using Mono.Cecil;
using static SemiPatch.AssemblyDiff;

namespace SemiPatch.RDAR {
    public class SemiPatchRDARAgent : IRDARAgent {
        public PatchData OldPatchData;
        public PatchData NewPatchData;
        public static Logger Logger = new Logger(nameof(SemiPatchRDARAgent));

        public SemiPatchRDARAgent(PatchData old_data, PatchData new_data) {
            OldPatchData = old_data;
            NewPatchData = new_data;
        }

        public void DoubleSearchMethods(IList<MemberDifference> diffs, IList<PatchMethodData> old_methods, IList<PatchMethodData> new_methods) {
            var old_method_map = new Dictionary<MethodPath, PatchMethodData>();
            var new_method_map = new Dictionary<MethodPath, PatchMethodData>();

            for (var i = 0; i < old_methods.Count; i++) {
                var method = old_methods[i];
                old_method_map[method.TargetPath] = method;
            }

            for (var i = 0; i < new_methods.Count; i++) {
                var method = new_methods[i];
                new_method_map[method.TargetPath] = method;

                if (old_method_map.TryGetValue(method.TargetPath, out PatchMethodData old_method)) {
                    if (method != old_method) {
                        Logger.Debug($"Method changed (patch changed): {method.TargetPath} patched in {method.PatchMethod.BuildPrefixedSignature()}");
                        diffs.Add(new MethodChanged(method.PatchMethod, method.TargetMethod.ToPath()));
                    }
                } else {
                    Logger.Debug($"Method changed (patch added): {method.TargetPath} patched in {method.PatchMethod.BuildPrefixedSignature()}");
                    diffs.Add(new MethodChanged(method.PatchMethod, method.TargetMethod.ToPath()));
                }
            }

            for (var i = 0; i < old_methods.Count; i++) {
                var method = old_methods[i];

                if (!old_method_map.ContainsKey(method.TargetPath)) {
                    Logger.Debug($"Method changed (patch removed): {method.TargetPath} patched in {method.PatchMethod.BuildPrefixedSignature()}");
                    diffs.Add(new MethodChanged(method.PatchMethod, method.TargetMethod.ToPath()));
                }
            }
        }

        public TypeChanged CalculateTypeChange(PatchTypeData old_type, PatchTypeData new_type) {
            var change = new TypeChanged(old_type.TargetType.Resolve(), old_type.TargetType.Resolve());
            DoubleSearchMethods(change.MemberDifferences, old_type.Methods, new_type.Methods);
            //DoubleSearchFields(change.MemberDifferences, old_type.Fields, new_type.Fields);
            //DoubleSearchProperties(change.MemberDifferences, old_type.Properties, new_type.Properties);
            //DoubleSearchTypes(change.NestedTypeDifferences, old_type.NestedTypes, new_type.NestedTypes);
            if (change.MemberDifferences.Count == 0 && change.NestedTypeDifferences.Count == 0) return null;
            return change;
        }

        public TypeChanged GetFullPatchTypeChange(PatchTypeData type_data, bool is_unpatch = false) {
            var change = new TypeChanged(type_data.TargetType.Resolve(), is_unpatch ? type_data.TargetType.Resolve() : type_data.PatchType);
            for (var i = 0; i < type_data.Methods.Count; i++) {
                var method_data = type_data.Methods[i];
                change.MemberDifferences.Add(new MethodChanged(is_unpatch ? method_data.TargetMethod : method_data.PatchMethod, method_data.TargetMethod.ToPath()));
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

        public AssemblyDiff ProduceDifference() {
            var diffs = new List<TypeDifference>();
            DoubleSearchTypes(diffs, OldPatchData.Types, NewPatchData.Types);
            return new AssemblyDiff(diffs);
        }
    }
}
