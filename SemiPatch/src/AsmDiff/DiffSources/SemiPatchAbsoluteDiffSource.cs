using System;
using System.Collections.Generic;
using ModTheGungeon;
using Mono.Cecil;
using static SemiPatch.AssemblyDiff;

namespace SemiPatch {
    public struct SemiPatchAbsoluteDiffSource : IDiffSource {
        public AbsoluteDiffSourceMode Mode;
        public PatchData PatchData;
        public static Logger Logger = new Logger(nameof(SemiPatchAbsoluteDiffSource));

        public SemiPatchAbsoluteDiffSource(AbsoluteDiffSourceMode mode, PatchData data) {
            Mode = mode;
            PatchData = data;
        }

        public MemberDifference GetMemberDifference<T, U, V>(MemberType member_type, V member)
        where T : class, IMemberDefinition
        where U : MemberPath<T>
        where V : PatchMemberData<T, U> {
            if (member.IsInsert) {
                if (Mode == AbsoluteDiffSourceMode.AllAdded) {
                    return new MemberAdded<T, U>(member_type, member.Patch, member.TargetPath);
                } else {
                    return new MemberRemoved<T, U>(member_type, member.TargetPath);
                }
            } else {
                if (Mode == AbsoluteDiffSourceMode.AllAdded) {
                    return new MemberChanged<T, U>(MemberType.Method, member.Patch, member.TargetPath);
                } else {
                    return new MemberChanged<T, U>(MemberType.Method, member.Target, member.TargetPath);
                }
            }
        }

        public void ProduceDifference(IList<TypeDifference> diffs) {
            for (var i = 0; i < PatchData.Types.Count; i++) {
                var type = PatchData.Types[i];
                var type_change = new TypeChanged(type.TargetType, type.TargetType);
                diffs.Add(type_change);

                for (var j = 0; j < type.Methods.Count; j++) {
                    type_change.MemberDifferences.Add(
                        GetMemberDifference<MethodDefinition, MethodPath, PatchMethodData>(
                            MemberType.Method, type.Methods[i]
                        )
                    );
                }

                for (var j = 0; j < type.Fields.Count; j++) {
                    type_change.MemberDifferences.Add(
                        GetMemberDifference<FieldDefinition, FieldPath, PatchFieldData>(
                            MemberType.Field, type.Fields[i]
                        )
                    );
                }

                for (var j = 0; j < type.Properties.Count; j++) {
                    type_change.MemberDifferences.Add(
                        GetMemberDifference<PropertyDefinition, PropertyPath, PatchPropertyData>(
                            MemberType.Property, type.Properties[i]
                        )
                    );
                }
            }
        }
    }
}
