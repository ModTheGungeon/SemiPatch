using System;
using System.Runtime.CompilerServices;
using SemiPatch;
using SemiPatch.RDAR;


namespace PatchTest.Patches {
    [Patch(type: typeof(TargetTest.MyClass))]
    public class MyClass {
        [ReceiveOriginal]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Main(VoidOrig<string[]> orig, string[] args) {
            orig(args);
        }
    }

    public class Blah {

    }
}
