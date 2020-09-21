using System;
using Mono.Cecil;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SemiPatch {
    /// <summary>
    /// Object containing data about a single method patch.
    /// See <see cref="PatchMemberData"/> for elements available on all type member
    /// patches.
    /// </summary>
    public class PatchMethodData : PatchMemberData {
        public abstract class MemberSideEffect {
            public readonly MemberPath Path;

            public MemberSideEffect(MemberPath path) { Path = path; }
            public abstract void Apply(MethodDefinition method, ModuleDefinition module);

            public override string ToString() {
                return "UNKNOWN";
            }
        }

        public class PropertySideEffect : MemberSideEffect {
            public enum EffectType {
                SetSet,
                SetGet,
                AddOther
            }

            public EffectType Type;

            public PropertySideEffect(EffectType type, MemberPath property_path) : base(property_path) {
                Type = type;
            }

            public override void Apply(MethodDefinition method, ModuleDefinition module) {
                var prop = Path.FindIn<PropertyDefinition>(module);

                switch(Type) {
                case EffectType.SetSet: prop.SetMethod = method; break;
                case EffectType.SetGet: prop.GetMethod = method; break;
                case EffectType.AddOther: prop.OtherMethods.Add(method); break;
                default: throw new InvalidOperationException($"Unknown property effect type: {Type}");
                }
            }

            public override string ToString() {
                return $"PROPERTY {Type} '{Path}'";
            }
        }

        public class EventSideEffect : MemberSideEffect {
            public enum EffectType {
                SetAdd,
                SetRemove,
                SetInvoke,
                AddOther
            }

            public EffectType Type;

            public EventSideEffect(EffectType type, MemberPath event_path) : base(event_path) {
                Type = type;
            }

            public override void Apply(MethodDefinition method, ModuleDefinition module) {
                throw new NotImplementedException();
            }


            public override string ToString() {
                return $"EVENT {Type} '{Path}'";
            }
        }

        private PatchMethodData() { }

        public PatchMethodData(
            MethodDefinition target, MethodDefinition patch,
            MethodPath target_path, MethodPath patch_path,
            EndOfPositionalArguments end = default(EndOfPositionalArguments),
            bool receives_original = false,
            bool explicitly_ignored = false,
            string aliased_name = null,
            bool proxy = false,
            bool rejected_default_ctor = false
        ) : base(
            target, patch,
            target_path, patch_path,
            receives_original: receives_original,
            explicitly_ignored: explicitly_ignored,
            aliased_name: aliased_name,
            proxy: proxy
        ) {
            FalseDefaultConstructor = rejected_default_ctor;
        }

        public PatchMethodData (
            MethodDefinition patch,
            MethodPath target_path, MethodPath patch_path,
            EndOfPositionalArguments end = default(EndOfPositionalArguments),
            bool receives_original = false,
            bool explicitly_ignored = false,
            string aliased_name = null,
            bool proxy = false,
            bool rejected_default_ctor = false
        ) : this(
            null, patch,
            target_path, patch_path,
            receives_original: receives_original,
            explicitly_ignored: explicitly_ignored,
            aliased_name: aliased_name,
            proxy: proxy,
            rejected_default_ctor: rejected_default_ctor
        ) { }

        /// <summary>
        /// If <c>true</c>, this data represents an empty, untagged, default
        /// parameterless constructor in the patch class that does not actually
        /// exist within the target class. This field is used for example in
        /// <see cref="Relinker"/> to reject attempts to construct objects that
        /// don't have a default constructor.
        /// </summary>
        public bool FalseDefaultConstructor = false;

        /// <summary>
        /// Object representing a certain action to be executed as part of this
        /// patch. This can for example set the getter of the target property
        /// if this patch method was tagged with an attribute like <see cref="GetMethodAttribute"/>
        /// or <see cref="SetMethodAttribute"/>. Call <see cref="MemberSideEffect.Apply"/>
        /// to apply the effect to an arbitrary ModuleDefinition.
        /// </summary>
        public MemberSideEffect SideEffect;

        public override bool EffectivelyIgnored => base.EffectivelyIgnored || FalseDefaultConstructor;

        /// <summary>
        /// Typed version of <see cref="PatchMemberData.TargetMember"/>;
        /// </summary>
        public MethodDefinition Target => (MethodDefinition)TargetMember;
        /// <summary>
        /// Typed version of <see cref="PatchMemberData.PatchMember"/>;
        /// </summary>
        public MethodDefinition Patch => (MethodDefinition)PatchMember;

        public override MemberType MemberType => MemberType.Method;

        public override string ToString(string indent) {
            var s = new StringBuilder(base.ToString(indent));
            if (FalseDefaultConstructor) {
                s.Append("\n").Append(indent);
                s.Append("False Default Constructor");
            }

            if (SideEffect != null) {
                s.Append("\n").Append(indent);
                s.Append("Side Effect: ");
                s.Append(SideEffect.ToString());
            }
            return s.ToString();
        }

        public override void Serialize(BinaryWriter writer) {
            base.Serialize(writer);
            writer.Write(FalseDefaultConstructor);
        }

        public static PatchMethodData Deserialize(TypeDefinition target_type, TypeDefinition patch_type, BinaryReader reader) {
            var member = new PatchMethodData();
            member.DeserializeMemberBase(
                "method",
                reader,
                (r) => r.ReadMethodPath(),
                target_type.Methods,
                patch_type.Methods
            );
            member.FalseDefaultConstructor = reader.ReadBoolean();
            return member;
        }
    }
}
