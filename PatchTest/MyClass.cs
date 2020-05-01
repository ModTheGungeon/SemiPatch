using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using SemiPatch;
using SemiPatch.MonoMod;
using SemiPatch.RDAR;

using BindingFlags = System.Reflection.BindingFlags;

namespace PatchTest.Patches {
    public static class NewClass {
        public static void PrintText() {
            Console.WriteLine($"X2!");
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
            Console.WriteLine($"X1!");
            Console.WriteLine($"orig = {orig}");
            Console.WriteLine($"name = {name}");
            orig(name);
        }
    }

    [Patch(type: typeof(TargetTest.MyClass))]
    public static class XClass {
        [Insert]
        public static ReloadableModule CurrentModule;

        [Insert]
        public static SingleTargetClient RDARClient;

        [Insert]
        [TreatLikeMethod]
        static XClass() {
            RDARClient = new SingleTargetClient(
                typeof(TargetTest.SmallClass).Assembly,
                ModuleDefinition.ReadModule("TargetTest.exe"),
                ModuleDefinition.ReadModule("MONOMODDED_TargetTest.exe")
            );
            CurrentModule = ReloadableModule.Read("PatchTest.spr", RDARClient.TargetModule);
        }

        [Insert]
        public static bool TestField;

        [ReceiveOriginal]
        public static void Main(VoidOrig<string[]> orig, string[] args) {
            Console.WriteLine($"Injected.");
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
                var rm = ReloadableModule.Read("PatchTest.spr", RDARClient.TargetModule);

                RDARClient.Reset();
                RDARClient.Preload(rm);
                RDARClient.Process(CurrentModule, rm);
                CurrentModule = rm;

                return true;
            }

            return false;
        }
    }
}
