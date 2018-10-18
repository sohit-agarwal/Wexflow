using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.Tasks.Tests
{
    [TestClass]
    public class Torrent
    {
        private static readonly string DownloadedFile = @"C:\WexflowTesting\Torrent\A Kernel Two-Sample Test.pdf";

        [TestInitialize]
        public void TestInitialize()
        {
            if (File.Exists(DownloadedFile)) File.Delete(DownloadedFile);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (File.Exists(DownloadedFile)) File.Delete(DownloadedFile);
        }

        [TestMethod]
        public void TorrentTest()
        {
            Assert.IsFalse(File.Exists(DownloadedFile));
            Helper.StartWorkflow(72);
            Assert.IsTrue(File.Exists(DownloadedFile));
        }
    }
}
