using System.Diagnostics;
using System.IO;
using System.Threading;
using Wexflow.Core;

namespace Wexflow.Tests
{
    public class Helper
    {
        private static readonly WexflowEngine WexflowEngine = new WexflowEngine(@"C:\Wexflow\Wexflow.xml");

        public static void Run()
        {
            WexflowEngine.Run();
        }

        public static void Stop()
        {
            WexflowEngine.Stop(false, false);
        }

        public static void StartWorkflow(int workflowId)
        {
            WexflowEngine.StartWorkflow(workflowId);

            // Wait until the workflow finishes
            Thread.Sleep(1000);
            var workflow = WexflowEngine.GetWorkflow(workflowId);
            var isRunning = workflow.IsRunning;
            while (isRunning)
            {
                Thread.Sleep(100);
                workflow = WexflowEngine.GetWorkflow(workflowId);
                isRunning = workflow.IsRunning;
            }
        }

        public static void StartWorkflowAsync(int workflowId)
        {
            WexflowEngine.StartWorkflow(workflowId);
        }

        public static void StopWorkflow(int workflowId)
        {
            WexflowEngine.StopWorkflow(workflowId);
        }

        public static void SuspendWorkflow(int workflowId)
        {
            WexflowEngine.SuspendWorkflow(workflowId);
        }

        public static void ResumeWorkflow(int workflowId)
        {
            WexflowEngine.ResumeWorkflow(workflowId);
        }

        public static Core.Workflow GetWorkflow(int workflowId)
        {
            return WexflowEngine.GetWorkflow(workflowId);
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
            string dirName = Path.GetFileName(src);
            string destDir = Path.Combine(dest, dirName);
            Directory.CreateDirectory(destDir);

            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(src, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(src, destDir));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(src, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(src, destDir), true);
        }

        public static void StartProcess(string name, string cmd, bool hideGui)
        {
            var startInfo = new ProcessStartInfo(name, cmd)
            {
                CreateNoWindow = hideGui,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var process = new Process { StartInfo = startInfo };
            process.OutputDataReceived += OutputHandler;
            process.ErrorDataReceived += ErrorHandler;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }

        public static string[] GetFiles(string dir, string pattern, SearchOption searchOption)
        {
            return Directory.GetFiles(dir, pattern, searchOption);
        }

        private static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            Debug.WriteLine("{0}", outLine.Data);
        }

        private static void ErrorHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            Debug.WriteLine("{0}", outLine.Data);
        }
    }
}
