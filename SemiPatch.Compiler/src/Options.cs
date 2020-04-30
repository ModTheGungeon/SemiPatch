using System;
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

        [Option('o', "output", HelpText = "Path to write the new SemiPatch Reloadable module into.")]
        public string OutputPath { get; set; } = null;
    }

    [Verb("extract", HelpText = "Extract the files that make up a SemiPatch Reloadable module.")]
    public class ExtractOptions {
        [Value(0, MetaName = "spr_path", HelpText = "Path to a SemiPatch Reloadable module.", Required = true)]
        public string Path { get; set; }

        [Option('o', "output", HelpText = "Path to the directory that will contain the extracted files. Will be created if doesn't exist.", Required = true)]
        public string OutputDir { get; set; } = null;
    }
}
