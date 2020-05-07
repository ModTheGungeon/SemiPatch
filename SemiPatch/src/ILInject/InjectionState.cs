using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SemiPatch {
    public class InjectionState {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Accessing this field within user code may lead to undefined behavior. It is only public out of necessity for code generation.")]
        public bool _OverrideReturn = false;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Accessing this field within user code may lead to undefined behavior. It is only public out of necessity for code generation.")]
        public IDictionary<string, object> _Locals = new Dictionary<string, object>();

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Accessing this field within user code may lead to undefined behavior. It is only public out of necessity for code generation.")]
        public string _HandlerPath;

        public void SetLocal<T>(string name, T value) {
            if (!_Locals.ContainsKey(name)) {
                throw new UncapturedLocalException(_HandlerPath, name);
            }
            _Locals[name] = value;
        }

        public T GetLocal<T>(string name) {
            if (!_Locals.ContainsKey(name)) {
                throw new UncapturedLocalException(_HandlerPath, name);
            }
            return (T)_Locals[name];
        }

        public void Cancel() {
            _OverrideReturn = true;
        }
    }

    public class InjectionState<T> {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Accessing this field within user code may lead to undefined behavior. It is only public out of necessity for code generation.")]
        public bool _OverrideReturn = false;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Accessing this field within user code may lead to undefined behavior. It is only public out of necessity for code generation.")]
        public T _ReturnValue = default(T);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Accessing this field within user code may lead to undefined behavior. It is only public out of necessity for code generation.")]
        public IDictionary<string, object> _Locals = new Dictionary<string, object>();

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Accessing this field within user code may lead to undefined behavior. It is only public out of necessity for code generation.")]
        public string _HandlerPath;

        public void SetLocal<T>(string name, T value) {
            if (!_Locals.ContainsKey(name)) {
                throw new UncapturedLocalException(_HandlerPath, name);
            }
            _Locals[name] = value;
        }

        public T GetLocal<T>(string name) {
            if (!_Locals.ContainsKey(name)) {
                throw new UncapturedLocalException(_HandlerPath, name);
            }
            return (T)_Locals[name];
        }


        public T ReturnValue {
            set {
                _OverrideReturn = true;
                _ReturnValue = value;
            }
        }
    }
}
