using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Wexflow.Core.Db
{
    public enum EntryOrderBy
    {
        StatusDateAscending,
        StatusDateDescending,
        WorkflowIdAscending,
        WorkflowIdDescending,
        NameAscending,
        NameDescending,
        LaunchTypeAscending,
        LaunchTypeDescending,
        DescriptionAscending,
        DescriptionDescending,
        StatusAscending,
        StatusDescending
    }

    public enum UserOrderBy
    {
        UsernameAscending,
        UsernameDescending
    }

    public abstract class Db
    {
        public string ConnectionString { get; }

        protected Db(string connectionString)
        {
            ConnectionString = connectionString;
        }

        protected void InsertDefaultUser()
        {
            var password = GetMd5("wexflow2018");
            var user = new User { Username = "admin", Password = password, UserProfile = UserProfile.SuperAdministrator };
            InsertUser(user);
        }

        public abstract void Init();
        public abstract IEnumerable<Workflow> GetWorkflows();
        public abstract string InsertWorkflow(Workflow workflow);
        public abstract Workflow GetWorkflow(string id);
        public abstract void UpdateWorkflow(string dbId, Workflow workflow);
        public abstract void DeleteWorkflow(string id);
        public abstract void DeleteUserWorkflowRelationsByWorkflowId(string workflowDbId);
        public abstract void DeleteWorkflows(string[] ids);
        public abstract void InsertUserWorkflowRelation(UserWorkflow userWorkflow);
        public abstract void DeleteUserWorkflowRelationsByUserId(string userId);
        public abstract IEnumerable<string> GetUserWorkflows(string userId);
        public abstract bool CheckUserWorkflow(string userId, string workflowId);
        public abstract IEnumerable<User> GetAdministrators(string keyword, UserOrderBy uo);
        public abstract void ClearStatusCount();
        public abstract void ClearEntries();
        public abstract StatusCount GetStatusCount();
        public abstract IEnumerable<Entry> GetEntries();
        public abstract void InsertUser(User user);
        public abstract void UpdateUser(string id, User user);
        public abstract void UpdateUsernameAndEmailAndUserProfile(string userId, string username, string email, UserProfile up);
        public abstract User GetUser(string username);
        public abstract void DeleteUser(string username, string password);
        public abstract string GetPassword(string username);
        public abstract IEnumerable<User> GetUsers();
        public abstract IEnumerable<User> GetUsers(string keyword, UserOrderBy uo);
        public abstract void UpdatePassword(string username, string password);
        public abstract IEnumerable<HistoryEntry> GetHistoryEntries();
        public abstract IEnumerable<HistoryEntry> GetHistoryEntries(string keyword);
        public abstract IEnumerable<HistoryEntry> GetHistoryEntries(string keyword, int page, int entriesCount);
        public abstract IEnumerable<HistoryEntry> GetHistoryEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy heo);
        public abstract IEnumerable<Entry> GetEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy eo);
        public abstract long GetHistoryEntriesCount(string keyword);
        public abstract long GetHistoryEntriesCount(string keyword, DateTime from, DateTime to);
        public abstract long GetEntriesCount(string keyword, DateTime from, DateTime to);
        public abstract DateTime GetHistoryEntryStatusDateMin();
        public abstract DateTime GetHistoryEntryStatusDateMax();
        public abstract DateTime GetEntryStatusDateMin();
        public abstract DateTime GetEntryStatusDateMax();
        public abstract void IncrementDisabledCount();
        public abstract void IncrementRunningCount();
        public abstract Entry GetEntry(int workflowId);
        public abstract void InsertEntry(Entry entry);
        public abstract void UpdateEntry(string id, Entry entry);
        public abstract void IncrementDisapprovedCount();
        public abstract void IncrementDoneCount();
        public abstract void IncrementWarningCount();
        public abstract void IncrementFailedCount();
        public abstract void InsertHistoryEntry(HistoryEntry entry);
        public abstract void DecrementRunningCount();
        public abstract void IncrementStoppedCount();
        public abstract void IncrementPendingCount();
        public abstract void DecrementPendingCount();

        public static string GetMd5(string input)
        {
            // Use input string to calculate MD5 hash
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
