using System;
using System.Collections.Generic;
using CommandLine;

namespace SemiPatch.Compiler.Options {
    [Verb("type", HelpText = "Determine whether a file is a SemiPatch Reloadable module, and if so, whether it is compressed or not.")]
    public class TypeOptions {
        [Value(0, MetaName = "spr_path", HelpText = "Path to a SemiPatch Reloadable module.", Required = true)]
        public string Path { get; set; }
    }

    [Verb("build", HelpText = "Build a SemiPatch Reloadable module from a compiled compliant .NET assembly.")]
    public class BuildOptions {
        [Value(0, MetaName = "target_dll_path", HelpText = "Path to the target assembly.", Required = true)]
        public string TargetPath { get; set; }

        [Value(1, MetaName = "patch_dll_path", HelpText = "Path to a SemiPatch compliant patch assembly.", Required = true)]
        public string PatchPath { get; set; }

        [Option('u', "uncompressed", HelpText = "Do not compress the resulting SPR module.")]
        public bool Uncompressed { get; set; }

        [Option('a', "assembly-dir", HelpText = "Add a directory to resolve assemblies from.", Separator = ';')]
        public IList<string> AssemblyDirectories { get; set; } = null;

        [Option('o', "output", HelpText = "Path to write the new SemiPatch Reloadable module into.")]
        public string OutputPath { get; set; } = null;

        [Option('i', "identifier", HelpText = "Force a certain value as the SemiPatch Reloadable module identifier.")]
        public string ForcedIdentifier { get; set; } = null;
    }

    [Verb("build-proxy", HelpText = "Build a proxy DLL from the target DLL and a number of SPR modules")]
    public class BuildProxyOptions {
        [Value(0, MetaName = "target_dll_path", HelpText = "Path to the target assembly.", Required = true)]
        public string TargetPath { get; set; }

        [Value(1, MetaName = "module_paths", HelpText = "List of SPR modules to include in the proxy.", Required = false)]
        public IList<string> ModulePaths { get; set; } = new List<string>();

        [Option('a', "assembly-dir", HelpText = "Add a directory to resolve assemblies from.", Separator = ';')]
        public IList<string> AssemblyDirectories { get; set; } = null;

        [Option('o', "output", HelpText = "Path to write the proxy assembly into.")]
        public string OutputPath { get; set; } = null;
    }

    [Verb("extract", HelpText = "Extract the files that make up a SemiPatch Reloadable module.")]
    public class ExtractOptions {
        [Value(0, MetaName = "spr_path", HelpText = "Path to a SemiPatch Reloadable module.", Required = true)]
        public string Path { get; set; }

        [Option('o', "output", HelpText = "Path to the directory that will contain the extracted files. Will be created if doesn't exist.", Required = true)]
        public string OutputDir { get; set; } = null;
    }

    [Verb("staticpatch", HelpText = "Patch a target statically using MonoMod.")]
    public class StaticPatchOptions {
        [Value(0, MetaName = "target_dll_path", HelpText = "Path to the target assembly.", Required = true)]
        public string TargetPath { get; set; }

        [Value(1, MetaName = "patch_modules", HelpText = "List of SemiPatch Reloadable modules to patch the target with.", Required = true)]
        public IList<string> PatchModules { get; set; }

        [Option('a', "assembly-dir", HelpText = "Add a directory to resolve assemblies from.", Separator = ';')]
        public IList<string> AssemblyDirectories { get; set; } = null;

        [Option('o', "output", HelpText = "Path to write the resulting patched assembly into.")]
        public string OutputPath { get; set; } = null;
    }
}
