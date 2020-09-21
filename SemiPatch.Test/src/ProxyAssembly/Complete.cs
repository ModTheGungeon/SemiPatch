using System;
using NUnit.Framework;
using static SemiPatch.Test.TestSets;


namespace SemiPatch.Test {
    public partial class ProxyAssemblyTests {
        [Test]
        public void CompleteProxy() {
            var test = new Test(
                "CompleteProxy",
                typeof(CompleteTarget)
            );
            test.ReloadFromDisk();

            var client = new ProxyClient(test.TargetModule);

            var pm1 = test.CreatePatchModule("foo", typeof(CompletePatch));
            var pm2 = test.CreatePatchModule("bar", typeof(CompletePatchInsertsOnly));

            var rm1 = test.MakeReloadable(pm1);
            var rm2 = test.MakeReloadable(pm2);

            client.AddModule(rm1);
            client.AddModule(rm2);

            client.Commit();
            
            client.WriteResult(test.TemporaryPath(client.ProxyModule.Assembly.FullName + ".dll"));
        }
    }
}
