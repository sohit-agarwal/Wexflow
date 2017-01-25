using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using System.Threading;

namespace Wexflow.Tasks.FileExists
{
    public class FileExists:Task
    {
        public string File { get; private set; }

        public FileExists(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            this.File = this.GetSetting("file");
        }

        public override TaskStatus Run()
        {
            this.Info("Checking file...");
            
            bool success = false;

            try
            {
                success = System.IO.File.Exists(this.File);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                this.InfoFormat("An error occured while checking file {0}. Error: {1}", this.File, e.Message);
                return new TaskStatus(Status.Error, false);
            }

            this.Info("Task finished");

            return new TaskStatus(Status.Success, success);
        }
    }
}
