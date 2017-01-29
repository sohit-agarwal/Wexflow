using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Configuration;
using Wexflow.Core.Service.Contracts;
using Nancy;
using Newtonsoft.Json;

namespace Wexflow.Clients.Cmd
{
    public class WexflowService : NancyModule, IWexflowService
    {
        public WexflowService() 
        {
            Get["workflows"] = parameters => GetWorkflows();
            Post["start/{id}"] = parameters => StartWorkflow(parameters["id"]);
            Post["stop/{id}"] = parameters => StopWorkflow(parameters["id"]);
            Post["suspend/{id}"] = parameters => SuspendWorkflow(parameters["id"]);
            Post["resume/{id}"] = parameters => ResumeWorkflow(parameters["id"]);
            Get["workflow/{id}"] = parameters => GetWorkflow(parameters["id"]);
        }

        private void AddCrossHeaders(Response response) 
        {
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Request-Method", "POST,GET,PUT,DELETE,OPTIONS");
            response.Headers.Add("Access-Control-Allow-Headers", "X-Requested-With,Content-Type");
        }

        // /workflows
        public Response GetWorkflows()
        {
            List<WorkflowInfo> wfis = new List<WorkflowInfo>();
            foreach (Workflow wf in Program.WexflowEngine.Workflows)
            {
                wfis.Add(new WorkflowInfo(wf.Id, wf.Name, (Core.Service.Contracts.LaunchType)wf.LaunchType, wf.IsEnabled, wf.Description, wf.IsRunning, wf.IsPaused));
            }
            string json = JsonConvert.SerializeObject(wfis);
            var response = (Response)json;
            response.ContentType = "application/json";
            AddCrossHeaders(response);
            return response;
        }

        // start/{id}
        public Response StartWorkflow(string id)
        {
            Program.WexflowEngine.StartWorkflow(int.Parse(id));
            var response = (Response)string.Empty;
            AddCrossHeaders(response);
            return response;
        }

        // stop/{id}
        public Response StopWorkflow(string id)
        {
            Program.WexflowEngine.StopWorkflow(int.Parse(id));
            var response = (Response)string.Empty;
            AddCrossHeaders(response);
            return response;
        }

        // suspend/{id}
        public Response SuspendWorkflow(string id)
        {
            Program.WexflowEngine.PauseWorkflow(int.Parse(id));
            var response = (Response)string.Empty;
            AddCrossHeaders(response);
            return response;
        }

        // resume/{id}
        public Response ResumeWorkflow(string id)
        {
            Program.WexflowEngine.ResumeWorkflow(int.Parse(id));
            var response = (Response)string.Empty;
            AddCrossHeaders(response);
            return response;
        }

        // workflow/{id}
        public Response GetWorkflow(string id)
        {
            Workflow wf = Program.WexflowEngine.GetWorkflow(int.Parse(id));
            WorkflowInfo wfi = new WorkflowInfo(wf.Id, wf.Name, (Core.Service.Contracts.LaunchType)wf.LaunchType, wf.IsEnabled, wf.Description, wf.IsRunning, wf.IsPaused);
            string json = JsonConvert.SerializeObject(wfi);
            var response = (Response)json;
            response.ContentType = "application/json";
            AddCrossHeaders(response);
            return response;
        }
    }
}
