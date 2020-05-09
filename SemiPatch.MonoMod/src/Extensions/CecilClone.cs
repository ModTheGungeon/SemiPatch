using System;
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
            ) {
                AddMethod = ev.AddMethod?.ToPath()?.WithDeclaringType(decl_type)?.FindIn<MethodDefinition>(target_module),
                RemoveMethod = ev.RemoveMethod?.ToPath()?.WithDeclaringType(decl_type)?.FindIn<MethodDefinition>(target_module),
                InvokeMethod = ev.InvokeMethod?.ToPath()?.WithDeclaringType(decl_type)?.FindIn<MethodDefinition>(target_module)
            };


            for (var i = 0; i < ev.OtherMethods.Count; i++) {
                var other_method = ev.OtherMethods[i];
                new_event.OtherMethods.Add(
                    other_method.ToPath().WithDeclaringType(decl_type).FindIn<MethodDefinition>(target_module)
                );
            }

            for (var i = 0; i < ev.CustomAttributes.Count; i++) {
                var attr = ev.CustomAttributes[i];
                var new_attr = new CustomAttribute(
                    target_module.ImportReference(attr.Constructor),
                    attr.GetBlob()
                );
                new_event.CustomAttributes.Add(new_attr);
            }

            return new_event;
        }

        public static PropertyDefinition Clone(this PropertyDefinition prop, TypeDefinition decl_type, ModuleDefinition target_module) {
            var new_prop = new PropertyDefinition(
                prop.Name,
                prop.Attributes,
                target_module.ImportReference(prop.PropertyType)
            ) {
                HasDefault = prop.HasDefault,
                GetMethod = prop.GetMethod?.ToPath()?.WithDeclaringType(decl_type)?.FindIn<MethodDefinition>(target_module),
                SetMethod = prop.SetMethod?.ToPath()?.WithDeclaringType(decl_type)?.FindIn<MethodDefinition>(target_module)
            };

            if (prop.HasConstant) {
                new_prop.Constant = ImportUntyped(prop.Constant, target_module);
            }

            for (var i = 0; i < prop.OtherMethods.Count; i++) {
                var other_method = prop.OtherMethods[i];
                new_prop.OtherMethods.Add(
                    other_method.ToPath().WithDeclaringType(decl_type).FindIn<MethodDefinition>(target_module)
                );
            }

            for (var i = 0; i < prop.CustomAttributes.Count; i++) {
                var attr = prop.CustomAttributes[i];
                var new_attr = new CustomAttribute(
                    target_module.ImportReference(attr.Constructor),
                    attr.GetBlob()
                );
                new_prop.CustomAttributes.Add(new_attr);
            }

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
                var new_attr = new CustomAttribute(
                    target_module.ImportReference(attr.Constructor),
                    attr.GetBlob()
                );
                new_field.CustomAttributes.Add(new_attr);
            }

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

        public static MethodDefinition Clone(this MethodDefinition method, TypeDefinition decl_type, ModuleDefinition target_module) {
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
                var new_attr = new CustomAttribute(
                    target_module.ImportReference(attr.Constructor),
                    attr.GetBlob()
                );
                new_method.CustomAttributes.Add(new_attr);
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

            new_method.Body = method.Body.Clone(new_method, target_module);

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
    }
}
