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

        public override void Run()
        {
            this.Info("Waiting...");
            Thread.Sleep(this.Duration);
            this.Info("Task finished.");
        }
    }
}
