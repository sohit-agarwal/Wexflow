using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            this.DestFolder = this.GetSetting("destFolder");
            this.Overwrite = bool.Parse(this.GetSetting("overwrite", "false"));
        }

        public override TaskStatus Run()
        {
            this.Info("Moving files...");

            bool success = true;
            bool atLeastOneSucceed = false;

            FileInf[] files = this.SelectFiles();
            for (int i = files.Length - 1; i > -1 ; i--) 
            {
                FileInf file = files[i];
                string destFilePath = Path.Combine(this.DestFolder, Path.GetFileName(file.Path));

                try
                {
                    if (File.Exists(destFilePath))
                    {
                        if (this.Overwrite)
                        {
                            File.Delete(destFilePath);
                        }
                        else
                        {
                            this.ErrorFormat("Destination file {0} already exists.", destFilePath);
                            success &= false;
                            continue;
                        }
                    }

                    File.Move(file.Path, destFilePath);
                    FileInf fi = new FileInf(destFilePath, this.Id);
                    this.Files.Add(fi);
                    this.Workflow.FilesPerTask[file.TaskId].Remove(file);
                    this.InfoFormat("File moved: {0} -> {1}", file.Path, destFilePath);
                    success &= true;
                    if (!atLeastOneSucceed) atLeastOneSucceed = true;
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                { 
                    this.ErrorFormat("An error occured while moving the file {0} to {1}", e, file.Path, destFilePath);
                    success &= false;
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

            this.Info("Task finished.");
            return new TaskStatus(status, false);
        }
    }
}
