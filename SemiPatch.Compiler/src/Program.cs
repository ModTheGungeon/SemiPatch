﻿using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using MonoMod.Utils;
using System.Linq;
using CommandLine;
using SemiPatch.Compiler.Options;
using System.IO.Compression;

namespace SemiPatch.MonoMod.Compiler {
    public class MainClass {
        public static int BuildMain(BuildOptions opts) {
            var target_path = opts.TargetPath;
            var patch_path = opts.PatchPath;

            if (!File.Exists(target_path)) throw new CompilerException($"Target assembly '{target_path}' doesn't exist.");
            if (!File.Exists(patch_path)) throw new CompilerException($"Patch assembly '{patch_path}' doesn't exist");

            var p = new Analyzer(target_path, new List<string> { patch_path });
            var patch_data = p.Analyze();
            Console.WriteLine(patch_data);

            var patch_module = patch_data.PatchModules[0];

            // save patch asm because MMSC will change it
            var patch_module_ms = new MemoryStream();
            patch_module.Write(patch_module_ms);
            patch_module_ms.Position = 0;


            var conv = new MonoModStaticConverter(patch_data);
            conv.Apply();
            var mmsg_module = patch_data.PatchModules[0];

            var spr = new ReloadableModule(
                patch_data.TargetModule,
                null,
                patch_asm_stream: patch_module_ms,
                mmsg_module: mmsg_module,
                rdbs_module: null,
                patch_data: patch_data
            );

            var output_path = opts.OutputPath ?? $"{patch_path.Substring(0, patch_path.Length - 4)}.spr";

            using (var f = File.OpenWrite(output_path)) {
                spr.Write(f, true);
            }

            return 0;
        }

        public static int TypeMain(TypeOptions opts) {
            using (var f = File.OpenRead(opts.Path)) {
                var compression_byte = f.ReadByte();

                if (compression_byte != 0 && compression_byte != 1) {
                    Console.WriteLine($"{opts.Path}: data");
                    return 0;
                }

                var magic_bytes = new byte[3];
                var read_bytes = 0;
                if (compression_byte == 0) read_bytes = f.Read(magic_bytes, 0, 3);
                else {
                    using (var deflate = new DeflateStream(f, CompressionMode.Decompress, true)) {
                        read_bytes = deflate.Read(magic_bytes, 0, 3);
                    }
                }

                if (read_bytes != 3) {
                    Console.WriteLine($"{opts.Path}: data");
                    return 0;
                }

                if (!ReloadableModule.IsSPRMagic(magic_bytes)) {
                    Console.WriteLine($"{opts.Path}: data");
                    return 0;
                }

                if (compression_byte == 0) {
                    Console.Write($"{opts.Path}: SemiPatch Reloadable module (uncompressed");
                } else {
                    Console.Write($"{opts.Path}: SemiPatch Reloadable module (compressed");
                }

                f.Position = 0;

                using (var spr = ReloadableModule.Read(f, null)) {
                    if (spr.HasMMSG) Console.Write(", with MMSG assembly");
                    if (spr.HasRDBS) Console.Write(", with RDBS assembly");
                }

                Console.WriteLine(")");
            }

            return 0;
        }

        public static int ExtractMain(ExtractOptions opts) {
            if (File.Exists(opts.OutputDir)) throw new CompilerException($"Cannot extract to '{opts.OutputDir}', because a file exists under that path.");

            if (!Directory.Exists(opts.OutputDir)) Directory.CreateDirectory(opts.OutputDir);

            using (var spr = ReloadableModule.Read(opts.Path, null)) {
                using (var f = File.OpenWrite(Path.Combine(opts.OutputDir, "patch_asm.dll")))
                    spr.PatchAssemblyStream.CopyTo(f);

                if (spr.HasMMSG)
                    using (var f = File.OpenWrite(Path.Combine(opts.OutputDir, "mmsg_asm.dll")))
                       spr.MMSGAssemblyStream.CopyTo(f);

                if (spr.HasRDBS)
                    using (var f = File.OpenWrite(Path.Combine(opts.OutputDir, "rdbs_asm.dll")))
                        spr.RDBSAssemblyStream.CopyTo(f);

                using (var f = File.OpenWrite(Path.Combine(opts.OutputDir, "patch_data.bin")))
                    spr.PatchDataStream.CopyTo(f);
            }

            return 0;
        }

        public static int HandleErrors(IEnumerable<Error> errors) {
            var error_status = false;
            using (var stderr = new StreamWriter(Console.OpenStandardError())) {
                foreach (var error in errors) {
                    if (error is VersionRequestedError) { }
                    else if (error is HelpVerbRequestedError) { }
                    else {
                        error_status = true;
                        break;
                    }
                }
            }
            return error_status ? 1 : 0;
        }

        public static int Main(string[] args) {
            var result = Parser.Default.ParseArguments<
                TypeOptions,
                BuildOptions,
                ExtractOptions
            >(args);

            try {
                return result.MapResult(
                    (TypeOptions opts) => TypeMain(opts),
                    (BuildOptions opts) => BuildMain(opts),
                    (ExtractOptions opts) => ExtractMain(opts),
                    errors => HandleErrors(errors)
                );
            } catch (Exception e) {
                using (var stderr = new StreamWriter(Console.OpenStandardError())) {
                    stderr.WriteLine(e.Message);
                    stderr.WriteLine(e.StackTrace);
                }
                return 1;
            }
        }
    }
}
