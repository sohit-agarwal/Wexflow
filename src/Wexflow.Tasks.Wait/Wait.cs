using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using System.Threading;

namespace Wexflow.Tasks.Wait
{
    public class Wait:Task
    {
        public TimeSpan Duration { get; private set; }

        public Wait(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            this.Duration = TimeSpan.Parse(this.GetSetting("duration"));
        }

        public override TaskStatus Run()
        {
            this.InfoFormat("Waiting for {0} ...", this.Duration);

            bool success = true;

            try
            {
                Thread.Sleep(this.Duration);
                success &= true;
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                this.ErrorFormat("An error occured while waiting. Error: {0}", e.Message);
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
