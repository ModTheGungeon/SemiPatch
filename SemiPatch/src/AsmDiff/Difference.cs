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

        public enum MemberType {
            Method,
            Field,
            Property
        }

        public abstract class MemberDifference {
            public abstract bool ExistsInOld { get; }
            public abstract bool ExistsInNew { get; }
            public abstract MemberType MemberType { get; }

            public abstract object MemberObject { get; }
            public abstract object TargetPathObject { get; }

        }

        public abstract class MemberDifference<T, U> : MemberDifference {
            public T Member;
            public U TargetPath;

            public override object MemberObject => Member;
            public override object TargetPathObject => TargetPath;

            protected MemberDifference(T member, U path) {
                Member = member;
                TargetPath = path;
            }
        }

        public abstract class MemberChanged<T, U> : MemberDifference<T, U> {
            public override bool ExistsInOld => true;
            public override bool ExistsInNew => true;


            internal MemberChanged(T member, U path) : base(member, path) {}
        }

        public abstract class MemberAdded<T, U> : MemberDifference<T, U> {
            public override bool ExistsInOld => false;
            public override bool ExistsInNew => true;

            internal MemberAdded(T member, U path) : base(member, path) {}
        }

        public abstract class MemberRemoved<T, U> : MemberDifference<T, U> {
            public override bool ExistsInOld => true;
            public override bool ExistsInNew => false;

            internal MemberRemoved(T member, U path) : base(member, path) {}
        }

        public class MethodChanged : MemberChanged<MethodDefinition, MethodPath> {
            public MethodChanged(MethodDefinition method, MethodPath path)
                : base(method, path) { }
            public override MemberType MemberType => MemberType.Method;
        }

        public class MethodAdded : MemberAdded<MethodDefinition, MethodPath> {
            public MethodAdded(MethodDefinition method, MethodPath path)
                : base(method, path) { }
            public override MemberType MemberType => MemberType.Method;
        }

        public class MethodRemoved : MemberRemoved<MethodDefinition, MethodPath> {
            public MethodRemoved(MethodDefinition method, MethodPath path)
                : base(method, path) { }
            public override MemberType MemberType => MemberType.Method;
        }

        public class FieldChanged : MemberChanged<FieldDefinition, FieldPath> {
            public FieldChanged(FieldDefinition method, FieldPath path)
                : base(method, path) { }
            public override MemberType MemberType => MemberType.Field;
        }

        public class FieldAdded : MemberAdded<FieldDefinition, FieldPath> {
            public FieldAdded(FieldDefinition method, FieldPath path)
                : base(method, path) { }
            public override MemberType MemberType => MemberType.Field;
        }

        public class FieldRemoved : MemberRemoved<FieldDefinition, FieldPath> {
            public FieldRemoved(FieldDefinition method, FieldPath path)
                : base(method, path) { }
            public override MemberType MemberType => MemberType.Field;
        }

        public class PropertyChanged : MemberChanged<PropertyDefinition, PropertyPath> {
            public PropertyChanged(PropertyDefinition method, PropertyPath path)
                : base(method, path) { }
            public override MemberType MemberType => MemberType.Property;
        }

        public class PropertyAdded : MemberAdded<PropertyDefinition, PropertyPath> {
            public PropertyAdded(PropertyDefinition method, PropertyPath path)
                : base(method, path) { }
            public override MemberType MemberType => MemberType.Property;
        }

        public class PropertyRemoved : MemberRemoved<PropertyDefinition, PropertyPath> {
            public PropertyRemoved(PropertyDefinition method, PropertyPath path)
                : base(method, path) { }
            public override MemberType MemberType => MemberType.Property;
        }
    }
}
