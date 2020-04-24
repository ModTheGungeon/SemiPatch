using System;
using System.Runtime.CompilerServices;
using SemiPatch;
using SemiPatch.RDAR;


namespace PatchTest.Patches {
    [Patch(type: typeof(TargetTest.MyClass))]
    public class MyClass {
        [MethodImpl(MethodImplOptions.NoInlining)]
        [ReceiveOriginal]
        public static void Main(VoidOrig<string[]> orig, string[] args) {
            orig(args);
        }

        [Insert]
        public void Test() {
            Console.WriteLine("test");
        }

        [Insert]
        public int SomeFieldIAdded;

        [Insert]
        public int ANewProperty {
            get => 0;
        }
    }

    public class Blah {

    }
}
