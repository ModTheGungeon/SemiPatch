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
            public int OldIndex;
            public int NewIndex;
            public TypeDefinition OldType;
            public TypeDefinition NewType;
            public IList<TypeDifference> NestedTypeDifferences;
            public IList<MemberDifference> MemberDifferences;

            public override bool ExistsInOld => true;
            public override bool ExistsInNew => true;

            internal TypeChanged(int old_idx, int new_idx, TypeDefinition old_type, TypeDefinition new_type) {
                OldIndex = old_idx;
                NewIndex = new_idx;
                OldType = old_type;
                NewType = new_type;
                MemberDifferences = new List<MemberDifference>();
                NestedTypeDifferences = new List<TypeDifference>();
            }
        }

        public class TypeAdded : TypeDifference {
            public int Index;
            public TypeDefinition Type;

            public override bool ExistsInOld => false;
            public override bool ExistsInNew => true;

            internal TypeAdded(int idx, TypeDefinition type) {
                Index = idx;
                Type = type;
            }
        }

        public class TypeRemoved : TypeDifference {
            public int Index;
            public TypeDefinition Type;

            public override bool ExistsInOld => true;
            public override bool ExistsInNew => false;

            internal TypeRemoved(int idx, TypeDefinition type) {
                Index = idx;
                Type = type;
            }
        }

        public enum MemberType {
            Method,
            Field,
            Property
        }

        public abstract class MemberDifference {
            public abstract bool ExistsInOld { get; }
            public abstract bool ExistsInNew { get; }
            public abstract MemberType MemberType { get; }
            public string Signature;
        }

        public abstract class MemberChanged<T> : MemberDifference {
            public int OldIndex;
            public int NewIndex;
            public T OldMember;
            public T NewMember;

            public override bool ExistsInOld => true;
            public override bool ExistsInNew => true;


            internal MemberChanged(int old_idx, int new_idx, T old_member, T new_member, string sig) {
                OldIndex = old_idx;
                NewIndex = new_idx;
                OldMember = old_member;
                NewMember = new_member;
                Signature = sig;
            }
        }

        public abstract class MemberAdded<T> : MemberDifference {
            public int Index;
            public T Member;

            public override bool ExistsInOld => false;
            public override bool ExistsInNew => true;

            internal MemberAdded(int idx, T member, string sig) {
                Index = idx;
                Member = member;
                Signature = sig;
            }
        }

        public abstract class MemberRemoved<T> : MemberDifference {
            public int Index;
            public T Member;

            public override bool ExistsInOld => true;
            public override bool ExistsInNew => false;

            internal MemberRemoved(int idx, T member, string sig) {
                Index = idx;
                Member = member;
                Signature = sig;
            }
        }

        public class MethodChanged : MemberChanged<MethodDefinition> {
            public MethodChanged(int old_idx, int new_idx, MethodDefinition old_method, MethodDefinition new_method)
                : base(old_idx, new_idx, old_method, new_method, old_method.BuildSignature()) { }
            public override MemberType MemberType => MemberType.Method;
        }

        public class MethodAdded : MemberAdded<MethodDefinition> {
            public MethodAdded(int idx, MethodDefinition method)
                : base(idx, method, method.BuildSignature()) { }
            public override MemberType MemberType => MemberType.Method;
        }

        public class MethodRemoved : MemberRemoved<MethodDefinition> {
            public MethodRemoved(int idx, MethodDefinition method)
                : base(idx, method, method.BuildSignature()) { }
            public override MemberType MemberType => MemberType.Method;
        }

        public class FieldChanged : MemberChanged<FieldDefinition> {
            public FieldChanged(int old_idx, int new_idx, FieldDefinition old_method, FieldDefinition new_method)
                : base(old_idx, new_idx, old_method, new_method, old_method.BuildSignature()) { }
            public override MemberType MemberType => MemberType.Field;
        }

        public class FieldAdded : MemberAdded<FieldDefinition> {
            public FieldAdded(int idx, FieldDefinition method)
                : base(idx, method, method.BuildSignature()) { }
            public override MemberType MemberType => MemberType.Field;
        }

        public class FieldRemoved : MemberRemoved<FieldDefinition> {
            public FieldRemoved(int idx, FieldDefinition method)
                : base(idx, method, method.BuildSignature()) { }
            public override MemberType MemberType => MemberType.Field;
        }

        public class PropertyChanged : MemberChanged<PropertyDefinition> {
            public PropertyChanged(int old_idx, int new_idx, PropertyDefinition old_method, PropertyDefinition new_method)
                : base(old_idx, new_idx, old_method, new_method, old_method.BuildSignature()) { }
            public override MemberType MemberType => MemberType.Property;
        }

        public class PropertyAdded : MemberAdded<PropertyDefinition> {
            public PropertyAdded(int idx, PropertyDefinition method)
                : base(idx, method, method.BuildSignature()) { }
            public override MemberType MemberType => MemberType.Property;
        }

        public class PropertyRemoved : MemberRemoved<PropertyDefinition> {
            public PropertyRemoved(int idx, PropertyDefinition method)
                : base(idx, method, method.BuildSignature()) { }
            public override MemberType MemberType => MemberType.Property;
        }
    }
}
