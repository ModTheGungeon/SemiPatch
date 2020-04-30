﻿using System;
using System.Collections.Generic;
using System.Text;
using ModTheGungeon;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SemiPatch {
    /// <summary>
    /// Recursive relinker based on <code>Mono.Cecil</code>. Allows mapping types
    /// and type members to <see cref="Entry"/> objects which specify how the
    /// object should be altered. Recursively traverses through the type hierarchy,
    /// attributes, method bodies etc.
    /// </summary>
    public class Relinker {
        public struct MemberEntry<T> where T : class, IMemberDefinition {
            public Exception RejectException;
            public T TargetMember;

            public static MemberEntry<T> Rejected(Exception except) {
                return new MemberEntry<T> { RejectException = except };
            }

            //public static MemberEntry<T> FromPatchTypeData(PatchTypeData type) {
            //    return new Member<Entry {
            //        TargetModule = type.TargetType.Module,
            //        Name = type.TargetType.Name,
            //        Namespace = type.TargetType.Namespace,
            //        DeclaringType = type.TargetType.DeclaringType?.Resolve()
            //    };
            //}

            public static MemberEntry<T> FromPatchData<U>(ModuleDefinition module, PatchTypeData type_data, PatchMemberData<T, U> member_data)
            where U : MemberPath<T> {
                var member = member_data.TargetPath.FindIn(module);

                return new MemberEntry<T> {
                    RejectException = null,
                    TargetMember = member
                };
            }

            public void CheckIfValid() {
                if (RejectException != null) throw RejectException;
            }

            public override string ToString() {
                if (RejectException != null) {
                    return $"<REJECT>";
                }

                return TargetMember.ToPathInterface().ToString();
            }
        }

        public struct TypeEntry {
            public Exception RejectException;
            public TypeDefinition TargetType;

            public static TypeEntry Rejected(Exception except) {
                return new TypeEntry { RejectException = except };
            }

            //public static MemberEntry<T> FromPatchTypeData(PatchTypeData type) {
            //    return new Member<Entry {
            //        TargetModule = type.TargetType.Module,
            //        Name = type.TargetType.Name,
            //        Namespace = type.TargetType.Namespace,
            //        DeclaringType = type.TargetType.DeclaringType?.Resolve()
            //    };
            //}

            public static TypeEntry FromPatchData(ModuleDefinition module, PatchTypeData type_data) {
                return new TypeEntry {
                    RejectException = null,
                    TargetType = type_data.TargetType.ToPath().FindIn(module)
                };
            }

            public void CheckIfValid() {
                if (RejectException != null) throw RejectException;
            }

            public override string ToString() {
                if (RejectException != null) {
                    return $"<REJECT>";
                }

                return TargetType.ToPath().ToString();
            }
        }


        public class State {
            public ModuleDefinition Module;
            public Dictionary<TypeReference, TypePath> RelinkedTypeMap = new Dictionary<TypeReference, TypePath>();

            public State(ModuleDefinition module) {
                Module = module;
            }

            public void MapPreRelinkType(TypeReference type, TypePath path) {
                RelinkedTypeMap[type] = path;
            }

            public MethodPath UnrelinkMemberPath(TypeReference type, MethodPath path) {
                if (RelinkedTypeMap.TryGetValue(type, out TypePath old_type_path)) {
                    return path.WithDeclaringTypePath(old_type_path);
                }
                return path;
            }

            public FieldPath UnrelinkMemberPath(TypeReference type, FieldPath path) {
                if (RelinkedTypeMap.TryGetValue(type, out TypePath old_type_path)) {
                    return path.WithDeclaringTypePath(old_type_path);
                }
                return path;
            }

            public PropertyPath UnrelinkMemberPath(TypeReference type, PropertyPath path) {
                if (RelinkedTypeMap.TryGetValue(type, out TypePath old_type_path)) {
                    return path.WithDeclaringTypePath(old_type_path);
                }
                return path;
            }
        }

        public static Logger Logger = new Logger(nameof(Relinker));

        public Dictionary<TypePath, TypeEntry> TypeEntries = new Dictionary<TypePath, TypeEntry>();
        public Dictionary<MethodPath, MemberEntry<MethodDefinition>> MethodEntries = new Dictionary<MethodPath, MemberEntry<MethodDefinition>>();
        public Dictionary<FieldPath, MemberEntry<FieldDefinition>> FieldEntries = new Dictionary<FieldPath, MemberEntry<FieldDefinition>>();
        public Dictionary<PropertyPath, MemberEntry<PropertyDefinition>> PropertyEntries = new Dictionary<PropertyPath, MemberEntry<PropertyDefinition>>();

        public List<KeyValuePair<IMemberDefinition, string>> ScheduledRenames = new List<KeyValuePair<IMemberDefinition, string>>();

        public bool LaxOnInserts = false;

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

            MapType(data.PatchType.ToPath(), TypeEntry.FromPatchData(module, data));
        }

        private void _LoadMethodRelinkMapFrom(PatchTypeData type_data, PatchMethodData data, ModuleDefinition module) {
            MethodDefinition member = data.Target;

            if (data.FalseDefaultConstructor) {
                MapMethod(data.PatchPath, MemberEntry<MethodDefinition>.Rejected(new FalseDefaultConstructorException(type_data.PatchType.ToPath(), type_data.TargetType.ToPath())));
                return;
            }

            if (data.IsInsert && !LaxOnInserts) {
                try {
                    member = data.TargetPath.FindIn(module);
                } catch (MemberPathSearchException<MethodDefinition> e) {
                    throw new TargetFieldRelinkerException(data.PatchPath, data.TargetPath);
                }
            }

            if (data.ReceivesOriginal) {
                MapMethod(data.PatchPath, MemberEntry<MethodDefinition>.Rejected(new ReceiveOriginalInvokeException(data.PatchPath)));
            } else {
                MapMethod(data.PatchPath, MemberEntry<MethodDefinition>.FromPatchData<MethodPath>(module, type_data, data));
            }
        }

        private void _LoadFieldRelinkMapFrom(PatchTypeData type_data, PatchFieldData data, ModuleDefinition module) {
            FieldDefinition member = data.Target;

            if (data.IsInsert && !LaxOnInserts) {
                try {
                    member = data.TargetPath.FindIn(module);
                } catch (MemberPathSearchException<FieldDefinition> e) {
                    throw new TargetFieldRelinkerException(data.PatchPath, data.TargetPath);
                }
            }

            MapField(data.PatchPath, MemberEntry<FieldDefinition>.FromPatchData<FieldPath>(module, type_data, data));
        }

        private void _LoadPropertyRelinkMapFrom(PatchTypeData type_data, PatchPropertyData data, ModuleDefinition module) {
            PropertyDefinition member = data.Target;

            if (data.IsInsert && !LaxOnInserts) {
                try {
                    member = data.TargetPath.FindIn(module);
                } catch (MemberPathSearchException<PropertyDefinition> e) {
                    throw new TargetFieldRelinkerException(data.PatchPath, data.TargetPath);
                }
            }

            MapProperty(data.PatchPath, MemberEntry<PropertyDefinition>.FromPatchData<PropertyPath>(module, type_data, data));
        }

        public void LoadRelinkMapFrom(PatchData data, ModuleDefinition current_module) {
            for (var i = 0; i < data.Types.Count; i++) {
                _LoadTypeRelinkMapFrom(data.Types[i], current_module);
            }
        }

        public void MapType(TypePath path, TypeEntry entry) {
            Logger.Debug($"Mapped type '{path}' to: {entry}");
            TypeEntries[path] = entry;
        }

        public void MapMethod(MethodPath path, MemberEntry<MethodDefinition> entry) {
            Logger.Debug($"Mapped method '{path}' to: {entry}");
            MethodEntries[path] = entry;
        }

        public void MapField(FieldPath path, MemberEntry<FieldDefinition> entry) {
            Logger.Debug($"Mapped field '{path}' to: {entry}");
            FieldEntries[path] = entry;
        }

        public void MapProperty(PropertyPath path, MemberEntry<PropertyDefinition> entry) {
            Logger.Debug($"Mapped property '{path}' to: {entry}");
            PropertyEntries[path] = entry;
        }

        public void ScheduleDefinitionRename<T>(T member, string name) where T : class, IMemberDefinition {
            var path = member.ToPath<T, MemberPath<T>>();
            Logger.Debug($"Scheduled definition rename '{path}' to: {name}");
            ScheduledRenames.Add(new KeyValuePair<IMemberDefinition, string>(member, name));
        }

        public TypeReference Relink(State state, TypeReference type) {
            if (type == null) return type;
            if (type is GenericParameter) return type;

            if (type is GenericInstanceType generic_type) {
                var old_generic_type = generic_type;
                var elem_type = Relink(state, old_generic_type.ElementType);
                type = generic_type = new GenericInstanceType(elem_type);
                for (var i = 0; i < old_generic_type.GenericArguments.Count; i++) {
                    var arg = old_generic_type.GenericArguments[i];
                    generic_type.GenericArguments.Add(Relink(state, arg));
                }
                return type;
            }

            var path = type.Resolve()?.ToPath();
            if (path == null) return type;
            TypeEntry entry;
            if (TypeEntries.TryGetValue(path, out entry)) {
                entry.CheckIfValid();

                state.MapPreRelinkType(type, path);

                var old_type = type;

                type = new TypeReference(
                    entry.TargetType.Namespace,
                    entry.TargetType.Name,
                    entry.TargetType.Module,
                    entry.TargetType.Scope
                );

                for (var i = 0; i < old_type.GenericParameters.Count; i++) {
                    var param = old_type.GenericParameters[i];
                    type.GenericParameters.Add(param);
                }

                type = state.Module.ImportReference(type);

                Logger.Debug($"Relinked type reference '{path}' to '{entry.TargetType.ToPath()}'");
            }

            return type;
        }

        public void Relink(State state, MethodDefinition method) {
            if (method == null) return;

            Relink(state, method.Body);
            method.ReturnType = Relink(state, method.ReturnType);
            for (var i = 0; i < method.GenericParameters.Count; i++) {
                var param = method.GenericParameters[i];
                method.GenericParameters[i] = Relink(state, param) as GenericParameter;
            }
            for (var i = 0; i < method.Parameters.Count; i++) Relink(state, method.Parameters[i]);
        }

        public void Relink(State state, ExceptionHandler ex) {
            if (ex == null) return;

            ex.CatchType = Relink(state, ex.CatchType);
        }

        public void Relink(State state, VariableDefinition var_def) {
            if (var_def == null) return;

            var_def.VariableType = Relink(state, var_def.VariableType);
        }

        public void Relink(State state, MethodBody body) {
            if (body == null) return;

            for (var i = 0; i < body.ExceptionHandlers.Count; i++) {
                Relink(state, body.ExceptionHandlers[i]);
            }

            for (var i = 0; i < body.Variables.Count; i++) {
                Relink(state, body.Variables[i]);
            }

            for (var i = 0; i < body.Instructions.Count; i++) {
                var instr = body.Instructions[i];
                instr.Operand = RelinkUntypedObject(state, instr.Operand);
            }
        }

        public MethodReference Relink(State state, MethodReference method) {
            if (method == null) return method;

            if (method is GenericInstanceMethod generic_method) {
                var old_generic_method = generic_method;
                method = generic_method = new GenericInstanceMethod(Relink(state, generic_method.ElementMethod));
                for (var i = 0; i < old_generic_method.GenericArguments.Count; i++) {
                    var param = old_generic_method.GenericArguments[i];
                    generic_method.GenericArguments.Add(Relink(state, param));
                }
                return method;
            }

            var path = method.Resolve().ToPath();
            MemberEntry<MethodDefinition> entry;
            if (MethodEntries.TryGetValue(path, out entry)) {
                entry.CheckIfValid();

                var old_method = method;
                method = new MethodReference(
                    entry.TargetMember.Name,
                    state.Module.ImportReference(entry.TargetMember.ReturnType),
                    state.Module.ImportReference(entry.TargetMember.DeclaringType)
                );
                for (var i = 0; i < old_method.GenericParameters.Count; i++) {
                    var param = old_method.GenericParameters[i];
                    var new_param = new GenericParameter(param.Name, method);
                    new_param.Attributes = param.Attributes;
                    for (var j = 0; j < param.CustomAttributes.Count; j++) {
                        new_param.CustomAttributes.Add(param.CustomAttributes[j]);
                    }
                    method.GenericParameters.Add(new_param);
                }
                method.HasThis = old_method.HasThis;
                method.ExplicitThis = old_method.ExplicitThis;
                for (var i = 0; i < old_method.Parameters.Count; i++) {
                    var param = old_method.Parameters[i];
                    var new_param = new ParameterDefinition(param.Name, param.Attributes, Relink(state, param.ParameterType));
                    new_param.Constant = param.Constant;
                    for (var j = 0; j < param.CustomAttributes.Count; j++) {
                        var attr = param.CustomAttributes[j];
                        new_param.CustomAttributes.Add(attr);
                    }
                    method.Parameters.Add(new_param);
                }

                method = state.Module.ImportReference(method);

                Logger.Debug($"Relinked method reference '{path}' to '{entry.TargetMember.ToPath()}'");
            }

            method.ReturnType = Relink(state, method.ReturnType);

            for (var i = 0; i < method.GenericParameters.Count; i++) {
                var param = method.GenericParameters[i];
                method.GenericParameters[i] = Relink(state, param) as GenericParameter;
            }
            for (var i = 0; i < method.Parameters.Count; i++) Relink(state, method.Parameters[i]);

            return method;
        }

        public FieldReference Relink(State state, FieldReference field) {
            if (field == null) return field;

            var path = field.Resolve().ToPath();
            MemberEntry<FieldDefinition> entry;
            if (FieldEntries.TryGetValue(path, out entry)) {
                entry.CheckIfValid();

                var old_field = field;

                field = new FieldReference(entry.TargetMember.Name, entry.TargetMember.FieldType, entry.TargetMember.DeclaringType);
                field = state.Module.ImportReference(field);

                Logger.Debug($"Relinked field reference '{path}' to '{entry.TargetMember.ToPath()}'");
            }

            field.FieldType = Relink(state, field.FieldType);
            return field;
        }

        public PropertyReference Relink(State state, PropertyReference prop) {
            if (prop == null) return prop;

            //var path = prop.Resolve().ToPath();
            //MemberEntry<PropertyDefinition> entry;
            //if (PropertyEntries.TryGetValue(path, out entry)) {
            //    entry.CheckIfValid();

            //    var old_prop = prop;
            //    prop = new PropertyReference();

            //    Logger.Debug($"Relinked property reference '{path}' to '{entry}'");
            //}

            prop.PropertyType = Relink(state, prop.PropertyType);
            for (var i = 0; i < prop.Parameters.Count; i++) Relink(state, prop.Parameters[i]);

            return prop;
        }

        public CustomAttributeNamedArgument Relink(State state, CustomAttributeNamedArgument arg) {
            return new CustomAttributeNamedArgument(arg.Name, Relink(state, arg.Argument));
        }

        public CustomAttributeArgument Relink(State state, CustomAttributeArgument arg) {
            return new CustomAttributeArgument(
                Relink(state, arg.Type),
                RelinkUntypedObject(state, arg.Value)
            );
        }

        public void Relink(State state, CustomAttribute attr) {
            if (attr == null) return;
            // AttributeType is Constructor.DeclaringType
            attr.Constructor = Relink(state, attr.Constructor);
            for (var i = 0; i < attr.ConstructorArguments.Count; i++) {
                var arg = attr.ConstructorArguments[i];
                attr.ConstructorArguments[i] = Relink(state, arg);
            }
            for (var i = 0; i < attr.Fields.Count; i++) {
                var arg = attr.Fields[i];
                attr.Fields[i] = Relink(state, arg);
            }
            for (var i = 0; i < attr.Properties.Count; i++) {
                var arg = attr.Properties[i];
                attr.Properties[i] = Relink(state, arg);
            }
        }

        public void Relink(State state, InterfaceImplementation iface) {
            if (iface == null) return;
            iface.InterfaceType = Relink(state, iface.InterfaceType);
            for (var i = 0; i < iface.CustomAttributes.Count; i++) Relink(state, iface.CustomAttributes[i]);
        }

        public void Relink(State state, EventDefinition ev) {
            if (ev == null) return;
            ev.EventType = Relink(state, ev.EventType);
            for (var i = 0; i < ev.CustomAttributes.Count; i++) Relink(state, ev.CustomAttributes[i]);
            // invoke/add/remove will be relinked as part of TypeDefinition relinking
        }

        public void Relink(State state, FieldDefinition field) {
            if (field == null) return;
            field.FieldType = Relink(state, field.FieldType);
            field.Constant = RelinkUntypedObject(state, field.Constant);
            for (var i = 0; i < field.CustomAttributes.Count; i++) Relink(state, field.CustomAttributes[i]);
        }

        public void Relink(State state, ParameterDefinition param) {
            if (param == null) return;
            param.ParameterType = Relink(state, param.ParameterType);
            param.Constant = RelinkUntypedObject(state, param.Constant);
            for (var i = 0; i < param.CustomAttributes.Count; i++) Relink(state, param.CustomAttributes[i]);
        }

        public void Relink(State state, PropertyDefinition prop) {
            if (prop == null) return;
            prop.PropertyType = Relink(state, prop.PropertyType);
            prop.Constant = RelinkUntypedObject(state, prop.Constant);
            for (var i = 0; i < prop.CustomAttributes.Count; i++) Relink(state, prop.CustomAttributes[i]);
            // GetMethod will be relinked as part of TypeDefinition relinking
            // SetMethod will be relinked as part of TypeDefinition relinking
            // backing field will be relinked as part of TypeDefinition relinking
            for (var i = 0; i < prop.Parameters.Count; i++) Relink(state, prop.Parameters[i]);
        }

        public void Relink(State state, TypeDefinition type) {
            if (type == null) return;

            for (var i = 0; i < type.CustomAttributes.Count; i++) Relink(state, type.CustomAttributes[i]);
            for (var i = 0; i < type.Events.Count; i++) Relink(state, type.Events[i]);
            for (var i = 0; i < type.Fields.Count; i++) Relink(state, type.Fields[i]);
            for (var i = 0; i < type.GenericParameters.Count; i++) {
                var param = type.GenericParameters[i];
                type.GenericParameters[i] = Relink(state, param) as GenericParameter;
            }
            for (var i = 0; i < type.Methods.Count; i++) Relink(state, type.Methods[i]);
            for (var i = 0; i < type.NestedTypes.Count; i++) Relink(state, type.NestedTypes[i]);
            for (var i = 0; i < type.Properties.Count; i++) Relink(state, type.Properties[i]);

            type.BaseType = Relink(state, type.BaseType);
            if (type.HasInterfaces) {
                for (var i = 0; i < type.Interfaces.Count; i++) {
                    Relink(state, type.Interfaces[i]);
                }
            }

            RelinkTypes(state, type.NestedTypes);
        }

        public void RelinkTypes(State state, Mono.Collections.Generic.Collection<TypeDefinition> types) {
            for (var i = 0; i < types.Count; i++) {
                Relink(state, types[i]);
            }
        }

        public object RelinkUntypedObject(State state, object value) {
            if (value == null) return value;
            if (value is TypeReference t) return Relink(state, t);
            else if (value is MethodReference m) return Relink(state, m);
            else if (value is FieldReference f) return Relink(state, f);
            else if (value is PropertyReference p) return Relink(state, p);
            return value;
        }

        public void Relink(State state, ModuleDefinition module) {
            RelinkTypes(state, module.Types);
        }

        public void Relink(ModuleDefinition module) {
            RelinkTypes(new State(module), module.Types);
        }

        public void CommitDefinitionRenames() {
            for (var i = 0; i < ScheduledRenames.Count; i++) {
                var kv = ScheduledRenames[i];
                kv.Key.Name = kv.Value;
                Logger.Debug($"Renamed '{kv.Key.ToPathInterface()}' to '{kv.Value}'");
            }
        }
    }
}
