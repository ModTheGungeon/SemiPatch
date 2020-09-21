using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Utils;

namespace SemiPatch {
    public static partial class Extensions {
        public static object ImportUntyped(this object obj, ModuleDefinition target_module) {
            if (obj is IMetadataTokenProvider) return (object)target_module.ImportReference((IMetadataTokenProvider)obj);
            return obj;
        }

        public static EventDefinition Clone(this EventDefinition ev, TypeDefinition decl_type, ModuleDefinition target_module) {
            var new_event = new EventDefinition(
                ev.Name,
                ev.Attributes,
                target_module.ImportReference(ev.EventType)
            );

            HashSet<Signature> other_method_sigs = null;
            if (ev.OtherMethods.Count > 0) {
                other_method_sigs = new HashSet<Signature>();
                for (var i = 0; i < ev.OtherMethods.Count; i++) {
                    other_method_sigs.Add(new Signature(ev.OtherMethods[i]));
                }
            }

            for (var i = 0; i < decl_type.Methods.Count; i++) {
                var m = decl_type.Methods[i];
                var sig = new Signature(m);

                if (ev.RemoveMethod != null && sig == new Signature(ev.RemoveMethod)) new_event.RemoveMethod = m;
                if (ev.AddMethod != null && sig == new Signature(ev.AddMethod)) new_event.AddMethod = m;
                if (ev.InvokeMethod != null && sig == new Signature(ev.InvokeMethod)) new_event.InvokeMethod = m;

                if (other_method_sigs != null) {
                    if (other_method_sigs.Contains(sig)) {
                        new_event.OtherMethods.Add(m);
                    }
                }
            }

            for (var i = 0; i < ev.CustomAttributes.Count; i++) {
                var attr = ev.CustomAttributes[i];
                new_event.CustomAttributes.Add(ev.CustomAttributes[i].Clone(target_module));
            }

            return new_event;
        }

        public static PropertyDefinition Clone(this PropertyDefinition prop, TypeDefinition decl_type, ModuleDefinition target_module) {
            /* using (var w = new StreamWriter(Console.OpenStandardError())) { */
            /*     w.WriteLine($"end {prop.FullName}"); */
            /* } */

            var new_prop = new PropertyDefinition(
                prop.Name,
                prop.Attributes,
                target_module.ImportReference(prop.PropertyType)
            ) {
                HasDefault = prop.HasDefault,
            };

            if (prop.HasConstant) {
                new_prop.Constant = ImportUntyped(prop.Constant, target_module);
            }

            HashSet<Signature> other_method_sigs = null;
            if (prop.OtherMethods.Count > 0) {
                other_method_sigs = new HashSet<Signature>();

                for (var i = 0; i < prop.OtherMethods.Count; i++) {
                    other_method_sigs.Add(new Signature(prop.OtherMethods[i]));
                }
            }

            for (var i = 0; i < decl_type.Methods.Count; i++) {
                var m = decl_type.Methods[i];
                var sig = new Signature(m);

                if (prop.GetMethod != null && sig == new Signature(prop.GetMethod)) new_prop.GetMethod = m;
                if (prop.SetMethod != null && sig == new Signature(prop.SetMethod)) new_prop.SetMethod = m;

                if (other_method_sigs != null) {
                    if (other_method_sigs.Contains(sig)) new_prop.OtherMethods.Add(m);
                }
            }

            for (var i = 0; i < prop.CustomAttributes.Count; i++) {
                var attr = prop.CustomAttributes[i];
                new_prop.CustomAttributes.Add(attr.Clone(target_module));
            }

            new_prop.DeclaringType = decl_type;

            return new_prop;
        }

        public static FieldDefinition Clone(this FieldDefinition field, TypeDefinition decl_type, ModuleDefinition target_module) {
            var new_field = new FieldDefinition(
                field.Name,
                field.Attributes,
                target_module.ImportReference(field.FieldType)
            ) {
                InitialValue = field.InitialValue,
                HasDefault = field.HasDefault
            };

            if (field.HasConstant) {
                field.Constant = ImportUntyped(field.Constant, target_module);
            }

            for (var i = 0; i < field.CustomAttributes.Count; i++) {
                var attr = field.CustomAttributes[i];
                new_field.CustomAttributes.Add(attr.Clone(target_module));
            }

            new_field.DeclaringType = decl_type;

            return new_field;
        }

        public static GenericParameter Clone(this GenericParameter param, IGenericParameterProvider owner, ModuleDefinition target_module) {
            var new_param = new GenericParameter(
                param.Name,
                owner
            );
            for (var i = 0; i < param.Constraints.Count; i++) {
                new_param.Constraints.Add(target_module.ImportReference(param.Constraints[i]));
            }
            return new_param;
        }

        public static CustomAttributeArgument Clone(this CustomAttributeArgument arg, ModuleDefinition target_module) {
            return new CustomAttributeArgument(
                target_module.ImportReference(arg.Type),
                ImportUntyped(arg.Value, target_module)
            );
        }

        public static CustomAttributeNamedArgument Clone(this CustomAttributeNamedArgument arg, ModuleDefinition target_module) {
            return new CustomAttributeNamedArgument(
                arg.Name,
                Clone(arg.Argument, target_module)
            );
        }

        public static CustomAttribute Clone(this CustomAttribute attr, ModuleDefinition target_module) {
            var new_attr = new CustomAttribute(
                target_module.ImportReference(attr.Constructor)
            );

            for (var i = 0; i < attr.ConstructorArguments.Count; i++) {
                new_attr.ConstructorArguments.Add(Clone(attr.ConstructorArguments[i], target_module));
            }

            for (var i = 0; i < attr.Fields.Count; i++) {
                new_attr.Fields.Add(Clone(attr.Fields[i], target_module));
            }

            for (var i = 0; i < attr.Properties.Count; i++) {
                new_attr.Properties.Add(Clone(attr.Properties[i], target_module));
            }
            return new_attr;
        }

