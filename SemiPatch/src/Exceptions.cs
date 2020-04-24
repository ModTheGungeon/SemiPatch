using System;
namespace SemiPatch {
    public class AnalyzerException : Exception {
        public AnalyzerException(string msg) : base(msg) { }
    }

    public class PatchDataVersionMismatchException : AnalyzerException {
        public PatchDataVersionMismatchException(int version) : base($"Loading PatchData failed: current version is {PatchData.CURRENT_VERSION}, but binary file's version is {version}. You will have to recreate this old data file under the latest version of SemiPatch to be able to load it.") { }
    }

    public class ReceiveOriginalInvokeException : AnalyzerException {
        public MethodPath Path;

        public ReceiveOriginalInvokeException(MethodPath method_path) : base(
            $"Attempted to invoke ReceiveOriginal-tagged method '{method_path}'. Please use a secondary Proxy-tagged method to call the target."
        ) {
            Path = method_path;
        }
    }
}
