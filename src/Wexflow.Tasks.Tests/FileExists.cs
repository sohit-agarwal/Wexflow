using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.Tasks.Tests
{
    [TestClass]
    public class FileExists
    {

        [TestInitialize]
        public void TestInitialize()
        {
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void Run()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            Helper.StartWorkflow(49);
            stopwatch.Stop();
            Assert.IsTrue(stopwatch.ElapsedMilliseconds > 1000 && stopwatch.ElapsedMilliseconds < 2000);
            stopwatch.Reset();
            stopwatch.Start();
            Helper.StartWorkflow(50);
            stopwatch.Stop();
            Assert.IsTrue(stopwatch.ElapsedMilliseconds > 2000);
        }
    }
}
