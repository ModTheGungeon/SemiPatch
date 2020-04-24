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
            public MemberType MemberType;

            public abstract object MemberObject { get; }
            public abstract object TargetPathObject { get; }

            public static MemberAdded<MethodDefinition, MethodPath> MethodAdded(MethodDefinition member, MethodPath path) {
                return new MemberAdded<MethodDefinition, MethodPath>(MemberType.Method, member, path);
            }

            public static MemberChanged<MethodDefinition, MethodPath> MethodChanged(MethodDefinition member, MethodPath path) {
                return new MemberChanged<MethodDefinition, MethodPath>(MemberType.Method, member, path);
            }

            public static MemberRemoved<MethodDefinition, MethodPath> MemberRemoved(MethodPath path) {
                return new MemberRemoved<MethodDefinition, MethodPath>(MemberType.Method, path);
            }

            public static MemberAdded<FieldDefinition, FieldPath> FieldAdded(FieldDefinition member, FieldPath path) {
                return new MemberAdded<FieldDefinition, FieldPath>(MemberType.Field, member, path);
            }

            public static MemberChanged<FieldDefinition, FieldPath> FieldChanged(FieldDefinition member, FieldPath path) {
                return new MemberChanged<FieldDefinition, FieldPath>(MemberType.Field, member, path);
            }

            public static MemberRemoved<FieldDefinition, FieldPath> MemberRemoved(FieldPath path) {
                return new MemberRemoved<FieldDefinition, FieldPath>(MemberType.Field, path);
            }

            public static MemberAdded<PropertyDefinition, PropertyPath> PropertyAdded(PropertyDefinition member, PropertyPath path) {
                return new MemberAdded<PropertyDefinition, PropertyPath>(MemberType.Property, member, path);
            }

            public static MemberChanged<PropertyDefinition, PropertyPath> PropertyChanged(PropertyDefinition member, PropertyPath path) {
                return new MemberChanged<PropertyDefinition, PropertyPath>(MemberType.Property, member, path);
            }

            public static MemberRemoved<PropertyDefinition, PropertyPath> MemberRemoved(PropertyPath path) {
                return new MemberRemoved<PropertyDefinition, PropertyPath>(MemberType.Property, path);
            }
        }

        public abstract class MemberDifference<T, U> : MemberDifference
        where T : class, IMemberDefinition {
            public T Member;
            public U TargetPath;

            public override object MemberObject => Member;
            public override object TargetPathObject => TargetPath;

            protected MemberDifference(MemberType type, T member, U path) {
                MemberType = type;
                Member = member;
                TargetPath = path;
            }
        }

        public class MemberChanged<T, U> : MemberDifference<T, U> where T : class, IMemberDefinition {
            public override bool ExistsInOld => true;
            public override bool ExistsInNew => true;


            public MemberChanged(MemberType type, T member, U path) : base(type, member, path) {}
        }

        public class MemberAdded<T, U> : MemberDifference<T, U> where T : class, IMemberDefinition {
            public override bool ExistsInOld => false;
            public override bool ExistsInNew => true;

            public MemberAdded(MemberType type, T member, U path) : base(type, member, path) { }
        }

        public class MemberRemoved<T, U> : MemberDifference<T, U> where T : class, IMemberDefinition {
            public override bool ExistsInOld => true;
            public override bool ExistsInNew => false;

            public MemberRemoved(MemberType type, U path) : base(type, null, path) { }
        }
    }
}
