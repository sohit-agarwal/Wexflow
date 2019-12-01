using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using Wexflow.Core.Db;

namespace Wexflow.Core.SQLite
{
    public class Db : Core.Db.Db
    {
        private static readonly string DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

        private string _connectionString;
        private string _dataSource;
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
                    if (connPart.StartsWith("Data Source="))
                    {
                        _dataSource = connPart.Replace("Data Source=", string.Empty);
                        break;
                    }
                }
            }

            _helper = new Helper(connectionString);

            _helper.CreateDatabaseIfNotExists(_dataSource);
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

            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("INSERT INTO " + Core.Db.StatusCount.DocumentName + "("
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
                    , conn))
                {
                    command.ExecuteNonQuery();
                }
            }

            // Entries
            ClearEntries();

            // Insert default user if necessary
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("SELECT COUNT(*) FROM " + Core.Db.User.DocumentName + ";", conn))
                {
                    var usersCount = (long)command.ExecuteScalar();

                    if (usersCount == 0)
                    {
                        InsertDefaultUser();
                    }
                }
            }
        }

        public override bool CheckUserWorkflow(string userId, string workflowId)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("SELECT COUNT(*) FROM " + Core.Db.UserWorkflow.DocumentName
                    + " WHERE " + UserWorkflow.ColumnName_UserId + "=" + int.Parse(userId)
                    + " AND " + UserWorkflow.ColumnName_WorkflowId + "=" + int.Parse(workflowId)
                    + ";", conn))
                {

                    var count = (long)command.ExecuteScalar();

                    return count > 0;
                }

            }
        }

        public override void ClearEntries()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("DELETE FROM " + Core.Db.Entry.DocumentName + ";", conn))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public override void ClearStatusCount()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("DELETE FROM " + Core.Db.StatusCount.DocumentName + ";", conn))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public override void DecrementPendingCount()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command1 = new SQLiteCommand("SELECT " + StatusCount.ColumnName_PendingCount + " FROM " + Core.Db.StatusCount.DocumentName + ";", conn))
                {

                    var count = (long)command1.ExecuteScalar();

                    count--;

                    using (var command2 = new SQLiteCommand("UPDATE " + Core.Db.StatusCount.DocumentName + " SET " + StatusCount.ColumnName_PendingCount + " = " + count + ";", conn))
                    {
                        command2.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void DecrementRunningCount()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command1 = new SQLiteCommand("SELECT " + StatusCount.ColumnName_RunningCount + " FROM " + Core.Db.StatusCount.DocumentName + ";", conn))
                {

                    var count = (long)command1.ExecuteScalar();

                    count--;

                    using (var command2 = new SQLiteCommand("UPDATE " + Core.Db.StatusCount.DocumentName + " SET " + StatusCount.ColumnName_RunningCount + " = " + count + ";", conn))
                    {
                        command2.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void DeleteUser(string username, string password)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("DELETE FROM " + Core.Db.User.DocumentName
                    + " WHERE " + User.ColumnName_Username + " = '" + username + "'"
                    + " AND " + User.ColumnName_Password + " = '" + password + "'"
                    + ";", conn))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public override void DeleteUserWorkflowRelationsByUserId(string userId)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("DELETE FROM " + Core.Db.UserWorkflow.DocumentName
                    + " WHERE " + UserWorkflow.ColumnName_UserId + " = " + int.Parse(userId) + ";", conn))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public override void DeleteUserWorkflowRelationsByWorkflowId(string workflowDbId)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("DELETE FROM " + Core.Db.UserWorkflow.DocumentName
                    + " WHERE " + UserWorkflow.ColumnName_WorkflowId + " = " + int.Parse(workflowDbId) + ";", conn))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public override void DeleteWorkflow(string id)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("DELETE FROM " + Core.Db.Workflow.DocumentName
                    + " WHERE " + Workflow.ColumnName_Id + " = " + int.Parse(id) + ";", conn))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public override void DeleteWorkflows(string[] ids)
        {
            using (var conn = new SQLiteConnection(_connectionString))
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

                using (var command = new SQLiteCommand("DELETE FROM " + Core.Db.Workflow.DocumentName
                    + " WHERE " + Workflow.ColumnName_Id + " IN " + builder.ToString() + ";", conn))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public override IEnumerable<Core.Db.User> GetAdministrators(string keyword, UserOrderBy uo)
        {
            List<User> admins = new List<User>();

            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("SELECT " + User.ColumnName_Id + ", "
                    + User.ColumnName_Username + ", "
                    + User.ColumnName_Password + ", "
                    + User.ColumnName_Email + ", "
                    + User.ColumnName_UserProfile + ", "
                    + User.ColumnName_CreatedOn + ", "
                    + User.ColumnName_ModifiedOn
                    + " FROM " + Core.Db.User.DocumentName
                    + " WHERE " + "(LOWER(" + User.ColumnName_Username + ")" + " LIKE '%" + keyword.ToLower() + "%'"
                    + " AND " + User.ColumnName_UserProfile + " = " + (int)UserProfile.Administrator + ")"
                    + " ORDER BY " + User.ColumnName_Username + (uo == UserOrderBy.UsernameAscending ? " ASC" : " DESC")
                    + ";", conn))
                {

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var admin = new User
                            {
                                Id = (long)reader[User.ColumnName_Id],
                                Username = (string)reader[User.ColumnName_Username],
                                Password = (string)reader[User.ColumnName_Password],
                                Email = (string)reader[User.ColumnName_Email],
                                UserProfile = (UserProfile)((long)reader[User.ColumnName_UserProfile]),
                                CreatedOn = DateTime.Parse((string)reader[User.ColumnName_CreatedOn]),
                                ModifiedOn = reader[User.ColumnName_ModifiedOn] == DBNull.Value ? DateTime.MinValue : DateTime.Parse((string)reader[User.ColumnName_ModifiedOn])
                            };

                            admins.Add(admin);
                        }
                    }
                }
            }

            return admins;
        }

        public override IEnumerable<Core.Db.Entry> GetEntries()
        {
            List<Entry> entries = new List<Entry>();

            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("SELECT "
                    + Entry.ColumnName_Id + ", "
                    + Entry.ColumnName_Name + ", "
                    + Entry.ColumnName_Description + ", "
                    + Entry.ColumnName_LaunchType + ", "
                    + Entry.ColumnName_Status + ", "
                    + Entry.ColumnName_StatusDate + ", "
                    + Entry.ColumnName_WorkflowId
                    + " FROM " + Core.Db.Entry.DocumentName + ";", conn))
                {

                    using (var reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            var entry = new Entry
                            {
                                Id = (long)reader[Entry.ColumnName_Id],
                                Name = (string)reader[Entry.ColumnName_Name],
                                Description = (string)reader[Entry.ColumnName_Description],
                                LaunchType = (LaunchType)((long)reader[Entry.ColumnName_LaunchType]),
                                Status = (Status)((long)reader[Entry.ColumnName_Status]),
                                StatusDate = DateTime.Parse((string)reader[Entry.ColumnName_StatusDate]),
                                WorkflowId = (int)((long)reader[Entry.ColumnName_WorkflowId])
                            };

                            entries.Add(entry);
                        }
                    }
                }
            }

            return entries;
        }

        public override IEnumerable<Core.Db.Entry> GetEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy eo)
        {
            List<Entry> entries = new List<Entry>();

            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                var sqlBuilder = new StringBuilder("SELECT "
                    + Entry.ColumnName_Id + ", "
                    + Entry.ColumnName_Name + ", "
                    + Entry.ColumnName_Description + ", "
                    + Entry.ColumnName_LaunchType + ", "
                    + Entry.ColumnName_Status + ", "
                    + Entry.ColumnName_StatusDate + ", "
                    + Entry.ColumnName_WorkflowId
                    + " FROM " + Core.Db.Entry.DocumentName
                    + " WHERE " + "(LOWER(" + Entry.ColumnName_Name + ") LIKE '%" + keyword.ToLower() + "%'"
                    + " OR " + "LOWER(" + Entry.ColumnName_Description + ") LIKE '%" + keyword.ToLower() + "%')"
                    + " AND (" + Entry.ColumnName_StatusDate + " BETWEEN '" + from.ToString(DateTimeFormat) + "' AND '" + to.ToString(DateTimeFormat) + "')"
                    + " ORDER BY ");

                switch (eo)
                {
                    case EntryOrderBy.StatusDateAscending:

                        sqlBuilder.Append(Entry.ColumnName_StatusDate).Append(" ASC");
                        break;

                    case EntryOrderBy.StatusDateDescending:

                        sqlBuilder.Append(Entry.ColumnName_StatusDate).Append(" DESC");
                        break;

                    case EntryOrderBy.WorkflowIdAscending:

                        sqlBuilder.Append(Entry.ColumnName_WorkflowId).Append(" ASC");
                        break;

                    case EntryOrderBy.WorkflowIdDescending:

                        sqlBuilder.Append(Entry.ColumnName_WorkflowId).Append(" DESC");
                        break;

                    case EntryOrderBy.NameAscending:

                        sqlBuilder.Append(Entry.ColumnName_Name).Append(" ASC");
                        break;

                    case EntryOrderBy.NameDescending:

                        sqlBuilder.Append(Entry.ColumnName_Name).Append(" DESC");
                        break;

                    case EntryOrderBy.LaunchTypeAscending:

                        sqlBuilder.Append(Entry.ColumnName_LaunchType).Append(" ASC");
                        break;

                    case EntryOrderBy.LaunchTypeDescending:

                        sqlBuilder.Append(Entry.ColumnName_LaunchType).Append(" DESC");
                        break;

                    case EntryOrderBy.DescriptionAscending:

                        sqlBuilder.Append(Entry.ColumnName_Description).Append(" ASC");
                        break;

                    case EntryOrderBy.DescriptionDescending:

                        sqlBuilder.Append(Entry.ColumnName_Description).Append(" DESC");
                        break;

                    case EntryOrderBy.StatusAscending:

                        sqlBuilder.Append(Entry.ColumnName_Status).Append(" ASC");
                        break;

                    case EntryOrderBy.StatusDescending:

                        sqlBuilder.Append(Entry.ColumnName_Status).Append(" DESC");
                        break;
                }

                sqlBuilder.Append(" LIMIT ").Append(entriesCount).Append(" OFFSET ").Append((page - 1) * entriesCount).Append(";");

                using (var command = new SQLiteCommand(sqlBuilder.ToString(), conn))
                {

                    using (var reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            var entry = new Entry
                            {
                                Id = (long)reader[Entry.ColumnName_Id],
                                Name = (string)reader[Entry.ColumnName_Name],
                                Description = (string)reader[Entry.ColumnName_Description],
                                LaunchType = (LaunchType)((long)reader[Entry.ColumnName_LaunchType]),
                                Status = (Status)((long)reader[Entry.ColumnName_Status]),
                                StatusDate = DateTime.Parse((string)reader[Entry.ColumnName_StatusDate]),
                                WorkflowId = (int)((long)reader[Entry.ColumnName_WorkflowId])
                            };

                            entries.Add(entry);
                        }
                    }

                }
            }

            return entries;
        }

        public override long GetEntriesCount(string keyword, DateTime from, DateTime to)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("SELECT COUNT(*)"
                    + " FROM " + Core.Db.Entry.DocumentName
                    + " WHERE " + "(LOWER(" + Entry.ColumnName_Name + ") LIKE '%" + keyword.ToLower() + "%'"
                    + " OR " + "LOWER(" + Entry.ColumnName_Description + ") LIKE '%" + keyword.ToLower() + "%')"
                    + " AND (" + Entry.ColumnName_StatusDate + " BETWEEN '" + from.ToString(DateTimeFormat) + "' AND '" + to.ToString(DateTimeFormat) + "');", conn))
                {

                    var count = (long)command.ExecuteScalar();

                    return count;
                }
            }
        }

        public override Core.Db.Entry GetEntry(int workflowId)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("SELECT "
                    + Entry.ColumnName_Id + ", "
                    + Entry.ColumnName_Name + ", "
                    + Entry.ColumnName_Description + ", "
                    + Entry.ColumnName_LaunchType + ", "
                    + Entry.ColumnName_Status + ", "
                    + Entry.ColumnName_StatusDate + ", "
                    + Entry.ColumnName_WorkflowId
                    + " FROM " + Core.Db.Entry.DocumentName
                    + " WHERE " + Entry.ColumnName_WorkflowId + " = " + workflowId + ";", conn))
                {

                    using (var reader = command.ExecuteReader())
                    {

                        if (reader.Read())
                        {
                            var entry = new Entry
                            {
                                Id = (long)reader[Entry.ColumnName_Id],
                                Name = (string)reader[Entry.ColumnName_Name],
                                Description = (string)reader[Entry.ColumnName_Description],
                                LaunchType = (LaunchType)((long)reader[Entry.ColumnName_LaunchType]),
                                Status = (Status)((long)reader[Entry.ColumnName_Status]),
                                StatusDate = DateTime.Parse((string)reader[Entry.ColumnName_StatusDate]),
                                WorkflowId = (int)((long)reader[Entry.ColumnName_WorkflowId])
                            };

                            return entry;
                        }
                    }
                }

            }

            return null;
        }

        public override DateTime GetEntryStatusDateMax()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("SELECT " + Entry.ColumnName_StatusDate
                    + " FROM " + Core.Db.Entry.DocumentName
                    + " ORDER BY " + Entry.ColumnName_StatusDate + " DESC LIMIT 1;", conn))
                {

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var statusDate = DateTime.Parse((string)reader[Entry.ColumnName_StatusDate]);

                            return statusDate;
                        }
                    }
                }
            }

            return DateTime.Now;
        }

        public override DateTime GetEntryStatusDateMin()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("SELECT " + Entry.ColumnName_StatusDate
                    + " FROM " + Core.Db.Entry.DocumentName
                    + " ORDER BY " + Entry.ColumnName_StatusDate + " ASC LIMIT 1;", conn))
                {

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var statusDate = DateTime.Parse((string)reader[Entry.ColumnName_StatusDate]);

                            return statusDate;
                        }
                    }
                }
            }

            return DateTime.Now;
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries()
        {
            List<HistoryEntry> entries = new List<HistoryEntry>();

            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("SELECT "
                    + HistoryEntry.ColumnName_Id + ", "
                    + HistoryEntry.ColumnName_Name + ", "
                    + HistoryEntry.ColumnName_Description + ", "
                    + HistoryEntry.ColumnName_LaunchType + ", "
                    + HistoryEntry.ColumnName_Status + ", "
                    + HistoryEntry.ColumnName_StatusDate + ", "
                    + HistoryEntry.ColumnName_WorkflowId
                    + " FROM " + Core.Db.HistoryEntry.DocumentName + ";", conn))
                {

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var entry = new HistoryEntry
                            {
                                Id = (long)reader[HistoryEntry.ColumnName_Id],
                                Name = (string)reader[HistoryEntry.ColumnName_Name],
                                Description = (string)reader[HistoryEntry.ColumnName_Description],
                                LaunchType = (LaunchType)((long)reader[HistoryEntry.ColumnName_LaunchType]),
                                Status = (Status)((long)reader[HistoryEntry.ColumnName_Status]),
                                StatusDate = DateTime.Parse((string)reader[HistoryEntry.ColumnName_StatusDate]),
                                WorkflowId = (int)((long)reader[HistoryEntry.ColumnName_WorkflowId])
                            };

                            entries.Add(entry);
                        }
                    }

                }
            }

            return entries;
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword)
        {
            List<HistoryEntry> entries = new List<HistoryEntry>();

            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("SELECT "
                    + HistoryEntry.ColumnName_Id + ", "
                    + HistoryEntry.ColumnName_Name + ", "
                    + HistoryEntry.ColumnName_Description + ", "
                    + HistoryEntry.ColumnName_LaunchType + ", "
                    + HistoryEntry.ColumnName_Status + ", "
                    + HistoryEntry.ColumnName_StatusDate + ", "
                    + HistoryEntry.ColumnName_WorkflowId
                    + " FROM " + Core.Db.HistoryEntry.DocumentName
                    + " WHERE " + "LOWER(" + HistoryEntry.ColumnName_Name + ") LIKE '%" + keyword.ToLower() + "%'"
                    + " OR " + "LOWER(" + HistoryEntry.ColumnName_Description + ") LIKE '%" + keyword.ToLower() + "%';", conn))
                {

                    using (var reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            var entry = new HistoryEntry
                            {
                                Id = (long)reader[HistoryEntry.ColumnName_Id],
                                Name = (string)reader[HistoryEntry.ColumnName_Name],
                                Description = (string)reader[HistoryEntry.ColumnName_Description],
                                LaunchType = (LaunchType)((long)reader[HistoryEntry.ColumnName_LaunchType]),
                                Status = (Status)((long)reader[HistoryEntry.ColumnName_Status]),
                                StatusDate = DateTime.Parse((string)reader[HistoryEntry.ColumnName_StatusDate]),
                                WorkflowId = (int)((long)reader[HistoryEntry.ColumnName_WorkflowId])
                            };

                            entries.Add(entry);
                        }
                    }

                }

            }

            return entries;
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword, int page, int entriesCount)
        {
            List<HistoryEntry> entries = new List<HistoryEntry>();

            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("SELECT "
                    + HistoryEntry.ColumnName_Id + ", "
                    + HistoryEntry.ColumnName_Name + ", "
                    + HistoryEntry.ColumnName_Description + ", "
                    + HistoryEntry.ColumnName_LaunchType + ", "
                    + HistoryEntry.ColumnName_Status + ", "
                    + HistoryEntry.ColumnName_StatusDate + ", "
                    + HistoryEntry.ColumnName_WorkflowId
                    + " FROM " + Core.Db.HistoryEntry.DocumentName
                    + " WHERE " + "LOWER(" + HistoryEntry.ColumnName_Name + ") LIKE '%" + keyword.ToLower() + "%'"
                    + " OR " + "LOWER(" + HistoryEntry.ColumnName_Description + ") LIKE '%" + keyword.ToLower() + "%'"
                    + " LIMIT " + entriesCount + " OFFSET " + (page - 1) * entriesCount + ";"
                    , conn))
                {

                    using (var reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            var entry = new HistoryEntry
                            {
                                Id = (long)reader[HistoryEntry.ColumnName_Id],
                                Name = (string)reader[HistoryEntry.ColumnName_Name],
                                Description = (string)reader[HistoryEntry.ColumnName_Description],
                                LaunchType = (LaunchType)((long)reader[HistoryEntry.ColumnName_LaunchType]),
                                Status = (Status)((long)reader[HistoryEntry.ColumnName_Status]),
                                StatusDate = DateTime.Parse((string)reader[HistoryEntry.ColumnName_StatusDate]),
                                WorkflowId = (int)((long)reader[HistoryEntry.ColumnName_WorkflowId])
                            };

                            entries.Add(entry);
                        }
                    }

                }
            }
            return entries;
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy heo)
        {
            List<HistoryEntry> entries = new List<HistoryEntry>();

            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                var sqlBuilder = new StringBuilder("SELECT "
                    + HistoryEntry.ColumnName_Id + ", "
                    + HistoryEntry.ColumnName_Name + ", "
                    + HistoryEntry.ColumnName_Description + ", "
                    + HistoryEntry.ColumnName_LaunchType + ", "
                    + HistoryEntry.ColumnName_Status + ", "
                    + HistoryEntry.ColumnName_StatusDate + ", "
                    + HistoryEntry.ColumnName_WorkflowId
                    + " FROM " + Core.Db.HistoryEntry.DocumentName
                    + " WHERE " + "(LOWER(" + HistoryEntry.ColumnName_Name + ") LIKE '%" + keyword.ToLower() + "%'"
                    + " OR " + "LOWER(" + HistoryEntry.ColumnName_Description + ") LIKE '%" + keyword.ToLower() + "%')"
                    + " AND (" + HistoryEntry.ColumnName_StatusDate + " BETWEEN '" + from.ToString(DateTimeFormat) + "' AND '" + to.ToString(DateTimeFormat) + "')"
                    + " ORDER BY ");

                switch (heo)
                {
                    case EntryOrderBy.StatusDateAscending:

                        sqlBuilder.Append(HistoryEntry.ColumnName_StatusDate).Append(" ASC");
                        break;

                    case EntryOrderBy.StatusDateDescending:

                        sqlBuilder.Append(HistoryEntry.ColumnName_StatusDate).Append(" DESC");
                        break;

                    case EntryOrderBy.WorkflowIdAscending:

                        sqlBuilder.Append(HistoryEntry.ColumnName_WorkflowId).Append(" ASC");
                        break;

                    case EntryOrderBy.WorkflowIdDescending:

                        sqlBuilder.Append(HistoryEntry.ColumnName_WorkflowId).Append(" DESC");
                        break;

                    case EntryOrderBy.NameAscending:

                        sqlBuilder.Append(HistoryEntry.ColumnName_Name).Append(" ASC");
                        break;

                    case EntryOrderBy.NameDescending:

                        sqlBuilder.Append(HistoryEntry.ColumnName_Name).Append(" DESC");
                        break;

                    case EntryOrderBy.LaunchTypeAscending:

                        sqlBuilder.Append(HistoryEntry.ColumnName_LaunchType).Append(" ASC");
                        break;

                    case EntryOrderBy.LaunchTypeDescending:

                        sqlBuilder.Append(HistoryEntry.ColumnName_LaunchType).Append(" DESC");
                        break;

                    case EntryOrderBy.DescriptionAscending:

                        sqlBuilder.Append(HistoryEntry.ColumnName_Description).Append(" ASC");
                        break;

                    case EntryOrderBy.DescriptionDescending:

                        sqlBuilder.Append(HistoryEntry.ColumnName_Description).Append(" DESC");
                        break;

                    case EntryOrderBy.StatusAscending:

                        sqlBuilder.Append(HistoryEntry.ColumnName_Status).Append(" ASC");
                        break;

                    case EntryOrderBy.StatusDescending:

                        sqlBuilder.Append(HistoryEntry.ColumnName_Status).Append(" DESC");
                        break;
                }

                sqlBuilder.Append(" LIMIT ").Append(entriesCount).Append(" OFFSET ").Append((page - 1) * entriesCount).Append(";");

                using (var command = new SQLiteCommand(sqlBuilder.ToString(), conn))
                {

                    using (var reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            var entry = new HistoryEntry
                            {
                                Id = (long)reader[HistoryEntry.ColumnName_Id],
                                Name = (string)reader[HistoryEntry.ColumnName_Name],
                                Description = (string)reader[HistoryEntry.ColumnName_Description],
                                LaunchType = (LaunchType)((long)reader[HistoryEntry.ColumnName_LaunchType]),
                                Status = (Status)((long)reader[HistoryEntry.ColumnName_Status]),
                                StatusDate = DateTime.Parse((string)reader[HistoryEntry.ColumnName_StatusDate]),
                                WorkflowId = (int)((long)reader[HistoryEntry.ColumnName_WorkflowId])
                            };

                            entries.Add(entry);
                        }
                    }
                }
            }

            return entries;
        }

        public override long GetHistoryEntriesCount(string keyword)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("SELECT COUNT(*)"
                    + " FROM " + Core.Db.HistoryEntry.DocumentName
                    + " WHERE " + "LOWER(" + HistoryEntry.ColumnName_Name + ") LIKE '%" + keyword.ToLower() + "%'"
                    + " OR " + "LOWER(" + HistoryEntry.ColumnName_Description + ") LIKE '%" + keyword.ToLower() + "%';", conn))
                {

                    var count = (long)command.ExecuteScalar();

                    return count;
                }
            }
        }

        public override long GetHistoryEntriesCount(string keyword, DateTime from, DateTime to)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("SELECT COUNT(*)"
                    + " FROM " + Core.Db.HistoryEntry.DocumentName
                    + " WHERE " + "(LOWER(" + HistoryEntry.ColumnName_Name + ") LIKE '%" + keyword.ToLower() + "%'"
                    + " OR " + "LOWER(" + HistoryEntry.ColumnName_Description + ") LIKE '%" + keyword.ToLower() + "%')"
                    + " AND (" + HistoryEntry.ColumnName_StatusDate + " BETWEEN '" + from.ToString(DateTimeFormat) + "' AND '" + to.ToString(DateTimeFormat) + "');", conn))
                {

                    var count = (long)command.ExecuteScalar();

                    return count;
                }
            }
        }

        public override DateTime GetHistoryEntryStatusDateMax()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("SELECT " + HistoryEntry.ColumnName_StatusDate
                    + " FROM " + Core.Db.HistoryEntry.DocumentName
                    + " ORDER BY " + HistoryEntry.ColumnName_StatusDate + " DESC LIMIT 1;", conn))
                {

                    using (var reader = command.ExecuteReader())
                    {

                        if (reader.Read())
                        {
                            var statusDate = DateTime.Parse((string)reader[HistoryEntry.ColumnName_StatusDate]);

                            return statusDate;
                        }
                    }
                }
            }

            return DateTime.Now;
        }

        public override DateTime GetHistoryEntryStatusDateMin()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("SELECT " + HistoryEntry.ColumnName_StatusDate
                    + " FROM " + Core.Db.HistoryEntry.DocumentName
                    + " ORDER BY " + HistoryEntry.ColumnName_StatusDate + " ASC LIMIT 1;", conn))
                {

                    using (var reader = command.ExecuteReader())
                    {

                        if (reader.Read())
                        {
                            var statusDate = DateTime.Parse((string)reader[HistoryEntry.ColumnName_StatusDate]);

                            return statusDate;
                        }
                    }
                }
            }

            return DateTime.Now;
        }

        public override string GetPassword(string username)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("SELECT " + User.ColumnName_Password
                    + " FROM " + Core.Db.User.DocumentName
                    + " WHERE " + User.ColumnName_Username + " = '" + username + "'"
                    + ";", conn))
                {

                    using (var reader = command.ExecuteReader())
                    {

                        if (reader.Read())
                        {
                            var password = (string)reader[User.ColumnName_Password];

                            return password;
                        }
                    }
                }
            }

            return null;
        }

        public override Core.Db.StatusCount GetStatusCount()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("SELECT " + StatusCount.ColumnName_Id + ", "
                    + StatusCount.ColumnName_PendingCount + ", "
                    + StatusCount.ColumnName_RunningCount + ", "
                    + StatusCount.ColumnName_DoneCount + ", "
                    + StatusCount.ColumnName_FailedCount + ", "
                    + StatusCount.ColumnName_WarningCount + ", "
                    + StatusCount.ColumnName_DisabledCount + ", "
                    + StatusCount.ColumnName_StoppedCount + ", "
                    + StatusCount.ColumnName_DisapprovedCount
                    + " FROM " + Core.Db.StatusCount.DocumentName
                    + ";", conn))
                {
                    using (var reader = command.ExecuteReader())
                    {

                        if (reader.Read())
                        {
                            var statusCount = new StatusCount
                            {
                                Id = (long)reader[StatusCount.ColumnName_Id],
                                PendingCount = (int)(long)reader[StatusCount.ColumnName_PendingCount],
                                RunningCount = (int)(long)reader[StatusCount.ColumnName_RunningCount],
                                DoneCount = (int)(long)reader[StatusCount.ColumnName_DoneCount],
                                FailedCount = (int)(long)reader[StatusCount.ColumnName_FailedCount],
                                WarningCount = (int)(long)reader[StatusCount.ColumnName_WarningCount],
                                DisabledCount = (int)(long)reader[StatusCount.ColumnName_DisabledCount],
                                StoppedCount = (int)(long)reader[StatusCount.ColumnName_StoppedCount],
                                DisapprovedCount = (int)(long)reader[StatusCount.ColumnName_DisapprovedCount]
                            };

                            return statusCount;
                        }
                    }
                }
            }

            return null;
        }

        public override Core.Db.User GetUser(string username)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("SELECT " + User.ColumnName_Id + ", "
                    + User.ColumnName_Username + ", "
                    + User.ColumnName_Password + ", "
                    + User.ColumnName_Email + ", "
                    + User.ColumnName_UserProfile + ", "
                    + User.ColumnName_CreatedOn + ", "
                    + User.ColumnName_ModifiedOn
                    + " FROM " + Core.Db.User.DocumentName
                    + " WHERE " + User.ColumnName_Username + " = '" + username + "'"
                    + ";", conn))
                {

                    using (var reader = command.ExecuteReader())
                    {

                        if (reader.Read())
                        {
                            var user = new User
                            {
                                Id = (long)reader[User.ColumnName_Id],
                                Username = (string)reader[User.ColumnName_Username],
                                Password = (string)reader[User.ColumnName_Password],
                                Email = (string)reader[User.ColumnName_Email],
                                UserProfile = (UserProfile)((long)reader[User.ColumnName_UserProfile]),
                                CreatedOn = DateTime.Parse((string)reader[User.ColumnName_CreatedOn]),
                                ModifiedOn = reader[User.ColumnName_ModifiedOn] == DBNull.Value ? DateTime.MinValue : DateTime.Parse((string)reader[User.ColumnName_ModifiedOn])
                            };

                            return user;
                        }
                    }
                }
            }

            return null;
        }

        public override Core.Db.User GetUserByUserId(string userId)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("SELECT " + User.ColumnName_Id + ", "
                    + User.ColumnName_Username + ", "
                    + User.ColumnName_Password + ", "
                    + User.ColumnName_Email + ", "
                    + User.ColumnName_UserProfile + ", "
                    + User.ColumnName_CreatedOn + ", "
                    + User.ColumnName_ModifiedOn
                    + " FROM " + Core.Db.User.DocumentName
                    + " WHERE " + User.ColumnName_Id + " = '" + int.Parse(userId) + "'"
                    + ";", conn))
                {

                    using (var reader = command.ExecuteReader())
                    {

                        if (reader.Read())
                        {
                            var user = new User
                            {
                                Id = (long)reader[User.ColumnName_Id],
                                Username = (string)reader[User.ColumnName_Username],
                                Password = (string)reader[User.ColumnName_Password],
                                Email = (string)reader[User.ColumnName_Email],
                                UserProfile = (UserProfile)((long)reader[User.ColumnName_UserProfile]),
                                CreatedOn = DateTime.Parse((string)reader[User.ColumnName_CreatedOn]),
                                ModifiedOn = reader[User.ColumnName_ModifiedOn] == DBNull.Value ? DateTime.MinValue : DateTime.Parse((string)reader[User.ColumnName_ModifiedOn])
                            };

                            return user;
                        }
                    }
                }
            }

            return null;
        }

        public override IEnumerable<Core.Db.User> GetUsers()
        {
            List<User> users = new List<User>();

            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("SELECT " + User.ColumnName_Id + ", "
                    + User.ColumnName_Username + ", "
                    + User.ColumnName_Password + ", "
                    + User.ColumnName_Email + ", "
                    + User.ColumnName_UserProfile + ", "
                    + User.ColumnName_CreatedOn + ", "
                    + User.ColumnName_ModifiedOn
                    + " FROM " + Core.Db.User.DocumentName
                    + ";", conn))
                {

                    using (var reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            var user = new User
                            {
                                Id = (long)reader[User.ColumnName_Id],
                                Username = (string)reader[User.ColumnName_Username],
                                Password = (string)reader[User.ColumnName_Password],
                                Email = (string)reader[User.ColumnName_Email],
                                UserProfile = (UserProfile)((long)reader[User.ColumnName_UserProfile]),
                                CreatedOn = DateTime.Parse((string)reader[User.ColumnName_CreatedOn]),
                                ModifiedOn = reader[User.ColumnName_ModifiedOn] == DBNull.Value ? DateTime.MinValue : DateTime.Parse((string)reader[User.ColumnName_ModifiedOn])
                            };

                            users.Add(user);
                        }
                    }
                }
            }

            return users;
        }

        public override IEnumerable<Core.Db.User> GetUsers(string keyword, UserOrderBy uo)
        {
            List<User> users = new List<User>();

            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("SELECT " + User.ColumnName_Id + ", "
                    + User.ColumnName_Username + ", "
                    + User.ColumnName_Password + ", "
                    + User.ColumnName_Email + ", "
                    + User.ColumnName_UserProfile + ", "
                    + User.ColumnName_CreatedOn + ", "
                    + User.ColumnName_ModifiedOn
                    + " FROM " + Core.Db.User.DocumentName
                    + " WHERE " + "LOWER(" + User.ColumnName_Username + ")" + " LIKE '%" + keyword.ToLower() + "%'"
                    + " ORDER BY " + User.ColumnName_Username + (uo == UserOrderBy.UsernameAscending ? " ASC" : " DESC")
                    + ";", conn))
                {

                    using (var reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            var user = new User
                            {
                                Id = (long)reader[User.ColumnName_Id],
                                Username = (string)reader[User.ColumnName_Username],
                                Password = (string)reader[User.ColumnName_Password],
                                Email = (string)reader[User.ColumnName_Email],
                                UserProfile = (UserProfile)((long)reader[User.ColumnName_UserProfile]),
                                CreatedOn = DateTime.Parse((string)reader[User.ColumnName_CreatedOn]),
                                ModifiedOn = reader[User.ColumnName_ModifiedOn] == DBNull.Value ? DateTime.MinValue : DateTime.Parse((string)reader[User.ColumnName_ModifiedOn])
                            };

                            users.Add(user);
                        }
                    }
                }
            }

            return users;
        }

        public override IEnumerable<string> GetUserWorkflows(string userId)
        {
            List<string> workflowIds = new List<string>();

            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("SELECT " + UserWorkflow.ColumnName_Id + ", "
                    + UserWorkflow.ColumnName_UserId + ", "
                    + UserWorkflow.ColumnName_WorkflowId
                    + " FROM " + Core.Db.UserWorkflow.DocumentName
                    + " WHERE " + UserWorkflow.ColumnName_UserId + " = " + int.Parse(userId)
                    + ";", conn))
                {

                    using (var reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            var workflowId = (long)reader[UserWorkflow.ColumnName_WorkflowId];

                            workflowIds.Add(workflowId.ToString());
                        }
                    }
                }
            }

            return workflowIds;
        }

        public override Core.Db.Workflow GetWorkflow(string id)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("SELECT " + Workflow.ColumnName_Id + ", "
                    + Workflow.ColumnName_Xml
                    + " FROM " + Core.Db.Workflow.DocumentName
                    + " WHERE " + Workflow.ColumnName_Id + " = " + int.Parse(id) + ";", conn))
                {

                    using (var reader = command.ExecuteReader())
                    {

                        if (reader.Read())
                        {
                            var workflow = new Workflow
                            {
                                Id = (long)reader[Workflow.ColumnName_Id],
                                Xml = (string)reader[Workflow.ColumnName_Xml]
                            };

                            return workflow;
                        }
                    }
                }
            }

            return null;
        }

        public override IEnumerable<Core.Db.Workflow> GetWorkflows()
        {
            List<Core.Db.Workflow> workflows = new List<Core.Db.Workflow>();

            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("SELECT " + Workflow.ColumnName_Id + ", "
                    + Workflow.ColumnName_Xml
                    + " FROM " + Core.Db.Workflow.DocumentName + ";", conn))
                {

                    using (var reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            var workflow = new Workflow
                            {
                                Id = (long)reader[Workflow.ColumnName_Id],
                                Xml = (string)reader[Workflow.ColumnName_Xml]
                            };

                            workflows.Add(workflow);
                        }
                    }
                }
            }

            return workflows;
        }

        public override void IncrementDisabledCount()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command1 = new SQLiteCommand("SELECT " + StatusCount.ColumnName_DisabledCount + " FROM " + Core.Db.StatusCount.DocumentName + ";", conn))
                {

                    var count = (long)command1.ExecuteScalar();

                    count++;

                    using (var command2 = new SQLiteCommand("UPDATE " + Core.Db.StatusCount.DocumentName + " SET " + StatusCount.ColumnName_DisabledCount + " = " + count + ";", conn))
                    {
                        command2.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void IncrementDisapprovedCount()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command1 = new SQLiteCommand("SELECT " + StatusCount.ColumnName_DisapprovedCount + " FROM " + Core.Db.StatusCount.DocumentName + ";", conn))
                {

                    var count = (long)command1.ExecuteScalar();

                    count++;

                    using (var command2 = new SQLiteCommand("UPDATE " + Core.Db.StatusCount.DocumentName + " SET " + StatusCount.ColumnName_DisapprovedCount + " = " + count + ";", conn))
                    {

                        command2.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void IncrementDoneCount()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command1 = new SQLiteCommand("SELECT " + StatusCount.ColumnName_DoneCount + " FROM " + Core.Db.StatusCount.DocumentName + ";", conn))
                {

                    var count = (long)command1.ExecuteScalar();

                    count++;

                    using (var command2 = new SQLiteCommand("UPDATE " + Core.Db.StatusCount.DocumentName + " SET " + StatusCount.ColumnName_DoneCount + " = " + count + ";", conn))
                    {

                        command2.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void IncrementFailedCount()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command1 = new SQLiteCommand("SELECT " + StatusCount.ColumnName_FailedCount + " FROM " + Core.Db.StatusCount.DocumentName + ";", conn))
                {

                    var count = (long)command1.ExecuteScalar();

                    count++;

                    using (var command2 = new SQLiteCommand("UPDATE " + Core.Db.StatusCount.DocumentName + " SET " + StatusCount.ColumnName_FailedCount + " = " + count + ";", conn))
                    {
                        command2.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void IncrementPendingCount()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command1 = new SQLiteCommand("SELECT " + StatusCount.ColumnName_PendingCount + " FROM " + Core.Db.StatusCount.DocumentName + ";", conn))
                {

                    var count = (long)command1.ExecuteScalar();

                    count++;

                    using (var command2 = new SQLiteCommand("UPDATE " + Core.Db.StatusCount.DocumentName + " SET " + StatusCount.ColumnName_PendingCount + " = " + count + ";", conn))
                    {

                        command2.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void IncrementRunningCount()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command1 = new SQLiteCommand("SELECT " + StatusCount.ColumnName_RunningCount + " FROM " + Core.Db.StatusCount.DocumentName + ";", conn))
                {

                    var count = (long)command1.ExecuteScalar();

                    count++;

                    using (var command2 = new SQLiteCommand("UPDATE " + Core.Db.StatusCount.DocumentName + " SET " + StatusCount.ColumnName_RunningCount + " = " + count + ";", conn))
                    {

                        command2.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void IncrementStoppedCount()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command1 = new SQLiteCommand("SELECT " + StatusCount.ColumnName_StoppedCount + " FROM " + Core.Db.StatusCount.DocumentName + ";", conn))
                {

                    var count = (long)command1.ExecuteScalar();

                    count++;

                    using (var command2 = new SQLiteCommand("UPDATE " + Core.Db.StatusCount.DocumentName + " SET " + StatusCount.ColumnName_StoppedCount + " = " + count + ";", conn))
                    {

                        command2.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void IncrementWarningCount()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command1 = new SQLiteCommand("SELECT " + StatusCount.ColumnName_WarningCount + " FROM " + Core.Db.StatusCount.DocumentName + ";", conn))
                {

                    var count = (long)command1.ExecuteScalar();

                    count++;

                    using (var command2 = new SQLiteCommand("UPDATE " + Core.Db.StatusCount.DocumentName + " SET " + StatusCount.ColumnName_WarningCount + " = " + count + ";", conn))
                    {

                        command2.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void InsertEntry(Core.Db.Entry entry)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("INSERT INTO " + Core.Db.Entry.DocumentName + "("
                    + Entry.ColumnName_Name + ", "
                    + Entry.ColumnName_Description + ", "
                    + Entry.ColumnName_LaunchType + ", "
                    + Entry.ColumnName_StatusDate + ", "
                    + Entry.ColumnName_Status + ", "
                    + Entry.ColumnName_WorkflowId + ", "
                    + Entry.ColumnName_Logs + ") VALUES("
                    + "'" + entry.Name + "'" + ", "
                    + "'" + entry.Description + "'" + ", "
                    + (int)entry.LaunchType + ", "
                    + "'" + entry.StatusDate.ToString(DateTimeFormat) + "'" + ", "
                    + (int)entry.Status + ", "
                    + entry.WorkflowId + ", "
                    + "'" + (entry.Logs ?? "").Replace("'", "''") + "'" + ");"
                    , conn))
                {

                    command.ExecuteNonQuery();
                }
            }
        }

        public override void InsertHistoryEntry(Core.Db.HistoryEntry entry)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("INSERT INTO " + Core.Db.HistoryEntry.DocumentName + "("
                    + HistoryEntry.ColumnName_Name + ", "
                    + HistoryEntry.ColumnName_Description + ", "
                    + HistoryEntry.ColumnName_LaunchType + ", "
                    + HistoryEntry.ColumnName_StatusDate + ", "
                    + HistoryEntry.ColumnName_Status + ", "
                    + HistoryEntry.ColumnName_WorkflowId + ", "
                    + HistoryEntry.ColumnName_Logs + ") VALUES("
                    + "'" + entry.Name + "'" + ", "
                    + "'" + entry.Description + "'" + ", "
                    + (int)entry.LaunchType + ", "
                    + "'" + entry.StatusDate.ToString(DateTimeFormat) + "'" + ", "
                    + (int)entry.Status + ", "
                    + entry.WorkflowId + ", "
                    + "'" + (entry.Logs ?? "").Replace("'", "''") + "'" + ");"
                    , conn))
                {

                    command.ExecuteNonQuery();
                }
            }
        }

        public override void InsertUser(Core.Db.User user)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("INSERT INTO " + Core.Db.User.DocumentName + "("
                    + User.ColumnName_Username + ", "
                    + User.ColumnName_Password + ", "
                    + User.ColumnName_UserProfile + ", "
                    + User.ColumnName_Email + ", "
                    + User.ColumnName_CreatedOn + ", "
                    + User.ColumnName_ModifiedOn + ") VALUES("
                    + "'" + user.Username + "'" + ", "
                    + "'" + user.Password + "'" + ", "
                    + (int)user.UserProfile + ", "
                    + "'" + user.Email + "'" + ", "
                    + "'" + DateTime.Now.ToString(DateTimeFormat) + "'" + ", "
                    + (user.ModifiedOn == DateTime.MinValue ? "NULL" : "'" + user.ModifiedOn.ToString(DateTimeFormat) + "'") + ");"
                    , conn))
                {

                    command.ExecuteNonQuery();
                }
            }
        }

        public override void InsertUserWorkflowRelation(Core.Db.UserWorkflow userWorkflow)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("INSERT INTO " + Core.Db.UserWorkflow.DocumentName + "("
                    + UserWorkflow.ColumnName_UserId + ", "
                    + UserWorkflow.ColumnName_WorkflowId + ") VALUES("
                    + int.Parse(userWorkflow.UserId) + ", "
                    + int.Parse(userWorkflow.WorkflowId) + ");"
                    , conn))
                {

                    command.ExecuteNonQuery();
                }
            }
        }

        public override string InsertWorkflow(Core.Db.Workflow workflow)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("INSERT INTO " + Core.Db.Workflow.DocumentName + "("
                    + Workflow.ColumnName_Xml + ") VALUES("
                    + "'" + workflow.Xml.Replace("'", "''") + "'" + "); SELECT last_insert_rowid();; "
                    , conn))
                {

                    var id = (long)command.ExecuteScalar();

                    return id.ToString();
                }
            }
        }

        public override void UpdateEntry(string id, Core.Db.Entry entry)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("UPDATE " + Core.Db.Entry.DocumentName + " SET "
                    + Entry.ColumnName_Name + " = '" + entry.Name + "', "
                    + Entry.ColumnName_Description + " = '" + entry.Description + "', "
                    + Entry.ColumnName_LaunchType + " = " + (int)entry.LaunchType + ", "
                    + Entry.ColumnName_StatusDate + " = '" + entry.StatusDate.ToString(DateTimeFormat) + "', "
                    + Entry.ColumnName_Status + " = " + (int)entry.Status + ", "
                    + Entry.ColumnName_WorkflowId + " = " + entry.WorkflowId + ", "
                    + Entry.ColumnName_Logs + " = '" + (entry.Logs ?? "").Replace("'", "''") + "'"
                    + " WHERE "
                    + Entry.ColumnName_Id + " = " + int.Parse(id) + ";"
                    , conn))
                {

                    command.ExecuteNonQuery();
                }
            }
        }

        public override void UpdatePassword(string username, string password)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("UPDATE " + Core.Db.User.DocumentName + " SET "
                    + User.ColumnName_Password + " = '" + password + "'"
                    + " WHERE "
                    + User.ColumnName_Username + " = '" + username + "';"
                    , conn))
                {

                    command.ExecuteNonQuery();
                }
            }
        }

        public override void UpdateUser(string id, Core.Db.User user)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("UPDATE " + Core.Db.User.DocumentName + " SET "
                    + User.ColumnName_Username + " = '" + user.Username + "', "
                    + User.ColumnName_Password + " = '" + user.Password + "', "
                    + User.ColumnName_UserProfile + " = " + (int)user.UserProfile + ", "
                    + User.ColumnName_Email + " = '" + user.Email + "', "
                    + User.ColumnName_CreatedOn + " = '" + user.CreatedOn.ToString(DateTimeFormat) + "', "
                    + User.ColumnName_ModifiedOn + " = '" + DateTime.Now.ToString(DateTimeFormat) + "'"
                    + " WHERE "
                    + User.ColumnName_Id + " = " + int.Parse(id) + ";"
                    , conn))
                {

                    command.ExecuteNonQuery();
                }
            }
        }

        public override void UpdateUsernameAndEmailAndUserProfile(string userId, string username, string email, UserProfile up)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("UPDATE " + Core.Db.User.DocumentName + " SET "
                    + User.ColumnName_Username + " = '" + username + "', "
                    + User.ColumnName_UserProfile + " = " + (int)up + ", "
                    + User.ColumnName_Email + " = '" + email + "', "
                    + User.ColumnName_ModifiedOn + " = '" + DateTime.Now.ToString(DateTimeFormat) + "'"
                    + " WHERE "
                    + User.ColumnName_Id + " = " + int.Parse(userId) + ";"
                    , conn))
                {

                    command.ExecuteNonQuery();
                }
            }
        }

        public override void UpdateWorkflow(string dbId, Core.Db.Workflow workflow)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand("UPDATE " + Core.Db.Workflow.DocumentName + " SET "
                    + Workflow.ColumnName_Xml + " = '" + workflow.Xml.Replace("'", "''") + "'"
                    + " WHERE "
                    + User.ColumnName_Id + " = " + int.Parse(dbId) + ";"
                    , conn))
                {

                    command.ExecuteNonQuery();
                }
            }
        }

        public override string GetEntryLogs(string entryId)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                var command = new SQLiteCommand("SELECT " + Entry.ColumnName_Logs
                    + " FROM " + Core.Db.Entry.DocumentName
                    + " WHERE "
                    + Entry.ColumnName_Id + " = " + int.Parse(entryId) + ";"
                    , conn);

                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    var logs = (string)reader[Entry.ColumnName_Logs];
                    return logs;
                }

            }

            return null;
        }

        public override string GetHistoryEntryLogs(string entryId)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                var command = new SQLiteCommand("SELECT " + HistoryEntry.ColumnName_Logs
                    + " FROM " + Core.Db.HistoryEntry.DocumentName
                    + " WHERE "
                    + HistoryEntry.ColumnName_Id + " = " + int.Parse(entryId) + ";"
                    , conn);

                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    var logs = (string)reader[HistoryEntry.ColumnName_Logs];
                    return logs;
                }

            }

            return null;
        }
    }
}
