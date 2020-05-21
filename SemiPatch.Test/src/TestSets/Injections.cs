using System;
using SemiPatch;

namespace SemiPatch.Test {
    public static partial class TestSets {
        public class CommonInjectionTarget {
            public static string Test(string name, int age) {
                var uppercase_name = string.Format("{0}{1}", name.Substring(0, 1).ToUpper(), name.Substring(1));
                var new_string = string.Format("{0} - age {1}", uppercase_name, age);
                return new_string;
            }
        }

        [TestPatch("CommonInjectionTarget")]
        public class HeadInjectionPatch {
            [Inject(
                inside: "string Test(string, int)",
                at: InjectQuery.Head,
                where: InjectPosition.After,
                path: null,
                index: 0
            )]
            public static void TestInject(InjectionState<string> state, string name, int age) {
                if (name.ToLowerInvariant() == "bob") {
                    state.ReturnValue = "i don't like bob";
                }
            }
        }

        [TestPatch("CommonInjectionTarget")]
        public class TailInjectionPatch {
            [Inject(
                inside: "string Test(string, int)",
                at: InjectQuery.Tail
            )]
            public void TestInject(InjectionState<string> state, string name, int age) {
                if (age < 18) {
                    state.ReturnValue = "sorry, semipatch is 18+";
                }
            }
        }

        [TestPatch("CommonInjectionTarget")]
        public class MethodCallWithCaptureInjectionPatch {
            [Inject(
                inside: "string Test(string, int)",
                at: InjectQuery.MethodCall,
                where: InjectPosition.Before,
                index: 1,
                path: "[System.String] string Format(string, System.Object, System.Object)"
            )]
            [CaptureLocal(
                index: 0,
                type: typeof(string),
                name: "formatted_name"
            )]
            public void TestInject(InjectionState<string> state, string name, int age) {
                var formatted_name = state.GetLocal<string>("formatted_name");
                formatted_name = $"Mr. {formatted_name}";
                state.SetLocal("formatted_name", formatted_name);
            }
        }
    }
}
