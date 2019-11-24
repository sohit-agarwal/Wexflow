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
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("SELECT " + StatusCount.ColumnName_PendingCount + " FROM " + Core.Db.StatusCount.DocumentName + ";", conn);

                var count = (int)command.ExecuteScalar();

                count--;

                command = new NpgsqlCommand("UPDATE " + Core.Db.StatusCount.DocumentName + " SET " + StatusCount.ColumnName_PendingCount + " = " + count + ";", conn);

                command.ExecuteNonQuery();
            }
        }

        public override void DecrementRunningCount()
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("SELECT " + StatusCount.ColumnName_RunningCount + " FROM " + Core.Db.StatusCount.DocumentName + ";", conn);

                var count = (int)command.ExecuteScalar();

                count--;

                command = new NpgsqlCommand("UPDATE " + Core.Db.StatusCount.DocumentName + " SET " + StatusCount.ColumnName_RunningCount + " = " + count + ";", conn);

                command.ExecuteNonQuery();
            }
        }

        public override void DeleteUser(string username, string password)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("DELETE FROM " + Core.Db.User.DocumentName
                    + " WHERE " + User.ColumnName_Username + " = '" + username + "'"
                    + " AND " + User.ColumnName_Password + " = '" + password + "'"
                    + ";", conn);

                command.ExecuteNonQuery();
            }
        }

        public override void DeleteUserWorkflowRelationsByUserId(string userId)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("DELETE FROM " + Core.Db.UserWorkflow.DocumentName
                    + " WHERE " + UserWorkflow.ColumnName_UserId + " = " + int.Parse(userId) + ";", conn);

                command.ExecuteNonQuery();
            }
        }

        public override void DeleteUserWorkflowRelationsByWorkflowId(string workflowDbId)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("DELETE FROM " + Core.Db.UserWorkflow.DocumentName
                    + " WHERE " + UserWorkflow.ColumnName_WorkflowId + " = " + int.Parse(workflowDbId) + ";", conn);

                command.ExecuteNonQuery();
            }
        }

        public override void DeleteWorkflow(string id)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("DELETE FROM " + Core.Db.Workflow.DocumentName
                    + " WHERE " + Workflow.ColumnName_Id + " = " + int.Parse(id) + ";", conn);

                command.ExecuteNonQuery();
            }
        }

        public override void DeleteWorkflows(string[] ids)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var builder = new StringBuilder("(");

                for (int i = 0; i < ids.Length; i++)
                {
                    var id = ids[i];
                    builder.Append(id);
                    if (i < ids.Length - 1)
                    {
                        builder.Append(", ");
                    }
                    else
                    {
                        builder.Append(")");
                    }
                }

                var command = new NpgsqlCommand("DELETE FROM " + Core.Db.Workflow.DocumentName
                    + " WHERE " + Workflow.ColumnName_Id + " IN " + builder.ToString() + ";", conn);

                command.ExecuteNonQuery();
            }
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
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("SELECT " + StatusCount.ColumnName_DisabledCount + " FROM " + Core.Db.StatusCount.DocumentName + ";", conn);

                var count = (int)command.ExecuteScalar();

                count++;

                command = new NpgsqlCommand("UPDATE " + Core.Db.StatusCount.DocumentName + " SET " + StatusCount.ColumnName_DisabledCount + " = " + count + ";", conn);

                command.ExecuteNonQuery();
            }
        }

        public override void IncrementDisapprovedCount()
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("SELECT " + StatusCount.ColumnName_DisapprovedCount + " FROM " + Core.Db.StatusCount.DocumentName + ";", conn);

                var count = (int)command.ExecuteScalar();

                count++;

                command = new NpgsqlCommand("UPDATE " + Core.Db.StatusCount.DocumentName + " SET " + StatusCount.ColumnName_DisapprovedCount + " = " + count + ";", conn);

                command.ExecuteNonQuery();
            }
        }

        public override void IncrementDoneCount()
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("SELECT " + StatusCount.ColumnName_DoneCount + " FROM " + Core.Db.StatusCount.DocumentName + ";", conn);

                var count = (int)command.ExecuteScalar();

                count++;

                command = new NpgsqlCommand("UPDATE " + Core.Db.StatusCount.DocumentName + " SET " + StatusCount.ColumnName_DoneCount + " = " + count + ";", conn);

                command.ExecuteNonQuery();
            }
        }

        public override void IncrementFailedCount()
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("SELECT " + StatusCount.ColumnName_FailedCount + " FROM " + Core.Db.StatusCount.DocumentName + ";", conn);

                var count = (int)command.ExecuteScalar();

                count++;

                command = new NpgsqlCommand("UPDATE " + Core.Db.StatusCount.DocumentName + " SET " + StatusCount.ColumnName_FailedCount + " = " + count + ";", conn);

                command.ExecuteNonQuery();
            }
        }

        public override void IncrementPendingCount()
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("SELECT " + StatusCount.ColumnName_PendingCount + " FROM " + Core.Db.StatusCount.DocumentName + ";", conn);

                var count = (int)command.ExecuteScalar();

                count++;

                command = new NpgsqlCommand("UPDATE " + Core.Db.StatusCount.DocumentName + " SET " + StatusCount.ColumnName_PendingCount + " = " + count + ";", conn);

                command.ExecuteNonQuery();
            }
        }

        public override void IncrementRunningCount()
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("SELECT " + StatusCount.ColumnName_RunningCount + " FROM " + Core.Db.StatusCount.DocumentName + ";", conn);

                var count = (int)command.ExecuteScalar();

                count++;

                command = new NpgsqlCommand("UPDATE " + Core.Db.StatusCount.DocumentName + " SET " + StatusCount.ColumnName_RunningCount + " = " + count + ";", conn);

                command.ExecuteNonQuery();
            }
        }

        public override void IncrementStoppedCount()
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("SELECT " + StatusCount.ColumnName_StoppedCount + " FROM " + Core.Db.StatusCount.DocumentName + ";", conn);

                var count = (int)command.ExecuteScalar();

                count++;

                command = new NpgsqlCommand("UPDATE " + Core.Db.StatusCount.DocumentName + " SET " + StatusCount.ColumnName_StoppedCount + " = " + count + ";", conn);

                command.ExecuteNonQuery();
            }
        }

        public override void IncrementWarningCount()
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("SELECT " + StatusCount.ColumnName_WarningCount + " FROM " + Core.Db.StatusCount.DocumentName + ";", conn);

                var count = (int)command.ExecuteScalar();

                count++;

                command = new NpgsqlCommand("UPDATE " + Core.Db.StatusCount.DocumentName + " SET " + StatusCount.ColumnName_WarningCount + " = " + count + ";", conn);

                command.ExecuteNonQuery();
            }
        }

        public override void InsertEntry(Core.Db.Entry entry)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("INSERT INTO " + Core.Db.Entry.DocumentName + "("
                    + Entry.ColumnName_Name + ", "
                    + Entry.ColumnName_Description + ", "
                    + Entry.ColumnName_LaunchType + ", "
                    + Entry.ColumnName_StatusDate + ", "
                    + Entry.ColumnName_Status + ", "
                    + Entry.ColumnName_WorkflowId + ") VALUES("
                    + "'" + entry.Name + "'" + ", "
                    + "'" + entry.Description + "'" + ", "
                    + (int)entry.LaunchType + ", "
                    + "'" + entry.StatusDate + "'" + ", "
                    + (int)entry.Status + ");"
                    , conn);

                command.ExecuteNonQuery();
            }
        }

        public override void InsertHistoryEntry(Core.Db.HistoryEntry entry)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("INSERT INTO " + Core.Db.HistoryEntry.DocumentName + "("
                    + HistoryEntry.ColumnName_Name + ", "
                    + HistoryEntry.ColumnName_Description + ", "
                    + HistoryEntry.ColumnName_LaunchType + ", "
                    + HistoryEntry.ColumnName_StatusDate + ", "
                    + HistoryEntry.ColumnName_Status + ", "
                    + HistoryEntry.ColumnName_WorkflowId + ") VALUES("
                    + "'" + entry.Name + "'" + ", "
                    + "'" + entry.Description + "'" + ", "
                    + (int)entry.LaunchType + ", "
                    + "'" + entry.StatusDate + "'" + ", "
                    + (int)entry.Status + ");"
                    , conn);

                command.ExecuteNonQuery();
            }
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
                    + "'" + DateTime.Now + "'" + ", "
                    + "'" + user.ModifiedOn + "'" + ");"
                    , conn);

                command.ExecuteNonQuery();
            }
        }

        public override void InsertUserWorkflowRelation(Core.Db.UserWorkflow userWorkflow)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("INSERT INTO " + Core.Db.UserWorkflow.DocumentName + "("
                    + UserWorkflow.ColumnName_UserId + ", "
                    + UserWorkflow.ColumnName_WorkflowId + ") VALUES("
                    + int.Parse(userWorkflow.UserId) + ", "
                    + int.Parse(userWorkflow.WorkflowId) + ");"
                    , conn);

                command.ExecuteNonQuery();
            }
        }

        public override string InsertWorkflow(Core.Db.Workflow workflow)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("INSERT INTO " + Core.Db.Workflow.DocumentName + "("
                    + Workflow.ColumnName_Xml + ") VALUES("
                    + "'" + workflow.Xml + "'" + ") RETURNING " + Workflow.ColumnName_Id + ";"
                    , conn);

                var id = (int)command.ExecuteScalar();

                return id.ToString();
            }
        }

        public override void UpdateEntry(string id, Core.Db.Entry entry)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("UPDATE " + Core.Db.Entry.DocumentName + " SET "
                    + Entry.ColumnName_Name + " = '" + entry.Name + "', "
                    + Entry.ColumnName_Description + " = '" + entry.Description + "', "
                    + Entry.ColumnName_LaunchType + " = " + (int)entry.LaunchType + ", "
                    + Entry.ColumnName_StatusDate + " = '" + entry.StatusDate + "', "
                    + Entry.ColumnName_Status + " = " + (int)entry.Status + ", "
                    + Entry.ColumnName_WorkflowId + " = " + entry.WorkflowId
                    + " WHERE "
                    + Entry.ColumnName_Id + " = " + int.Parse(id) + ";"
                    , conn);

                command.ExecuteNonQuery();
            }
        }

        public override void UpdatePassword(string username, string password)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("UPDATE " + Core.Db.User.DocumentName + " SET "
                    + User.ColumnName_Password + " = '" + password + "'"
                    + " WHERE "
                    + User.ColumnName_Username + " = '" + username + "';"
                    , conn);

                command.ExecuteNonQuery();
            }
        }

        public override void UpdateUser(string id, Core.Db.User user)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("UPDATE " + Core.Db.User.DocumentName + " SET "
                    + User.ColumnName_Username + " = '" + user.Username + "', "
                    + User.ColumnName_Password + " = '" + user.Password + "', "
                    + User.ColumnName_UserProfile + " = " + (int)user.UserProfile + ", "
                    + User.ColumnName_Email + " = '" + user.Email + "', "
                    + User.ColumnName_CreatedOn + " = '" + user.CreatedOn + "', "
                    + User.ColumnName_ModifiedOn + " = '" + DateTime.Now + "'"
                    + " WHERE "
                    + User.ColumnName_Id + " = " + int.Parse(id) + ";"
                    , conn);

                command.ExecuteNonQuery();
            }
        }

        public override void UpdateUsernameAndEmailAndUserProfile(string userId, string username, string email, UserProfile up)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("UPDATE " + Core.Db.User.DocumentName + " SET "
                    + User.ColumnName_Username + " = '" + username + "', "
                    + User.ColumnName_UserProfile + " = " + (int)up + ", "
                    + User.ColumnName_Email + " = '" + email + "', "
                    + User.ColumnName_ModifiedOn + " = '" + DateTime.Now + "'"
                    + " WHERE "
                    + User.ColumnName_Id + " = " + int.Parse(userId) + ";"
                    , conn);

                command.ExecuteNonQuery();
            }
        }

        public override void UpdateWorkflow(string dbId, Core.Db.Workflow workflow)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("UPDATE " + Core.Db.Workflow.DocumentName + " SET "
                    + Workflow.ColumnName_Xml + " = '" + workflow.Xml + "'"
                    + " WHERE "
                    + User.ColumnName_Id + " = " + int.Parse(dbId) + ";"
                    , conn);

                command.ExecuteNonQuery();
            }
        }
    }
}
