using System;
using System.Collections.Generic;
using System.Text;
using ModTheGungeon;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SemiPatch {
    public class Relinker {
        public struct Entry {
            public Exception RejectException;
            public string Namespace;
            public TypeDefinition DeclaringType;
            public string Name;

            public Entry(string nspace, TypeDefinition decl_type, string name) {
                RejectException = null;
                Namespace = nspace;
                DeclaringType = decl_type;
                Name = name;
            }

            public static Entry Rejected(Exception except) {
                return new Entry { RejectException = except };
            }

            public static Entry FromPatchTypeData(PatchTypeData type) {
                return new Entry {
                    Name = type.TargetType.Name,
                    Namespace = type.TargetType.Namespace,
                    DeclaringType = type.TargetType.DeclaringType?.Resolve()
                };
            }

            public static Entry FromPatchMemberData<T, U>(PatchTypeData type, PatchMemberData<T, U> member)
            where T : class, IMemberDefinition
            where U : MemberPath<T> {
                return new Entry {
                    Name = member.AliasedName,
                    DeclaringType = type.TargetType.Resolve()
                };
            }

            public static Entry FromPath<T>(MemberPath<T> path, ModuleDefinition target_module)
            where T : IMemberDefinition {
                var elem = path.FindIn(target_module);
                return new Entry {
                    Name = elem.Name,
                    DeclaringType = elem.DeclaringType
                };
            }

            public static Entry FromPath(TypePath path, ModuleDefinition target_module) {
                var type = path.FindIn(target_module);
                return new Entry {
                    Namespace = type.Namespace,
                    Name = type.Name,
                    DeclaringType = type.DeclaringType
                };
            }

            public override string ToString() {
                if (RejectException != null) {
                    return $"<REJECT>";
                }
                var s = new StringBuilder();
                if (Namespace == "" || Namespace == null) s.Append("global::");
                else s.Append(Namespace).Append(".");
                if (DeclaringType != null) {
                    s.Append(DeclaringType.FullName.Substring(Namespace?.Length ?? 0));
                    s.Append(".");
                }

                s.Append(Name);

                return s.ToString();
            }
        }

        public static Logger Logger = new Logger("SPRelinker");

        public Dictionary<TypePath, Entry> TypeEntries = new Dictionary<TypePath, Entry>();
        public Dictionary<MethodPath, Entry> MethodEntries = new Dictionary<MethodPath, Entry>();
        public Dictionary<FieldPath, Entry> FieldEntries = new Dictionary<FieldPath, Entry>();
        public Dictionary<PropertyPath, Entry> PropertyEntries = new Dictionary<PropertyPath, Entry>();

        public List<KeyValuePair<IMemberDefinition, string>> ScheduledRenames = new List<KeyValuePair<IMemberDefinition, string>>();

        private void _LoadTypeRelinkMapFrom(PatchTypeData data, ModuleDefinition module) {
            // if support for inserting members at runtime ever happens,
            // the agent must make sure to first create methods before
            // attempting to load in relinked data
            // as there may be references to inserted members within
            // the target class

            for (var i = 0; i < data.Fields.Count; i++) {
                _LoadFieldRelinkMapFrom(data, data.Fields[i], module);
            }

            for (var i = 0; i < data.Methods.Count; i++) {
                _LoadMethodRelinkMapFrom(data, data.Methods[i], module);
            }

            for (var i = 0; i < data.Properties.Count; i++) {
                _LoadPropertyRelinkMapFrom(data, data.Properties[i], module);
            }

            MapType(data.PatchType.ToPath(), Entry.FromPatchTypeData(data));
        }

        private void _LoadMethodRelinkMapFrom(PatchTypeData type_data, PatchMethodData data, ModuleDefinition module) {
            if (data.ReceivesOriginal) {
                MapMethod(data.PatchPath, Entry.Rejected(new ReceiveOriginalInvokeException(data.PatchPath)));
            } else {
                MapMethod(data.PatchPath, Entry.FromPatchMemberData(type_data, data));
            }
        }

        private void _LoadFieldRelinkMapFrom(PatchTypeData type_data, PatchFieldData data, ModuleDefinition module) {
            MapField(data.PatchPath, Entry.FromPatchMemberData(type_data, data));
        }

        private void _LoadPropertyRelinkMapFrom(PatchTypeData type_data, PatchPropertyData data, ModuleDefinition module) {
            MapProperty(data.PatchPath, Entry.FromPatchMemberData(type_data, data));
        }

        public void LoadRelinkMapFrom(PatchData data) {
            for (var i = 0; i < data.Types.Count; i++) {
                _LoadTypeRelinkMapFrom(data.Types[i], data.TargetModule);
            }
        }

        public void MapType(TypePath path, Entry entry) {
            Logger.Debug($"Mapped type '{path}' to: {entry}");
            TypeEntries[path] = entry;
        }

        public void MapMethod(MethodPath path, Entry entry) {
            Logger.Debug($"Mapped method '{path}' to: {entry}");
            MethodEntries[path] = entry;
        }

        public void MapField(FieldPath path, Entry entry) {
            Logger.Debug($"Mapped method '{path}' to: {entry}");
            FieldEntries[path] = entry;
        }

        public void MapProperty(PropertyPath path, Entry entry) {
            Logger.Debug($"Mapped method '{path}' to: {entry}");
            PropertyEntries[path] = entry;
        }

        public void ScheduleDefinitionRename(MemberPath path, string name) {
            Logger.Debug($"Scheduled definition rename '{path}' to: {name}");
        }

        public void Relink(TypeReference type) {
            if (type == null) return;

            if (type is GenericInstanceType generic_type) {
                Relink(generic_type.ElementType);
                for (var i = 0; i < generic_type.GenericArguments.Count; i++) Relink(generic_type.GenericArguments[i]);
                return;
            }

            var path = type.Resolve()?.ToPath();
            if (path == null) return;
            Entry entry;
            if (!TypeEntries.TryGetValue(path, out entry)) return;

            if (entry.Namespace != null) type.Namespace = entry.Namespace;
            if (entry.DeclaringType != null) type.DeclaringType = entry.DeclaringType;
            if (entry.Name != null) type.Name = entry.Name;

            Logger.Debug($"Relinked type reference '{path}' to '{entry}'");
        }

        public void Relink(MethodDefinition method) {
            if (method == null) return;

            Relink(method.Body);
            Relink(method.ReturnType);
            for (var i = 0; i < method.GenericParameters.Count; i++) Relink(method.GenericParameters[i]);
            for (var i = 0; i < method.Parameters.Count; i++) Relink(method.Parameters[i]);
        }

        public void Relink(MethodBody body) {
            if (body == null) return;

            for (var i = 0; i < body.Instructions.Count; i++) {
                var instr = body.Instructions[i];
                RelinkUntypedObject(instr.Operand);
            }
        }

        public void Relink(MethodReference method) {
            if (method == null) return;

            Relink(method.ReturnType);
            if (method is GenericInstanceMethod generic_method) {
                Relink(generic_method.ElementMethod);
                for (var i = 0; i < generic_method.GenericArguments.Count; i++) Relink(generic_method.GenericArguments[i]);
                return;
            }
            for (var i = 0; i < method.GenericParameters.Count; i++) Relink(method.GenericParameters[i]);
            for (var i = 0; i < method.Parameters.Count; i++) Relink(method.Parameters[i]);

            var path = method.Resolve().ToPath();
            Entry entry;
            if (!MethodEntries.TryGetValue(path, out entry)) return;

            if (entry.DeclaringType != null) method.DeclaringType = entry.DeclaringType;
            if (entry.Name != null) method.Name = entry.Name;

            Logger.Debug($"Relinked method reference '{path}' to '{entry}'");
        }

        public void Relink(FieldReference field) {
            if (field == null) return;

            Relink(field.FieldType);

            var path = field.Resolve().ToPath();
            Entry entry;
            if (!FieldEntries.TryGetValue(path, out entry)) return;

            if (entry.DeclaringType != null) field.DeclaringType = entry.DeclaringType;
            if (entry.Name != null) field.Name = entry.Name;

            Logger.Debug($"Relinked field reference '{path}' to '{entry}'");
        }

        public void Relink(PropertyReference prop) {
            if (prop == null) return;

            Relink(prop.PropertyType);
            for (var i = 0; i < prop.Parameters.Count; i++) Relink(prop.Parameters[i]);

            var path = prop.Resolve().ToPath();
            Entry entry;
            if (!PropertyEntries.TryGetValue(path, out entry)) return;

            var snap = path.Snapshot();

            if (entry.DeclaringType != null) prop.DeclaringType = entry.DeclaringType;
            if (entry.Name != null) prop.Name = entry.Name;

            Logger.Debug($"Relinked property reference '{path}' to '{entry}'");
        }

        public void Relink(CustomAttributeNamedArgument arg) {
            Relink(arg.Argument);
        }

        public void Relink(CustomAttributeArgument arg) {
            Relink(arg.Type);
            RelinkUntypedObject(arg.Value);
        }

        public void Relink(CustomAttribute attr) {
            if (attr == null) return;
            Relink(attr.AttributeType);
            Relink(attr.Constructor);
            for (var i = 0; i < attr.ConstructorArguments.Count; i++) Relink(attr.ConstructorArguments[i]);
            for (var i = 0; i < attr.Fields.Count; i++) Relink(attr.Fields[i]);
            for (var i = 0; i < attr.Properties.Count; i++) Relink(attr.Properties[i]);
        }

        public void Relink(InterfaceImplementation iface) {
            if (iface == null) return;
            Relink(iface.InterfaceType);
            for (var i = 0; i < iface.CustomAttributes.Count; i++) Relink(iface.CustomAttributes[i]);
        }

        public void Relink(EventDefinition ev) {
            if (ev == null) return;
            Relink(ev.EventType);
            for (var i = 0; i < ev.CustomAttributes.Count; i++) Relink(ev.CustomAttributes[i]);
            // invoke/add/remove will be relinked as part of TypeDefinition relinking
        }

        public void Relink(FieldDefinition field) {
            if (field == null) return;
            Relink(field.FieldType);
            RelinkUntypedObject(field.Constant);
            for (var i = 0; i < field.CustomAttributes.Count; i++) Relink(field.CustomAttributes[i]);
        }

        public void Relink(ParameterDefinition param) {
            if (param == null) return;
            Relink(param.ParameterType);
            RelinkUntypedObject(param.Constant);
            for (var i = 0; i < param.CustomAttributes.Count; i++) Relink(param.CustomAttributes[i]);
        }

        public void Relink(PropertyDefinition prop) {
            if (prop == null) return;
            Relink(prop.PropertyType);
            RelinkUntypedObject(prop.Constant);
            for (var i = 0; i < prop.CustomAttributes.Count; i++) Relink(prop.CustomAttributes[i]);
            // GetMethod will be relinked as part of TypeDefinition relinking
            // SetMethod will be relinked as part of TypeDefinition relinking
            // backing field will be relinked as part of TypeDefinition relinking
            for (var i = 0; i < prop.Parameters.Count; i++) Relink(prop.Parameters[i]);
        }

        public void Relink(TypeDefinition type) {
            if (type == null) return;
            if (type.BaseType != null) Relink(type.BaseType);
            if (type.HasInterfaces) {
                for (var i = 0; i < type.Interfaces.Count; i++) {
                    Relink(type.Interfaces[i]);
                }
            }
            for (var i = 0; i < type.CustomAttributes.Count; i++) Relink(type.CustomAttributes[i]);
            for (var i = 0; i < type.Events.Count; i++) Relink(type.Events[i]);
            for (var i = 0; i < type.Fields.Count; i++) Relink(type.Fields[i]);
            for (var i = 0; i < type.GenericParameters.Count; i++) Relink(type.GenericParameters[i]);
            for (var i = 0; i < type.Methods.Count; i++) Relink(type.Methods[i]);
            for (var i = 0; i < type.NestedTypes.Count; i++) Relink(type.NestedTypes[i]);
            for (var i = 0; i < type.Properties.Count; i++) Relink(type.Properties[i]);

            RelinkTypes(type.NestedTypes);
        }

        public void RelinkTypes(Mono.Collections.Generic.Collection<TypeDefinition> types) {
            for (var i = 0; i < types.Count; i++) {
                Relink(types[i]);
            }
        }

        public void RelinkUntypedObject(object value) {
            if (value == null) return;
            if (value is TypeReference t) Relink(t);
            else if (value is MethodReference m) Relink(m);
            else if (value is FieldReference f) Relink(f);
            else if (value is PropertyReference p) Relink(p);
        }

        public void Relink(ModuleDefinition module) {
            RelinkTypes(module.Types);
        }

        public void CommitDefinitionRenames() {
            for (var i = 0; i < ScheduledRenames.Count; i++) {
                var kv = ScheduledRenames[i];
                kv.Key.Name = kv.Value;
                Logger.Debug($"Renamed '{kv.Key.ToPath()}' to '{kv.Value}'");
            }
        }
    }
}
