using System;
using SemiPatch;
using NUnit.Framework;
using MonoMod.Utils;
using static SemiPatch.Test.TestSets;

namespace SemiPatch.Test {
    public partial class StaticTests {
        [Test]
        public void HeadInjection() {
            var types = Test.SimpleTest(
                "HeadInjection",
                typeof(CommonInjectionTarget),
                typeof(HeadInjectionPatch)
            );
            var target_method = types.Target.GetMethod("Test");
            var patched_method = types.Patched.GetMethod("Test");

            Assert.AreEqual("Tom - age 42", target_method.Invoke(null, new object[] { "Tom", 42 }));
            Assert.AreEqual("Bob - age 35", target_method.Invoke(null, new object[] { "Bob", 35 }));

            Assert.AreEqual("Tom - age 42", patched_method.Invoke(null, new object[] { "Tom", 42 }));
            Assert.AreEqual("i don't like bob", patched_method.Invoke(null, new object[] { "Bob", 35 }));
        }

        [Test]
        public void TailInjection() {
            var types = Test.SimpleTest(
                "TailInjection",
                typeof(CommonInjectionTarget),
                typeof(TailInjectionPatch)
            );
            var target_method = types.Target.GetMethod("Test");
            var patched_method = types.Patched.GetMethod("Test");

            Assert.AreEqual("Tom - age 42", target_method.Invoke(null, new object[] { "Tom", 42 }));
            Assert.AreEqual("Bob - age 16", target_method.Invoke(null, new object[] { "Bob", 16 }));

            Assert.AreEqual("Tom - age 42", patched_method.Invoke(null, new object[] { "Tom", 42 }));
            Assert.AreEqual("sorry, semipatch is 18+", patched_method.Invoke(null, new object[] { "Bob", 16 }));
        }

        [Test]
        public void MethodCallWithCaptureInjection() {
            var types = Test.SimpleTest(
                "MethodCallWithCapture",
                typeof(CommonInjectionTarget),
                typeof(MethodCallWithCaptureInjectionPatch)
            );
            var target_method = types.Target.GetMethod("Test");
            var patched_method = types.Patched.GetMethod("Test");

            Assert.AreEqual("Tom - age 42", target_method.Invoke(null, new object[] { "tom", 42 }));
            Assert.AreEqual("Bob - age 35", target_method.Invoke(null, new object[] { "Bob", 35 }));

            Assert.AreEqual("Mr. Tom - age 42", patched_method.Invoke(null, new object[] { "tom", 42 }));
            Assert.AreEqual("Mr. Bob - age 35", patched_method.Invoke(null, new object[] { "Bob", 35 }));
        }
    }
}
