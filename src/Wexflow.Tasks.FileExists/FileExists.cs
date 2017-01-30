using System;
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
            File = GetSetting("file");
        }

        public override TaskStatus Run()
        {
            Info("Checking file...");
            
            bool success;

            try
            {
                success = System.IO.File.Exists(File);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                InfoFormat("An error occured while checking file {0}. Error: {1}", File, e.Message);
                return new TaskStatus(Status.Error, false);
            }

            Info("Task finished");

            return new TaskStatus(Status.Success, success);
        }
    }
}
