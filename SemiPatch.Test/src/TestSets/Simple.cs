using SemiPatch;

namespace SemiPatch.Test {
    public static partial class TestSets {
        public class SimpleMethodTarget {
            public static int HelloWorld() {
                return 42;
            }
        }

        [TestPatch("SimpleMethodTarget")]
        public class SimpleMethodPatch {
            public static int HelloWorld() {
                return 10;
            }
        }

        public class ReceiveOriginalMethodTarget {
            public static int HelloWorld() {
                return 10;
            }
        }

        [TestPatch("ReceiveOriginalMethodTarget")]
        public class ReceiveOriginalMethodPatch {
            [ReceiveOriginal]
            public static int HelloWorld(Orig<int> orig) {
                return orig() * 2;
            }
        }

        public class OverwriteInstanceMethodTarget {
            public string Name = "world";
            public string Test() => $"Hello, {Name}!";
        }

        [TestPatch("OverwriteInstanceMethodTarget")]
        public class OverwriteInstanceMethodPatch {
            [Proxy]
            public string Name;
            public string Test() => $"Bye, {Name}!";
        }

        public class MultiMethodTarget {
            public static string Test() => "a";
        }

        [TestPatch("MultiMethodTarget")]
        public class MultiMethodPatch1 {
            [ReceiveOriginal]
            public static string Test(Orig<string> orig) => orig() + "b";
        }

        [TestPatch("MultiMethodTarget")]
        public class MultiMethodPatch2 {
            [ReceiveOriginal]
            public static string Test(Orig<string> orig) => orig() + "c";
        }

    }
}
