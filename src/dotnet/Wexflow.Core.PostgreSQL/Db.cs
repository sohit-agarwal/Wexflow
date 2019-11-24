using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wexflow.Core.Db;

namespace Wexflow.Core.PostgreSQL
{
    public class Db : Core.Db.Db
    {
        private string _connectionString;
        private string _databaseName;
        private Helper _helper;

        public Db(string connectionString) : base(connectionString)
        {
            _connectionString = connectionString;

            var connectionStringParts = ConnectionString.Split(';');

            foreach (var part in connectionStringParts)
            {
                if (!string.IsNullOrEmpty(part.Trim()))
                {
                    string connPart = part.TrimStart(' ').TrimEnd(' ');
                    if (connPart.StartsWith("Database="))
                    {
                        _databaseName = connPart.Replace("Database=", string.Empty);
                        break;
                    }
                }
            }

            _helper = new Helper(connectionString);

            _helper.CreateDatabaseIfNotExists(_databaseName);
            _helper.CreateTableIfNotExists(Core.Db.Entry.DocumentName, Entry.TableStruct);
            _helper.CreateTableIfNotExists(Core.Db.HistoryEntry.DocumentName, HistoryEntry.TableStruct);
            _helper.CreateTableIfNotExists(Core.Db.StatusCount.DocumentName, StatusCount.TableStruct);
            _helper.CreateTableIfNotExists(Core.Db.User.DocumentName, User.TableStruct);
            _helper.CreateTableIfNotExists(Core.Db.UserWorkflow.DocumentName, UserWorkflow.TableStruct);
            _helper.CreateTableIfNotExists(Core.Db.Workflow.DocumentName, Workflow.TableStruct);
        }

        public override void Init()
        {
            // StatusCount
            ClearStatusCount();

            var statusCount = new StatusCount
            {
                PendingCount = 0,
                RunningCount = 0,
                DoneCount = 0,
                FailedCount = 0,
                WarningCount = 0,
                DisabledCount = 0,
                StoppedCount = 0
            };

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("INSERT INTO " + Core.Db.StatusCount.DocumentName + "("
                    + StatusCount.ColumnName_PendingCount + ", "
                    + StatusCount.ColumnName_RunningCount + ", "
                    + StatusCount.ColumnName_DoneCount + ", "
                    + StatusCount.ColumnName_FailedCount + ", "
                    + StatusCount.ColumnName_WarningCount + ", "
                    + StatusCount.ColumnName_DisabledCount + ", "
                    + StatusCount.ColumnName_StoppedCount + ", "
                    + StatusCount.ColumnName_DisapprovedCount + ") VALUES("
                    + statusCount.PendingCount + ", "
                    + statusCount.RunningCount + ", "
                    + statusCount.DoneCount + ", "
                    + statusCount.FailedCount + ", "
                    + statusCount.WarningCount + ", "
                    + statusCount.DisabledCount + ", "
                    + statusCount.StoppedCount + ", "
                    + statusCount.DisapprovedCount + ");"
                    , conn);

                command.ExecuteNonQuery();
            }

            // Entries
            ClearEntries();

            // Insert default user if necessary
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("SELECT COUNT(*) FROM " + Core.Db.User.DocumentName + ";", conn);

                var usersCount = (int)command.ExecuteScalar();

