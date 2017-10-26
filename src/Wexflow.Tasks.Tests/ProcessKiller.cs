using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.Tasks.Tests
{
    [TestClass]
    public class ProcessKiller
    {
        [TestInitialize]
        public void TestInitialize()
        {
            StartProcess(@"C:\Windows\System32\notepad.exe", "", false);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            StartProcess("taskkill", "/im \"notepad.exe\" /f", true);
        }

        [TestMethod]
        public void Run()
        {
            Helper.StartWorkflow(58);
            Process[] notepadProcesses = Process.GetProcessesByName("notepad");
            Assert.IsTrue(notepadProcesses.Length == 0);
        }

        private void StartProcess(string name, string cmd, bool hideGui)
        {
            var startInfo = new ProcessStartInfo(name,cmd)
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

        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            Debug.WriteLine("{0}", outLine.Data);
        }

        private void ErrorHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            Debug.WriteLine("{0}", outLine.Data);
        }
    }
}
