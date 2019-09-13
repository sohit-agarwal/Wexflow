using Wexflow.Core.Service.Contracts;
using System.Net;
using Newtonsoft.Json;
using System.Security;

namespace Wexflow.Core.Service.Client
{
    public class WexflowServiceClient
    {
        public string Uri { get; private set; }

        public WexflowServiceClient(string uri)
        {
            Uri = uri.TrimEnd('/');
        }

        public WorkflowInfo[] Search(string keyword, string username, string password)
        {
            string uri = Uri + "/search?s=" + keyword + "&u=" + SecurityElement.Escape(username) + "&p=" + SecurityElement.Escape(password);
            var webClient = new WebClient { Encoding = System.Text.Encoding.UTF8 };
            var response = webClient.DownloadString(uri);
            var workflows = JsonConvert.DeserializeObject<WorkflowInfo[]>(response);
            return workflows;
        }

        public void StartWorkflow(int id, string username, string password)
        {
            string uri = Uri + "/start?w=" + id + "&u=" + SecurityElement.Escape(username) + "&p=" + SecurityElement.Escape(password);
            var webClient = new WebClient();
            webClient.UploadString(uri, string.Empty);
        }

        public void StopWorkflow(int id, string username, string password)
        {
            string uri = Uri + "/stop?w=" + id + "&u=" + SecurityElement.Escape(username) + "&p=" + SecurityElement.Escape(password);
            var webClient = new WebClient();
            webClient.UploadString(uri, string.Empty);
        }

        public void SuspendWorkflow(int id, string username, string password)
        {
            string uri = Uri + "/suspend?w=" + id + "&u=" + SecurityElement.Escape(username) + "&p=" + SecurityElement.Escape(password);
            var webClient = new WebClient();
            webClient.UploadString(uri, string.Empty);
        }

        public void ResumeWorkflow(int id, string username, string password)
        {
            string uri = Uri + "/resume?w=" + id + "&u=" + SecurityElement.Escape(username) + "&p=" + SecurityElement.Escape(password);
            var webClient = new WebClient();
            webClient.UploadString(uri, string.Empty);
        }

        public void ApproveWorkflow(int id, string username, string password)
        {
            string uri = Uri + "/approve?w=" + id + "&u=" + SecurityElement.Escape(username) + "&p=" + SecurityElement.Escape(password);
            var webClient = new WebClient();
            webClient.UploadString(uri, string.Empty);
        }

        public void DisapproveWorkflow(int id, string username, string password)
        {
            string uri = Uri + "/disapprove?w=" + id + "&u=" + SecurityElement.Escape(username) + "&p=" + SecurityElement.Escape(password);
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

        public User GetUser(string username)
        {
            string uri = Uri + "/user?username=" + username;
            var webClient = new WebClient();
            var response = webClient.DownloadString(uri);
            var user = JsonConvert.DeserializeObject<User>(response);
            return user;
        }

    }
}
