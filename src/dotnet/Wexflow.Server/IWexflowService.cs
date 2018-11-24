using System.IO;
using System.ServiceModel;
using Wexflow.Core.Service.Contracts;

namespace Wexflow.Server
{
    [ServiceContract(Namespace = "http://wexflow.com/")]
    public interface IWexflowService
    {
        [OperationContract]
        WorkflowInfo[] GetWorkflows();

        [OperationContract]
        WorkflowInfo[] Search(string keyword);

        [OperationContract]
        void StartWorkflow(string id);

        [OperationContract]
        bool StopWorkflow(string id);

        [OperationContract]
        bool SuspendWorkflow(string id);

        [OperationContract]
        void ResumeWorkflow(string id);

        [OperationContract]
        WorkflowInfo GetWorkflow(string id);

        [OperationContract]
        TaskInfo[] GetTasks(string id);

        [OperationContract]
        string GetWorkflowXml(string id);

        [OperationContract]
        bool SaveWorkflow(Stream streamdata);

        [OperationContract]
        string[] GetTaskNames();

        [OperationContract]
        string GetWorkflowsFolder();

        [OperationContract]
        bool IsWorkflowIdValid(string id);

        [OperationContract]
        bool IsCronExpressionValid(string expression);

        [OperationContract]
        bool IsPeriodValid(string period);

        [OperationContract]
        bool DeleteWorkflow(string id);

        [OperationContract]
        string[] GetSettings(string taskName);

        [OperationContract]
        Node[] GetExecutionGraph(string id);

        [OperationContract]
        string GetTaskXml(Stream streamdata);

        [OperationContract]
        StatusCount GetStatusCount();

        [OperationContract]
        Entry[] GetEntries();

        [OperationContract]
        User GetUser(string username);

        [OperationContract]
        User[] GetUsers();

        [OperationContract]
        User[] SearchUsers(string keyword, int uo);

        [OperationContract]
        string GetPassword(string username);

        [OperationContract]
        bool InsertUser(string username, string password, int userProfile, string email);

        [OperationContract]
        bool UpdateUser(int userId, string username, string password, int userProfile, string email);

        [OperationContract]
        bool UpdateUsernameAndEmailAndUserProfile(int userId, string username, string email, int up);

        [OperationContract]
        bool DeleteUser(string username, string password);

        [OperationContract]
        bool ResetPassword(string username, string email);

        [OperationContract]
        HistoryEntry[] GetHistoryEntries();

        [OperationContract]
        HistoryEntry[] SearchHistoryEntries(string keyword);

        [OperationContract]
        HistoryEntry[] SearchHistoryEntriesByPage(string keyword, int page, int entriesCount);

        [OperationContract]
        long GetHistoryEntriesCount(string keyword);

        [OperationContract]
        long GetHistoryEntriesCountByDate(string keyword, double from, double to);

        [OperationContract]
        long GetEntriesCountByDate(string keyword, double from, double to);

        [OperationContract]
        HistoryEntry[] SearchHistoryEntriesByPageOrderBy(string keyword, double from, double to, int page, int entriesCount, int heo);

        [OperationContract]
        Entry[] SearchEntriesByPageOrderBy(string keyword, double from, double to, int page, int entriesCount, int heo);

        [OperationContract]
        double GetHistoryEntryStatusDateMin();

        [OperationContract]
        double GetHistoryEntryStatusDateMax();

        [OperationContract]
        double GetEntryStatusDateMin();

        [OperationContract]
        double GetEntryStatusDateMax();
    }
}
