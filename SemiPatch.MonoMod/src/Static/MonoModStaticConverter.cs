using System;
using System.Collections.Generic;
using System.Text;
using ModTheGungeon;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using MonoMod;
using MonoMod.InlineRT;

namespace SemiPatch {
    public class MonoModStaticConverter {
        public static ModuleDefinition MscorlibModule;
        public static TypeReference StringType;
        public static ModuleDefinition MonoModModule;
        public static MethodDefinition MonoModPatchAttributeConstructor;
        public static MethodDefinition MonoModConstructorAttributeConstructor;
        public static MethodDefinition MonoModIgnoreAttributeConstructor;
        public static MethodDefinition MonoModOriginalNameAttributeConstructor;
        public static MethodDefinition RDARSupportNameAliasedFromAttributeConstructor;
        public static MethodDefinition RDARSupportHasOriginalInAttributeConstructor;

        static MonoModStaticConverter() {
            MscorlibModule = ModuleDefinition.ReadModule(typeof(string).Assembly.Location);
            StringType = MscorlibModule.GetType("System.String");

            MonoModModule = ModuleDefinition.ReadModule(typeof(MonoMod.MonoModder).Assembly.Location);
            MonoModPatchAttributeConstructor = MonoModModule.GetType("MonoMod.MonoModPatch").Methods[0];
            MonoModConstructorAttributeConstructor = MonoModModule.GetType("MonoMod.MonoModConstructor").Methods[0];
            MonoModIgnoreAttributeConstructor = MonoModModule.GetType("MonoMod.MonoModIgnore").Methods[0];
            MonoModOriginalNameAttributeConstructor = MonoModModule.GetType("MonoMod.MonoModOriginalName").Methods[0];
            RDARSupportNameAliasedFromAttributeConstructor = RDAR.Support.RDARSupport.RDARSupportNameAliasedFromAttribute.Methods[0];
            RDARSupportHasOriginalInAttributeConstructor = RDAR.Support.RDARSupport.RDARSupportHasOriginalInAttribute.Methods[0];
        }

        public PatchData PatchData;
        public ModuleDefinition TargetModule;
        public Logger Logger;
        public IDictionary<MethodPath, string> OrigNameMap;
        public Relinker Relinker;

        public List<KeyValuePair<MethodDefinition, string>> PostRelinkMethodRenames;
        public List<KeyValuePair<MethodDefinition, string>> PostRelink;

        public MonoModStaticConverter(PatchData data) {
            PatchData = data;
            TargetModule = data.TargetModule;
            Logger = new Logger($"MonoModStaticConverter({TargetModule.Name})");
            OrigNameMap = new Dictionary<MethodPath, string>();
            Relinker = new Relinker();
        }

        public string MapOrigForMethod(PatchMethodData method) {
            var path = method.PatchPath;
            if (OrigNameMap.TryGetValue(path, out string name)) return $"orig_{name}";

            var s = new StringBuilder();
            s.Append("$SEMIPATCH$ORIG$$");
            s.Append(method.Target.Name);
            var new_name = OrigNameMap[path] = s.ToString();
            return $"orig_{new_name}";
        }

        public static string BuildMonoModSignature(TypeReference type) {
            var s = new StringBuilder();
            if (type.Namespace == "") {
                s.Append("global::");
            }
            s.Append(type.FullName);
            return s.ToString();
        }


        protected void AddAttribute(ModuleDefinition module, ICustomAttributeProvider obj, MethodReference ctor, params CustomAttributeArgument[] args) {
            var ctor_def = module.ImportReference(ctor);
            var attr = new CustomAttribute(ctor_def);
            for (var i = 0; i < args.Length; i++) attr.ConstructorArguments.Add(args[i]);
            obj.CustomAttributes.Add(attr);
        }