        public static ParameterDefinition Clone(this ParameterDefinition param, ModuleDefinition target_module) {
            var new_param = new ParameterDefinition(
                param.Name,
                param.Attributes,
                target_module.ImportReference(param.ParameterType)
            ) { HasDefault = param.HasDefault };
            if (param.HasConstant) {
                new_param.Constant = ImportUntyped(param.Constant, target_module);
            }
            return new_param;
        }

        public static MethodDefinition Clone(this MethodDefinition method, TypeDefinition decl_type, ModuleDefinition target_module, bool strip_body = false) {
            var new_method = new MethodDefinition(
                method.Name,
                method.Attributes,
                target_module.ImportReference(method.ReturnType)
            ) {
                ImplAttributes = method.ImplAttributes,
                SemanticsAttributes = method.SemanticsAttributes,
                CallingConvention = method.CallingConvention,
                AggressiveInlining = method.AggressiveInlining,
                DebugInformation = method.DebugInformation,
                ExplicitThis = method.ExplicitThis,
                HasThis = method.HasThis,
                NoOptimization = method.NoOptimization
            };

            for (var i = 0; i < method.CustomAttributes.Count; i++) {
                var attr = method.CustomAttributes[i];
                new_method.CustomAttributes.Add(attr.Clone(target_module));
            }

            for (var i = 0; i < method.Parameters.Count; i++) {
                new_method.Parameters.Add(Clone(method.Parameters[i], target_module));
            }

            for (var i = 0; i < method.GenericParameters.Count; i++) {
                var param = method.GenericParameters[i];
                var new_param = new GenericParameter(
                    param.Name,
                    new_method
                );
                new_method.GenericParameters.Add(new_param);
            }

            if (strip_body) {
                method.Attributes = method.Attributes | MethodAttributes.PInvokeImpl;
                method.IsPInvokeImpl = true;
            } else {
                new_method.Body = method.Body.Clone(new_method, target_module);
            }

            //@NOTE 18.08.2020 added recently - for some reason decl_type was never used,
            //perhaps this causes problems
            new_method.DeclaringType = decl_type;

            return new_method;
        }

        public static InterfaceImplementation Clone(this InterfaceImplementation impl, ModuleDefinition target_module) {
            var new_impl = new InterfaceImplementation(
                target_module.ImportReference(impl.InterfaceType)
            );
            for (var i = 0; i < impl.CustomAttributes.Count; i++) {
                new_impl.CustomAttributes.Add(Clone(impl.CustomAttributes[i], target_module));
            }
            return new_impl;
        }

        public static MethodBody Clone(this MethodBody body, MethodDefinition new_owner, ModuleDefinition target_module) {
            var new_body = body.Clone(new_owner);
            for (var i = 0; i < body.Variables.Count; i++) {
                new_body.Variables[i].VariableType = target_module.ImportReference(body.Variables[i].VariableType);
            }

            for (var i = 0; i < body.Instructions.Count; i++) {
                var new_instr = new_body.Instructions[i];
                var old_instr = body.Instructions[i];

                if (old_instr.Operand is IMetadataTokenProvider mref) new_instr.Operand = target_module.ImportReference(mref);
                if (old_instr.Operand is ParameterReference) {
                    var param = (ParameterReference)old_instr.Operand;
                    new_instr.Operand = new_owner.Parameters[param.Index];
                }
            }

            return new_body;
        }

        public static TypeDefinition Clone(this TypeDefinition type, ModuleDefinition target_module, Func<TypeDefinition, bool> exclude = null, bool strip_method_bodies = false) {
            var new_type = new TypeDefinition(
                type.Namespace,
                type.Name,
                type.Attributes,
                type.BaseType != null ? target_module.ImportReference(type.BaseType) : null
            );

            for (var i = 0; i < type.Fields.Count; i++) {
                new_type.Fields.Add(type.Fields[i].Clone(new_type, target_module));
            }

            // since properties and events depend on existing methods,
            // methods have to be copied first so that they can be resolved

            for (var i = 0; i < type.Methods.Count; i++) {
                var new_method = type.Methods[i].Clone(new_type, target_module, strip_body: strip_method_bodies);
                new_type.Methods.Add(new_method);
            }

            for (var i = 0; i < type.Properties.Count; i++) {
                new_type.Properties.Add(type.Properties[i].Clone(new_type, target_module));
            }

            for (var i = 0; i < type.Events.Count; i++) {
                new_type.Events.Add(type.Events[i].Clone(new_type, target_module));
            }

            for (var i = 0; i < type.GenericParameters.Count; i++) {
                new_type.GenericParameters.Add(type.GenericParameters[i].Clone(new_type, target_module));
            }

            for (var i = 0; i < type.CustomAttributes.Count; i++) {
                new_type.CustomAttributes.Add(type.CustomAttributes[i].Clone(target_module));
            }

            for (var i = 0; i < type.Interfaces.Count; i++) {
                new_type.Interfaces.Add(type.Interfaces[i].Clone(target_module));
            }

            for (var i = 0; i < type.NestedTypes.Count; i++) {
                var nested_type = type.NestedTypes[i];
                if (exclude != null && exclude(nested_type)) continue;
                new_type.NestedTypes.Add(nested_type.Clone(target_module, exclude));
            }

            return new_type;
        }
    }
}
