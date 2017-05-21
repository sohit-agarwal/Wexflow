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
        public string WexflowWebServiceUri { get; }
        public WorkflowAction Action { get; }
        public int[] WorkflowIds { get; }

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
                            ErrorFormat("Can't start the workflow {0} because it's already running.");
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
                            ErrorFormat("Can't suspend the workflow {0} because it's not running.");   
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
                            ErrorFormat("Can't resume the workflow {0} because it's not suspended.");
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
                            ErrorFormat("Can't stop the workflow {0} because it's not running.");
                        }
                        break;
                }
            }
            Info("Task finished.");
            var status = Status.Success;

            if (!success && atLeastOneSucceed)
            {
                status = Status.Warning;
            }
            else if (!success)
            {
                status = Status.Error;
            }
            
            return new TaskStatus(status);
        }
    }
}
