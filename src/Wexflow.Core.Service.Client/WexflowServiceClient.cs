using Wexflow.Core.Service.Contracts;
using System.Net;
using Newtonsoft.Json;

namespace Wexflow.Core.Service.Client
{
    public class WexflowServiceClient
    {
        public string Uri { get; private set; }

        private WebClient _webClient;

        public WexflowServiceClient(string uri)
        {
            this.Uri = uri.TrimEnd('/');
            this._webClient = new WebClient();
        }

        public WorkflowInfo[] GetWorkflows()
        {
            string uri = this.Uri + "/workflows";
            string response = this._webClient.DownloadString(uri);
            WorkflowInfo[] workflows = JsonConvert.DeserializeObject<WorkflowInfo[]>(response);
            return workflows;
        }

        public void StartWorkflow(int id)
        {
            string uri = this.Uri + "/start/" + id;
            this._webClient.UploadString(uri, string.Empty);
        }

        public void StopWorkflow(int id)
        {
            string uri = this.Uri + "/strop/" + id;
            this._webClient.UploadString(uri, string.Empty);
        }

        public void SuspendWorkflow(int id)
        {
            string uri = this.Uri + "/suspend/" + id;
            this._webClient.UploadString(uri, string.Empty);
        }

        public void ResumeWorkflow(int id)
        {
            string uri = this.Uri + "/resume/" + id;
            this._webClient.UploadString(uri, string.Empty);
        }

        public WorkflowInfo GetWorkflow(int id)
        {
            string uri = this.Uri + "/workflow/" + id;
            string response = this._webClient.DownloadString(uri);
            WorkflowInfo workflow = JsonConvert.DeserializeObject<WorkflowInfo>(response);
            return workflow;
        }
    }
}
