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
            var test = new Test(
                "RuntimeBootstrapSimpleMethodOverwrite",
                typeof(SimpleMethodTarget)
            );
            test.ReloadFromDisk();
            var target_asm = test.LoadTarget();
            var type = target_asm.GetType("SimpleMethodTarget");

            var method = new Func<int>(() => (int)type.GetMethod("HelloWorld").Invoke(null, new object[0]));
            Assert.AreEqual(42, method());

            var pm = test.CreatePatchModule("foo", typeof(SimpleMethodPatch));
            var rm = test.MakeReloadable(pm);

            var client = new RuntimeClient(
                target_asm,
                test.TargetModule,
                test.TargetModule
            );

            client.AddModule(rm);
            client.Commit();

            Assert.AreEqual(10, method());

        }

        [Test]
        public void RuntimeBootstrapHeadInjection() {
            var test = new Test(
                "RuntimeBootstrapHeadInjection",
                typeof(CommonInjectionTarget)
            );
            var target_asm = test.LoadTarget();
            var type = target_asm.GetType("CommonInjectionTarget");

            var method = new Func<string, int, string>((name, age) => {
                return (string)type.GetMethod("Test").Invoke(null, new object[] { name, age });
            });
            Assert.AreEqual("Bob - age 42", method("bob", 42));
            test.ReloadFromDisk();

            var pm = test.CreatePatchModule("foo", typeof(HeadInjectionPatch));
            var rm = test.MakeReloadable(pm);

            var client = new RuntimeClient(
                target_asm,
                test.TargetModule,
                test.TargetModule
            );

            client.AddModule(rm);
            client.Commit();

            Assert.AreEqual("i don't like bob", method("bob", 42));
        }

    }
}
