using System;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Threading;

namespace Wexflow.Tasks.FilesMover
{
    public class FilesMover:Task
    {
        public string DestFolder { get; private set; }
        public bool Overwrite { get; private set; }

        public FilesMover(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            DestFolder = GetSetting("destFolder");
            Overwrite = bool.Parse(GetSetting("overwrite", "false"));
        }

        public override TaskStatus Run()
        {
            Info("Moving files...");

            bool success = true;
            bool atLeastOneSucceed = false;

            var files = SelectFiles();
            for (int i = files.Length - 1; i > -1 ; i--) 
            {
                var file = files[i];
                var destFilePath = Path.Combine(DestFolder, Path.GetFileName(file.Path));

                try
                {
                    if (File.Exists(destFilePath))
                    {
                        if (Overwrite)
                        {
                            File.Delete(destFilePath);
                        }
                        else
                        {
                            ErrorFormat("Destination file {0} already exists.", destFilePath);
                            success = false;
                            continue;
                        }
                    }

                    File.Move(file.Path, destFilePath);
                    var fi = new FileInf(destFilePath, Id);
                    Files.Add(fi);
                    Workflow.FilesPerTask[file.TaskId].Remove(file);
                    InfoFormat("File moved: {0} -> {1}", file.Path, destFilePath);
                    if (!atLeastOneSucceed) atLeastOneSucceed = true;
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                { 
                    ErrorFormat("An error occured while moving the file {0} to {1}", e, file.Path, destFilePath);
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
    }
}
