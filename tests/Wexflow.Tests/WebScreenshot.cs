using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class WebScreenshot
    {
        private static readonly string DestDir = @"C:\WexflowTesting\WebScreenshot";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFiles(DestDir);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFiles(DestDir);
        }

        [TestMethod]
        public void WebScreenshotTest()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            Helper.StartWorkflow(96);
            files = GetFiles();
            Assert.AreEqual(2, files.Length);
        }

        private string[] GetFiles()
        {
            return Helper.GetFiles(DestDir, "*.png", SearchOption.TopDirectoryOnly);
        }
    }
}
