using System;
using System.Collections.Generic;
using Mono.Cecil;

namespace SemiPatch {
    public partial struct AssemblyDiff {
        public abstract class TypeDifference {
            public abstract bool ExistsInOld { get; }
            public abstract bool ExistsInNew { get; }
        }

        public class TypeChanged : TypeDifference {
            public TypeDefinition OldType;
            public TypeDefinition NewType;
            public IList<TypeDifference> NestedTypeDifferences;
            public IList<MemberDifference> MemberDifferences;

            public override bool ExistsInOld => true;
            public override bool ExistsInNew => true;

            internal TypeChanged(TypeDefinition old_type, TypeDefinition new_type) {
                OldType = old_type;
                NewType = new_type;
                MemberDifferences = new List<MemberDifference>();
                NestedTypeDifferences = new List<TypeDifference>();
            }
        }

        public class TypeAdded : TypeDifference {
            public TypeDefinition Type;

            public override bool ExistsInOld => false;
            public override bool ExistsInNew => true;

            internal TypeAdded(TypeDefinition type) {
                Type = type;
            }
        }

        public class TypeRemoved : TypeDifference {
            public TypeDefinition Type;

            public override bool ExistsInOld => true;
            public override bool ExistsInNew => false;

            internal TypeRemoved(TypeDefinition type) {
                Type = type;
            }
        }

        public abstract class MemberDifference {
            public abstract bool ExistsInOld { get; }
            public abstract bool ExistsInNew { get; }
            public MemberType Type;

            public IMemberDefinition Member;
            public MemberPath TargetPath;

            protected MemberDifference(IMemberDefinition member, MemberPath target_path) {
                Type = target_path.Type;
                Member = member;
                TargetPath = target_path;
            }
        }

        public class MemberChanged : MemberDifference {
            public override bool ExistsInOld => true;
            public override bool ExistsInNew => true;

            public MemberChanged(IMemberDefinition member, MemberPath path)
            : base(member, path) {}
        }

        public class MemberAdded : MemberDifference {
            public override bool ExistsInOld => false;
            public override bool ExistsInNew => true;

            public MemberAdded(IMemberDefinition member, MemberPath path)
            : base(member, path) { }
        }

        public class MemberRemoved : MemberDifference {
            public override bool ExistsInOld => true;
            public override bool ExistsInNew => false;

            public MemberRemoved(MemberPath path)
            : base(null, path) { }
        }
    }
}
