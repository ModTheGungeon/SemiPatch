using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SemiPatch {
    public partial struct AssemblyDiff {
        /// <summary>
        /// Abstract class that represents a diff of a type.
        /// </summary>
        public abstract class TypeDifference {
            public abstract bool ExistsInOld { get; }
            public abstract bool ExistsInNew { get; }

            public TypeDefinition OldType = null;
            public TypeDefinition NewType = null;

            public override string ToString() {
                if (OldType != null && NewType != null) {
                    return $"{OldType.BuildSignature()} -> {NewType.BuildSignature()}";
                } else if (OldType != null) {
                    return $"{OldType.BuildSignature()} -> (none)";
                } else {
                    return $"(none) -> {NewType.BuildSignature()}";
                }
            }
        }

        /// <summary>
        /// Represents a changed type, that is a type in which the set of members
        /// and/or the metadata are different.
        /// </summary>
        public class TypeChanged : TypeDifference {
            public IList<TypeDifference> NestedTypeDifferences;
            public IList<MemberDifference> MemberDifferences;
            public IList<InjectionDifference> InjectionDifferences;

            public override bool ExistsInOld => true;
            public override bool ExistsInNew => true;

            internal TypeChanged(TypeDefinition old_type, TypeDefinition new_type) {
                OldType = old_type;
                NewType = new_type;
                MemberDifferences = new List<MemberDifference>();
                NestedTypeDifferences = new List<TypeDifference>();
                InjectionDifferences = new List<InjectionDifference>();
            }
        }

        /// <summary>
        /// Represents an added type.
        /// </summary>
        public class TypeAdded : TypeDifference {
            public override bool ExistsInOld => false;
            public override bool ExistsInNew => true;

            internal TypeAdded(TypeDefinition type) {
                NewType = type;
            }
        }

        /// <summary>
        /// Represents a removed type.
        /// </summary>
        public class TypeRemoved : TypeDifference {
            public override bool ExistsInOld => true;
            public override bool ExistsInNew => false;

            internal TypeRemoved(TypeDefinition type) {
                OldType = type;
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

        public abstract class InjectionDifference {
            public abstract bool ExistsInOld { get; }
            public abstract bool ExistsInNew { get; }

            public MethodDefinition Target;
            public MethodPath TargetPath;
            public MethodDefinition Handler;
            public MethodPath HandlerPath;
            public Instruction InjectionPoint;
            public IList<CaptureLocalAttribute> LocalCaptures;
            public InjectPosition Position;

            public InjectionSignature Signature;

            protected InjectionDifference(
                MethodDefinition target,
                MethodPath target_path,
                MethodDefinition handler,
                MethodPath handler_path,
                Instruction injection_point,
                IList<CaptureLocalAttribute> local_captures,
                InjectPosition position
            ) {
                Target = target;
                TargetPath = target_path;
                Handler = handler;
                HandlerPath = handler_path;
                InjectionPoint = injection_point;
                LocalCaptures = local_captures;
                Position = position;

                Signature = new InjectionSignature(this);
            }
        }

        public class InjectionChanged : InjectionDifference {
            public override bool ExistsInOld => true;
            public override bool ExistsInNew => true;

            public InjectionChanged(
                MethodDefinition target,
                MethodPath target_path,
                MethodDefinition handler,
                MethodPath handler_path,
                Instruction injection_point,
                IList<CaptureLocalAttribute> local_captures,
                InjectPosition position
            )
            : base(
                target, target_path,
                handler, handler_path,
                injection_point, local_captures, position
            ) {}
        }

        public class InjectionAdded : InjectionDifference {
            public override bool ExistsInOld => false;
            public override bool ExistsInNew => true;

            public InjectionAdded(
                MethodDefinition target,
                MethodPath target_path,
                MethodDefinition handler,
                MethodPath handler_path,
                Instruction injection_point,
                IList<CaptureLocalAttribute> local_captures,
                InjectPosition position
            )
            : base(
                target, target_path,
                handler, handler_path,
                injection_point, local_captures, position
            ) { }
        }

        public class InjectionRemoved : InjectionDifference {
            public override bool ExistsInOld => true;
            public override bool ExistsInNew => false;

            public InjectionRemoved(
                MethodDefinition target,
                MethodPath target_path,
                MethodPath handler_path,
                Instruction injection_point,
                IList<CaptureLocalAttribute> local_captures,
                InjectPosition position
            )
            : base(
                target, target_path,
                null, handler_path,
                injection_point, local_captures, position
            ) { }
        }
    }
}
