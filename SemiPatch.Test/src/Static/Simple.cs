using System;
using System.Reflection;
using Mono.Cecil;
using NUnit.Framework;
using static SemiPatch.Test.TestSets;

namespace SemiPatch.Test {
    public partial class StaticTests {
        [Test]
        public void SimpleMethodOverwrite() {
            var result = Test.StaticTest<int>(
                "SimpleMethod",
                "HelloWorld",
                new object[0],
                typeof(SimpleMethodTarget),
                typeof(SimpleMethodPatch)
            );
            Assert.AreEqual(42, result.Target);
            Assert.AreEqual(10, result.Patched);
        }

        [Test]
        public void ReceiveOriginalMethod() {
            var result = Test.StaticTest<int>(
                "ReceiveOriginalMethod",
                "HelloWorld",
                new object[0],
                typeof(ReceiveOriginalMethodTarget),
                typeof(ReceiveOriginalMethodPatch)
            );
            Assert.AreEqual(10, result.Target);
            Assert.AreEqual(20, result.Patched);
        }

        [Test]
        public void OverwriteInstanceMethodTest() {
            var result = Test.SimpleTest(
                "OverwriteInstanceMethod",
                typeof(OverwriteInstanceMethodTarget),
                typeof(OverwriteInstanceMethodPatch)
            );
            var target_inst = Activator.CreateInstance(result.Target);
            Assert.AreEqual("Hello, world!", result.Target.GetMethod("Test").Invoke(target_inst, new object[0]));
            var patch_inst = Activator.CreateInstance(result.Patched);
            Assert.AreEqual("Bye, world!", result.Patched.GetMethod("Test").Invoke(patch_inst, new object[0]));
        }

        [Test]
        public void MultiMethodTest() {
            var result = Test.StaticTest<string>(
                "MultiMethod",
                "Test",
                new object[0],
                typeof(MultiMethodTarget),
                typeof(MultiMethodPatch1),
                typeof(MultiMethodPatch2)
            );

            Assert.AreEqual("a", result.Target);
            Assert.AreEqual("abc", result.Patched);
        }
    }
    
}
