using System;
using SemiPatch;

namespace SemiPatch.Test {
    public static partial class TestSets {
        public class CompleteTarget {
            public static int StaticField;
            public int InstanceField;
            public static int StaticGetProperty { get; }
            public int InstanceGetProperty { get; }
            public static int StaticSetProperty { set { } }
            public int InstanceSetProperty { set { } }
            public static int StaticProperty { get; set; }
            public int InstanceProperty { get; set; }

            public static void StaticVoidMethod() {}
            public static int StaticMethod() => 10;
            public static void StaticVoidMethodWithArgs(int a, int b) {}
            public static int StaticMethodWithArgs(int a, int b) => a * b;
        }

        [TestPatch("CompleteTarget")]
        public class CompletePatch {
            [Ignore]
            public static int NonExistantField;

            [Proxy]
            public static int StaticField;

            [Proxy]
            public int InstanceField;

            [Proxy]
            public static int StaticGetProperty { get; }

            [Proxy]
            public int InstanceGetProperty { get; }

            [Proxy]
            public static int StaticSetProperty { set {} }

            [Proxy]
            public int InstanceSetProperty { set {} }

            [Proxy]
            public static int StaticProperty { get; set; }

            [Proxy]
            public int InstanceProperty { get; set; }

            [Insert]
            public static int AddedStaticField;

            [Insert]
            public int AddedInstanceField;

            [Insert]
            public static int AddedStaticProperty { get; set; }

            [Insert]
            public int AddedInstanceProperty { get; set; }

            [Insert]
            [SetMethod("StaticGetProperty")]
            public void SetStaticGetProperty(int value) {}

            [Insert]
            [GetMethod("StaticSetProperty")]
            public int GetStaticSetProperty() => 0;

            public static void StaticVoidMethod() { /* changed */ }

            [ReceiveOriginal]
            public static int StaticMethod(Orig<int> orig) => orig();

            [Inject(
                inside: "void StaticVoidMethodWithArgs(int, int)",
                at: InjectQuery.Head
            )]
            public void StaticVoidMethodWithArgsInject(InjectionState state, int a, int b) {
            }

            [Inject(
                inside: "int StaticMethodWithArgs(int, int)",
                at: InjectQuery.Head
            )]
            public void StaticMethodWithArgsInject(InjectionState<int> state, int a, int b) {
            }

            [Insert]
            public void AddedInstanceMethod() {}

            [Insert]
            public static void AddedStaticMethod() {}
        }
    }

    [TestPatch("CompleteTarget")]
    public class CompletePatchInsertsOnly {
        [Insert]
        public float NewFloatField;

        [Insert]
        public void AnotherNewMethod() {}
    }
}