                if (usersCount == 0)
                {
                    InsertDefaultUser();
                }
            }
        }

        public override bool CheckUserWorkflow(string userId, string workflowId)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("SELECT COUNT(*) FROM " + Core.Db.UserWorkflow.DocumentName
                    + " WHERE " + UserWorkflow.ColumnName_UserId + "=" + int.Parse(userId)
                    + " AND " + UserWorkflow.ColumnName_WorkflowId + "=" + int.Parse(workflowId)
                    + ";", conn);

                var count = (int)command.ExecuteScalar();

                return count > 0;

            }
        }

        public override void ClearEntries()
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("DELETE FROM " + Core.Db.Entry.DocumentName + ";", conn);

                command.ExecuteNonQuery();
            }
        }

        public override void ClearStatusCount()
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("DELETE FROM " + Core.Db.StatusCount.DocumentName + ";", conn);

                command.ExecuteNonQuery();
            }
        }

        public override void DecrementPendingCount()
        {
            throw new NotImplementedException();
        }

        public override void DecrementRunningCount()
        {
            throw new NotImplementedException();
        }

        public override void DeleteUser(string username, string password)
        {
            throw new NotImplementedException();
        }

        public override void DeleteUserWorkflowRelationsByUserId(string userId)
        {
            throw new NotImplementedException();
        }

        public override void DeleteUserWorkflowRelationsByWorkflowId(string workflowDbId)
        {
            throw new NotImplementedException();
        }

        public override void DeleteWorkflow(string id)
        {
            throw new NotImplementedException();
        }

        public override void DeleteWorkflows(string[] ids)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Core.Db.User> GetAdministrators(string keyword, UserOrderBy uo)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Core.Db.Entry> GetEntries()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Core.Db.Entry> GetEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy eo)
        {
            throw new NotImplementedException();
        }

        public override long GetEntriesCount(string keyword, DateTime from, DateTime to)
        {
            throw new NotImplementedException();
        }

        public override Core.Db.Entry GetEntry(int workflowId)
        {
            throw new NotImplementedException();
        }

        public override DateTime GetEntryStatusDateMax()
        {
            throw new NotImplementedException();
        }

        public override DateTime GetEntryStatusDateMin()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword, int page, int entriesCount)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy heo)
        {
            throw new NotImplementedException();
        }

        public override long GetHistoryEntriesCount(string keyword)
        {
            throw new NotImplementedException();
        }

        public override long GetHistoryEntriesCount(string keyword, DateTime from, DateTime to)
        {
            throw new NotImplementedException();
        }

        public override DateTime GetHistoryEntryStatusDateMax()
        {
            throw new NotImplementedException();
        }

        public override DateTime GetHistoryEntryStatusDateMin()
        {
            throw new NotImplementedException();
        }

        public override string GetPassword(string username)
        {
            throw new NotImplementedException();
        }

        public override Core.Db.StatusCount GetStatusCount()
        {
            throw new NotImplementedException();
        }

        public override Core.Db.User GetUser(string username)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Core.Db.User> GetUsers()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Core.Db.User> GetUsers(string keyword, UserOrderBy uo)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<string> GetUserWorkflows(string userId)
        {
            throw new NotImplementedException();
        }

        public override Core.Db.Workflow GetWorkflow(string id)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Core.Db.Workflow> GetWorkflows()
        {
            throw new NotImplementedException();
        }

        public override void IncrementDisabledCount()
        {
            throw new NotImplementedException();
        }

        public override void IncrementDisapprovedCount()
        {
            throw new NotImplementedException();
        }

        public override void IncrementDoneCount()
        {
            throw new NotImplementedException();
        }

        public override void IncrementFailedCount()
        {
            throw new NotImplementedException();
        }

        public override void IncrementPendingCount()
        {
            throw new NotImplementedException();
        }

        public override void IncrementRunningCount()
        {
            throw new NotImplementedException();
        }

        public override void IncrementStoppedCount()
        {
            throw new NotImplementedException();
        }

        public override void IncrementWarningCount()
        {
            throw new NotImplementedException();
        }

        public override void InsertEntry(Core.Db.Entry entry)
        {
            throw new NotImplementedException();
        }

        public override void InsertHistoryEntry(Core.Db.HistoryEntry entry)
        {
            throw new NotImplementedException();
        }

        public override void InsertUser(Core.Db.User user)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("INSERT INTO " + Core.Db.User.DocumentName + "("
                    + User.ColumnName_Username + ", "
                    + User.ColumnName_Password + ", "
                    + User.ColumnName_Email + ", "
                    + User.ColumnName_CreatedOn + ", "
                    + User.ColumnName_ModifiedOn + ") VALUES("
                    + "'" + user.Username + "'" + ", "
                    + "'" + user.Password + "'" + ", "
                    + "'" + user.Email + "'" + ", "
                    + "'" + user.CreatedOn + "'" + ", "
                    + "'" + user.ModifiedOn + "'" + ");"
                    , conn);

                command.ExecuteNonQuery();
            }
        }

        public override void InsertUserWorkflowRelation(Core.Db.UserWorkflow userWorkflow)
        {
            throw new NotImplementedException();
        }

        public override string InsertWorkflow(Core.Db.Workflow workflow)
        {
            throw new NotImplementedException();
        }

        public override void UpdateEntry(string id, Core.Db.Entry entry)
        {
            throw new NotImplementedException();
        }

        public override void UpdatePassword(string username, string password)
        {
            throw new NotImplementedException();
        }

        public override void UpdateUser(string id, Core.Db.User user)
        {
            throw new NotImplementedException();
        }

        public override void UpdateUsernameAndEmailAndUserProfile(string userId, string username, string email, UserProfile up)
        {
            throw new NotImplementedException();
        }

        public override void UpdateWorkflow(string dbId, Core.Db.Workflow workflow)
        {
            throw new NotImplementedException();
        }
    }
}
