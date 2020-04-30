using System;
namespace SemiPatch {
    public class SemiPatchException : Exception {
        public SemiPatchException(string msg) : base(msg) { }
    }

    public class AnalyzerException : SemiPatchException {
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

    public class RelinkerException : SemiPatchException {
        public RelinkerException(string msg) : base(msg) { }
    }

    public class TargetFieldRelinkerException : RelinkerException {
        public MemberPath From;
        public MemberPath To;

        public TargetFieldRelinkerException(MemberPath from, MemberPath to)
        : base($"Attempted to initialize relinker with mapping from '{from}' to '{to}', but the target doesn't exist.") {
            From = from;
            To = to;
        }
    }

    public class FalseDefaultConstructorException : RelinkerException {
        public TypePath PatchTypePath;
        public TypePath TargetTypePath;

        public FalseDefaultConstructorException(TypePath patch_type_path, TypePath target_type_path)
        : base($"Attempted to call the default (parameterless) constructor of '{patch_type_path}', however the target type '{target_type_path}' does not have a parameterless constructor. Please define one of the available constructors within the patch class using the TreatLikeMethod and Proxy attributes, or use Insert with TreatLikeMethod to add a parameterless constructor.") {
            PatchTypePath = patch_type_path;
            TargetTypePath = target_type_path;
        }
    }
}
