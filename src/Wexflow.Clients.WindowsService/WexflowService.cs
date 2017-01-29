using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Configuration;
using System.ServiceModel;
using Wexflow.Core.Service.Contracts;
using System.ServiceModel.Web;

namespace Wexflow.Clients.WindowsService
{
    [ServiceBehavior(IncludeExceptionDetailInFaults=true)]
    public class WexflowService:IWexflowService
    {
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "workflows")]
        public WorkflowInfo[] GetWorkflows()
        {
            List<WorkflowInfo> wfis = new List<WorkflowInfo>();
            foreach (Workflow wf in WexflowWindowsService.WexflowEngine.Workflows)
            {
                wfis.Add(new WorkflowInfo(wf.Id, wf.Name, GetLaunchType(wf.LaunchType), wf.IsEnabled, wf.Description, wf.IsRunning, wf.IsPaused));
            }
            return wfis.ToArray();
        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "start/{id}")]
        public void StartWorkflow(string id)
        {
            WexflowWindowsService.WexflowEngine.StartWorkflow(int.Parse(id));
        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "stop/{id}")]
        public void StopWorkflow(string id)
        {
            WexflowWindowsService.WexflowEngine.StopWorkflow(int.Parse(id));
        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "suspend/{id}")]
        public void SuspendWorkflow(string id)
        {
            WexflowWindowsService.WexflowEngine.PauseWorkflow(int.Parse(id));
        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "resume/{id}")]
        public void ResumeWorkflow(string id)
        {
            WexflowWindowsService.WexflowEngine.ResumeWorkflow(int.Parse(id));
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "workflow/{id}")]
        public WorkflowInfo GetWorkflow(string id)
        {
            Workflow wf = WexflowWindowsService.WexflowEngine.GetWorkflow(int.Parse(id));
            return new WorkflowInfo(wf.Id, wf.Name, GetLaunchType(wf.LaunchType), wf.IsEnabled, wf.Description, wf.IsRunning, wf.IsPaused);
        }

        private Core.Service.Contracts.LaunchType GetLaunchType(Core.LaunchType launchType)
        {
            Core.Service.Contracts.LaunchType lt = Core.Service.Contracts.LaunchType.Trigger;

            switch (launchType)
            {
                case Core.LaunchType.Periodic:
                    lt = Core.Service.Contracts.LaunchType.Periodic;
                    break;
                case Core.LaunchType.Startup:
                    lt = Core.Service.Contracts.LaunchType.Startup;
                    break;
                case Core.LaunchType.Trigger:
                    lt = Core.Service.Contracts.LaunchType.Trigger;
                    break;
            }

            return lt;
        }
    }
}
