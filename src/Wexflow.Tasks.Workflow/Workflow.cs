using System;
using System.Xml.Linq;
using Wexflow.Core;
using Wexflow.Core.Service.Client;
using Wexflow.Core.Service.Contracts;

namespace Wexflow.Tasks.Workflow
{
    public enum WorkflowAction
    {
        Start,
        Suspend,
        Resume,
        Stop
    }

    public class Workflow:Task
    {
        public string WexflowWebServiceUri { get; private set; }
        public WorkflowAction Action { get; private set; }
        public int[] WorkflowIds { get; private set; }

        public Workflow(XElement xe, Core.Workflow wf) : base(xe, wf)
        {
            WexflowWebServiceUri = GetSetting("wexflowWebServiceUri");
            Action = (WorkflowAction) Enum.Parse(typeof(WorkflowAction), GetSetting("action"), true);
            WorkflowIds = GetSettingsInt("id");
        }

        public override TaskStatus Run()
        {
            Info("Task started.");
            bool success = true;
            bool atLeastOneSucceed = false;
            foreach (var id in WorkflowIds)
            {
                WexflowServiceClient client = new WexflowServiceClient(WexflowWebServiceUri);
                WorkflowInfo wfInfo = client.GetWorkflow(id);
                switch (Action)
                {
                    case WorkflowAction.Start:
                        if (wfInfo.IsRunning)
                        {
                            success = false;
                            ErrorFormat("Can't start the workflow {0} because it's already running.", Workflow.Id);
                        }
                        else
                        {
                            client.StartWorkflow(id);
                            InfoFormat("Workflow {0} started.", id);
                            if (!atLeastOneSucceed) atLeastOneSucceed = true;
                        }
                        break;
                    case WorkflowAction.Suspend:
                        if (wfInfo.IsRunning)
                        {
                            client.SuspendWorkflow(id);
                            InfoFormat("Workflow {0} suspended.", id);
                            if (!atLeastOneSucceed) atLeastOneSucceed = true;
                        }
                        else
                        {
                            success = false;
                            ErrorFormat("Can't suspend the workflow {0} because it's not running.", Workflow.Id);   
                        }
                        break;
                    case WorkflowAction.Resume:
                        if (wfInfo.IsPaused)
                        {
                            client.ResumeWorkflow(id);
                            InfoFormat("Workflow {0} resumed.", id);
                            if (!atLeastOneSucceed) atLeastOneSucceed = true;
                        }
                        else
                        {
                            success = false;
                            ErrorFormat("Can't resume the workflow {0} because it's not suspended.", Workflow.Id);
                        }
                        break;
                    case WorkflowAction.Stop:
                        if (wfInfo.IsRunning)
                        {
                            client.StopWorkflow(id);
                            InfoFormat("Workflow {0} stopped.", id);
                            if (!atLeastOneSucceed) atLeastOneSucceed = true;
                        }
                        else
                        {
                            success = false;
                            ErrorFormat("Can't stop the workflow {0} because it's not running.", Workflow.Id);
                        }
                        break;
                }
            }
            Info("Task finished.");
            var status = Core.Status.Success;

            if (!success && atLeastOneSucceed)
            {
                status = Core.Status.Warning;
            }
            else if (!success)
            {
                status = Core.Status.Error;
            }
            
            return new TaskStatus(status);
        }
    }
}
