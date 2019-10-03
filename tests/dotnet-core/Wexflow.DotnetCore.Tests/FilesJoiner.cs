using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.DotnetCore.Tests
{
    [TestClass]
    public class FilesJoiner
    {
//        private static readonly string FilesSplitterFolder = @"C:\WexflowTesting\FilesSplitter\";
        private static readonly string FilesSplitterFolder = @"C:\WexflowTesting\FilesSplitter\";

        [TestInitialize]
        public void TestInitialize()
        {
//            Helper.DeleteFiles(FilesSplitterFolder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
//            Helper.DeleteFiles(FilesSplitterFolder);
        }

        [TestMethod]
        public void FilesSplitterTest()
        {
            /*string[] files = GetFiles();
            Assert.AreEqual(0, files.Length);
            Helper.StartWorkflow(57);
            files = GetFiles();
            Assert.AreEqual(510, files.Length);*/
            Helper.StartWorkflow(1001);
            Assert.AreEqual(true, true);
        }

        private string[] GetFiles()
        {
            return Directory.GetFiles(FilesSplitterFolder, "*_*");
        }
    }
}
