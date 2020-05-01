using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using SemiPatch;

using BindingFlags = System.Reflection.BindingFlags;

namespace PatchTest.Patches {
    public static class NewClass {
        public static void PrintText() {
            Console.WriteLine($"a lot has");
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
            Console.WriteLine($"changed");
            Console.WriteLine($"at");
            Console.WriteLine($"runtime!");
            orig("test");
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
        public static RuntimeClient Client;

        [Insert]
        [TreatLikeMethod]
        static XClass() {
            Client = new RuntimeClient(
                typeof(TargetTest.SmallClass).Assembly,
                ModuleDefinition.ReadModule("TargetTest.exe"),
                ModuleDefinition.ReadModule("PATCHED_TargetTest.exe")
            );
            CurrentModule = Client.Load("PatchTest.spr");
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
                Console.WriteLine($"Really changed at runtime!!!");
                return true;
            }

            if (cmd == "reload") {
                var rm = Client.Load("PatchTest.spr");
                Client.Reset();
                Client.Preload(rm);
                Client.Process(CurrentModule, rm);
                CurrentModule = rm;

                return true;
            }

            return false;
        }
    }
}
