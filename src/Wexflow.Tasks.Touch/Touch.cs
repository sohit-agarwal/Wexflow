using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Threading;

namespace Wexflow.Tasks.Touch
{
    public class Touch:Task
    {
        public string[] TFiles { get; private set; }

        public Touch(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            this.TFiles = this.GetSettings("file");
        }

        public override TaskStatus Run()
        {
            this.Info("Touching files...");

            bool success = true;
            bool atLeastOneSucceed = false;

            foreach (string file in this.TFiles)
            {
                try
                {
                    TouchFile(file);
                    this.InfoFormat("File {0} created.", file);
                    this.Files.Add(new FileInf(file, this.Id));
                    success &= true;
                    if (!atLeastOneSucceed) atLeastOneSucceed = true;
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    this.ErrorFormat("An error occured while creating the file {0}", e, file);
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

        private void TouchFile(string file)
        {
            using (File.Create(file)) { }
        }
    }
}
