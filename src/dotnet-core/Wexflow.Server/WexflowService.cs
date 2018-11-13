using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using Newtonsoft.Json;
using Wexflow.Server.Contracts;

namespace Wexflow.Server
{
    public sealed class WexflowService : NancyModule
    {
        private static readonly string Root = "/wexflow/";

        public WexflowService(IAppConfiguration appConfig)
        {
            Hello();
            GetWorkflows();
            Search();
            GetWorkflow();
            StartWorkflow();
        }

        private void Hello()
        {
            Get(Root, args => "Hello from Wexflow workflow engine running on CoreCLR");
        }

        /// <summary>
        /// Returns the list of workflows.
        /// </summary>
        private void GetWorkflows()
        {
            Get(Root + "workflows", args =>
            {
                var workflows = Program.WexflowEngine.Workflows.Select(wf => new WorkflowInfo(wf.Id, wf.Name,
                        (LaunchType) wf.LaunchType, wf.IsEnabled, wf.Description, wf.IsRunning, wf.IsPaused,
                        wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression, wf.WorkflowFilePath,
                        wf.IsExecutionGraphEmpty))
                    .ToArray();
                var workflowsStr = JsonConvert.SerializeObject(workflows);
                var workflowsBytes = Encoding.UTF8.GetBytes(workflowsStr);

                return new Response()
                {
                    ContentType = "application/json",
                    Contents = s => s.Write(workflowsBytes, 0, workflowsBytes.Length)
                };
            });
        }

        /// <summary>
        /// Search for workflows.
        /// </summary>
        private void Search()
        {
            Get(Root + "search", args =>
            {
                string keywordToUpper = Request.Query["s"].ToString().ToUpper();
                var workflows = Program.WexflowEngine.Workflows
                    .Where(wf =>
                        wf.Name.ToUpper().Contains(keywordToUpper) || wf.Description.ToUpper().Contains(keywordToUpper))
                    .Select(wf => new WorkflowInfo(wf.Id, wf.Name,
                        (LaunchType) wf.LaunchType, wf.IsEnabled, wf.Description, wf.IsRunning, wf.IsPaused,
                        wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression, wf.WorkflowFilePath,
                        wf.IsExecutionGraphEmpty))
                    .ToArray();
                var workflowsStr = JsonConvert.SerializeObject(workflows);
                var workflowsBytes = Encoding.UTF8.GetBytes(workflowsStr);

                return new Response()
                {
                    ContentType = "application/json",
                    Contents = s => s.Write(workflowsBytes, 0, workflowsBytes.Length)
                };
            });
        }

        /// <summary>
        /// Returns a workflow from its id.
        /// </summary>
        private void GetWorkflow()
        {
            Get(Root + "workflow/{id}", args =>
            {
                var wf = Program.WexflowEngine.GetWorkflow(args.id);
                if (wf != null)
                {
                    var workflow = new WorkflowInfo(wf.Id, wf.Name, (LaunchType)wf.LaunchType, wf.IsEnabled, wf.Description,
                        wf.IsRunning, wf.IsPaused, wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression,
                        wf.WorkflowFilePath, wf.IsExecutionGraphEmpty);
                    var workflowStr = JsonConvert.SerializeObject(workflow);
                    var workflowBytes = Encoding.UTF8.GetBytes(workflowStr);

                    return new Response()
                    {
                        ContentType = "application/json",
                        Contents = s => s.Write(workflowBytes, 0, workflowBytes.Length)
                    };
                }

                return new Response()
                {
                    ContentType = "application/json"
                };
            });
        }

        /// <summary>
        /// Starts a workflow.
        /// </summary>
        private void StartWorkflow()
        {
            Post(Root + "start/{id}", args =>
            {
                Program.WexflowEngine.StartWorkflow(args.id);
                
                return new Response
                {
                    ContentType = "application/json"
                };
            });
        }

    }
}