        public void ApplyForMethod(PatchMethodData method, List<PatchMethodData> methods_to_remove) {
            Logger.Debug($"Applying for patch method: '{method.PatchPath}' targetting '{method.TargetPath}'");

            if (method.ExplicitlyIgnored) {
                AddAttribute(method.Patch.Module, method.Patch, MonoModIgnoreAttributeConstructor);
                return;
            }

            if (method.Patch.IsConstructor) {
                AddAttribute(method.Patch.Module, method.Patch, MonoModConstructorAttributeConstructor);
            }

            if (method.AliasedName != null) {
                Logger.Debug($"Renaming method '{method.PatchPath}' to {method.AliasedName}");
                AddAttribute(
                    method.Patch.Module,
                    method.Patch,
                    RDARSupportNameAliasedFromAttributeConstructor,
                    new CustomAttributeArgument(StringType, method.Patch.Name)
                );
                Relinker.QueueMethodRename(method.Patch, method.AliasedName);
            }

            if (method.Proxy) {
                // don't emit it at all so that we avoid having multiple methods with the same name
                // (this could happen with proxies to ReceiveOriginal patches for example)
                Logger.Debug($"Method marked for removal (proxy)");
                methods_to_remove.Add(method);
                return;
            }

            if (!method.IsInsert) {
                var orig_name = MapOrigForMethod(method);
                Logger.Debug($"Creating orig method: '{orig_name}'");

                var orig_def = new MethodDefinition(orig_name, method.Patch.Attributes | MethodAttributes.PInvokeImpl, method.Patch.ReturnType);
                for (var i = 0; i < method.Patch.GenericParameters.Count; i++) {
                    var patch_param = method.Patch.GenericParameters[i];
                    var orig_param = new GenericParameter(patch_param.Name, orig_def);
                    orig_def.GenericParameters.Add(orig_param);
                }
                for (var i = 1; i < method.Patch.Parameters.Count; i++) {
                    orig_def.Parameters.Add(method.Patch.Parameters[i]);
                }
                //var il = orig_def.Body.GetILProcessor();
                //il.Append(Instruction.Create(OpCodes.Ret));
                method.Patch.DeclaringType.Methods.Add(orig_def);
                orig_def.DeclaringType = method.Patch.DeclaringType;

                AddAttribute(
                    method.Patch.Module,
                    method.Patch,
                    MonoModOriginalNameAttributeConstructor,
                    new CustomAttributeArgument(StringType, orig_name)
                );

                var orig = orig_def.Module.ImportReference(OrigFactory.OrigTypeForMethod(orig_def.Module, orig_def));

                if (method.ReceivesOriginal) {
                    VariableDefinition orig_delegate_local = null;
                    Logger.Debug($"Rewriting Orig/VoidOrig Invokes for ReceiveOriginal patch method");

                    var il = method.Patch.Body.GetILProcessor();
                    method.Patch.Body.SimplifyMacros();

                    for (var i = 0; i < il.Body.Instructions.Count; i++) {
                        var instr = il.Body.Instructions[i];

                        if (instr.OpCode == OpCodes.Callvirt) {
                            var call_target = (MethodReference)instr.Operand;
                            if (call_target.DeclaringType.IsSame(orig) && call_target.Name == "Invoke") {
                                Logger.Debug($"Attempting orig optimization from IL_{instr.Offset.ToString("x4")}");

                                Instruction orig_ldarg_instr = null;
                                var success = false;
                                var stack_count = 0;
                                var prev = instr.Previous;
                                while (prev != null) {
                                    stack_count += prev.ComputeStackDelta();

                                    if (stack_count == orig_def.Parameters.Count + 1) {
                                        if (prev.OpCode == OpCodes.Ldarg) {
                                            var param = (ParameterReference)prev.Operand;
                                            if (param == method.Patch.Parameters[0]) {
                                                orig_ldarg_instr = prev;
                                                success = true;
                                                break;
                                            }
                                        } else {
                                            break;
                                        }
                                    }
                                    prev = prev.Previous;
                                }

                                if (success) {
                                    Logger.Debug($"Optimization successful, orig passed at: {orig_ldarg_instr}");

                                    if (orig_def.IsStatic) {
                                        orig_ldarg_instr.OpCode = OpCodes.Nop;
                                        orig_ldarg_instr.Operand = null;
                                    } else {
                                        orig_ldarg_instr.OpCode = OpCodes.Ldarg_0;
                                        orig_ldarg_instr.Operand = null;
                                    }

                                    MethodReference orig_ref = orig_def;
                                    if (orig_def.DeclaringType.HasGenericParameters) {
                                        Logger.Debug($"Fixing declaring type on optimized direct call");
                                        var generic_type = new GenericInstanceType(orig_ref.DeclaringType);
                                        for (var j = 0; j < orig_ref.DeclaringType.GenericParameters.Count; j++) {
                                            generic_type.GenericArguments.Add(orig_ref.DeclaringType.GenericParameters[j]);
                                        }
                                        var orig_target_type = orig_def.Module.ImportReference(generic_type);
                                        orig_ref = orig_def.MakeReference();
                                        orig_ref.DeclaringType = orig_target_type;
                                    }

                                    instr.OpCode = orig_def.IsVirtual ? OpCodes.Callvirt : OpCodes.Call;
                                    instr.Operand = orig_ref;
                                } else {
                                    Logger.Debug($"Optimization unsuccessful");
                                }
                            }
                        }
                    }

                    for (var i = 0; i < il.Body.Instructions.Count; i++) {
                        var instr = il.Body.Instructions[i];

                        if (instr.OpCode == OpCodes.Ldarg) {
                            var param = (ParameterReference)instr.Operand;
                            if (param != method.Patch.Parameters[0]) continue;    Instruction new_instr;

                            if (orig_delegate_local == null) {
                                orig_delegate_local = new VariableDefinition(orig);
                                method.Patch.Body.Variables.Add(orig_delegate_local);
                                if (method.Patch.IsStatic) {
                                    il.Replace(instr, new_instr = il.Create(OpCodes.Ldnull));
                                } else {
                                    il.Replace(instr, new_instr = il.Create(OpCodes.Ldarg_0));
                                }
                                Instruction ldftn_instr;

                                MethodReference orig_def_spec = orig_def;
                                if (orig_def.HasGenericParameters) {
                                    var orig_def_spec_generic = new GenericInstanceMethod(orig_def);
                                    for (var j = 0; j < method.Patch.GenericParameters.Count; j++) {
                                        orig_def_spec_generic.GenericArguments.Add(method.Patch.GenericParameters[j]);
                                    }
                                    orig_def_spec = orig_def_spec_generic;
                                }
                                il.InsertAfter(new_instr, ldftn_instr = il.Create(OpCodes.Ldftn, orig_def_spec));
                                i += 1;
                                var ctor = orig_def.Module.ImportReference(OrigFactory.NativePointerConstructorForOrigType(orig_def.Module, orig));
                                Instruction newobj_instr;
                                il.InsertAfter(ldftn_instr, newobj_instr = il.Create(OpCodes.Newobj, ctor));
                                i += 1;
                                Instruction stloc_instr;
                                il.InsertAfter(newobj_instr, stloc_instr = il.Create(OpCodes.Stloc, orig_delegate_local));
                                i += 1;
                                il.InsertAfter(stloc_instr, il.Create(OpCodes.Ldloc, orig_delegate_local));
                                i += 1;
                            } else {
                                il.Replace(instr, il.Create(OpCodes.Ldloc, orig_delegate_local));
                            }
                        }
                    }
                    method.Patch.Body.OptimizeMacros();

                    AddAttribute(
                        method.Patch.Module,
                        method.Patch,
                        RDARSupportHasOriginalInAttributeConstructor,
                        new CustomAttributeArgument(StringType, orig_def.Name)
                    );

                    Logger.Debug($"Removing Orig/VoidOrig parameter");
                    method.Patch.Parameters.RemoveAt(0);

                }
            }
        }

