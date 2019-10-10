using Newtonsoft.Json;
using System.Net;
using Wexflow.Core.Service.Contracts;

namespace Wexflow.Core.Service.Client
{
    public class WexflowServiceClient
    {
        public string Uri { get; private set; }

        public WexflowServiceClient(string uri)
        {
            Uri = uri.TrimEnd('/');
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public WorkflowInfo[] Search(string keyword, string username, string password)
        {
            string uri = Uri + "/search?s=" + keyword;
            var webClient = new WebClient { Encoding = System.Text.Encoding.UTF8 };
            webClient.Headers.Add("Authorization", Base64Encode(username + ":" + password));
            var response = webClient.DownloadString(uri);
            var workflows = JsonConvert.DeserializeObject<WorkflowInfo[]>(response);
            return workflows;
        }

        public void StartWorkflow(int id, string username, string password)
        {
            string uri = Uri + "/start?w=" + id;
            var webClient = new WebClient();
            webClient.Headers.Add("Authorization", Base64Encode(username + ":" + password));
            webClient.UploadString(uri, string.Empty);
        }

        public void StopWorkflow(int id, string username, string password)
        {
            string uri = Uri + "/stop?w=" + id;
            var webClient = new WebClient();
            webClient.Headers.Add("Authorization", Base64Encode(username + ":" + password));
            webClient.UploadString(uri, string.Empty);
        }

        public void SuspendWorkflow(int id, string username, string password)
        {
            string uri = Uri + "/suspend?w=" + id;
            var webClient = new WebClient();
            webClient.Headers.Add("Authorization", Base64Encode(username + ":" + password));
            webClient.UploadString(uri, string.Empty);
        }

        public void ResumeWorkflow(int id, string username, string password)
        {
            string uri = Uri + "/resume?w=" + id;
            var webClient = new WebClient();
            webClient.Headers.Add("Authorization", Base64Encode(username + ":" + password));
            webClient.UploadString(uri, string.Empty);
        }

        public void ApproveWorkflow(int id, string username, string password)
        {
            string uri = Uri + "/approve?w=" + id;
            var webClient = new WebClient();
            webClient.Headers.Add("Authorization", Base64Encode(username + ":" + password));
            webClient.UploadString(uri, string.Empty);
        }

        public void DisapproveWorkflow(int id, string username, string password)
        {
            string uri = Uri + "/disapprove?w=" + id;
            var webClient = new WebClient();
            webClient.Headers.Add("Authorization", Base64Encode(username + ":" + password));
            webClient.UploadString(uri, string.Empty);
        }

        public WorkflowInfo GetWorkflow(string username, string password, int id)
        {
            string uri = Uri + "/workflow?w=" + id;
            var webClient = new WebClient();
            webClient.Headers.Add("Authorization", Base64Encode(username + ":" + password));
            var response = webClient.DownloadString(uri);
            var workflow = JsonConvert.DeserializeObject<WorkflowInfo>(response);
            return workflow;
        }

        public User GetUser(string qusername, string qpassword, string username)
        {
            string uri = Uri + "/user?username=" + username;
            var webClient = new WebClient();
            webClient.Headers.Add("Authorization", Base64Encode(qusername + ":" + qpassword));
            var response = webClient.DownloadString(uri);
            var user = JsonConvert.DeserializeObject<User>(response);
            return user;
        }

    }
}
