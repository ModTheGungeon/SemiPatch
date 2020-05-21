using System;
using System.Collections.Generic;
using NUnit.Framework;
using Mono.Cecil;
using static SemiPatch.Test.TestSets;

namespace SemiPatch.Test
{
    public class RuntimeBootstrapSimpleMethodsTest {
        [Test]
        public void RuntimeBootstrapSimpleMethodOverwrite() {
            var test = new Test("RuntimeBootstrapSimpleMethodOverwrite");
            test.Target(typeof(SimpleMethodTarget));
            test.WriteTarget();
            var target_asm = test.LoadTarget();
            var type = target_asm.GetType("SimpleMethodTarget");

            var method = new Func<int>(() => (int)type.GetMethod("HelloWorld").Invoke(null, new object[0]));
            Assert.AreEqual(42, method());

            test.Patch("foo", typeof(SimpleMethodPatch));
            test.AnalyzeAll();
            test.WritePatches();
            test.ReloadFromDisk();

            var rm = test.Reloadable("foo");

            var client = new RuntimeClient(
                target_asm,
                test.TargetModule.Definition,
                test.TargetModule.Definition
            );

            client.BeginProcessing();
            client.Process(null, rm);
            client.FinishProcessing();

            Assert.AreEqual(10, method());

        }

        [Test]
        public void RuntimeBootstrapHeadInjection() {
            var test = new Test("RuntimeBootstrapHeadInjection");
            test.Target(typeof(CommonInjectionTarget));
            test.WriteTarget();
            var target_asm = test.LoadTarget();
            var type = target_asm.GetType("CommonInjectionTarget");

            var method = new Func<string, int, string>((name, age) => {
                return (string)type.GetMethod("Test").Invoke(null, new object[] { name, age });
            });
            Assert.AreEqual("Bob - age 42", method("bob", 42));

            test.Patch("foo", typeof(HeadInjectionPatch));
            test.AnalyzeAll();
            test.WritePatches();
            test.ReloadFromDisk();

            var rm = test.Reloadable("foo");
            var client = new RuntimeClient(
                target_asm,
                test.TargetModule.Definition,
                test.TargetModule.Definition
            );

            client.BeginProcessing();
            client.Process(null, rm);
            client.FinishProcessing();

            Assert.AreEqual("i don't like bob", method("bob", 42));
        }

    }
}
