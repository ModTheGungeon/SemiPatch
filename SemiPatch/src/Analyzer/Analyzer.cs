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
        public IDictionary<string, MethodDefinition> MethodMap;
        public IDictionary<string, FieldDefinition> FieldMap;
        public IDictionary<string, PropertyDefinition> PropertyMap;

        public HashSet<MethodDefinition> IgnoredMethods;
        public HashSet<FieldDefinition> IgnoredFields;

        public PatchData PatchData;

        public Analyzer(string target_path, IList<string> patch_paths) {
            Logger.Debug($"New Patcher created from {patch_paths.Count} paths");
            MethodMap = new Dictionary<string, MethodDefinition>();
            FieldMap = new Dictionary<string, FieldDefinition>();
            PropertyMap = new Dictionary<string, PropertyDefinition>();
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
                    var sig = method.BuildSignature();
                    var sig_prefixed = type.PrefixSignature(sig);
                    Logger.Debug($"Caching method: '{sig_prefixed}'");
                    MethodMap[sig_prefixed] = method;
                }
                foreach (var field in type.Fields) {
                    var sig = field.BuildSignature();
                    var sig_prefixed = type.PrefixSignature(sig);
                    Logger.Debug($"Caching field: '{sig_prefixed}'");
                    FieldMap[sig_prefixed] = field;
                }
                foreach (var prop in type.Properties) {
                    var sig = prop.BuildSignature();
                    var sig_prefixed = type.PrefixSignature(sig);
                    Logger.Debug($"Caching property: '{sig_prefixed}'");
                    PropertyMap[sig_prefixed] = prop;
                }
            }
        }

        public MethodDefinition TryGetTargetMethod(string sig) {
            if (MethodMap.TryGetValue(sig, out MethodDefinition def)) return def;
            return null;
        }

        public FieldDefinition TryGetTargetField(string sig) {
            if (FieldMap.TryGetValue(sig, out FieldDefinition def)) return def;
            return null;
        }

        public PropertyDefinition TryGetTargetProperty(string sig) {
            if (PropertyMap.TryGetValue(sig, out PropertyDefinition def)) return def;
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

                var patch_sig = method.BuildSignature();
                var patch_sig_prefixed = method.DeclaringType.PrefixSignature(patch_sig);
                Logger.Debug($"Signature: '{patch_sig_prefixed}'");
                var method_data = new PatchMethodData(method, patch_sig, receives_original: method_attrs.ReceiveOriginal);
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
                    throw new Exception($"Method '{patch_sig}' is marked as ReceiveOriginal, but it has no arguments.");
                }

                var name = method_attrs.AliasedName ?? method.Name;

                if (method_attrs.IsPropertyMethod) {
                    if (method_attrs.PropertyGetter != null && method_attrs.PropertySetter != null) {
                        throw new Exception($"Method '{patch_sig_prefixed}' may not be marked as both Getter and Setter at the same time.");
                    }

                    string target_prop_sig = null;

                    if (method_attrs.PropertyGetter != null) {
                        name = $"get_{method_attrs.PropertyGetter}";
                        target_prop_sig = method.BuildPropertySignatureFromGetter(method_attrs.PropertyGetter);
                    } else if (method_attrs.PropertySetter != null) {
                        name = $"set_{method_attrs.PropertySetter}";
                        target_prop_sig = method.BuildPropertySignatureFromSetter(method_attrs.PropertySetter);
                    }

                    method_data.AliasedName = name;

                    var target_prop_sig_prefixed = type_data.TargetType.PrefixSignature(target_prop_sig);

                    var target_prop = TryGetTargetProperty(target_prop_sig_prefixed);

                    if (method_attrs.Insert) {
                        if (target_prop != null && ((target_prop.GetMethod != null && method_attrs.PropertyGetter != null) || (target_prop.SetMethod != null && method_attrs.PropertySetter != null))) {
                            throw new Exception($"Found matching property '{target_prop_sig_prefixed}', but Getter/Setter patch method '{patch_sig_prefixed}' was marked Insert - drop the attribute if you want to modify the getter/setter of this property.");
                        }
                    } else {
                        if (target_prop == null) {
                            throw new Exception($"Failed to locate property '{target_prop_sig_prefixed}' patched in Getter/Setter patch method '{patch_sig_prefixed}' - use the Insert attribute on a real property if you want to add one, or change the Getter/Setter attribute argument if you want to use a different attribute.");

                        }
                        if (target_prop != null && ((target_prop.GetMethod == null && method_attrs.PropertyGetter != null) || (target_prop.SetMethod == null && method_attrs.PropertySetter != null))) {
                            throw new Exception($"Found matching property '{target_prop_sig_prefixed}', but the getter/setter targetted by the patch method '{patch_sig_prefixed}' doesn't exist - use the Insert attribute on the Getter/Setter method to add it.");
                        }
                    }
                }

                var target_sig = method.BuildSignature(method_attrs.ReceiveOriginal, forced_name: name);
                var target_sig_prefixed = type_data.TargetType.PrefixSignature(target_sig);
                var target = TryGetTargetMethod(target_sig_prefixed);

                if (method_attrs.Proxy) {
                    if (method_attrs.ReceiveOriginal) {
                        throw new Exception($"Proxy method '{patch_sig_prefixed}' may not be marked as ReceiveOriginal");
                    }
                    if (target == null) {
                        throw new Exception($"Failed to locate method '{target_sig_prefixed}' proxied in '{patch_sig_prefixed}' - use the Insert attribute instead of Proxy if it should be added, or the TargetName attribute if you want to use a different name.");
                    }
                    Logger.Debug($"Ignored (Proxy)!");
                    method_data.Proxy = true;
                    continue;
                }

                if (method_attrs.ReceiveOriginal) {
                    if (method.Parameters.Count == 0) throw new Exception($"Method '{patch_sig_prefixed}' is marked as ReceiveOriginal, but it has no arguments");
                    var orig_type = method.Parameters[0].ParameterType;
                    if (OrigFactory.TypeIsGenericOrig(orig_type)) {
                        if (is_void) throw new Exception($"First parameter of method '{patch_sig_prefixed}' (marked ReceiveOriginal) is of type Orig, but the method does not return anything.");
                    } else if (OrigFactory.TypeIsGenericVoidOrig(orig_type)) {
                        if (!is_void) throw new Exception($"First parameter of method '{patch_sig_prefixed}' (marked ReceiveOriginal) is of type VoidOrig, but the method returns a non-void value.");
                    } else {
                        throw new Exception($"First parameter of method '{patch_sig_prefixed}' (tagged with ReceiveOriginal) must be a Orig or VoidOrig delegate.");
                    }

                    var orig_sig = OrigFactory.BuildMethodSignatureFromOrig(orig_type, method_attrs.AliasedName ?? method.Name, method.GenericParameters);
                    var maybe_new_orig_sig = OrigFactory.OrigTypeForMethod(method.Module, method, skip_first_arg: true).BuildSignature();

                    if (target_sig != orig_sig) {
                        throw new Exception($"Orig mismatch detected in method '{patch_sig_prefixed}'. Method is tagged as ReceiveOriginal and contains an Orig parameter '{orig_type.BuildSignature()}'. The method's signature points to '{target_sig}', but the signature generated from the first argument of the method is '{orig_sig}'. Check if your patch method's signature matches the original method. If it is the Orig/VoidOrig parameter that's wrong, use this signature: '{maybe_new_orig_sig}'.");
                    }
                } else {
                    if (method.Parameters.Count > 0 && (OrigFactory.TypeIsGenericOrig(method.Parameters[0].ParameterType) || OrigFactory.TypeIsGenericVoidOrig(method.Parameters[0].ParameterType))) {
                        throw new Exception($"First parameter of method '{patch_sig_prefixed}' is an Orig or VoidOrig delegate, but the method is not marked with the ReceiveOriginal attribute. Please add the attribute if you wish to call the original method within the patch or get rid of the argument if you don't.");
                    }
                }


                method_data.PatchSignature = patch_sig;
                method_data.ReceivesOriginal = method_attrs.ReceiveOriginal;

                if (method_attrs.Insert) {
                    Logger.Debug($"Method is marked for insertion");
                    if (target != null) {
                        throw new Exception($"Found matching method '{target_sig_prefixed}', but patch in '{patch_sig_prefixed}' was marked Insert - drop the attribute if you want to modify the method.");
                    }
                } else {
                    Logger.Debug($"Searching in target type");
                    if (target == null) {
                        throw new Exception($"Failed to locate method '{target_sig_prefixed}' patched in '{patch_sig_prefixed}' - use the Insert attribute if it should be added, or the TargetName attribute if you want to use a different name.");
                    }

                    var target_attrs = target.Attributes;
                    var patch_attrs = method.Attributes;

                    if (method_attrs.IsPropertyMethod) {
                        patch_attrs |= MethodAttributes.SpecialName;
                    }

                    if (target_attrs != patch_attrs) {
                        throw new Exception($"Attribute mismatch in patch method '{patch_sig_prefixed}' targetting method '{target_sig_prefixed}' - patch attributes are '{method.Attributes}', but target attributes are '{target.Attributes}'. The mismatch is with the following attribute(s): '{((MethodAttributes)((uint)target_attrs ^ (uint)patch_attrs)).ToString().Replace("ReuseSlot, ", "")}'.");
                    }

                    method_data.TargetSignature = target_sig;
                    method_data.TargetMethod = target;
                }
            }
        }

        public void ScanFields(PatchTypeData type_data, Collection<FieldDefinition> fields) {
            foreach (var field in fields) {
                if (IgnoredFields.Contains(field)) {
                    Logger.Debug($"Field {field.BuildSignature()} is excluded from analysis");
                    continue;
                }

                var patch_sig = field.BuildSignature();
                var patch_sig_prefixed = type_data.PatchType.PrefixSignature(patch_sig);

                Logger.Debug($"Scanning field: {field.FullName}");
                var field_attrs = new SpecialAttributeData(field.CustomAttributes);
                var field_data = new PatchFieldData(patch_sig, patch_sig, field);
                type_data.Fields.Add(field_data);

                if (field_attrs.Ignore) {
                    Logger.Debug($"Ignored!");
                    field_data.ExplicitlyIgnored = true;
                    continue;
                }

                if (field_attrs.AliasedName != null) {
                    field_data.AliasedName = field_attrs.AliasedName;
                }

                var target_sig = field_attrs.AliasedName == null ? patch_sig : field.BuildSignature(forced_name: field_attrs.AliasedName);
                var target_sig_prefixed = type_data.TargetType.PrefixSignature(target_sig);

                field_data.TargetSignature = target_sig;

                var target_field = TryGetTargetField(target_sig_prefixed);

                if (field_attrs.Proxy) {
                    Logger.Debug($"Field is marked for proxying");
                    if (target_field == null) {
                        throw new Exception($"Failed to locate field'{target_sig_prefixed}' proxied in patch field '{patch_sig_prefixed}'. Use the Insert attribute if you want to add the field.");
                    }
                    field_data.TargetField = target_field;
                } else if (field_attrs.Insert) {
                    Logger.Debug($"Field is marked for insertion");
                    if (target_field != null) {
                        throw new Exception($"Found matching field '{target_sig_prefixed}', but patch field '{patch_sig_prefixed}' was marked Insert - use the Proxy attribute instead if you want to access fields on the class.");
                    }
                    field_data.IsInsert = true;
                } else {
                    throw new Exception($"Field '{patch_sig_prefixed}' must be marked as either Ignore, Insert, or Proxy. Fields without attributes are not allowed.");
                }
            }
        }

        public void ScanProperties(PatchTypeData type_data, Collection<PropertyDefinition> props) {
            for (var i = 0; i < props.Count; i++) {
                var prop = props[i];

                var patch_sig = prop.BuildSignature();
                var patch_sig_prefixed = type_data.PatchType.PrefixSignature(patch_sig);

                Logger.Debug($"Scanning property: {prop.FullName}");
                var prop_attrs = new SpecialAttributeData(prop.CustomAttributes);

                if (!prop_attrs.Insert && !prop_attrs.Proxy && !prop_attrs.Ignore) {
                    throw new Exception($"Failed patching property '{patch_sig_prefixed}'. Properties may not be used in a patch class unless they are marked with Insert, Proxy or Ignore. For patching properties, use the Getter and Setter attributes.");
                }

                var prop_data = new PatchPropertyData(patch_sig, patch_sig, prop);
                type_data.Properties.Add(prop_data);

                if (prop_attrs.Proxy) {
                    prop_data.Proxy = true;
                }

                if (prop_attrs.AliasedName != null) {
                    prop_data.AliasedName = prop_attrs.AliasedName;
                }

                if (prop.GetMethod != null) {
                    var get_sig = prop.GetMethod.BuildSignature();
                    var get_data = new PatchMethodData(prop.GetMethod, get_sig, get_sig);
                    if (prop_attrs.Proxy) get_data.Proxy = true;
                    if (prop_data.AliasedName != null) get_data.AliasedName = $"get_{prop_data.AliasedName}";
                    if (prop_attrs.Ignore) get_data.ExplicitlyIgnored = true;
                    type_data.Methods.Add(get_data);
                    IgnoredMethods.Add(prop.GetMethod);
                }

                if (prop.SetMethod != null) {
                    var set_sig = prop.SetMethod.BuildSignature();
                    var set_data = new PatchMethodData(prop.SetMethod, set_sig, set_sig);
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
                    var backing_field_sig = backing_field.BuildSignature();
                    var backing_field_data = new PatchFieldData(backing_field_sig, backing_field_sig, backing_field);
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

                var target_sig = prop_attrs.AliasedName == null ? patch_sig : prop.BuildSignature(forced_name: prop_attrs.AliasedName);
                var target_sig_prefixed = type_data.TargetType.PrefixSignature(target_sig);

                prop_data.TargetSignature = target_sig;

                var target_prop = TryGetTargetProperty(target_sig_prefixed);

                if (prop_attrs.Insert) {
                    if (target_prop != null) {
                        throw new Exception($"Property '{patch_sig_prefixed}' was marked with Insert, but the target class also contains this property - '{target_sig_prefixed}'. If you want to patch properties, use the Getter and Setter attributes.");
                    }
                } else if (prop_attrs.Proxy) {
                    if (target_prop == null) {
                        throw new Exception($"Property '{patch_sig_prefixed}' was marked with Proxy, but the target class does not contain the property '{target_sig_prefixed}'. If you want to add properties, use the Insert attribute.");
                    }

                    if (prop.GetMethod != null && target_prop.GetMethod == null) {
                        throw new Exception($"Property '{patch_sig_prefixed}' was marked with Proxy and the target property '{target_sig_prefixed}' exists, but it does not have a getter. If you want to add the getter, use the Getter and Insert attributes on a method.");
                    }

                    if (prop.SetMethod != null && target_prop.SetMethod == null) {
                        throw new Exception($"Property '{patch_sig_prefixed}' was marked with Proxy and the target property '{target_sig_prefixed}' exists, but it does not have a setter. If you want to add the setter, use the Setter and Insert attributes on a method.");
                    }
                }
            }
        }

        public void ScanTypes(Mono.Collections.Generic.Collection<TypeDefinition> types) {
            Logger.Info($"Scanning {types.Count} types");
            foreach (var type in types) {
                Logger.Debug($"Scanning type: {type.Name}");
                Logger.Debug($"Scanning type: {MonoModStaticConverter.BuildMonoModSignature(type)}");
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
