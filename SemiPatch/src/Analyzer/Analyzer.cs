using System;
using System.Collections.Generic;
using ModTheGungeon;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace SemiPatch {
    public class Analyzer {
        public static Logger Logger = new Logger("SemiPatch");

        public ModuleDefinition TargetModule;
        public IList<ModuleDefinition> PatchModules;
        public IDictionary<MethodPath, MethodDefinition> MethodMap;
        public IDictionary<FieldPath, FieldDefinition> FieldMap;
        public IDictionary<PropertyPath, PropertyDefinition> PropertyMap;

        public HashSet<MethodDefinition> IgnoredMethods;
        public HashSet<FieldDefinition> IgnoredFields;

        public PatchData PatchData;

        public Analyzer(string target_path, IList<string> patch_paths) {
            Logger.Debug($"New Patcher created from {patch_paths.Count} paths");
            MethodMap = new Dictionary<MethodPath, MethodDefinition>();
            FieldMap = new Dictionary<FieldPath, FieldDefinition>();
            PropertyMap = new Dictionary<PropertyPath, PropertyDefinition>();
            IgnoredMethods = new HashSet<MethodDefinition>();
            IgnoredFields = new HashSet<FieldDefinition>();

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

        public void ScanMethods(PatchTypeData type_data, Mono.Collections.Generic.Collection<MethodDefinition> methods) {
            foreach (var method in methods) {
                if (IgnoredMethods.Contains(method)) {
                    Logger.Debug($"Method {method.BuildSignature()} is excluded from analysis");
                    continue;
                }

                var is_void = method.ReturnType.IsSame(SemiPatch.VoidType);

                Logger.Debug($"Scanning method: {method.BuildSignature()}");
                var method_attrs = new SpecialAttributeData(method.CustomAttributes);

                if (method.IsConstructor && !method_attrs.TreatConstructorLikeMethod) {
                    Logger.Debug($"Skipping constructor");
                    continue;
                }

                var patch_path = method.ToPath();
                var name = method_attrs.AliasedName ?? method.Name;
                var target_path = method.ToPath(method_attrs.ReceiveOriginal, forced_name: name).WithDeclaringType(type_data.TargetType.Resolve());
                var target = TryGetTargetMethod(target_path);

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
                    throw new Exception($"Method '{patch_path}' is marked as ReceiveOriginal, but it has no arguments.");
                }
                

                if (method_attrs.IsPropertyMethod) {
                    if (method_attrs.PropertyGetter != null && method_attrs.PropertySetter != null) {
                        throw new Exception($"Method '{patch_path}' may not be marked as both Getter and Setter at the same time.");
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
                            throw new Exception($"Found matching property '{target_prop_path}', but Getter/Setter patch method '{patch_path}' was marked Insert - drop the attribute if you want to modify the getter/setter of this property.");
                        }
                    } else {
                        if (target_prop == null) {
                            throw new Exception($"Failed to locate property '{target_prop_path}' patched in Getter/Setter patch method '{patch_path}' - use the Insert attribute on a real property if you want to add one, or change the Getter/Setter attribute argument if you want to use a different attribute.");

                        }
                        if (target_prop != null && ((target_prop.GetMethod == null && method_attrs.PropertyGetter != null) || (target_prop.SetMethod == null && method_attrs.PropertySetter != null))) {
                            throw new Exception($"Found matching property '{target_prop_path}', but the getter/setter targetted by the patch method '{patch_path}' doesn't exist - use the Insert attribute on the Getter/Setter method to add it.");
                        }
                    }
                }

                if (method_attrs.Proxy) {
                    if (method_attrs.ReceiveOriginal) {
                        throw new Exception($"Proxy method '{patch_path}' may not be marked as ReceiveOriginal");
                    }
                    if (target == null) {
                        throw new Exception($"Failed to locate method '{target_path}' proxied in '{patch_path}' - use the Insert attribute instead of Proxy if it should be added, or the TargetName attribute if you want to use a different name.");
                    }
                    Logger.Debug($"Ignored (Proxy)!");
                    method_data.Proxy = true;
                    continue;
                }

                if (method_attrs.ReceiveOriginal) {
                    if (method.Parameters.Count == 0) throw new Exception($"Method '{patch_path}' is marked as ReceiveOriginal, but it has no arguments");
                    var orig_type = method.Parameters[0].ParameterType;
                    if (OrigFactory.TypeIsGenericOrig(orig_type)) {
                        if (is_void) throw new Exception($"First parameter of method '{patch_path}' (marked ReceiveOriginal) is of type Orig, but the method does not return anything.");
                    } else if (OrigFactory.TypeIsGenericVoidOrig(orig_type)) {
                        if (!is_void) throw new Exception($"First parameter of method '{patch_path}' (marked ReceiveOriginal) is of type VoidOrig, but the method returns a non-void value.");
                    } else {
                        throw new Exception($"First parameter of method '{patch_path}' (tagged with ReceiveOriginal) must be a Orig or VoidOrig delegate.");
                    }

                    var orig_sig = OrigFactory.GetMethodSignatureFromOrig(orig_type, method_attrs.AliasedName ?? method.Name, method.GenericParameters);
                    var maybe_new_orig_sig = new Signature(OrigFactory.OrigTypeForMethod(method.Module, method, skip_first_arg: true));

                    if (target_path.Signature != orig_sig) {
                        throw new Exception($"Orig mismatch detected in method '{patch_path}'. Method is tagged as ReceiveOriginal and contains an Orig parameter '{orig_type.BuildSignature()}'. The method's signature points to '{target_path}', but the signature generated from the first argument of the method is '{orig_sig}'. Check if your patch method's signature matches the original method. If it is the Orig/VoidOrig parameter that's wrong, use this signature: '{maybe_new_orig_sig}'.");
                    }
                } else {
                    if (method.Parameters.Count > 0 && (OrigFactory.TypeIsGenericOrig(method.Parameters[0].ParameterType) || OrigFactory.TypeIsGenericVoidOrig(method.Parameters[0].ParameterType))) {
                        throw new Exception($"First parameter of method '{patch_path}' is an Orig or VoidOrig delegate, but the method is not marked with the ReceiveOriginal attribute. Please add the attribute if you wish to call the original method within the patch or get rid of the argument if you don't.");
                    }
                }


                method_data.TargetPath = target_path;
                method_data.ReceivesOriginal = method_attrs.ReceiveOriginal;

                if (method_attrs.Insert) {
                    Logger.Debug($"Method is marked for insertion");
                    if (target != null) {
                        throw new Exception($"Found matching method '{target_path}', but patch in '{patch_path}' was marked Insert - drop the attribute if you want to modify the method.");
                    }
                } else {
                    Logger.Debug($"Searching in target type");
                    if (target == null) {
                        throw new Exception($"Failed to locate method '{target_path}' patched in '{patch_path}' - use the Insert attribute if it should be added, or the TargetName attribute if you want to use a different name.");
                    }

                    var target_attrs = target.Attributes;
                    var patch_attrs = method.Attributes;

                    if (method_attrs.IsPropertyMethod) {
                        patch_attrs |= MethodAttributes.SpecialName;
                    }

                    if (target_attrs != patch_attrs) {
                        throw new Exception($"Attribute mismatch in patch method '{patch_path}' targetting method '{target_path}' - patch attributes are '{method.Attributes}', but target attributes are '{target.Attributes}'. The mismatch is with the following attribute(s): '{((MethodAttributes)((uint)target_attrs ^ (uint)patch_attrs)).ToString().Replace("ReuseSlot, ", "")}'.");
                    }

                    method_data.Target = target;
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
                        throw new Exception($"Failed to locate field'{target_path}' proxied in patch field '{patch_path}'. Use the Insert attribute if you want to add the field.");
                    }
                    field_data.Target = target_field;
                } else if (field_attrs.Insert) {
                    Logger.Debug($"Field is marked for insertion");
                    if (target_field != null) {
                        throw new Exception($"Found matching field '{target_path}', but patch field '{patch_path}' was marked Insert - use the Proxy attribute instead if you want to access fields on the class.");
                    }
                } else {
                    throw new Exception($"Field '{patch_path}' must be marked as either Ignore, Insert, or Proxy. Fields without attributes are not allowed.");
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
                    throw new Exception($"Failed patching property '{patch_path}'. Properties may not be used in a patch class unless they are marked with Insert, Proxy or Ignore. For patching properties, use the Getter and Setter attributes.");
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
                    var get_data = new PatchMethodData(prop.GetMethod, get_path, get_path);
                    if (prop_attrs.Proxy) get_data.Proxy = true;
                    if (prop_data.AliasedName != null) get_data.AliasedName = $"get_{prop_data.AliasedName}";
                    if (prop_attrs.Ignore) get_data.ExplicitlyIgnored = true;
                    type_data.Methods.Add(get_data);
                    IgnoredMethods.Add(prop.GetMethod);
                }

                if (prop.SetMethod != null) {
                    var set_path = prop.SetMethod.ToPath();
                    var set_data = new PatchMethodData(prop.SetMethod, set_path, set_path);
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
                        throw new Exception($"Property '{patch_path}' was marked with Insert, but the target class also contains this property - '{target_path}'. If you want to patch properties, use the Getter and Setter attributes.");
                    }
                } else if (prop_attrs.Proxy) {
                    if (target_prop == null) {
                        throw new Exception($"Property '{patch_path}' was marked with Proxy, but the target class does not contain the property '{target_path}'. If you want to add properties, use the Insert attribute.");
                    }

                    if (prop.GetMethod != null && target_prop.GetMethod == null) {
                        throw new Exception($"Property '{patch_path}' was marked with Proxy and the target property '{target_path}' exists, but it does not have a getter. If you want to add the getter, use the Getter and Insert attributes on a method.");
                    }

                    if (prop.SetMethod != null && target_prop.SetMethod == null) {
                        throw new Exception($"Property '{patch_path}' was marked with Proxy and the target property '{target_path}' exists, but it does not have a setter. If you want to add the setter, use the Setter and Insert attributes on a method.");
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
                    throw new Exception($"Patch target must be a type within the target module ('{TargetModule.FileName}')");
                }

                var type_data = new PatchTypeData(attrs.PatchType, type);
                PatchData.Types.Add(type_data);

                ScanProperties(type_data, type.Properties);
                ScanFields(type_data, type.Fields);
                ScanMethods(type_data, type.Methods);
                ScanTypes(type.NestedTypes);
            }
        }

        public PatchData Analyze() {
            PatchData = new PatchData(TargetModule, PatchModules);
            foreach (var mod in PatchModules) {
                Logger.Info($"Scanning module: {mod.Name}");
                ScanTypes(mod.Types);
            }
            return PatchData;
        }
    }
}
