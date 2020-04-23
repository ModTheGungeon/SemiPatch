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

        [Ignore]
        public extern void Test();
    }

    public class Blah {

    }
}
