using System;
namespace SemiPatch {
    public struct InjectionSignature {
        private MethodPath _HandlerPath;
        private MethodPath _TargetPath;
        private string _Value;

        public InjectionSignature(string value) {
            _Value = value;
            _HandlerPath = null;
            _TargetPath = null;
        }

        public InjectionSignature(MethodPath handler_path, MethodPath target_path) {
            _Value = null;
            _HandlerPath = handler_path;
            _TargetPath = target_path;
        }

        public InjectionSignature(AssemblyDiff.InjectionDifference diff) {
            _Value = null;
            _HandlerPath = diff.HandlerPath;
            _TargetPath = diff.TargetPath;
        }

        public override string ToString() {
            return _Value ?? $"{_HandlerPath.Signature} -> {_TargetPath}";
        }

        public override int GetHashCode() {
            return ToString().GetHashCode();
        }

        public bool Equals(InjectionSignature sig) {
            return ToString() == sig.ToString();
        }

        public override bool Equals(object obj) {
            if (obj is null) return false;
            return Equals((InjectionSignature)obj);
        }

        public static bool operator==(InjectionSignature a, InjectionSignature b) {
            return a.Equals(b);
        }

        public static bool operator!=(InjectionSignature a, InjectionSignature b) {
            return !a.Equals(b);
        }
    }
}
