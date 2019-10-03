using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.DotnetCore.Tests
{
    [TestClass]
    public class FilesLoaderEx
    {
        private static readonly string TempFolderBase = Helper.IsUnixPlatform
            ? @"/opt/wexflow/Wexflow/Temp"
            : @"C:\Wexflow-dotnet-core\Temp";

        private static readonly string SourceFilesFolder = Helper.IsUnixPlatform
            ? "C:\\WexflowTesting\\FilesLoaderEx\\"
            : "/opt/wexflow/WexflowTesting/FilesLoaderEx/";

        private const string ExpectedResult138AddMaxCreateDate =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"+
            "<WexflowProcessing>\n"+
            "  <Workflow id=\"138\" name=\"Workflow_FilesLoaderEx_AddMaxCreateDate\" description=\"Workflow_FilesLoaderEx_AddMaxCreateDate\">\n"+
            "    <Files>\n"+
            "      <File taskId=\"1\" path=\"/opt/wexflow/WexflowTesting/FilesLoaderEx/file4.txt\" name=\"file4.txt\" renameTo=\"\" renameToOrName=\"file4.txt\" />\n"+
            "      <File taskId=\"1\" path=\"/opt/wexflow/WexflowTesting/FilesLoaderEx/file5.txt\" name=\"file5.txt\" renameTo=\"\" renameToOrName=\"file5.txt\" />\n"+
            "    </Files>\n"+
            "  </Workflow>\n"+
            "</WexflowProcessing>";

        private const string ExpectedResult139AddMinCreateDate =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"+
            "<WexflowProcessing>\n"+
            "  <Workflow id=\"139\" name=\"Workflow_FilesLoaderEx_AddMinCreateDate\" description=\"Workflow_FilesLoaderEx_AddMinCreateDate\">\n"+
            "    <Files>\n"+
            "      <File taskId=\"1\" path=\"/opt/wexflow/WexflowTesting/FilesLoaderEx/file1.txt\" name=\"file1.txt\" renameTo=\"\" renameToOrName=\"file1.txt\" />\n"+
            "      <File taskId=\"1\" path=\"/opt/wexflow/WexflowTesting/FilesLoaderEx/file2.txt\" name=\"file2.txt\" renameTo=\"\" renameToOrName=\"file2.txt\" />\n"+
            "    </Files>\n"+
            "  </Workflow>\n"+
            "</WexflowProcessing>";

        private const string ExpectedResult140AddMaxModifyDate =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"+
            "<WexflowProcessing>\n"+
            "  <Workflow id=\"140\" name=\"Workflow_FilesLoaderEx_AddMaxModifyDate\" description=\"Workflow_FilesLoaderEx_AddMaxModifyDate\">\n"+
            "    <Files>\n"+
            "      <File taskId=\"1\" path=\"/opt/wexflow/WexflowTesting/FilesLoaderEx/file4.txt\" name=\"file4.txt\" renameTo=\"\" renameToOrName=\"file4.txt\" />\n"+
            "      <File taskId=\"1\" path=\"/opt/wexflow/WexflowTesting/FilesLoaderEx/file5.txt\" name=\"file5.txt\" renameTo=\"\" renameToOrName=\"file5.txt\" />\n"+
            "    </Files>\n"+
            "  </Workflow>\n"+
            "</WexflowProcessing>";

        private const string ExpectedResult141AddMinModifyDate =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"+
            "<WexflowProcessing>\n"+
            "  <Workflow id=\"141\" name=\"Workflow_FilesLoaderEx_AddMinModifyDate\" description=\"Workflow_FilesLoaderEx_AddMinModifyDate\">\n"+
            "    <Files>\n"+
            "      <File taskId=\"1\" path=\"/opt/wexflow/WexflowTesting/FilesLoaderEx/file1.txt\" name=\"file1.txt\" renameTo=\"\" renameToOrName=\"file1.txt\" />\n"+
            "      <File taskId=\"1\" path=\"/opt/wexflow/WexflowTesting/FilesLoaderEx/file2.txt\" name=\"file2.txt\" renameTo=\"\" renameToOrName=\"file2.txt\" />\n"+
            "    </Files>\n"+
            "  </Workflow>\n"+
            "</WexflowProcessing>";

        private const string ExpectedResult142RemoveMaxCreateDate =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"+
            "<WexflowProcessing>\n"+
            "  <Workflow id=\"142\" name=\"Workflow_FilesLoaderEx_RemoveMaxCreateDate\" description=\"Workflow_FilesLoaderEx_RemoveMaxCreateDate\">\n"+
            "    <Files>\n"+
            "      <File taskId=\"1\" path=\"/opt/wexflow/WexflowTesting/FilesLoaderEx/file1.txt\" name=\"file1.txt\" renameTo=\"\" renameToOrName=\"file1.txt\" />\n"+
            "      <File taskId=\"1\" path=\"/opt/wexflow/WexflowTesting/FilesLoaderEx/file2.txt\" name=\"file2.txt\" renameTo=\"\" renameToOrName=\"file2.txt\" />\n"+
            "      <File taskId=\"1\" path=\"/opt/wexflow/WexflowTesting/FilesLoaderEx/file3.txt\" name=\"file3.txt\" renameTo=\"\" renameToOrName=\"file3.txt\" />\n"+
            "    </Files>\n"+
            "  </Workflow>\n"+
            "</WexflowProcessing>";

        private const string ExpectedResult143RemoveMinCreateDate =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"+
            "<WexflowProcessing>\n"+
            "  <Workflow id=\"143\" name=\"Workflow_FilesLoaderEx_RemoveMinCreateDate\" description=\"Workflow_FilesLoaderEx_RemoveMinCreateDate\">\n"+
            "    <Files>\n"+
            "      <File taskId=\"1\" path=\"/opt/wexflow/WexflowTesting/FilesLoaderEx/file3.txt\" name=\"file3.txt\" renameTo=\"\" renameToOrName=\"file3.txt\" />\n"+
            "      <File taskId=\"1\" path=\"/opt/wexflow/WexflowTesting/FilesLoaderEx/file4.txt\" name=\"file4.txt\" renameTo=\"\" renameToOrName=\"file4.txt\" />\n"+
            "      <File taskId=\"1\" path=\"/opt/wexflow/WexflowTesting/FilesLoaderEx/file5.txt\" name=\"file5.txt\" renameTo=\"\" renameToOrName=\"file5.txt\" />\n"+
            "    </Files>\n"+
            "  </Workflow>\n"+
            "</WexflowProcessing>";

        private const string ExpectedResult144RemoveMaxModifyDate =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"+
            "<WexflowProcessing>\n"+
            "  <Workflow id=\"144\" name=\"Workflow_FilesLoaderEx_RemoveMaxModifyDate\" description=\"Workflow_FilesLoaderEx_RemoveMaxModifyDate\">\n"+
            "    <Files>\n"+
            "      <File taskId=\"1\" path=\"/opt/wexflow/WexflowTesting/FilesLoaderEx/file1.txt\" name=\"file1.txt\" renameTo=\"\" renameToOrName=\"file1.txt\" />\n"+
            "      <File taskId=\"1\" path=\"/opt/wexflow/WexflowTesting/FilesLoaderEx/file2.txt\" name=\"file2.txt\" renameTo=\"\" renameToOrName=\"file2.txt\" />\n"+
            "      <File taskId=\"1\" path=\"/opt/wexflow/WexflowTesting/FilesLoaderEx/file3.txt\" name=\"file3.txt\" renameTo=\"\" renameToOrName=\"file3.txt\" />\n"+
            "    </Files>\n"+
            "  </Workflow>\n"+
            "</WexflowProcessing>";

        private const string ExpectedResult145RemoveMinModifyDate =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"+
            "<WexflowProcessing>\n"+
            "  <Workflow id=\"145\" name=\"Workflow_FilesLoaderEx_RemoveMinModifyDate\" description=\"Workflow_FilesLoaderEx_RemoveMinModifyDate\">\n"+
            "    <Files>\n"+
            "      <File taskId=\"1\" path=\"/opt/wexflow/WexflowTesting/FilesLoaderEx/file3.txt\" name=\"file3.txt\" renameTo=\"\" renameToOrName=\"file3.txt\" />\n"+
            "      <File taskId=\"1\" path=\"/opt/wexflow/WexflowTesting/FilesLoaderEx/file4.txt\" name=\"file4.txt\" renameTo=\"\" renameToOrName=\"file4.txt\" />\n"+
            "      <File taskId=\"1\" path=\"/opt/wexflow/WexflowTesting/FilesLoaderEx/file5.txt\" name=\"file5.txt\" renameTo=\"\" renameToOrName=\"file5.txt\" />\n"+
            "    </Files>\n"+
            "  </Workflow>\n"+
            "</WexflowProcessing>";

        public void TestInitialize(int workflowId)
        {
            var tempFolder = Path.Combine(TempFolderBase, workflowId.ToString());
            if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);
            Helper.DeleteFilesAndFolders(tempFolder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        [DataRow(138, ExpectedResult138AddMaxCreateDate)]
        [DataRow(139, ExpectedResult139AddMinCreateDate)]
        [DataRow(140, ExpectedResult140AddMaxModifyDate)]
        [DataRow(141, ExpectedResult141AddMinModifyDate)]
        [DataRow(142, ExpectedResult142RemoveMaxCreateDate)]
        [DataRow(143, ExpectedResult143RemoveMinCreateDate)]
        [DataRow(144, ExpectedResult144RemoveMaxModifyDate)]
        [DataRow(145, ExpectedResult145RemoveMinModifyDate)]
        public void FilesLoaderExTest(int workflowId, string expectedResult)
        {
            TestInitialize(workflowId);
            Helper.StartWorkflow(workflowId);

            // Check the workflow result
            string[] files = Directory.GetFiles(
                Path.Combine(TempFolderBase, workflowId.ToString()),
                "ListFiles*.xml",
                SearchOption.AllDirectories);
            Assert.AreEqual(1, files.Length);

            string content = File.ReadAllText(files[0]);
            Assert.AreEqual(expectedResult, content);
        }
    }
}
