using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.Tasks.Tests
{
    [TestClass]
    public class PeriodicAndStratup
    {
        [TestInitialize]
        public void TestInitialize()
        {
            Helper.Run(); // Run Wexflow engine instance
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void PeriodicAndStratupTest()
        {
            Thread.Sleep(2000);
            // TODO
        }
    }
}
