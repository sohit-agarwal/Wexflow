using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Threading;

namespace Wexflow.Tasks.Mkdir
{
    public class Mkdir:Task
    {
        public string[] Folders { get; private set; }

        public Mkdir(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            this.Folders = this.GetSettings("folder");
        }

        public override TaskStatus Run()
        {
            this.Info("Creating folders...");
            
            bool success = true;
            bool atLeastOneSucceed = false;

            foreach (string folder in this.Folders)
            {
                try
                {
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                    this.InfoFormat("Folder {0} created.", folder);
                    success &= true;
                    if (!atLeastOneSucceed) atLeastOneSucceed = true;
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    this.ErrorFormat("An error occured while creating the folder {0}", e, folder);
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
