using System;
using System.Collections.Generic;
using ModTheGungeon;
using Mono.Cecil;
using static SemiPatch.AssemblyDiff;

namespace SemiPatch {
    /// <summary>
    /// Produces a difference using only one SemiPatch metadata as input by either comparing
    /// its contents to nothing (<see cref="AbsoluteDiffSourceMode.AllRemoved"/>),
    /// or by comparing nothing to its contents (<see cref="AbsoluteDiffSourceMode.AllAdded"/>).
    /// </summary>
    public struct SemiPatchAbsoluteDiffSource : IDiffSource {
        public AbsoluteDiffSourceMode Mode;
        public PatchData PatchData;
        public static Logger Logger = new Logger(nameof(SemiPatchAbsoluteDiffSource));

        public SemiPatchAbsoluteDiffSource(AbsoluteDiffSourceMode mode, PatchData data) {
            Mode = mode;
            PatchData = data;
        }

        public MemberDifference GetMemberDifference(MemberType member_type, PatchMemberData member) {
            if (member.IsInsert) {
                if (Mode == AbsoluteDiffSourceMode.AllAdded) {
                    return new MemberAdded(member.PatchMember, member.TargetPath);
                } else {
                    return new MemberRemoved(member.TargetPath);
                }
            } else {
                if (Mode == AbsoluteDiffSourceMode.AllAdded) {
                    return new MemberChanged(member.PatchMember, member.TargetPath);
                } else {
                    return new MemberChanged(member.TargetMember, member.TargetPath);
                }
            }
        }

        public InjectionDifference GetInjectionDifference(PatchInjectData inject) {
            if (Mode == AbsoluteDiffSourceMode.AllAdded) {
                return new InjectionAdded(
                    inject.Target, inject.TargetPath,
                    inject.Handler, inject.HandlerPath,
                    inject.InjectionPoint, inject.LocalCaptures, inject.Position
                );
            } else {
                return new InjectionRemoved(
                    inject.Target, inject.TargetPath,
                    inject.HandlerPath,
                    inject.InjectionPoint, inject.LocalCaptures, inject.Position
                );
            }
        }

        public void ProduceDifference(IList<TypeDifference> diffs) {
            for (var i = 0; i < PatchData.Types.Count; i++) {
                var type = PatchData.Types[i];
                var type_change = new TypeChanged(type.TargetType, type.TargetType);
                diffs.Add(type_change);

                for (var j = 0; j < type.Methods.Count; j++) {
                    type_change.MemberDifferences.Add(
                        GetMemberDifference(MemberType.Method, type.Methods[i])
                    );
                }

                for (var j = 0; j < type.Fields.Count; j++) {
                    type_change.MemberDifferences.Add(
                        GetMemberDifference(MemberType.Field, type.Fields[i])
                    );
                }

                for (var j = 0; j < type.Properties.Count; j++) {
                    type_change.MemberDifferences.Add(
                        GetMemberDifference(MemberType.Property, type.Properties[i])
                    );
                }

                for (var j = 0; j < type.Injections.Count; j++) {
                    type_change.InjectionDifferences.Add(
                        GetInjectionDifference(type.Injections[i])
                    );
                }
            }
        }
    }
}
