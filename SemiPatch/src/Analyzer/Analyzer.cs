﻿using System;
using System.Collections.Generic;
using ModTheGungeon;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace SemiPatch {
    /// <summary>
    /// The SemiPatch analyzer is the first thing you will ever run when
    /// patching with SP. The analyzer reads through the types of a patch assembly
    /// and extracts information specifically tagged for SemiPatch, such as
    /// which classes actually patch something, what members they patch, etc.
    /// </summary>
    public class Analyzer {
        public static Logger Logger = new Logger("Analyzer");
        private Dictionary<InjectQuery, IInjectionAnalysisHandler> _InjectQueryHandlers = new Dictionary<InjectQuery, IInjectionAnalysisHandler>{
            [InjectQuery.Head] = new HeadInjectionAnalysisHandler(),
            [InjectQuery.Tail] = new TailInjectionAnalysisHandler(),
            [InjectQuery.MethodCall] = new MethodCallInjectionAnalysisHandler()
        };

        public ModuleDefinition TargetModule;
        public IList<ModuleDefinition> PatchModules;
        public IDictionary<MethodPath, MethodDefinition> MethodMap;
        public IDictionary<FieldPath, FieldDefinition> FieldMap;
        public IDictionary<PropertyPath, PropertyDefinition> PropertyMap;

        public HashSet<MethodDefinition> IgnoredMethods;
        public HashSet<FieldDefinition> IgnoredFields;

        public PatchData PatchData;
        public Relinker ValidationRelinker;

        private Dictionary<TypePath, MethodReference> _ParameterlessCtorCache;

        public Analyzer(string target_path, IList<string> patch_paths) {
            Logger.Debug($"New Patcher created from {patch_paths.Count} paths");
            MethodMap = new Dictionary<MethodPath, MethodDefinition>();
            FieldMap = new Dictionary<FieldPath, FieldDefinition>();
            PropertyMap = new Dictionary<PropertyPath, PropertyDefinition>();
            IgnoredMethods = new HashSet<MethodDefinition>();
            IgnoredFields = new HashSet<FieldDefinition>();
            ValidationRelinker = new Relinker();

            ValidationRelinker.Map(
                SemiPatch.InjectionStateOverrideReturnField.ToPath(),
                Relinker.MemberEntry.Rejected(new InjectionStateIllegalAccessException(
                    SemiPatch.InjectionStateOverrideReturnField.ToPath()
                ))
            );

            ValidationRelinker.Map(
                SemiPatch.VoidInjectionStateOverrideReturnField.ToPath(),
                Relinker.MemberEntry.Rejected(new InjectionStateIllegalAccessException(
                    SemiPatch.VoidInjectionStateOverrideReturnField.ToPath()
                ))
            );

            ValidationRelinker.Map(
                SemiPatch.InjectionStateReturnValueField.ToPath(),
                Relinker.MemberEntry.Rejected(new InjectionStateIllegalAccessException(
                    SemiPatch.InjectionStateReturnValueField.ToPath()
                ))
            );

            _ParameterlessCtorCache = new Dictionary<TypePath, MethodReference>();

            PatchModules = new ModuleDefinition[patch_paths.Count];
            var i = 0;
            foreach (var path in patch_paths) {
                var mod = PatchModules[i] = ModuleDefinition.ReadModule(path);
                Logger.Debug($"Adding new module to Patcher: {mod.Name}");
                i += 1;
            }
            TargetModule = ModuleDefinition.ReadModule(target_path);

            Logger.Info($"Analyzing target module: {TargetModule.Name}");
            foreach (var type in TargetModule.Types) {
                Logger.Debug($"Scanning type: {type.FullName}");
                foreach (var method in type.Methods) {
                    var path = method.ToPath();
                    Logger.Debug($"Caching method: '{path}'");
                    MethodMap[path] = method;
                }
                foreach (var field in type.Fields) {
                    var path = field.ToPath();
                    Logger.Debug($"Caching field: '{path}'");
                    FieldMap[path] = field;
                }
                foreach (var prop in type.Properties) {
                    var path = prop.ToPath();
                    Logger.Debug($"Caching property: '{path}'");
                    PropertyMap[path] = prop;
                }
            }
        }

        public MethodReference TryGetParameterlessCtor(TypeReference type) {
            var r = type.Resolve();
            var path = r.ToPath();
            if (_ParameterlessCtorCache.TryGetValue(path, out MethodReference method)) return method;
            for (var i = 0; i < r.Methods.Count; i++) {
                var typemethod = r.Methods[i];
                if (typemethod.IsConstructor && typemethod.Parameters.Count == 0) {
                    return _ParameterlessCtorCache[path] = typemethod;
                }
            }
            return _ParameterlessCtorCache[path] = null;
        }

        public MethodDefinition TryGetTargetMethod(MethodPath path) {
            if (MethodMap.TryGetValue(path, out MethodDefinition def)) return def;
            return null;
        }

        public FieldDefinition TryGetTargetField(FieldPath path) {
            if (FieldMap.TryGetValue(path, out FieldDefinition def)) return def;
            return null;
        }

        public PropertyDefinition TryGetTargetProperty(PropertyPath path) {
            if (PropertyMap.TryGetValue(path, out PropertyDefinition def)) return def;
            return null;
        }

        private IInjectionAnalysisHandler _GetInjectionAnalysisHandler(MethodPath handler_path, InjectQuery query) {
            if (_InjectQueryHandlers.TryGetValue(query, out IInjectionAnalysisHandler handler)) return handler;
            throw new InvalidInjectQueryException(handler_path, query);
        }

        private void _HandleInject(PatchTypeData type_data, MethodDefinition handler, SpecialAttributeData attrs) {
            Logger.Debug($"Processing injection handler: {handler.BuildSignature()}");

            var handler_path = handler.ToPath();
            var inject_data = attrs.InjectData;

            MethodDefinition target = null;
            for (var i = 0; i < type_data.TargetType.Methods.Count; i++) {
                var candidate_method = type_data.TargetType.Methods[i];
                var candidate_sig = new Signature(candidate_method);

                if (candidate_sig == inject_data.Inside) {
                    target = candidate_method;
                    break;
                }
            }

            if (target == null) {
                var error_target_path = new MethodPath(new Signature(inject_data.Inside, null), type_data.TargetType);
                throw new TargetMethodSearchFailureException(error_target_path, $"target of injection handler '{handler_path}', within the type '{type_data.TargetType.FullName}'");
            }

            if (target.RVA == 0 || !target.HasBody || target.Body.Instructions.Count == 0) {
                throw new EmptyInjectTargetMethodException(target.ToPath());
            }

            var target_path = target.ToPath();
            var expected_callback_type = Injector.GetInjectionStateType(handler.Module, target.ReturnType);

            var is_valid_handler = true;
            if (target.Parameters.Count + 1 != handler.Parameters.Count) is_valid_handler = false;
            if (is_valid_handler && !handler.Parameters[0].ParameterType.IsSame(expected_callback_type, exclude_generic_args: true)) {
                is_valid_handler = false;
            }
            if (is_valid_handler) {
                for (var i = 0; i < target.Parameters.Count; i++) {
                    if (!handler.Parameters[i + 1].ParameterType.IsSame(target.Parameters[i].ParameterType)) {
                        is_valid_handler = false;
                        break;
                    }
                }
            }

            if (!is_valid_handler) {
                throw new InvalidInjectHandlerException(
                    handler_path, target_path,
                    new Signature(
                        target.BuildSignature(
                            forced_name: "<handler name>",
                            forced_return_type: "void",
                            forced_first_arg: $"{expected_callback_type.BuildSignature()}"
                        ),
                        "<handler name>"
                    )
                );
            }

            var analysis_handler = _GetInjectionAnalysisHandler(handler_path, inject_data.Query);

            var position = inject_data.Position == InjectPosition.Default ? analysis_handler.DefaultPosition : inject_data.Position;

            var patch_inject_data = analysis_handler.GetPatchData(
                position,
                type_data.TargetType.Module,
                type_data.PatchType.Module,
                target, handler,
                new InjectAttribute.ArgumentHandler(inject_data)
            ).Unwrap();
            patch_inject_data.LocalCaptures = attrs.LocalCaptures;

            type_data.Injections.Add(patch_inject_data);


            //var handler_insert_target_path = new MethodPath(
            //    new Signature(handler, forced_name: attrs.AliasedName),
            //    type_data.TargetType
            //);
            //var handler_insert_target = TryGetTargetMethod(handler_insert_target_path);
            //if (handler_insert_target != null) {
            //    throw new InjectHandlerNameTakenException(handler_path, handler_insert_target_path);
            //}

            //var handler_insert_data = new PatchMethodData(
            //    handler,
            //    handler_insert_target_path, handler_path,
            //    aliased_name: attrs.AliasedName,
            //    injection_handler: true
            //);

            //type_data.Methods.Add(handler_insert_data);
        }

        public void ScanMethods(PatchTypeData type_data, Mono.Collections.Generic.Collection<MethodDefinition> methods) {
            foreach (var method in methods) {
                if (IgnoredMethods.Contains(method)) {
                    Logger.Debug($"Method {method.BuildSignature()} is excluded from analysis");
                    continue;
                }

                Logger.Debug($"Scanning method: {method.BuildSignature()}");

                var method_attrs = new SpecialAttributeData(method.CustomAttributes);

                if (method_attrs.InjectData != null) {
                    _HandleInject(type_data, method, method_attrs);
                    continue;
                }

                var is_void = method.ReturnType.IsSame(SemiPatch.VoidType);
                var patch_path = method.ToPath();
                var name = method_attrs.AliasedName ?? method.Name;
                var target_path = method.ToPath(method_attrs.ReceiveOriginal, forced_name: name).WithDeclaringType(type_data.TargetType.Resolve());
                var target = TryGetTargetMethod(target_path);

                if (method.IsConstructor) {
                    if (!method_attrs.TreatConstructorLikeMethod) {
                        if (method.Parameters.Count != 0) throw new UntaggedConstructorException(patch_path);
                        if (method.Body.Instructions.Count == 0) {
                            Logger.Debug($"Skipping default constructor with 0 instructions (some weirdness is afoot?)");
                            continue;
                        }

                        var instr = method.Body.Instructions[0];
                        instr = instr.FirstAfterNops();
                        if (instr == null || instr.OpCode != OpCodes.Ldarg_0) {
                            throw new UntaggedConstructorException(patch_path);
                        }
                        var base_type_ctor_path = TryGetParameterlessCtor(type_data.PatchType.BaseType).Resolve().ToPath();
                        instr = instr.Next?.FirstAfterNops();
                        if (instr == null || (instr.OpCode != OpCodes.Call || ((MethodReference)instr.Operand).Resolve().ToPath() != base_type_ctor_path)) {
                            throw new UntaggedConstructorException(patch_path);
                        }

                        instr = instr.Next?.FirstAfterNops();
                        if (instr == null || instr.OpCode != OpCodes.Ret) {
                            throw new UntaggedConstructorException(patch_path);
                        }

                        Logger.Debug($"Adding empty default constructor as Proxy: '{patch_path}' -> '{target_path}'");

                        var ctor_data = new PatchMethodData(target, method, target_path, patch_path, proxy: true);
                        type_data.Methods.Add(ctor_data);

                        if (target == null) {
                            Logger.Warn($"Empty default constructor '{patch_path}' doesn't exist in target under '{target_path}' - it will be marked as FalseDefaultConstructor, and Relinker will reject calls.");
                            ctor_data.FalseDefaultConstructor = true;

                        }
                        continue;
                    }
                }

                Logger.Debug($"Path: '{patch_path}'");

                var method_data = new PatchMethodData(method, target_path, patch_path, receives_original: method_attrs.ReceiveOriginal);
                type_data.Methods.Add(method_data);

                if (method_attrs.AliasedName != null) {
                    method_data.AliasedName = method_attrs.AliasedName;
                }

                if (method_attrs.Ignore) {
                    Logger.Debug($"Ignored!");
                    method_data.ExplicitlyIgnored = true;
                    continue;
                }

                if (method_attrs.ReceiveOriginal && method.Parameters.Count == 0) {
                    throw new InvalidReceiveOriginalPatchException($"Method '{patch_path}' is marked as ReceiveOriginal, but it has no arguments.", patch_path);
                }
                

                if (method_attrs.IsPropertyMethod) {
                    if (method_attrs.PropertyGetter != null && method_attrs.PropertySetter != null) {
                        throw new InvalidAttributeCombinationException($"Method '{patch_path}' may not be marked as both Getter and Setter at the same time.");
                    }

                    PropertyPath target_prop_path = null;

                    if (method_attrs.PropertyGetter != null) {
                        name = $"get_{method_attrs.PropertyGetter}";
                        target_prop_path = method.ToPropertyPathFromGetter(method_attrs.PropertyGetter);
                    } else if (method_attrs.PropertySetter != null) {
                        name = $"set_{method_attrs.PropertySetter}";
                        target_prop_path = method.ToPropertyPathFromSetter(method_attrs.PropertySetter, skip_first_arg: method_attrs.ReceiveOriginal);
                    }

                    method_data.AliasedName = name;

                    target_prop_path = target_prop_path.WithDeclaringType(type_data.TargetType.Resolve());

                    var target_prop = TryGetTargetProperty(target_prop_path);

                    if (method_attrs.Insert) {
                        if (target_prop != null && ((target_prop.GetMethod != null && method_attrs.PropertyGetter != null) || (target_prop.SetMethod != null && method_attrs.PropertySetter != null))) {
                            throw new InsertTargetExistsException($"Found matching property '{target_prop_path}', but Getter/Setter patch method '{patch_path}' was marked Insert - drop the attribute if you want to modify the getter/setter of this property.", patch_path, target_prop_path);
                        }
                    } else {
                        if (target_prop == null) {
                            throw new PatchTargetNotFoundException($"Failed to locate property '{target_prop_path}' patched in Getter/Setter patch method '{patch_path}' - use the Insert attribute on a real property if you want to add one, or change the Getter/Setter attribute argument if you want to use a different attribute.", patch_path, target_prop_path);

                        }
                        if (target_prop != null && ((target_prop.GetMethod == null && method_attrs.PropertyGetter != null) || (target_prop.SetMethod == null && method_attrs.PropertySetter != null))) {
                            throw new PatchTargetNotFoundException($"Found matching property '{target_prop_path}', but the getter/setter targetted by the patch method '{patch_path}' doesn't exist - use the Insert attribute on the Getter/Setter method to add it.", patch_path, target_prop_path);
                        }
                    }
                }

                if (method_attrs.Proxy) {
                    if (method_attrs.ReceiveOriginal) {
                        throw new InvalidAttributeCombinationException($"Proxy method '{patch_path}' may not be marked as ReceiveOriginal");
                    }
                    if (target == null) {
                        throw new PatchTargetNotFoundException($"Failed to locate method '{target_path}' proxied in '{patch_path}' - use the Insert attribute instead of Proxy if it should be added, or the TargetName attribute if you want to use a different name.", patch_path, target_path);
                    }
                    Logger.Debug($"Ignored (Proxy)!");
                    method_data.Proxy = true;
                    continue;
                }

                if (method_attrs.ReceiveOriginal) {
                    if (method.Parameters.Count == 0) throw new InvalidReceiveOriginalPatchException($"Method '{patch_path}' is marked as ReceiveOriginal, but it has no arguments", patch_path);
                    var orig_type = method.Parameters[0].ParameterType;
                    if (OrigFactory.TypeIsGenericOrig(orig_type)) {
                        if (is_void) throw new InvalidReceiveOriginalPatchException($"First parameter of method '{patch_path}' (marked ReceiveOriginal) is of type Orig, but the method does not return anything.", patch_path);
                    } else if (OrigFactory.TypeIsGenericVoidOrig(orig_type)) {
                        if (!is_void) throw new InvalidReceiveOriginalPatchException($"First parameter of method '{patch_path}' (marked ReceiveOriginal) is of type VoidOrig, but the method returns a non-void value.", patch_path);
                    } else {
                        throw new InvalidReceiveOriginalPatchException($"First parameter of method '{patch_path}' (tagged with ReceiveOriginal) must be a Orig or VoidOrig delegate.", patch_path);
                    }

                    var orig_sig = OrigFactory.GetMethodSignatureFromOrig(orig_type, method_attrs.AliasedName ?? method.Name, method.GenericParameters);
                    if (target_path.Signature != orig_sig) {
                        var maybe_new_orig_sig = new Signature(OrigFactory.OrigTypeForMethod(method.Module, method, skip_first_arg: true));

                        throw new InvalidReceiveOriginalPatchException($"Orig mismatch detected in method '{patch_path}'. Method is tagged as ReceiveOriginal and contains an Orig parameter '{orig_type.BuildSignature()}'. The method's signature points to '{target_path}', but the signature generated from the first argument of the method is '{orig_sig}'. Check if your patch method's signature matches the original method. If it is the Orig/VoidOrig parameter that's wrong, use this signature: '{maybe_new_orig_sig}'.", patch_path);
                    }

                    ValidationRelinker.Map(
                        patch_path,
                        Relinker.MemberEntry.Rejected(new ReceiveOriginalInvokeException(
                            patch_path
                        ))
                    );
                } else {
                    if (method.Parameters.Count > 0 && (OrigFactory.TypeIsGenericOrig(method.Parameters[0].ParameterType) || OrigFactory.TypeIsGenericVoidOrig(method.Parameters[0].ParameterType))) {
                        throw new InvalidReceiveOriginalPatchException($"First parameter of method '{patch_path}' is an Orig or VoidOrig delegate, but the method is not marked with the ReceiveOriginal attribute. Please add the attribute if you wish to call the original method within the patch or get rid of the argument if you don't.", patch_path);
                    }
                }


                method_data.TargetPath = target_path;
                method_data.ReceivesOriginal = method_attrs.ReceiveOriginal;

                if (method_attrs.Insert) {
                    Logger.Debug($"Method is marked for insertion");
                    if (target != null) {
                        throw new InsertTargetExistsException($"Found matching method '{target_path}', but patch in '{patch_path}' was marked Insert - drop the attribute if you want to modify the method.", patch_path, target_path);
                    }
                } else {
                    Logger.Debug($"Searching in target type");
                    if (target == null) {
                        throw new PatchTargetNotFoundException($"Failed to locate method '{target_path}' patched in '{patch_path}' - use the Insert attribute if it should be added, or the TargetName attribute if you want to use a different name.", patch_path, target_path);
                    }

                    var target_attrs = target.Attributes;
                    var patch_attrs = method.Attributes;

                    if (method_attrs.IsPropertyMethod) {
                        patch_attrs |= MethodAttributes.SpecialName;
                    }

                    if (target_attrs != patch_attrs) {
                        throw new PatchTargetAttributeMismatchException($"Attribute mismatch in patch method '{patch_path}' targetting method '{target_path}' - patch attributes are '{method.Attributes}', but target attributes are '{target.Attributes}'. The mismatch is with the following attribute(s): '{((MethodAttributes)((uint)target_attrs ^ (uint)patch_attrs)).ToString().Replace("ReuseSlot, ", "")}'.", patch_path, target_path);
                    }

                    method_data.TargetMember = target;
                }
            }
        }

        public void ScanFields(PatchTypeData type_data, Collection<FieldDefinition> fields) {
            foreach (var field in fields) {
                if (IgnoredFields.Contains(field)) {
                    Logger.Debug($"Field {field.BuildSignature()} is excluded from analysis");
                    continue;
                }

                Logger.Debug($"Scanning field: {field.FullName}");
                var field_attrs = new SpecialAttributeData(field.CustomAttributes);

                var patch_path = field.ToPath();
                var target_path = field_attrs.AliasedName == null ? patch_path : field.ToPath(forced_name: field_attrs.AliasedName);
                target_path = target_path.WithDeclaringType(type_data.TargetType.Resolve());

                var field_data = new PatchFieldData(field, target_path, patch_path);
                type_data.Fields.Add(field_data);

                if (field_attrs.Ignore) {
                    Logger.Debug($"Ignored!");
                    field_data.ExplicitlyIgnored = true;
                    continue;
                }

                if (field_attrs.AliasedName != null) {
                    field_data.AliasedName = field_attrs.AliasedName;
                }

                var target_field = TryGetTargetField(target_path);

                if (field_attrs.Proxy) {
                    Logger.Debug($"Field is marked for proxying");
                    if (target_field == null) {
                        throw new PatchTargetNotFoundException($"Failed to locate field'{target_path}' proxied in patch field '{patch_path}'. Use the Insert attribute if you want to add the field.", patch_path, target_path);
                    }
                    field_data.TargetMember = target_field;
                } else if (field_attrs.Insert) {
                    Logger.Debug($"Field is marked for insertion");
                    if (target_field != null) {
                        throw new InsertTargetExistsException($"Found matching field '{target_path}', but patch field '{patch_path}' was marked Insert - use the Proxy attribute instead if you want to access fields on the class.", patch_path, target_path);
                    }
                } else {
                    throw new InvalidAttributeCombinationException($"Field '{patch_path}' must be marked as either Ignore, Insert, or Proxy. Fields without attributes are not allowed.");
                }
            }
        }

        public void ScanProperties(PatchTypeData type_data, Collection<PropertyDefinition> props) {
            for (var i = 0; i < props.Count; i++) {
                var prop = props[i];

                Logger.Debug($"Scanning property: {prop.FullName}");

                var patch_path = prop.ToPath();

                var prop_attrs = new SpecialAttributeData(prop.CustomAttributes);
                if (!prop_attrs.Insert && !prop_attrs.Proxy && !prop_attrs.Ignore) {
                    throw new InvalidAttributeCombinationException($"Failed patching property '{patch_path}'. Properties may not be used in a patch class unless they are marked with Insert, Proxy or Ignore. For patching properties, use the Getter and Setter attributes.");
                }

                var target_path = prop_attrs.AliasedName == null ? patch_path : prop.ToPath(forced_name: prop_attrs.AliasedName);
                target_path = target_path.WithDeclaringType(type_data.TargetType.Resolve());

                var prop_data = new PatchPropertyData(prop, target_path, patch_path);
                type_data.Properties.Add(prop_data);

                if (prop_attrs.Proxy) {
                    prop_data.Proxy = true;
                }

                if (prop_attrs.AliasedName != null) {
                    prop_data.AliasedName = prop_attrs.AliasedName;
                }

                if (prop.GetMethod != null) {
                    var get_path = prop.GetMethod.ToPath();
                    var get_data = new PatchMethodData(prop.GetMethod, get_path.WithDeclaringType(type_data.TargetType.Resolve()), get_path);
                    if (prop_attrs.Proxy) get_data.Proxy = true;
                    if (prop_data.AliasedName != null) get_data.AliasedName = $"get_{prop_data.AliasedName}";
                    if (prop_attrs.Ignore) get_data.ExplicitlyIgnored = true;
                    type_data.Methods.Add(get_data);
                    IgnoredMethods.Add(prop.GetMethod);
                }

                if (prop.SetMethod != null) {
                    var set_path = prop.SetMethod.ToPath();
                    var set_data = new PatchMethodData(prop.SetMethod, set_path.WithDeclaringType(type_data.TargetType.Resolve()), set_path);
                    if (prop_attrs.Proxy) set_data.Proxy = true;
                    if (prop_data.AliasedName != null) set_data.AliasedName = $"set_{prop_data.AliasedName}";
                    if (prop_attrs.Ignore) set_data.ExplicitlyIgnored = true;
                    type_data.Methods.Add(set_data);
                    IgnoredMethods.Add(prop.SetMethod);
                }

                var backing_field_name = $"<{prop.Name}>k__BackingField";
                FieldDefinition backing_field = null;
                for (var j = 0; j < prop.DeclaringType.Fields.Count; j++) {
                    var field = prop.DeclaringType.Fields[j];
                    if (field.Name == backing_field_name) {
                        backing_field = field;
                        break;
                    }
                }

                if (backing_field != null) {
                    var backing_field_path = backing_field.ToPath();
                    var backing_field_data = new PatchFieldData(backing_field, backing_field_path, backing_field_path);
                    if (prop_attrs.Proxy) backing_field_data.Proxy = true;
                    if (backing_field_data.AliasedName != null) backing_field_data.AliasedName = $"<{prop_data.AliasedName}>k__BackingField";
                    if (prop_attrs.Ignore) backing_field_data.ExplicitlyIgnored = true;
                    type_data.Fields.Add(backing_field_data);
                    IgnoredFields.Add(backing_field);
                }

                if (prop_attrs.Ignore) {
                    Logger.Debug($"Ignored!");
                    prop_data.ExplicitlyIgnored = true;
                    continue;
                }

                var target_prop = TryGetTargetProperty(target_path);

                if (prop_attrs.Insert) {
                    if (target_prop != null) {
                        throw new InsertTargetExistsException($"Property '{patch_path}' was marked with Insert, but the target class also contains this property - '{target_path}'. If you want to patch properties, use the Getter and Setter attributes.", patch_path, target_path);
                    }
                } else if (prop_attrs.Proxy) {
                    if (target_prop == null) {
                        throw new PatchTargetNotFoundException($"Property '{patch_path}' was marked with Proxy, but the target class does not contain the property '{target_path}'. If you want to add properties, use the Insert attribute.", patch_path, target_path);
                    }

                    if (prop.GetMethod != null && target_prop.GetMethod == null) {
                        throw new PatchTargetNotFoundException($"Property '{patch_path}' was marked with Proxy and the target property '{target_path}' exists, but it does not have a getter. If you want to add the getter, use the Getter and Insert attributes on a method.", patch_path, target_path);
                    }

                    if (prop.SetMethod != null && target_prop.SetMethod == null) {
                        throw new PatchTargetNotFoundException($"Property '{patch_path}' was marked with Proxy and the target property '{target_path}' exists, but it does not have a setter. If you want to add the setter, use the Setter and Insert attributes on a method.", patch_path, target_path);
                    }
                }
            }
        }

        public void ScanTypes(Mono.Collections.Generic.Collection<TypeDefinition> types) {
            Logger.Info($"Scanning {types.Count} types");
            foreach (var type in types) {
                Logger.Debug($"Scanning type: {type.Name}");
                var attrs = new SpecialAttributeData(type.CustomAttributes);
                if (attrs.PatchType == null) continue;
                if (attrs.Ignore) continue;

                Logger.Debug($"Patch attribute detected on type: {type.Name}");

                if (!attrs.PatchType.Scope.IsSame(TargetModule)) {
                    throw new InvalidTargetTypeScopeException($"Patch target must be a type within the target module ('{TargetModule.FileName}')", attrs.PatchType.ToPath());
                }

                var type_data = new PatchTypeData(attrs.PatchType, type);
                PatchData.Types.Add(type_data);

                ScanProperties(type_data, type.Properties);
                ScanFields(type_data, type.Fields);
                ScanMethods(type_data, type.Methods);
                ScanTypes(type.NestedTypes);
            }
        }

        private void _Validate(ModuleDefinition module) {
            ValidationRelinker.Relink(module);
        }

        public PatchData Analyze() {
            PatchData = new PatchData(TargetModule, PatchModules);
            foreach (var mod in PatchModules) {
                Logger.Info($"Scanning module: {mod.Name}");
                ScanTypes(mod.Types);
            }
            foreach (var mod in PatchModules) {
                _Validate(mod);
            }
            return PatchData;
        }
    }
}
