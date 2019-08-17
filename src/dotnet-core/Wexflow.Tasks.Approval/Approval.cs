using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.Approval
{
    public class Approval : Task
    {
        public Approval(XElement xe, Workflow wf) : base(xe, wf)
        {
        }

        public override TaskStatus Run()
        {
            Info("Approval process starting...");

            var status = Status.Success;

            try
            {
                var trigger = Path.Combine(Workflow.ApprovalFolder, Workflow.Id.ToString(), Id.ToString(), "task.approved");

                IsWaitingForApproval = true;
                Workflow.IsWaitingForApproval = true;

                while (!File.Exists(trigger))
                {
                    Thread.Sleep(500);
                }

                IsWaitingForApproval = false;
                Workflow.IsWaitingForApproval = false;
                InfoFormat("Task approved: {0}", trigger);
                File.Delete(trigger);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                Error("An error occured during approval process.", e);
                status = Status.Error;
            }

            Info("Approval process finished.");
            return new TaskStatus(status);
        }
    }
}
