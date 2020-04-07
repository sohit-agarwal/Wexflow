using System;
using System.Collections.Generic;
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
        Stop,
        Approve,
        Reject
    }

    public class Workflow : Task
    {
        public string WexflowWebServiceUri { get; }
        public string Username { get; }
        public string Password { get; }
        public WorkflowAction Action { get; }
        public int[] WorkflowIds { get; }
        public Dictionary<int, Guid> Jobs { get; }

        public Workflow(XElement xe, Core.Workflow wf) : base(xe, wf)
        {
            Jobs = new Dictionary<int, Guid>();
            WexflowWebServiceUri = GetSetting("wexflowWebServiceUri");
            Username = GetSetting("username");
            Password = GetSetting("password");
            Action = (WorkflowAction)Enum.Parse(typeof(WorkflowAction), GetSetting("action"), true);
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
                WorkflowInfo wfInfo = client.GetWorkflow(Username, Password, id);
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
                            var instanceId = client.StartWorkflow(id, Username, Password);
                            if (Jobs.ContainsKey(id))
                            {
                                Jobs[id] = instanceId;
                            }
                            else
                            {
                                Jobs.Add(id, instanceId);
                            }
                            InfoFormat("Workflow {0} started.", id);
                            if (!atLeastOneSucceed) atLeastOneSucceed = true;
                        }
                        break;
                    case WorkflowAction.Suspend:
                        if (wfInfo.IsRunning)
                        {
                            client.SuspendWorkflow(id, Jobs[id], Username, Password);
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
                            client.ResumeWorkflow(id, Jobs[id], Username, Password);
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
                            client.StopWorkflow(id, Jobs[id], Username, Password);
                            InfoFormat("Workflow {0} stopped.", id);
                            if (!atLeastOneSucceed) atLeastOneSucceed = true;
                        }
                        else
                        {
                            success = false;
                            ErrorFormat("Can't stop the workflow {0} because it's not running.", Workflow.Id);
                        }
                        break;
                    case WorkflowAction.Approve:
                        if (wfInfo.IsApproval && wfInfo.IsWaitingForApproval)
                        {
                            client.ApproveWorkflow(id, Jobs[id], Username, Password);
                            InfoFormat("Workflow {0} approved.", id);
                            if (!atLeastOneSucceed) atLeastOneSucceed = true;
                        }
                        else
                        {
                            success = false;
                            ErrorFormat("Can't approve the workflow {0} because it's not waiting for approval.", Workflow.Id);
                        }
                        break;
                    case WorkflowAction.Reject:
                        if (wfInfo.IsApproval && wfInfo.IsWaitingForApproval)
                        {
                            client.RejectWorkflow(id, Jobs[id], Username, Password);
                            InfoFormat("Workflow {0} rejected.", id);
                            if (!atLeastOneSucceed) atLeastOneSucceed = true;
                        }
                        else
                        {
                            success = false;
                            ErrorFormat("Can't reject the workflow {0} because it's not waiting for approval.", Workflow.Id);
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
