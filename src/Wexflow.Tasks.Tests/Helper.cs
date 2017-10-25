using System;
using System.IO;
using System.Threading;
using Wexflow.Core;

namespace Wexflow.Tasks.Tests
{
    public class Helper
    {
        private static readonly WexflowEngine WexflowEngine = new WexflowEngine(@"C:\Wexflow\Wexflow.xml");

        public static void StartWorkflow(int workflowId)
        {
            WexflowEngine.StartWorkflow(workflowId);

            // Wait until the workflow finishes
            var workflow = WexflowEngine.GetWorkflow(workflowId);
            var isRunning = workflow.IsRunning;
            while (isRunning)
            {
                Thread.Sleep(100);
                workflow = WexflowEngine.GetWorkflow(workflowId);
                isRunning = workflow.IsRunning;
            }
        }

        public static void DeleteFilesAndFolders(string folder)
        {
            DeleteFiles(folder);

            foreach (var dir in Directory.GetDirectories(folder))
            {
                DeleteDirRec(dir);
            }
        }

        public static void DeleteFiles(string dir)
        {
            foreach (var file in Directory.GetFiles(dir))
            {
                File.Delete(file);
            }
        }

        private static void DeleteDirRec(string dir)
        {
            //foreach (var file in Directory.GetFiles(dir))
            //{
            //    File.Delete(file);
            //}

            foreach (var subdir in Directory.GetDirectories(dir))
            {
                DeleteDirRec(subdir);
            }

            Directory.Delete(dir, true);
        }

        public static void CopyDirRec(string src, string dest)
        {
            string dirName = Path.GetFileName(src) ?? throw new InvalidOperationException();
            string destDir = Path.Combine(dest, dirName);
            Directory.CreateDirectory(destDir);

            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(src, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(src, destDir));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(src, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(src, destDir), true);
        }

    }
}
