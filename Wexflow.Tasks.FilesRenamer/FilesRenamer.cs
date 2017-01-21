using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using System.Threading;
using System.IO;

namespace Wexflow.Tasks.FilesRenamer
{
    public class FilesRenamer:Task
    {
        public bool Overwrite { get; private set; }

        public FilesRenamer(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            this.Overwrite = bool.Parse(this.GetSetting("overwrite", "false"));
        }

        public override void Run()
        {
            this.Info("Renaming files...");

            foreach (var file in this.SelectFiles())
            {
                try
                {
                    if (!string.IsNullOrEmpty(file.RenameTo))
                    {
                        string destPath = Path.Combine(Path.GetDirectoryName(file.Path), file.RenameTo);

                        if (File.Exists(destPath))
                        {
                            if (this.Overwrite)
                            {
                                if (file.Path != destPath)
                                {
                                    File.Delete(destPath);
                                }
                                else
                                {
                                    this.InfoFormat("The file {0} and its new name {1} are the same.", file.Path, file.RenameTo);
                                    continue;
                                }
                            }
                            else
                            {
                                this.ErrorFormat("The destination file {0} already exists.", destPath);
                                continue;
                            }
                        }

                        File.Move(file.Path, destPath);
                        this.InfoFormat("File {0} renamed to {1}", file.Path, file.RenameTo);
                        file.Path = destPath;
                        file.RenameTo = string.Empty;
                    }
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    this.ErrorFormat("An error occured while renaming the file {0} to {1}. Error: {2}", file.Path, file.RenameTo, e.Message);
                }
            }

            this.Info("Task finished.");
        }
    }
}
