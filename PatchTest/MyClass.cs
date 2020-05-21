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
            Console.WriteLine($"a lot dafdasdfadfaas");
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
        [Inject(
            inside: "void Hello(string)",
            at: InjectQuery.MethodCall,
            where: InjectPosition.After,
            path: "[System.Console] void WriteLine(string)",
            index: 1
        )]
        [CaptureLocal(
            index: 0,
            type: typeof(string),
            name: "test_local"
        )]
        public void HelloInject1(InjectionState state, string name) {
            var test_loc = state.GetLocal<string>("test_local");
            state.SetLocal("test_local", "injection!");
            Console.WriteLine($"injection changed at runtime! (name = {name}, test_local = {test_loc})");
        }

        [Inject(
            inside: "void Hello(string)",
            at: InjectQuery.MethodCall,
            where: InjectPosition.After,
            path: "[System.Console] void WriteLine(string)",
            index: 1
        )]
        [CaptureLocal(
            index: 0,
            type: typeof(string),
            name: "test_local"
        )]
        public void HelloInject2(InjectionState state, string name) {
            var test_loc = state.GetLocal<string>("test_local");
            state.SetLocal("test_local", "injection!");
            Console.WriteLine($"After second call (name = {name}, test_local = {test_loc})");
        }

        [Inject(
            inside: "string GetName(int)",
            at: InjectQuery.Tail,
            where: InjectPosition.Before
        )]
        [CaptureLocal(
            index: 0,
            type: typeof(string),
            name: "format_result"
        )]
        public void GetNameInject(InjectionState<string> state, int num) {
            Console.WriteLine($"GetName num: {num}");
            Console.WriteLine($"Format result: {state.GetLocal<string>("format_result")}");
            state.ReturnValue = "changed_name";
        }


        //[ReceiveOriginal]
        //public void Hello(VoidOrig<string> orig, string name) {
        //    Console.WriteLine($"changed");
        //    orig(name);
        //}

        //public void GetNameInject(InjectionState<string> state, string name) {
        //    Console.WriteLine($"getnameinject: {name}");
        //    state.ReturnValue = "x";
        //}

        //[ReceiveOriginal]
        //public void Hello(VoidOrig<string> orig, string name) {
        //    NewClass.PrintText();
        //    Console.WriteLine($"changed");
        //    Console.WriteLine($"at");
        //    Console.WriteLine($"runtime!");
        //    orig("test");
        //    Console.WriteLine($"orig = {orig}");
        //    Console.WriteLine($"name = {name}");
        //    orig(name);
        //}
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
                Console.WriteLine($"Really chaasdfasdfnged at runtime!!!");
                return true;
            }

            if (cmd == "reload") {
                var rm = Client.Load("PatchTest.spr");
                Client.BeginProcessing();
                Client.Process(CurrentModule, rm);
                Client.FinishProcessing();

                CurrentModule = rm;

                return true;
            }

            return false;
        }
    }
}
