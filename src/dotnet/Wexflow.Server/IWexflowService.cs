using System.IO;
using System.ServiceModel;
using Wexflow.Core.Service.Contracts;

namespace Wexflow.Server
{
    [ServiceContract(Namespace = "http://wexflow.com/")]
    public interface IWexflowService
    {
        //[OperationContract]
        //WorkflowInfo[] GetWorkflows();

        //[OperationContract]
        //WorkflowInfo[] GetApprovalWorkflows();

        [OperationContract]
        WorkflowInfo[] Search(string keyword, string username, string password);

        [OperationContract]
        WorkflowInfo[] SearchApprovalWorkflows(string keyword, string username, string password);

        [OperationContract]
        void StartWorkflow(string id, string username, string password);

        [OperationContract]
        void StartWorkflowWithVariables(Stream streamdata);

        [OperationContract]
        bool StopWorkflow(string id, string username, string password);

        [OperationContract]
        bool SuspendWorkflow(string id, string username, string password);

        [OperationContract]
        void ResumeWorkflow(string id, string username, string password);

        [OperationContract]
        bool ApproveWorkflow(string id, string username, string password);

        [OperationContract]
        bool DisapproveWorkflow(string id, string username, string password);

        [OperationContract]
        WorkflowInfo GetWorkflow(string username, string password, int id);

        [OperationContract]
        TaskInfo[] GetTasks(string id);

        [OperationContract]
        string GetWorkflowXml(string id);

        [OperationContract]
        bool IsXmlWorkflowValid(Stream streamdata);

        [OperationContract]
        bool SaveXmlWorkflow(Stream streamdata);

        [OperationContract]
        bool SaveWorkflow(Stream streamdata);

        [OperationContract]
        string[] GetTaskNames();

        [OperationContract]
        bool IsWorkflowIdValid(string id);

        [OperationContract]
        bool IsCronExpressionValid(string expression);

        [OperationContract]
        bool IsPeriodValid(string period);

        [OperationContract]
        bool DeleteWorkflow(string id, string username, string password);

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
        User GetUser(string qusername, string qpassword, string username);

        //[OperationContract]
        //User[] GetUsers();

        [OperationContract]
        User[] SearchUsers(string qusername, string qpassword, string keyword, int uo);

        [OperationContract]
        bool SaveUserWorkflows(Stream streamdata);

        [OperationContract]
        WorkflowInfo[] GetUserWorkflows(string qusername, string qpassword, string userId);

        [OperationContract]
        User[] SearchAdministrators(string qusername, string qpassword, string keyword, int uo);

        //[OperationContract]
        //string GetPassword(string qusername, string qpassword, string username);

        [OperationContract]
        bool InsertUser(string qusername, string qpassword, string username, string password, int userProfile, string email);

        [OperationContract]
        bool UpdateUser(string qusername, string qpassword, string userId, string username, string password, int userProfile, string email);

        [OperationContract]
        bool UpdateUsernameAndEmailAndUserProfile(string qusername, string qpassword, string userId, string username, string email, int up);

        [OperationContract]
        bool DeleteUser(string qusername, string qpassword, string username, string password);

        [OperationContract]
        bool ResetPassword(string username);

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

        [OperationContract]
        bool DeleteWorkflows(Stream streamdata);
    }
}
