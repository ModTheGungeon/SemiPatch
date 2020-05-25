using NUnit.Framework;
using static SemiPatch.Test.TestSets;
using Mono.Cecil;
using System;

namespace SemiPatch.Test
{
    public partial class AnalyzerTests {
        [Test]
        public void Completeness() {
            var test = new Test(
                "Completeness",
                typeof(CompleteTarget),
                typeof(CompletePatch)
            );

            var analyzer = new Analyzer(test.TargetModule, new ModuleDefinition[] { test.PatchModules[0] });
            var data = analyzer.Analyze();

            Assert.AreEqual(1, data.Types.Count);
            var type = data.Types[0];

            Assert.AreEqual(19, type.Methods.Count);
            Assert.AreEqual(8, type.Properties.Count);
            Assert.AreEqual(11, type.Fields.Count);

            // TODO
        }
    }
}
