using System;
using ModTheGungeon;
using System.IO;
using System.Collections.Generic;
using Mono.Cecil;
using SemiPatch.RDAR;

namespace SemiPatch {
    public static class Program {
        public const string VERSION = "0.1";
        public static Logger Logger = new Logger("CLI");

        public static void Main(string[] args) {
            Logger.Info($"SemiPatch {VERSION}");

            if (args.Length == 0) FailWithUsage("missing target assembly");
            if (args.Length == 1) FailWithUsage("missing patch assembly");
            var target = args[0];

            var patches = new List<string>();
            if (!File.Exists(target)) FailWithUsage("target assembly doesn't exist");
            for (var i = 1; i < args.Length; i++) {
                var patch = args[i];
                if (!File.Exists(patch)) FailWithUsage("patch assembly doesn't exist");
                patches.Add(patch);
            }

            var p = new Analyzer(target, patches);
            var data = p.Analyze();
            Console.WriteLine(data);
            using (var f = new BinaryWriter(File.OpenWrite("test.bin"))) data.Serialize(f);


            using (var f = new BinaryReader(File.OpenRead("test.bin"))) {
                var new_data = PatchData.Deserialize(f);

                Console.WriteLine(new_data);
            }

            using (var f = new StreamWriter(File.OpenWrite("inserts.txt"))) {
                data.WriteInsertList(f);
            }

            var conv = new MonoModStaticConverter(data);
            conv.Apply();
            using (var f = File.OpenWrite("test.dll")) {
                data.PatchModules[0].Write(f);
            }

            var old_dll = ModuleDefinition.ReadModule("TEST_PatchTest.dll");
            var new_dll = ModuleDefinition.ReadModule("PatchTest.dll");

            var agent = new SemiPatchDiffSource(
                PatchData.ReadFrom("TEST_test.bin", new Dictionary<string, ModuleDefinition> {
                    [old_dll.Assembly.FullName] = ModuleDefinition.ReadModule("TEST_PatchTest.dll"),
                    [new_dll.Assembly.FullName] = ModuleDefinition.ReadModule("PatchTest.dll"),
                }),
                PatchData.ReadFrom("test.bin", new Dictionary<string, ModuleDefinition> {
                    [old_dll.Assembly.FullName] = ModuleDefinition.ReadModule("TEST_PatchTest.dll"),
                    [new_dll.Assembly.FullName] = ModuleDefinition.ReadModule("PatchTest.dll"),
                })
            );
            //agent.ExcludeTypesWithAttribute(SemiPatch.PatchAttribute);
            var diff = agent.ProduceDifference();
            Console.WriteLine(diff);
        }

        private static void Usage() {
            Logger.Info("SemiPatch.exe ASSEMBLY");
        }

        private static void FailWithUsage(string err) {
            Logger.Error($"Error: {err}");
            Usage();
            Environment.Exit(1);
        }
    }
}
