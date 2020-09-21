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

    public class NonPatchTypeException : AnalyzerException {
        public NonPatchTypeException(TypeDefinition type) : base($"Type '{type.FullName}' is not marked with the Patch attribute") {}
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

    public class TargetMethodSearchFailureException : AnalyzerException {
        public MethodPath MethodPath;

        public TargetMethodSearchFailureException(MethodPath target_method, string extra_info)
        : base($"Failed to locate target method '{target_method}' ({extra_info}).") {
            MethodPath = target_method;
        }
    }

    public class UntaggedConstructorException : AnalyzerException {
        public MethodPath PatchPath;

        public UntaggedConstructorException(MethodPath patch_path)
        : base($"Only a single untagged, empty and parameterless constructor can exist in a patch class. If you wish to patch, insert or otherwise alter '{patch_path}', tag it with the TreatLikeMethod attribute.") {
            PatchPath = patch_path;
        }
    }

    public class InsertTargetExistsException : AnalyzerException {
        public MemberPath PatchPath;
        public MemberPath TargetPath;

        public InsertTargetExistsException(string msg, MemberPath patch_path, MemberPath target_path)
        : base($"{msg} (patch path: '{patch_path}', target path: {target_path})") {
            PatchPath = patch_path;
            TargetPath = target_path;
        }
    }

    public class PatchTargetNotFoundException : AnalyzerException {
        public MemberPath PatchPath;
        public MemberPath TargetPath;

        public PatchTargetNotFoundException(string msg, MemberPath patch_path, MemberPath target_path)
        : base(msg) {
            PatchPath = patch_path;
            TargetPath = target_path;
        }
    }

    public class InvalidTargetTypeScopeException : AnalyzerException {
        public TypePath Path;

        public InvalidTargetTypeScopeException(string msg, TypePath target_path)
        : base(msg) {
            Path = target_path;
        }
    }

    public class PatchTargetAttributeMismatchException : AnalyzerException {
        public MemberPath PatchPath;
        public MemberPath TargetPath;

        public PatchTargetAttributeMismatchException(string msg, MemberPath patch_path, MemberPath target_path)
        : base(msg) {
            PatchPath = patch_path;
            TargetPath = target_path;
        }
    }

    public class InvalidReceiveOriginalPatchException : AnalyzerException { 
        public MethodPath PatchPath;

        public InvalidReceiveOriginalPatchException(string msg, MethodPath patch_path)
        : base(msg) {
            PatchPath = patch_path;
        }
    }

    public class InvalidAttributeCombinationException : AnalyzerException {
        public InvalidAttributeCombinationException(string info) : base($"Invalid attribute combination. {info}") { }
    }

    public class PatchDataDeserializationException : AnalyzerException {
        public PatchDataDeserializationException(string msg) : base($"Deserialization error: {msg}") {}
    }

    /// <summary>
    /// Thrown when attempting to use codegen-only fields from <see cref="InjectionState"/>
    /// or <see cref="InjectionState{T}"/>.
    /// in a patch assembly.
    /// </summary>
    public class InjectionStateIllegalAccessException : AnalyzerException {
        public FieldPath Path;

        public InjectionStateIllegalAccessException(FieldPath path) : base(
            $"Attempted to access field '{path}', which is restricted to codegen only and may not be accessed by user code."
        ) {
            Path = path;
        }
    }

    public class EmptyInjectTargetMethodException : AnalyzerException {
        public MethodPath Path;
        public EmptyInjectTargetMethodException(MethodPath path)
            : base($"Injection target '{path}' has no body. If it is marked as extern, injecting into it is not possible.") { }
    }

    public class InvalidInjectHandlerException : AnalyzerException {
        public MethodPath HandlerPath;
        public MethodPath TargetPath;
        public Signature ExpectedSignature;
        public InvalidInjectHandlerException(MethodPath handler_path, MethodPath target_path, Signature expected_sig)
        : base($"Injection handler '{handler_path}' has an invalid signature. Injection handlers of target method '{target_path}' should have the signature '{expected_sig}'."){
            HandlerPath = handler_path;
            TargetPath = target_path;
        }
    }

    public class InvalidInjectQueryException : AnalyzerException {
        public MethodPath HandlerPath;
        public InjectQuery Query;

        public InvalidInjectQueryException(MethodPath handler_path, InjectQuery query)
            : base($"Injection handler '{handler_path}' requested unsupported query type: '{query}'.") { }
    }

    public class InjectHandlerNameTakenException : AnalyzerException {
        public MethodPath HandlerPath;
        public MethodPath InsertTargetPath;

        public InjectHandlerNameTakenException(MethodPath handler_path, MethodPath insert_target_path)
        : base($"Injection handler '{handler_path}' cannot be inserted, because a method with the same signature already exists in the target: '{insert_target_path}'. Please rename the injection handler.") {
            HandlerPath = handler_path;
            InsertTargetPath = insert_target_path;
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

    public class UnhandledInjectParameterException : SemiPatchException {
        public UnhandledInjectParameterException(InjectQuery query)
            : base($"Inject attribute query '{query}' did not handle all parameters.") { }
    }

    public class MissingInjectParameterException : SemiPatchException {
        public MissingInjectParameterException(InjectQuery query, string name)
            : base($"Inject attribute query '{query}' requires the '{name}' parameter.") { }
    }

    public class InvalidInjectParameterException : SemiPatchException {
        public InvalidInjectParameterException(InjectQuery query, string name)
            : base($"Inject attribute query '{query}' does not use the '{name}' parameter.") { }
    }

    public class InjectSearchFailureException : SemiPatchException {
        public MethodPath HandlerPath;

        public InjectSearchFailureException(MethodPath handler_path, string msg)
        : base(msg) {
            HandlerPath = handler_path;
       }
    }

    public class InvalidInjectPositionException : InjectSearchFailureException {
        public InjectQuery Query;
        public InjectPosition Position;

        public InvalidInjectPositionException(MethodPath handler_path, InjectQuery query, InjectPosition pos)
        : base(handler_path, $"Inject query type '{query}' used in injection handler '{handler_path}' does not support the following positioning: '{pos}'.") {
            Query = query;
            Position = pos;
        }
    }

    public class InvalidCallInjectFormatException : InjectSearchFailureException {
        public string CallArgument;

        public InvalidCallInjectFormatException(MethodPath handler_path, string call)
        : base(handler_path, $"Method call injection handler '{handler_path}' specified an invalid call path: '{call}'.") {
            CallArgument = call;
        }
    }

    public class MissingCallInjectTargetException : InjectSearchFailureException {
        public string CallArgument;

        public MissingCallInjectTargetException(MethodPath handler_path, string call)
        : base(handler_path, $"Could not identify searched method call in method call injection handler '{handler_path}': '{call}'.") {
            CallArgument = call;
        }
    }

    public class CallSearchMissingInInjectTargetException : InjectSearchFailureException {
        public MethodPath SearchedMethodPath;
        public int AmountFound;
        public int IndexRequired;

        public CallSearchMissingInInjectTargetException(MethodPath handler_path, MethodPath search_method_path, int amount_found, int idx_required)
        : base(handler_path, $"Could not find method call in the target of injection handler '{handler_path}': a call to '{search_method_path}' appears in target {amount_found} times. Injection search was made for a call to that method at zero-based index {idx_required}.") {
            SearchedMethodPath = search_method_path;
            AmountFound = amount_found;
            IndexRequired = idx_required;
        }
    }

    public class UncapturedLocalException : SemiPatchException {
        public string HandlerPathString;
        public string Name;

        public UncapturedLocalException(string handler_path, string name)
        : base($"Attempted to access uncaptured local '{name}' within the '{handler_path}' injection handler. Use the CaptureLocal attribute to be able to use locals within the body of an injection handler.") {
            HandlerPathString = handler_path;
            Name = name;
        }
    }

    public class InvalidLocalIndexException : SemiPatchException {
        public MethodPath HandlerPath;
        public string Name;
        public int Index;

        public InvalidLocalIndexException(MethodPath handler_path, string name, int index)
        : base($"Attempted to access capture local at index {index} as '{name}' within the '{handler_path}' injection handler, but the target method doesn't have a local variable with index {index}.") {
            HandlerPath = handler_path;
            Name = name;
            Index = index;
        }
    }

    public class InvalidLocalTypeException : SemiPatchException {
        public MethodPath HandlerPath;
        public string Name;
        public int Index;
        public TypeReference ExpectedType;
        public TypeReference ActualType;

        public InvalidLocalTypeException(MethodPath handler_path, string name, int index, TypeReference expected_type, TypeReference actual_type)
        : base($"Attempted to access capture local at index {index} of type '{expected_type.BuildSignature()}' as '{name}' within the '{handler_path}' injection handler, but the target method's local variable with index {index} is of type '{actual_type.BuildSignature()}'.") {
            HandlerPath = handler_path;
            Name = name;
            Index = index;
            ExpectedType = expected_type;
            ActualType = actual_type;
        }
    }
}
