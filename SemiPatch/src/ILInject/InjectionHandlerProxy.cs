using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SemiPatch {
    public interface IInjectionHandlerProxy {
        Instruction EmitAfterStateNewobj(ILProcessor il, Instruction instr, VariableDefinition state_loc);
        Instruction EmitBeforeArgs(ILProcessor il, Instruction instr, TypeReference decl_type, VariableDefinition state_loc);
        Instruction EmitAfterArgs(ILProcessor il, Instruction instr);
        Instruction EmitAfterArg(ILProcessor il, Instruction instr, TypeReference type, int index);
        Instruction EmitBeforeArg(ILProcessor il, Instruction instr, TypeReference type, int index);
        bool SkipFirstParameter { get; }
    }

    public struct DirectMethodCallHandlerProxy : IInjectionHandlerProxy {
        public MethodReference Method;

        public bool SkipFirstParameter => false;

        public DirectMethodCallHandlerProxy(MethodReference method) {
            Method = method;
        }

        public Instruction EmitBeforeArgs(ILProcessor il, Instruction instr, TypeReference decl_type, VariableDefinition state_loc) => instr;

        public Instruction EmitAfterArgs(ILProcessor il, Instruction instr) {
            il.InsertAfter(instr, instr = il.Create(
                Method.Resolve().IsVirtual ? OpCodes.Callvirt : OpCodes.Call,
                Method
            ));

            return instr;
        }

        public Instruction EmitAfterArg(ILProcessor il, Instruction instr, TypeReference type, int index) => instr;

        public Instruction EmitBeforeArg(ILProcessor il, Instruction instr, TypeReference type, int index) => instr;

        public Instruction EmitAfterStateNewobj(ILProcessor il, Instruction instr, VariableDefinition state_local) {
            if (!Method.Resolve().IsStatic) il.InsertAfter(instr,
                instr = il.Create(OpCodes.Ldarg_0)
            );
            il.InsertAfter(instr, instr = il.Create(OpCodes.Ldloc, state_local));
            il.InsertAfter(instr, instr = il.Create(OpCodes.Dup));
            return instr;
        }
    }

    public class DynamicMethodCallHandlerProxy : IInjectionHandlerProxy, IDisposable {
        private const int CACHE_STEP = 100;
        private static System.Reflection.MethodBase[] _Cache = new System.Reflection.MethodBase[CACHE_STEP];
        private static int _CacheSize = CACHE_STEP;

        private static int _NextCacheIndex = 0;
        private static TypeDefinition _DynamicMethodCallHandlerProxyType = SemiPatch.SemiPatchModule.GetType("SemiPatch.DynamicMethodCallHandlerProxy");
        private static FieldReference _CacheField =
            _DynamicMethodCallHandlerProxyType.GetFieldDef("System.Reflection.MethodBase[] _Cache");
        private static MethodReference _MethodBaseInvokeMethod =
            SemiPatch.MscorlibModule.GetType("System.Reflection.MethodBase")
            .GetMethodDef("System.Object Invoke(System.Object, System.Object[])");
        private static TypeReference _ObjectType = SemiPatch.MscorlibModule.GetType("System.Object");

        private int _CacheIndex;
        private int _ParamCount;
        private bool _PushThis;
        public System.Reflection.MethodBase Method;

        public bool SkipFirstParameter => _PushThis;

        public DynamicMethodCallHandlerProxy(System.Reflection.MethodBase reflection_method, bool push_this) {
            _CacheIndex = _Allocate(reflection_method);
            _ParamCount = reflection_method.GetParameters().Length;
            Method = reflection_method;
            _PushThis = push_this;
        }

        public void Dispose() {
            _Cache[_CacheIndex] = null;
        }

        private int _Allocate(System.Reflection.MethodBase method) {
            var index = _NextCacheIndex;

            if (index >= _CacheSize) {
                var found_hole = false;
                for (var i = 0; i < _CacheSize; i++) {
                    var cached = _Cache[i];
                    if (cached == null) {
                        index = i;
                        found_hole = true;
                        break;
                    }
                }

                if (!found_hole) {
                    _CacheSize += CACHE_STEP;
                    Array.Resize(ref _Cache, _CacheSize);
                }
            }

            _Cache[index] = method;
            _NextCacheIndex += 1;

            return index;
        }

        public Instruction EmitBeforeArgs(ILProcessor il, Instruction instr, TypeReference decl_type, VariableDefinition state_loc) {
            if (_PushThis) {
                il.InsertAfter(instr, instr = il.Create(OpCodes.Dup));
                il.InsertAfter(instr, instr = il.Create(OpCodes.Ldc_I4, 0));
                il.InsertAfter(instr, instr = il.Create(OpCodes.Ldarg_0));
                if (decl_type.IsValueType) {
                    il.InsertAfter(instr, instr = il.Create(OpCodes.Box, _ObjectType));
                }
                il.InsertAfter(instr, instr = il.Create(OpCodes.Stelem_Ref));
            }

            il.InsertAfter(instr, instr = il.Create(OpCodes.Dup));
            il.InsertAfter(instr, instr = il.Create(OpCodes.Ldc_I4, 1));
            il.InsertAfter(instr, instr = il.Create(OpCodes.Ldloc, state_loc));
            il.InsertAfter(instr, instr = il.Create(OpCodes.Stelem_Ref));

            return instr;
        }

        public Instruction EmitBeforeArg(ILProcessor il, Instruction instr, TypeReference type, int index) {
            il.InsertAfter(instr, instr = il.Create(OpCodes.Dup));
            il.InsertAfter(instr, instr = il.Create(OpCodes.Ldc_I4, index + 1));
            return instr;
        }

        public Instruction EmitAfterArg(ILProcessor il, Instruction instr, TypeReference type, int index) {
            if (type.IsValueType) {
                il.InsertAfter(instr, instr = il.Create(OpCodes.Box, _ObjectType));
            }
            il.InsertAfter(instr, instr = il.Create(OpCodes.Stelem_Ref));
            return instr;
        }

        public Instruction EmitAfterArgs(ILProcessor il, Instruction instr) {
            il.InsertAfter(instr, instr = il.Create(OpCodes.Callvirt, _MethodBaseInvokeMethod));
            il.InsertAfter(instr, instr = il.Create(OpCodes.Pop));
            return instr;
        }

        public Instruction EmitAfterStateNewobj(ILProcessor il, Instruction instr, VariableDefinition state_local) {
            il.InsertAfter(instr, instr = il.Create(OpCodes.Ldsfld, _CacheField));
            il.InsertAfter(instr, instr = il.Create(OpCodes.Ldc_I4, _CacheIndex));
            il.InsertAfter(instr, instr = il.Create(OpCodes.Ldelem_Ref));
            il.InsertAfter(instr, instr = il.Create(OpCodes.Ldnull));
            il.InsertAfter(instr, instr = il.Create(OpCodes.Castclass, _ObjectType));
            il.InsertAfter(instr, instr = il.Create(OpCodes.Ldc_I4, _ParamCount));
            il.InsertAfter(instr, instr = il.Create(OpCodes.Newarr, _ObjectType));
            il.InsertAfter(instr, instr = il.Create(OpCodes.Ldloc, state_local));
            return instr;
        }
    }
}

