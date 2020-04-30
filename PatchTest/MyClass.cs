using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using SemiPatch;
using SemiPatch.RDAR;

using BindingFlags = System.Reflection.BindingFlags;

namespace PatchTest.Patches {
    public static class NewClass {
        public static void PrintText() {
            Console.WriteLine($"Hello, world!");
        }
    }

    [Patch(type: typeof(TargetTest.UtilityClass<>))]
    public class PatchUtilityClass<T> {
        [ReceiveOriginal]
        public int Test(Orig<int, string, int> orig, int a, string b) {
            Console.WriteLine(orig);
            return orig(a, b);
        }
    }

    [Patch(typeof(TargetTest.SmallClass))]
    public class PatchSmallClass {
        [ReceiveOriginal]
        public void Hello(VoidOrig<string> orig, string name) {
            NewClass.PrintText();
            Console.WriteLine($"orig = {orig}");
            Console.WriteLine($"name = {name}");
            orig(name);
        }
    }

    [Patch(type: typeof(TargetTest.MyClass))]
    public static class XClass {
        [Insert]
        public static PatchData CurrentPatchData;

        [Insert]
        public static ModuleDefinition PatchModule;

        [Insert]
        public static ModuleDefinition TargetModule;

        [Insert]
        public static ModuleDefinition StaticallyPatchedModule;

        [Insert]
        public static SemiPatch.MonoMod.RuntimePatchManager PatchManager;

        [Insert]
        [TreatLikeMethod]
        static XClass() {
            Console.WriteLine($"cctor");
        }

        [Insert]
        public static bool TestField;

        [ReceiveOriginal]
        public static void Main(VoidOrig<string[]> orig, string[] args) {
            Console.WriteLine($"Injected.");
            CurrentPatchData = PatchData.ReadFrom("test.bin");
            PatchModule = ModuleDefinition.ReadModule("PatchTest.dll");
            TargetModule = ModuleDefinition.ReadModule("TargetTest.exe");
            StaticallyPatchedModule = ModuleDefinition.ReadModule("MONOMODDED_TargetTest.exe");
            PatchManager = new SemiPatch.MonoMod.RuntimePatchManager(
                typeof(TargetTest.SmallClass).Assembly,
                StaticallyPatchedModule
            );
            Console.WriteLine($"Loaded SemiPatch data:");
            Console.WriteLine(CurrentPatchData);
            orig(args);
        }

        [ReceiveOriginal]
        public static bool ExecuteCommand(Orig<string, bool> orig, string cmd) {
            var result = orig(cmd);

            if (result) return true;

            if (cmd == "testpatch") {
                Console.WriteLine($"Patched.");
                return true;
            }

            if (cmd == "reload") {
                var module = ModuleDefinition.ReadModule("TEST_PatchTest.dll");

                Console.WriteLine($"fully qualified name: {module.Assembly.FullName}");
                var new_patch_data = PatchData.ReadFrom("test.bin", new Dictionary<string, ModuleDefinition> {
                    [TargetModule.Assembly.FullName] = TargetModule,
                    [module.Assembly.FullName] = module
                });
                Console.WriteLine(new_patch_data);
                var diffsource = new SemiPatchDiffSource(CurrentPatchData, new_patch_data);
                var diff = diffsource.ProduceDifference();
                var relinker = new SemiPatch.Relinker();
                relinker.LoadRelinkMapFrom(new_patch_data, StaticallyPatchedModule);
                CurrentPatchData = new_patch_data;

                PatchManager.ResetDetours();
                PatchManager.ProcessDifference(relinker, diff);

                var xmodule = ModuleDefinition.ReadModule("PatchTest.dll");

                var diffsource2 = new CILDiffSource(PatchModule, xmodule);
                diffsource2.ExcludeTypesWithAttribute(SemiPatch.SemiPatch.PatchAttribute);
                var diff2 = diffsource2.ProduceDifference();
                PatchManager.ProcessDifference(relinker, diff2);

                return true;
            }

            return false;
        }
    }
}