        public void ApplyForField(PatchFieldData field, IList<PatchFieldData> fields_to_remove) {
            Logger.Debug($"Applying for patch field: '{field.PatchPath}' targetting '{field.TargetPath}'");

            if (field.AliasedName != null) {
                Logger.Debug($"Renaming field '{field.PatchPath}' to {field.AliasedName}");
                AddAttribute(
                    field.Patch.Module,
                    field.Patch,
                    RDARSupportNameAliasedFromAttributeConstructor,
                    new CustomAttributeArgument(StringType, field.Patch.Name)
                );
                Relinker.QueueFieldRename(field.Patch, field.AliasedName);
            }

            if (field.Proxy) {
                Logger.Debug($"Field marked for removal (proxy)");
                fields_to_remove.Add(field);
                return;
            }

            if (field.ExplicitlyIgnored || !field.IsInsert) {
                AddAttribute(field.Patch.Module, field.Patch, MonoModIgnoreAttributeConstructor);
                return;
            }
        }

        public void ApplyForProperty(PatchPropertyData prop, IList<PatchPropertyData> props_to_remove) {
            Logger.Debug($"Applying for patch property: '{prop.PatchPath}' targetting '{prop.TargetPath}'");
            if (prop.AliasedName != null) {
                Logger.Debug($"Renaming property '{prop.PatchPath}' to {prop.AliasedName}");
                prop.Patch.Name = prop.AliasedName;
            }
            if (prop.Proxy) {
                Logger.Debug($"Property marked for removal (proxy)");
                props_to_remove.Add(prop);
                return;
            }
        }

