using System;
using Mono.Cecil;

namespace SemiPatch {
    /// <summary>
    /// Base class for all exceptions thrown by SemiPatch.
    /// </summary>
    public class SemiPatchException : Exception {
        public SemiPatchException(string msg) : base(msg) { }
    }

    /// <summary>
    /// Base class for all exceptions thrown by the SemiPatch <see cref="Analyzer"/>.
    /// </summary>
    public class AnalyzerException : SemiPatchException {
        public AnalyzerException(string msg) : base(msg) { }
    }

    /// <summary>
    /// Thrown when attempting to deserialize an outdated <see cref="PatchData"/>
    /// object.
    /// </summary>
    public class PatchDataVersionMismatchException : AnalyzerException {
        public PatchDataVersionMismatchException(int version) : base($"Loading PatchData failed: current version is {PatchData.CURRENT_VERSION}, but binary file's version is {version}. You will have to recreate this old data file under the latest version of SemiPatch to be able to load it.") { }
    }

    /// <summary>
    /// Thrown when attempting to call a method tagged with <see cref="ReceiveOriginalAttribute"/>
    /// in a patch assembly.
    /// </summary>
    public class ReceiveOriginalInvokeException : AnalyzerException {
        public MethodPath Path;

        public ReceiveOriginalInvokeException(MethodPath method_path) : base(
            $"Attempted to invoke ReceiveOriginal-tagged method '{method_path}'. Please use a secondary Proxy-tagged method to call the target."
        ) {
            Path = method_path;
        }
    }

    /// <summary>
    /// Base class for exceptions thrown by the SemiPatch <see cref="Relinker"/>.
    /// </summary>
    public class RelinkerException : SemiPatchException {
        public RelinkerException(string msg) : base(msg) { }
    }

    /// <summary>
    /// Thrown when an attempt is made to create a <see cref="Relinker"/> mapping
    /// to a field that doesn't exist in the target.
    /// </summary>
    public class TargetFieldRelinkerException : RelinkerException {
        public MemberPath From;
        public MemberPath To;

        public TargetFieldRelinkerException(MemberPath from, MemberPath to)
        : base($"Attempted to initialize relinker with mapping from '{from}' to '{to}', but the target doesn't exist.") {
            From = from;
            To = to;
        }
    }

    /// <summary>
    /// Thrown when attempting to construct a patch class using a default, untagged,
    /// parameterless constructor.
    /// </summary>
    public class FalseDefaultConstructorException : RelinkerException {
        public TypePath PatchTypePath;
        public TypePath TargetTypePath;

        public FalseDefaultConstructorException(TypePath patch_type_path, TypePath target_type_path)
        : base($"Attempted to call the default (parameterless) constructor of '{patch_type_path}', however the target type '{target_type_path}' does not have a parameterless constructor. Please define one of the available constructors within the patch class using the TreatLikeMethod and Proxy attributes, or use Insert with TreatLikeMethod to add a parameterless constructor.") {
            PatchTypePath = patch_type_path;
            TargetTypePath = target_type_path;
        }
    }

    /// <summary>
    /// Thrown when a <see cref="TypePath"/> cannot be resolved within the
    /// assembly specified in the <see cref="TypePath.FindIn(ModuleDefinition)"/>
    /// or <see cref="TypePath.FindIn(System.Reflection.Assembly)"/> methods.
    /// </summary>
    public class TypePathSearchException : SemiPatchException {
        public TypePathSearchException(TypePath path) : base($"Failed to find type path '{path}'") { }
    }

    /// <summary>
    /// Thrown when a <see cref="MemberPath"/> cannot be resolved within the
    /// assembly specified in the <see cref="TypePath.FindIn(ModuleDefinition)"/>
    /// or <see cref="TypePath.FindIn(System.Reflection.Assembly)"/> methods.
    /// </summary>
    public class MemberPathSearchException : SemiPatchException {
        public MemberPathSearchException(MemberPath path) : base($"Failed to find member path '{path}'") { }
    }
}
