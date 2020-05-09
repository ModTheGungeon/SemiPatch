using System;
using System.IO;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SemiPatch {
    /// <summary>
    /// Class containing extension methods used all over SemiPatch.
    /// </summary>
    public static partial class Exceptions {
        // Based on https://github.com/jbevain/cecil/blob/b2958c0d8473aa6eaa6b3bd7ad19d5f643a97804/Mono.Cecil.Cil/CodeWriter.cs#L435
        // Modified to use publicly accessible fields and types
        public static int ComputeStackDelta(this Instruction instruction) {
            var stack_size = 0;
            switch (instruction.OpCode.FlowControl) {
            case FlowControl.Call: {
                    var method = (IMethodSignature)instruction.Operand;
                    // pop 'this' argument
                    if ((method.HasThis && !method.ExplicitThis) && instruction.OpCode.Code != Code.Newobj)
                        stack_size--;
                    // pop normal arguments
                    if (method.HasParameters)
                        stack_size -= method.Parameters.Count;
                    // pop function pointer
                    if (instruction.OpCode.Code == Code.Calli)
                        stack_size--;
                    // push return value
                    if (method.ReturnType.MetadataType != MetadataType.Void || instruction.OpCode.Code == Code.Newobj)
                        stack_size++;
                    break;
                }
            default:
                ComputePopDelta(instruction.OpCode.StackBehaviourPop, ref stack_size);
                ComputePushDelta(instruction.OpCode.StackBehaviourPush, ref stack_size);
                break;
            }
            return stack_size;
        }

        public static void ComputePopDelta(StackBehaviour pop_behavior, ref int stack_size) {
            switch (pop_behavior) {
            case StackBehaviour.Popi:
            case StackBehaviour.Popref:
            case StackBehaviour.Pop1:
                stack_size--;
                break;
            case StackBehaviour.Pop1_pop1:
            case StackBehaviour.Popi_pop1:
            case StackBehaviour.Popi_popi:
            case StackBehaviour.Popi_popi8:
            case StackBehaviour.Popi_popr4:
            case StackBehaviour.Popi_popr8:
            case StackBehaviour.Popref_pop1:
            case StackBehaviour.Popref_popi:
                stack_size -= 2;
                break;
            case StackBehaviour.Popi_popi_popi:
            case StackBehaviour.Popref_popi_popi:
            case StackBehaviour.Popref_popi_popi8:
            case StackBehaviour.Popref_popi_popr4:
            case StackBehaviour.Popref_popi_popr8:
            case StackBehaviour.Popref_popi_popref:
                stack_size -= 3;
                break;
            case StackBehaviour.PopAll:
                stack_size = 0;
                break;
            }
        }

        public static void ComputePushDelta(StackBehaviour push_behaviour, ref int stack_size) {
            switch (push_behaviour) {
            case StackBehaviour.Push1:
            case StackBehaviour.Pushi:
            case StackBehaviour.Pushi8:
            case StackBehaviour.Pushr4:
            case StackBehaviour.Pushr8:
            case StackBehaviour.Pushref:
                stack_size++;
                break;
            case StackBehaviour.Push1_push1:
                stack_size += 2;
                break;
            }
        }

        public static MethodReference MakeReference(this MethodDefinition self) {
            var new_inst = new MethodReference(self.Name, self.ReturnType, self.DeclaringType);
            for (var i = 0; i < self.Parameters.Count; i++) {
                var param = self.Parameters[i];
                new_inst.Parameters.Add(new ParameterDefinition(
                    param.Name,
                    param.Attributes,
                    param.ParameterType
                ));
            }
            for (var i = 0; i < self.GenericParameters.Count; i++) {
                var param = self.GenericParameters[i];
                new_inst.GenericParameters.Add(new GenericParameter(param.Name, new_inst));
            }
            new_inst.HasThis = self.HasThis;
            new_inst.ExplicitThis = self.ExplicitThis;
            return new_inst;
        }

        public static Instruction FirstAfterNops(this Instruction instr) {
            while (instr != null && instr.OpCode == OpCodes.Nop) {
                instr = instr.Next;
            }

            return instr;
        }

        public static Type ToReflection(this TypeReference type) {
            var resolved = type.Resolve();
            var asm = System.Reflection.Assembly.Load(resolved.Module.Assembly.FullName);
            var reflection_type = resolved.ToPath().FindIn(asm);

            if (type is GenericInstanceType generic_type) {
                var type_args = new Type[generic_type.GenericArguments.Count];
                for (var i = 0; i < generic_type.GenericArguments.Count; i++) {
                    type_args[i] = generic_type.GenericArguments[i].ToReflection();
                }
                reflection_type = reflection_type.MakeGenericType(type_args);
            }

            return reflection_type;
        }

        // https://stackoverflow.com/a/5730893
        public static void CopyTo(this Stream input, Stream output) {
            byte[] buffer = new byte[16 * 1024]; // Fairly arbitrary size
            int byte_count;

            while ((byte_count = input.Read(buffer, 0, buffer.Length)) > 0) {
                output.Write(buffer, 0, byte_count);
            }
        }
    }
}