        public void ApplyForType(PatchTypeData type) {
            Logger.Info($"Applying for patch type: {new System.Reflection.AssemblyName(type.PatchModuleName).Name} {type.PatchType.BuildSignature()} targetting {type.TargetType.BuildSignature()}");

            AddAttribute(type.PatchType.Module, type.PatchType, MonoModPatchAttributeConstructor, new CustomAttributeArgument(
                StringType,
                BuildMonoModSignature(type.TargetType)
            ));

            var methods_to_remove = new List<PatchMethodData>();
            for (var i = 0; i < type.Methods.Count; i++) {
                var method = type.Methods[i];

                ApplyForMethod(method, methods_to_remove);
            }

            for (var i = 0; i < methods_to_remove.Count; i++) {
                var m = methods_to_remove[i];
                m.Patch.DeclaringType.Methods.Remove(m.Patch);
            }

            var fields_to_remove = new List<PatchFieldData>();
            for (var i = 0; i < type.Fields.Count; i++) {
                var field = type.Fields[i];

                ApplyForField(field, fields_to_remove);
            }

            for (var i = 0; i < fields_to_remove.Count; i++) {
                var f = fields_to_remove[i];
                f.Patch.DeclaringType.Fields.Remove(f.Patch);
            }

            var props_to_remove = new List<PatchPropertyData>();
            for (var i = 0; i < type.Properties.Count; i++) {
                var prop = type.Properties[i];

                ApplyForProperty(prop, props_to_remove);
            }

            for (var i = 0; i < props_to_remove.Count; i++) {
                var p = props_to_remove[i];
                p.Patch.DeclaringType.Resolve().Properties.Remove(p.Patch.Resolve());
            }
        }

        public void Apply() {
            Logger.Info($"Applying for target module: {TargetModule.Name}, {PatchData.PatchModules.Count} patch modules");
            for (var i = 0; i < PatchData.Types.Count; i++) {
                var type = PatchData.Types[i];

                ApplyForType(type);
            }

            Logger.Info($"Relinking");
            for (var i = 0; i < PatchData.PatchModules.Count; i++) {
                Relinker.Relink(PatchData.PatchModules[i]);
            }
            Relinker.FixDefinitions();
        }
    }
}
