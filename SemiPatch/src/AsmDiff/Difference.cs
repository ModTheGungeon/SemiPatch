using System;
using System.Collections.Generic;
using Mono.Cecil;

namespace SemiPatch {
    public partial struct AssemblyDiff {
        /// <summary>
        /// Abstract class that represents a diff of a type.
        /// </summary>
        public abstract class TypeDifference {
            public abstract bool ExistsInOld { get; }
            public abstract bool ExistsInNew { get; }
        }

        /// <summary>
        /// Represents a changed type, that is a type in which the set of members
        /// and/or the metadata are different.
        /// </summary>
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

        /// <summary>
        /// Represents an added type.
        /// </summary>
        public class TypeAdded : TypeDifference {
            public TypeDefinition Type;

            public override bool ExistsInOld => false;
            public override bool ExistsInNew => true;

            internal TypeAdded(TypeDefinition type) {
                Type = type;
            }
        }

        /// <summary>
        /// Represents a removed type.
        /// </summary>
        public class TypeRemoved : TypeDifference {
            public TypeDefinition Type;

            public override bool ExistsInOld => true;
            public override bool ExistsInNew => false;

            internal TypeRemoved(TypeDefinition type) {
                Type = type;
            }
        }

        /// <summary>
        /// Abstract class that represents the diff of a type member.
        /// </summary>
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

        /// <summary>
        /// Represents a member that had its data changed (e.g. the contents of
        /// a method, or the attributes on other members).
        /// </summary>
        public class MemberChanged : MemberDifference {
            public override bool ExistsInOld => true;
            public override bool ExistsInNew => true;

            public MemberChanged(IMemberDefinition member, MemberPath path)
            : base(member, path) {}
        }

        /// <summary>
        /// Represents an added member.
        /// </summary>
        public class MemberAdded : MemberDifference {
            public override bool ExistsInOld => false;
            public override bool ExistsInNew => true;

            public MemberAdded(IMemberDefinition member, MemberPath path)
            : base(member, path) { }
        }

        /// <summary>
        /// Represents a removed member.
        /// </summary>
        public class MemberRemoved : MemberDifference {
            public override bool ExistsInOld => true;
            public override bool ExistsInNew => false;

            public MemberRemoved(MemberPath path)
            : base(null, path) { }
        }
    }
}
