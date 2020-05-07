using System;
namespace SemiPatch {
    public struct InjectionSignature {
        private MethodPath _HandlerPath;
        private MethodPath _TargetPath;

        public InjectionSignature(MethodPath handler_path, MethodPath target_path) {
            _HandlerPath = handler_path;
            _TargetPath = target_path;
        }

        public InjectionSignature(AssemblyDiff.InjectionDifference diff) {
            _HandlerPath = diff.HandlerPath;
            _TargetPath = diff.TargetPath;
        }

        public override string ToString() {
            return $"{_HandlerPath} -> {_TargetPath}";
        }

        public override int GetHashCode() {
            return _HandlerPath.GetHashCode() ^ (_TargetPath.GetHashCode() * 73561);
        }

        public bool Equals(InjectionSignature sig) {
            return _HandlerPath == sig._HandlerPath && _TargetPath == sig._TargetPath;
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
