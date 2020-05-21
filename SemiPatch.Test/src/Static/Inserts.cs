using System;
using SemiPatch;
using NUnit.Framework;
using static SemiPatch.Test.TestSets;

namespace SemiPatch.Test {
    public partial class StaticTests {
        [Test]
        public void SimpleInserts() {
            var result = Test.SimpleTest(
                "SimpleInserts",
                typeof(SimpleInsertsTarget),
                typeof(SimpleInsertsPatch1),
                typeof(SimpleInsertsPatch2)
            );

            Assert.IsNotNull(result.Target.GetField("Foo"));
            Assert.IsNotNull(result.Target.GetField("Bar"));
            Assert.IsNotNull(result.Target.GetProperty("Baz"));
            Assert.IsNull(result.Target.GetField("NewFoo"));
            Assert.IsNull(result.Target.GetProperty("InsertedProperty"));
            Assert.IsNull(result.Target.GetMethod("Test"));
            Assert.IsNull(result.Target.GetField("NewFoo2"));
            Assert.IsNull(result.Target.GetProperty("InsertedProperty2"));
            Assert.IsNull(result.Target.GetMethod("Test2"));

            Assert.IsNotNull(result.Patched.GetField("Foo"));
            Assert.IsNotNull(result.Patched.GetField("Bar"));
            Assert.IsNotNull(result.Patched.GetProperty("Baz"));
            Assert.IsNotNull(result.Patched.GetField("NewFoo"));
            Assert.IsNotNull(result.Patched.GetProperty("InsertedProperty"));
            Assert.IsNotNull(result.Patched.GetMethod("Test"));
            Assert.IsNotNull(result.Patched.GetField("NewFoo2"));
            Assert.IsNotNull(result.Patched.GetProperty("InsertedProperty2"));
            Assert.IsNotNull(result.Patched.GetMethod("Test2"));
        }
    }
}
