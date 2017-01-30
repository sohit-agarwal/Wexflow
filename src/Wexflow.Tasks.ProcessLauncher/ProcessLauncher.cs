using System;
using Wexflow.Core;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace Wexflow.Tasks.ProcessLauncher
{
    public class ProcessLauncher:Task
    {
        public string ProcessPath { get; set; }
        public string ProcessCmd { get; set; }
        public bool HideGui { get; set; }
        public bool GeneratesFiles { get; set; }

        const string VAR_FILE_PATH = "$filePath";
        const string VAR_FILE_NAME = "$fileName";
        const string VAR_FILE_NAME_WITHOUT_EXTENSION = "$fileNameWithoutExtension";
        const string VAR_OUTPUT = "$output";

        public ProcessLauncher(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            ProcessPath = GetSetting("processPath");
            ProcessCmd = GetSetting("processCmd");
            HideGui = bool.Parse(GetSetting("hideGui"));
            GeneratesFiles = bool.Parse(GetSetting("generatesFiles"));
        }

        public override TaskStatus Run()
        {
            Info("Launching process...");

            if (GeneratesFiles && !(ProcessCmd.Contains(VAR_FILE_NAME) && (ProcessCmd.Contains(VAR_OUTPUT) && (ProcessCmd.Contains(VAR_FILE_NAME) || ProcessCmd.Contains(VAR_FILE_NAME_WITHOUT_EXTENSION)))))
            {
                Error("Error in process command. Please read the documentation.");
                return new TaskStatus(Status.Error, false);
            }

            bool success = true;
            bool atLeastOneSucceed = false;

            if (!GeneratesFiles)
            {
                return StartProcess(ProcessPath, ProcessCmd, HideGui);
            }
            
			foreach (FileInf file in SelectFiles())
			{
				string cmd = string.Empty;
				string outputFilePath = string.Empty;

				try
				{
					cmd = ProcessCmd.Replace(string.Format("{{{0}}}", VAR_FILE_PATH), string.Format("\"{0}\"", file.Path));

					string outputRegexPattern = @"{\$output:(?:\$fileNameWithoutExtension|\$fileName)(?:[a-zA-Z0-9._-]*})";
					var outputRegex = new Regex(outputRegexPattern);
					var m = outputRegex.Match(cmd);

					if (m.Success)
					{
						string val = m.Value;
						outputFilePath = val;
						if (outputFilePath.Contains(VAR_FILE_NAME_WITHOUT_EXTENSION))
						{
							outputFilePath = outputFilePath.Replace(VAR_FILE_NAME_WITHOUT_EXTENSION, Path.GetFileNameWithoutExtension(file.FileName));
						}
						else if (outputFilePath.Contains(VAR_FILE_NAME))
						{
							outputFilePath = outputFilePath.Replace(VAR_FILE_NAME, file.FileName);
						}
						outputFilePath = outputFilePath.Replace("{" + VAR_OUTPUT + ":", Workflow.WorkflowTempFolder.Trim('\\') + "\\");
						outputFilePath = outputFilePath.Trim('}');

						cmd = cmd.Replace(val, "\"" + outputFilePath + "\"");
					}
					else
					{
						Error("Error in process command. Please read the documentation.");
						return new TaskStatus(Status.Error, false);
					}
				}
				catch (ThreadAbortException)
				{
					throw;
				}
				catch (Exception e)
				{
					ErrorFormat("Error in process command. Please read the documentation. Error: {0}", e.Message);
					return new TaskStatus(Status.Error, false);
				}

				if (StartProcess(ProcessPath, cmd, HideGui).Status == Status.Success)
				{
					Files.Add(new FileInf(outputFilePath, Id));

					if (!atLeastOneSucceed) atLeastOneSucceed = true;
				}
				else
				{
					success = false;
				}
			}

            Status status = Status.Success;

            if (!success && atLeastOneSucceed)
            {
                status = Status.Warning;
            }
            else if (!success)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status, false);
        }

        TaskStatus StartProcess(string processPath, string processCmd, bool hideGui)
        {
            try
            {
                var startInfo = new ProcessStartInfo(ProcessPath, processCmd);
                startInfo.CreateNoWindow = hideGui;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;

                var process = new Process();
                process.StartInfo = startInfo;
                process.OutputDataReceived += OutputHandler;
                process.ErrorDataReceived += ErrorHandler;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                return new TaskStatus(Status.Success, false);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while launching the process {0}", e, processPath);
                return new TaskStatus(Status.Error, false);
            }
        }

        void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            InfoFormat("{0}", outLine.Data);
        }

        void ErrorHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            ErrorFormat("{0}", outLine.Data);
        }

    }
}
