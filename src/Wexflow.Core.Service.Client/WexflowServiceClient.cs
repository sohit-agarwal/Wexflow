using Wexflow.Core.Service.Contracts;
using System.Net;
using Newtonsoft.Json;

namespace Wexflow.Core.Service.Client
{
    public class WexflowServiceClient
    {
        public string Uri { get; private set; }

        public WexflowServiceClient(string uri)
        {
            this.Uri = uri.TrimEnd('/');
        }

        public WorkflowInfo[] GetWorkflows()
        {
            string uri = this.Uri + "/workflows";
            WebClient webClient = new WebClient();
            string response = webClient.DownloadString(uri);
            WorkflowInfo[] workflows = JsonConvert.DeserializeObject<WorkflowInfo[]>(response);
            return workflows;
        }

        public void StartWorkflow(int id)
        {
            string uri = this.Uri + "/start/" + id;
            WebClient webClient = new WebClient();
            webClient.UploadString(uri, string.Empty);
        }

        public void StopWorkflow(int id)
        {
            string uri = this.Uri + "/stop/" + id;
            WebClient webClient = new WebClient();
            webClient.UploadString(uri, string.Empty);
        }

        public void SuspendWorkflow(int id)
        {
            string uri = this.Uri + "/suspend/" + id;
            WebClient webClient = new WebClient();
            webClient.UploadString(uri, string.Empty);
        }

        public void ResumeWorkflow(int id)
        {
            string uri = this.Uri + "/resume/" + id;
            WebClient webClient = new WebClient();
            webClient.UploadString(uri, string.Empty);
        }

        public WorkflowInfo GetWorkflow(int id)
        {
            string uri = this.Uri + "/workflow/" + id;
            WebClient webClient = new WebClient();
            string response = webClient.DownloadString(uri);
            WorkflowInfo workflow = JsonConvert.DeserializeObject<WorkflowInfo>(response);
            return workflow;
        }
    }
}
