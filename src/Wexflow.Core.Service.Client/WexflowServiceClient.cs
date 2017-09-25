using Wexflow.Core.Service.Contracts;
using System.Net;
using Newtonsoft.Json;

namespace Wexflow.Core.Service.Client
{
    public class WexflowServiceClient
    {
        public string Uri { get; }

        public WexflowServiceClient(string uri)
        {
            Uri = uri.TrimEnd('/');
        }

        public WorkflowInfo[] GetWorkflows()
        {
            string uri = Uri + "/workflows";
            var webClient = new WebClient();
            webClient.Encoding = System.Text.Encoding.UTF8;
            var response = webClient.DownloadString(uri);
            var workflows = JsonConvert.DeserializeObject<WorkflowInfo[]>(response);
            return workflows;
        }

        public void StartWorkflow(int id)
        {
            string uri = Uri + "/start/" + id;
            var webClient = new WebClient();
            webClient.UploadString(uri, string.Empty);
        }

        public void StopWorkflow(int id)
        {
            string uri = Uri + "/stop/" + id;
            var webClient = new WebClient();
            webClient.UploadString(uri, string.Empty);
        }

        public void SuspendWorkflow(int id)
        {
            string uri = Uri + "/suspend/" + id;
            var webClient = new WebClient();
            webClient.UploadString(uri, string.Empty);
        }

        public void ResumeWorkflow(int id)
        {
            string uri = Uri + "/resume/" + id;
            var webClient = new WebClient();
            webClient.UploadString(uri, string.Empty);
        }

        public WorkflowInfo GetWorkflow(int id)
        {
            string uri = Uri + "/workflow/" + id;
            var webClient = new WebClient();
            var response = webClient.DownloadString(uri);
            var workflow = JsonConvert.DeserializeObject<WorkflowInfo>(response);
            return workflow;
        }
    }
}
