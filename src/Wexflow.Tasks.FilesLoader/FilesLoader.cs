using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using System.Threading;
using System.IO;

namespace Wexflow.Tasks.FilesLoader
{
    public class FilesLoader : Task
    {
        public string[] Folders { get; private set; }
        public string[] FLFiles { get; private set; }

        public FilesLoader(XElement xe, Workflow wf): base(xe, wf)
        {
            this.Folders = this.GetSettings("folder");
            this.FLFiles = this.GetSettings("file");
        }

        public override TaskStatus Run()
        {
            this.Info("Loading files...");

            bool success = true;

            try
            {
                foreach (string folder in this.Folders)
                {
                    foreach (string file in Directory.GetFiles(folder))
                    {
                        FileInf fi = new FileInf(file, this.Id);
                        this.Files.Add(fi);
                        this.InfoFormat("File loaded: {0}", file);
                        success &= true;
                    }
                }

                foreach (string file in this.FLFiles)
                {
                    if (File.Exists(file))
                    {
                        this.Files.Add(new FileInf(file, this.Id));
                        this.InfoFormat("File loaded: {0}", file);
                        success &= true;
                    }
                    else
                    {
                        this.ErrorFormat("File not found: {0}", file);
                        success &= false;
                    }
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                this.ErrorFormat("An error occured while loading files.", e);
                success &= false;
            }

            Status status = Status.Success;

            if (!success)
            {
                status = Status.Error;
            }

            this.Info("Task finished.");
            return new TaskStatus(status, false);

        }
    }
}
