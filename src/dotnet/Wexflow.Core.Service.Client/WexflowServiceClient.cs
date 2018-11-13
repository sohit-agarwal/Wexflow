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
            Uri = uri.TrimEnd('/');
        }

        public WorkflowInfo[] GetWorkflows()
        {
            string uri = Uri + "/workflows";
            var webClient = new WebClient {Encoding = System.Text.Encoding.UTF8};
            var response = webClient.DownloadString(uri);
            var workflows = JsonConvert.DeserializeObject<WorkflowInfo[]>(response);
            return workflows;
        }

        public WorkflowInfo[] Search(string keyword)
        {
            string uri = Uri + "/search?s=" + keyword;
            var webClient = new WebClient { Encoding = System.Text.Encoding.UTF8 };
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

        public StatusCount GetStatusCount()
        {
            string uri = Uri + "/statusCount/";
            var webClient = new WebClient();
            var response = webClient.DownloadString(uri);
            var statusCount = JsonConvert.DeserializeObject<StatusCount>(response);
            return statusCount;
        }

        public Entry[] GetEntries()
        {
            string uri = Uri + "/entries/";
            var webClient = new WebClient();
            var response = webClient.DownloadString(uri);
            var entries = JsonConvert.DeserializeObject<Entry[]>(response);
            return entries;
        }

        public User GetUser(string username)
        {
            string uri = Uri + "/user?username=" + username;
            var webClient = new WebClient();
            var response = webClient.DownloadString(uri);
            var user = JsonConvert.DeserializeObject<User>(response);
            return user;
        }

        public void InsertUser(string username, string password)
        {
            string uri = Uri + "/insertUser?username=" + username + "&password=" + password;
            var webClient = new WebClient();
            webClient.UploadString(uri, string.Empty);
        }

        public HistoryEntry[] GetHistoryEntries()
        {
            string uri = Uri + "/historyEntries/";
            var webClient = new WebClient();
            var response = webClient.DownloadString(uri);
            var entries = JsonConvert.DeserializeObject<HistoryEntry[]>(response);
            return entries;
        }

        public HistoryEntry[] GetHistoryEntries(string keyword)
        {
            string uri = Uri + "/searchHistoryEntries?s=" + keyword;
            var webClient = new WebClient();
            var response = webClient.DownloadString(uri);
            var entries = JsonConvert.DeserializeObject<HistoryEntry[]>(response);
            return entries;
        }

        public HistoryEntry[] GetHistoryEntries(string keyword, int page, int entriesCount)
        {
            string uri = Uri + "/searchHistoryEntriesByPage?s=" + keyword + "&page=" + page + "&entriesCount=" + entriesCount;
            var webClient = new WebClient();
            var response = webClient.DownloadString(uri);
            var entries = JsonConvert.DeserializeObject<HistoryEntry[]>(response);
            return entries;
        }

        public long GetHistoryEntriesCount(string keyword)
        {
            string uri = Uri + "/historyEntriesCount?s=" + keyword;
            var webClient = new WebClient();
            var response = webClient.DownloadString(uri);
            var count = JsonConvert.DeserializeObject<long>(response);
            return count;
        }

    }
}
