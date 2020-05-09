using System;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MonoMod.Utils;

namespace SemiPatch {
    public static class RDARPrimitive {
        public static System.Reflection.MethodInfo CreateThunk(System.Reflection.MethodBase source, System.Reflection.MethodBase target) {
            var patched_params = source.GetParameters();
            var stub_param_length = patched_params.Length;
            if (!source.IsStatic) stub_param_length += 1;

            var param_types = new Type[stub_param_length];
            if (!source.IsStatic) param_types[0] = source.DeclaringType;
            for (var i = 0; i < patched_params.Length; i++) {
                param_types[source.IsStatic ? i : i + 1] = patched_params[i].ParameterType;
            }
            var dmd = new DynamicMethodDefinition(source.Name, (source as System.Reflection.MethodInfo)?.ReturnType ?? typeof(void), param_types);

            var body = dmd.Definition.Body;
            var il = body.GetILProcessor();

            dmd.Definition.IsStatic = true;
            dmd.Definition.HasThis = false;
            dmd.Definition.ExplicitThis = false;

            for (var i = 0; i < stub_param_length; i++) {
                il.Append(il.Create(OpCodes.Ldarg, dmd.Definition.Parameters[i]));
            }

            if (target.IsVirtual) {
                il.Append(il.Create(OpCodes.Callvirt, target));
            } else il.Append(il.Create(OpCodes.Call, target));

            il.Append(il.Create(OpCodes.Ret));

            body.OptimizeMacros();

            var gen = dmd.Generate();
            return gen;
        }
    }
}
