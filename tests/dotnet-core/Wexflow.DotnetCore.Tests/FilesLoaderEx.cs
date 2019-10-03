using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.DotnetCore.Tests
{
    [TestClass]
    public class FilesLoaderEx
    {
        public static readonly string SourceFilesFolder =
            Path.Combine(Helper.SourceFilesFolder, "FilesLoaderEx") + Path.DirectorySeparatorChar;

        private static readonly string ExpectedResult138AddMaxCreateDate =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"+
            "<WexflowProcessing>\n"+
            "  <Workflow id=\"138\" name=\"Workflow_FilesLoaderEx_AddMaxCreateDate\" description=\"Workflow_FilesLoaderEx_AddMaxCreateDate\">\n"+
            "    <Files>\n"+
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file4.txt\" name=\"file4.txt\" renameTo=\"\" renameToOrName=\"file4.txt\" />\n"+
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file5.txt\" name=\"file5.txt\" renameTo=\"\" renameToOrName=\"file5.txt\" />\n"+
            "    </Files>\n"+
            "  </Workflow>\n"+
            "</WexflowProcessing>";

        private static readonly string ExpectedResult139AddMinCreateDate =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"+
            "<WexflowProcessing>\n"+
            "  <Workflow id=\"139\" name=\"Workflow_FilesLoaderEx_AddMinCreateDate\" description=\"Workflow_FilesLoaderEx_AddMinCreateDate\">\n"+
            "    <Files>\n"+
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file1.txt\" name=\"file1.txt\" renameTo=\"\" renameToOrName=\"file1.txt\" />\n"+
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file2.txt\" name=\"file2.txt\" renameTo=\"\" renameToOrName=\"file2.txt\" />\n"+
            "    </Files>\n"+
            "  </Workflow>\n"+
            "</WexflowProcessing>";

        private static readonly string ExpectedResult140AddMaxModifyDate =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"+
            "<WexflowProcessing>\n"+
            "  <Workflow id=\"140\" name=\"Workflow_FilesLoaderEx_AddMaxModifyDate\" description=\"Workflow_FilesLoaderEx_AddMaxModifyDate\">\n"+
            "    <Files>\n"+
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file4.txt\" name=\"file4.txt\" renameTo=\"\" renameToOrName=\"file4.txt\" />\n"+
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file5.txt\" name=\"file5.txt\" renameTo=\"\" renameToOrName=\"file5.txt\" />\n"+
            "    </Files>\n"+
            "  </Workflow>\n"+
            "</WexflowProcessing>";

        private static readonly string ExpectedResult141AddMinModifyDate =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"+
            "<WexflowProcessing>\n"+
            "  <Workflow id=\"141\" name=\"Workflow_FilesLoaderEx_AddMinModifyDate\" description=\"Workflow_FilesLoaderEx_AddMinModifyDate\">\n"+
            "    <Files>\n"+
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file1.txt\" name=\"file1.txt\" renameTo=\"\" renameToOrName=\"file1.txt\" />\n"+
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file2.txt\" name=\"file2.txt\" renameTo=\"\" renameToOrName=\"file2.txt\" />\n"+
            "    </Files>\n"+
            "  </Workflow>\n"+
            "</WexflowProcessing>";

        private static readonly string ExpectedResult142RemoveMaxCreateDate =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"+
            "<WexflowProcessing>\n"+
            "  <Workflow id=\"142\" name=\"Workflow_FilesLoaderEx_RemoveMaxCreateDate\" description=\"Workflow_FilesLoaderEx_RemoveMaxCreateDate\">\n"+
            "    <Files>\n"+
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file1.txt\" name=\"file1.txt\" renameTo=\"\" renameToOrName=\"file1.txt\" />\n"+
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file2.txt\" name=\"file2.txt\" renameTo=\"\" renameToOrName=\"file2.txt\" />\n"+
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file3.txt\" name=\"file3.txt\" renameTo=\"\" renameToOrName=\"file3.txt\" />\n"+
            "    </Files>\n"+
            "  </Workflow>\n"+
            "</WexflowProcessing>";

        private static readonly string ExpectedResult143RemoveMinCreateDate =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"+
            "<WexflowProcessing>\n"+
            "  <Workflow id=\"143\" name=\"Workflow_FilesLoaderEx_RemoveMinCreateDate\" description=\"Workflow_FilesLoaderEx_RemoveMinCreateDate\">\n"+
            "    <Files>\n"+
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file3.txt\" name=\"file3.txt\" renameTo=\"\" renameToOrName=\"file3.txt\" />\n"+
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file4.txt\" name=\"file4.txt\" renameTo=\"\" renameToOrName=\"file4.txt\" />\n"+
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file5.txt\" name=\"file5.txt\" renameTo=\"\" renameToOrName=\"file5.txt\" />\n"+
            "    </Files>\n"+
            "  </Workflow>\n"+
            "</WexflowProcessing>";

        private static readonly string ExpectedResult144RemoveMaxModifyDate =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"+
            "<WexflowProcessing>\n"+
            "  <Workflow id=\"144\" name=\"Workflow_FilesLoaderEx_RemoveMaxModifyDate\" description=\"Workflow_FilesLoaderEx_RemoveMaxModifyDate\">\n"+
            "    <Files>\n"+
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file1.txt\" name=\"file1.txt\" renameTo=\"\" renameToOrName=\"file1.txt\" />\n"+
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file2.txt\" name=\"file2.txt\" renameTo=\"\" renameToOrName=\"file2.txt\" />\n"+
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file3.txt\" name=\"file3.txt\" renameTo=\"\" renameToOrName=\"file3.txt\" />\n"+
            "    </Files>\n"+
            "  </Workflow>\n"+
            "</WexflowProcessing>";

        private static readonly string ExpectedResult145RemoveMinModifyDate =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"+
            "<WexflowProcessing>\n"+
            "  <Workflow id=\"145\" name=\"Workflow_FilesLoaderEx_RemoveMinModifyDate\" description=\"Workflow_FilesLoaderEx_RemoveMinModifyDate\">\n"+
            "    <Files>\n"+
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file3.txt\" name=\"file3.txt\" renameTo=\"\" renameToOrName=\"file3.txt\" />\n"+
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file4.txt\" name=\"file4.txt\" renameTo=\"\" renameToOrName=\"file4.txt\" />\n"+
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file5.txt\" name=\"file5.txt\" renameTo=\"\" renameToOrName=\"file5.txt\" />\n"+
            "    </Files>\n"+
            "  </Workflow>\n"+
            "</WexflowProcessing>";

        public void TestInitialize(int workflowId)
        {
            var tempFolder = Path.Combine(Helper.TempFolder, workflowId.ToString());
            if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);
            Helper.DeleteFilesAndFolders(tempFolder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        public void Execute(int workflowId, string expectedResult)
        {
            TestInitialize(workflowId);
            Helper.StartWorkflow(workflowId);

            // Check the workflow result
            string[] files = Directory.GetFiles(
                Path.Combine(Helper.TempFolder, workflowId.ToString()),
                "ListFiles*.xml",
                SearchOption.AllDirectories);
            Assert.AreEqual(1, files.Length);

            string content = File.ReadAllText(files[0]);
            Assert.AreEqual(expectedResult, content);
        }

        [TestMethod]
        public void FilesLoaderExTest_138AddMaxCreateDate() => Execute(138, ExpectedResult138AddMaxCreateDate);

        [TestMethod]
        public void FilesLoaderExTest_139AddMinCreateDate() => Execute(139, ExpectedResult139AddMinCreateDate);

        [TestMethod]
        public void FilesLoaderExTest_140AddMaxModifyDate() => Execute(140, ExpectedResult140AddMaxModifyDate);

        [TestMethod]
        public void FilesLoaderExTest_141AddMinModifyDate() => Execute(141, ExpectedResult141AddMinModifyDate);

        [TestMethod]
        public void FilesLoaderExTest_142RemoveMaxCreateDate() => Execute(142, ExpectedResult142RemoveMaxCreateDate);

        [TestMethod]
        public void FilesLoaderExTest_143RemoveMinCreateDate() => Execute(143, ExpectedResult143RemoveMinCreateDate);

        [TestMethod]
        public void FilesLoaderExTest_144RemoveMaxModifyDate() => Execute(144, ExpectedResult144RemoveMaxModifyDate);

        [TestMethod]
        public void FilesLoaderExTest_145RemoveMinModifyDate() => Execute(145, ExpectedResult145RemoveMinModifyDate);
    }
}
