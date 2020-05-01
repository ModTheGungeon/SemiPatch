using System;
using Mono.Cecil;

namespace SemiPatch {
    /// <summary>
    /// Thrown when <see cref="Client.Load(string)"/> or <see cref="Client.Load(System.IO.Stream)"/>
    /// is called to load an SPR module, but the module refers to a different
    /// target assembly than the client.
    /// </summary>
    public class InvalidTargetException : SemiPatchException {
        public ModuleDefinition ExpectedTarget;
        public ModuleDefinition ActualTarget;

        public InvalidTargetException(ModuleDefinition expected, ModuleDefinition actual)
        : base($"Invalid target in reloadable module: expected '{expected.Assembly.FullName}', but module was built against '{actual.Assembly.FullName}'.") {
            ExpectedTarget = expected;
            ActualTarget = actual;
        }
    }

    /// <summary>
    /// Thrown when SemiPatch RDAR is unable to reload a member or type within an
    /// assembly.
    /// </summary>
    public class UnsupportedRDAROperationException : SemiPatchException {
        public UnsupportedRDAROperationException(AssemblyDiff.MemberDifference diff)
        : base($"RuntimeDetour-Assisted Reloading does not support applying this diff: {diff}") { }

        public UnsupportedRDAROperationException(AssemblyDiff.TypeDifference diff)
        : base($"RuntimeDetour-Assisted Reloading does not support applying this diff: {diff}") { }
    }

    /// <summary>
    /// Thrown when an SPR module cannot be loaded by <see cref="ReloadableModule"/>.
    /// </summary>
    public class CorruptedReloadableModuleException : SemiPatchException {
        public CorruptedReloadableModuleException(string section)
            : base($"Attempted to load corrupted reloadable module. Error while reading section: '{section}'.") { }
    }

    /// <summary>
    /// Thrown when an attempt is made to load an outdated SPR module using
    /// <see cref="ReloadableModule"/>.
    /// </summary>
    public class InvalidVersionReloadableModuleException : SemiPatchException {
        public InvalidVersionReloadableModuleException(int version)
            : base($"Attempted to load a reloadable module that is either too new or too old. Current format version is {ReloadableModule.VERSION}, but version you're trying to load is {version}.") { }
    }
}
