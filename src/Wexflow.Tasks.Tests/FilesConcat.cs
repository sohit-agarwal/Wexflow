using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.Tasks.Tests
{
    [TestClass]
    public class FilesConcat
    {
        private static readonly string FilesConcatFile = @"C:\WexflowTesting\FilesConcat\file1_file2_file3";
        private static readonly string ExpectedResult = "abcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabc";

        [TestInitialize]
        public void TestInitialize()
        {
            if (File.Exists(FilesConcatFile))
            {
                File.Delete(FilesConcatFile);
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            File.Delete(FilesConcatFile);
        }

        [TestMethod]
        public void Run()
        {
            Assert.IsFalse(File.Exists(FilesConcatFile));
            Helper.StartWorkflow(53);
            Assert.IsTrue(File.Exists(FilesConcatFile));
            string content = File.ReadAllText(FilesConcatFile);
            Assert.AreEqual(ExpectedResult, content);
        }

    }
}
