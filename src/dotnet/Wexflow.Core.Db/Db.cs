using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
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

    public class Db
    {
        private static LiteDatabase db;

        public string ConnectionString { get; private set; }


        public Db(string connectionString)
        {
            ConnectionString = connectionString;
            db = new LiteDatabase(ConnectionString);
        }

        public void Init()
        {
            // StatusCount
            ClearStatusCount();
            var statusCountCol = db.GetCollection<StatusCount>("statusCount");

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

            statusCountCol.Insert(statusCount);

            // Entries
            ClearEntries();

            // Insert default user if necessary
            var usersCol = db.GetCollection<User>("users");
            if (usersCol.Count() == 0)
            {
                InsertDefaultUser();
            }
        }

        public void ClearStatusCount()
        {
            var col = db.GetCollection<StatusCount>("statusCount");
            var statusCount = col.FindAll();
            col.Delete(s => statusCount.Where(ss => ss.Id == s.Id).Count() > 0);
        }

        public void ClearEntries()
        {
            var col = db.GetCollection<StatusCount>("entries");
            var entries = col.FindAll();
            col.Delete(e => entries.Where(ee => ee.Id == e.Id).Count() > 0);
        }

        public StatusCount GetStatusCount()
        {
            var col = db.GetCollection<StatusCount>("statusCount");
            var statusCount = col.FindAll().FirstOrDefault();
            return statusCount;
        }

        public void IncrementPendingCount()
        {
            var col = db.GetCollection<StatusCount>("statusCount");
            var statusCount = col.FindAll().FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.PendingCount++;
                col.Update(statusCount);
            }
        }

        public void DecrementPendingCount()
        {
            var col = db.GetCollection<StatusCount>("statusCount");
            var statusCount = col.FindAll().FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.PendingCount--;
                col.Update(statusCount);
            }
        }

        public void IncrementRunningCount()
        {
            var col = db.GetCollection<StatusCount>("statusCount");
            var statusCount = col.FindAll().FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.RunningCount++;
                col.Update(statusCount);
            }
        }

        public void DecrementRunningCount()
        {
            var col = db.GetCollection<StatusCount>("statusCount");
            var statusCount = col.FindAll().FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.RunningCount--;
                col.Update(statusCount);
            }
        }

        public void IncrementDoneCount()
        {
            var col = db.GetCollection<StatusCount>("statusCount");
            var statusCount = col.FindAll().FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.DoneCount++;
                col.Update(statusCount);
            }
        }

        public void DecrementDoneCount()
        {
            var col = db.GetCollection<StatusCount>("statusCount");
            var statusCount = col.FindAll().FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.DoneCount--;
                col.Update(statusCount);
            }
        }

        public void IncrementFailedCount()
        {
            var col = db.GetCollection<StatusCount>("statusCount");
            var statusCount = col.FindAll().FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.FailedCount++;
                col.Update(statusCount);
            }
        }

        public void DecrementFailedCount()
        {
            var col = db.GetCollection<StatusCount>("statusCount");
            var statusCount = col.FindAll().FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.FailedCount--;
                col.Update(statusCount);
            }
        }

        public void IncrementWarningCount()
        {
            var col = db.GetCollection<StatusCount>("statusCount");
            var statusCount = col.FindAll().FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.WarningCount++;
                col.Update(statusCount);
            }
        }

        public void DecrementWarningCount()
        {
            var col = db.GetCollection<StatusCount>("statusCount");
            var statusCount = col.FindAll().FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.WarningCount--;
                col.Update(statusCount);
            }
        }

        public void IncrementDisabledCount()
        {
            var col = db.GetCollection<StatusCount>("statusCount");
            var statusCount = col.FindAll().FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.DisabledCount++;
                col.Update(statusCount);
            }
        }

        public void DecrementDisabledCount()
        {
            var col = db.GetCollection<StatusCount>("statusCount");
            var statusCount = col.FindAll().FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.DisabledCount--;
                col.Update(statusCount);
            }
        }

        public void IncrementStoppedCount()
        {
            var col = db.GetCollection<StatusCount>("statusCount");
            var statusCount = col.FindAll().FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.StoppedCount++;
                col.Update(statusCount);
            }
        }

        public void DecrementStoppedCount()
        {
            var col = db.GetCollection<StatusCount>("statusCount");
            var statusCount = col.FindAll().FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.StoppedCount--;
                col.Update(statusCount);
            }
        }

        public void ResetStatusCount()
        {
            var col = db.GetCollection<StatusCount>("statusCount");
            var statusCount = col.FindAll().FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.PendingCount = 0;
                statusCount.RunningCount = 0;
                statusCount.DoneCount = 0;
                statusCount.FailedCount = 0;
                statusCount.WarningCount = 0;
                statusCount.DisabledCount = 0;
                col.Update(statusCount);
            }
        }

        public IEnumerable<Entry> GetEntries()
        {
            var col = db.GetCollection<Entry>("entries");
            return col.FindAll();
        }

        public Entry GetEntry(int workflowId)
        {
            var col = db.GetCollection<Entry>("entries");
            return col.FindOne(e => e.WorkflowId == workflowId);
        }

        public void InsertEntry(Entry entry)
        {
            var col = db.GetCollection<Entry>("entries");
            col.Insert(entry);
            col.EnsureIndex(e => e.WorkflowId);
            col.EnsureIndex(e => e.Name, "LOWER($.Name)");
            col.EnsureIndex(e => e.LaunchType);
            col.EnsureIndex(e => e.Description, "LOWER($.Name)");
            col.EnsureIndex(e => e.Status);
            col.EnsureIndex(e => e.StatusDate);
        }

        public void UpdateEntry(Entry entry)
        {
            var col = db.GetCollection<Entry>("entries");
            col.Update(entry);
        }

        public void DeleteEntry(int workflowId)
        {
            var col = db.GetCollection<Entry>("entries");
            col.Delete(e => e.WorkflowId == workflowId);
        }

        public void InsertUser(User user)
        {
            var col = db.GetCollection<User>("users");
            user.CreatedOn = DateTime.Now;
            col.Insert(user);
            col.EnsureIndex(u => u.Username, "LOWER($.Username)");
            col.EnsureIndex(u => u.UserProfile);
        }

        public void UpdatePassword(string username, string password)
        {
            var col = db.GetCollection<User>("users");
            var dbUser = col.FindOne(u => u.Username == username);
            dbUser.Password = password;
            col.Update(dbUser);
        }

        public void UpdateUser(User user)
        {
            var col = db.GetCollection<User>("users");
            var dbUser = col.FindOne(u => u.Id == user.Id);
            dbUser.ModifiedOn = DateTime.Now;
            dbUser.Username = user.Username;
            dbUser.Password = user.Password;
            dbUser.UserProfile = user.UserProfile;
            dbUser.Email = user.Email;
            col.Update(dbUser);
        }

        public void UpdateUsernameAndEmailAndUserProfile(int userId, string username, string email, UserProfile up)
        {
            var col = db.GetCollection<User>("users");
            var dbUser = col.FindOne(u => u.Id == userId);
            dbUser.ModifiedOn = DateTime.Now;
            dbUser.Username = username;
            dbUser.Email = email;
            dbUser.UserProfile = up;
            col.Update(dbUser);
        }

        public void DeleteUser(string username, string password)
        {
            var col = db.GetCollection<User>("users");
            var user = col.FindOne(u => u.Username == username);
            if (user != null && user.Password == password)
            {
                col.Delete(u => u.Username == username);
            }
            else
            {
                throw new Exception("The password is incorrect.");
            }
        }

        public void InsertDefaultUser()
        {
            var password = GetMd5("wexflow2018");
            var user = new User { Username = "admin", Password = password, UserProfile = UserProfile.Administrator };
            InsertUser(user);
        }

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

        public User GetUser(string username)
        {
            var col = db.GetCollection<User>("users");
            User user = col.FindOne(u => u.Username == username);
            return user;
        }

        public string GetPassword(string username)
        {
            var col = db.GetCollection<User>("users");
            User user = col.FindOne(u => u.Username == username);
            return user.Password;
        }

        public IEnumerable<User> GetUsers()
        {
            var col = db.GetCollection<User>("users");
            return col.FindAll();
        }

        public IEnumerable<User> GetUsers(string keyword, UserOrderBy uo)
        {
            var col = db.GetCollection<User>("users");
            var keywordToLower = keyword.ToLower();
            Query query = null;

            if (!string.IsNullOrEmpty(keyword))
            {
                query = Query.Contains("Username", keywordToLower);
            }

            switch (uo)
            {
                case UserOrderBy.UsernameAscending:
                    if (query != null)
                    {
                        return col.Find(Query.And(Query.All("Username"), query));
                    }
                    else
                    {
                        return col.Find(Query.All("Username"));
                    }

                case UserOrderBy.UsernameDescending:

                    if (query != null)
                    {
                        return col.Find(Query.And(Query.All("Username", Query.Descending), query));
                    }
                    else
                    {
                        return col.Find(Query.All("Username", Query.Descending));
                    }
            }

            return new User[] { };
        }

        public void InsertHistoryEntry(HistoryEntry entry)
        {
            var col = db.GetCollection<HistoryEntry>("historyEntries");
            col.Insert(entry);
            col.EnsureIndex(e => e.WorkflowId);
            col.EnsureIndex(e => e.Name, "LOWER($.Name)");
            col.EnsureIndex(e => e.LaunchType);
            col.EnsureIndex(e => e.Description, "LOWER($.Name)");
            col.EnsureIndex(e => e.Status);
            col.EnsureIndex(e => e.StatusDate);
        }

        public void UpdateHistoryEntry(HistoryEntry entry)
        {
            var col = db.GetCollection<HistoryEntry>("historyEntries");
            col.Update(entry);
        }

        public void DeleteHistoryEntries(int workflowId)
        {
            var col = db.GetCollection<HistoryEntry>("historyEntries");
            col.Delete(e => e.WorkflowId == workflowId);
        }

        public IEnumerable<HistoryEntry> GetHistoryEntries()
        {
            var col = db.GetCollection<HistoryEntry>("historyEntries");
            return col.FindAll();
        }

        public IEnumerable<HistoryEntry> GetHistoryEntries(string keyword)
        {
            var keywordToUpper = keyword.ToUpper();
            var col = db.GetCollection<HistoryEntry>("historyEntries");
            return col.Find(e => e.Name.ToUpper().Contains(keywordToUpper) || e.Description.ToUpper().Contains(keywordToUpper));
        }

        public IEnumerable<HistoryEntry> GetHistoryEntries(string keyword, int page, int entriesCount)
        {
            var keywordToUpper = keyword.ToUpper();
            var col = db.GetCollection<HistoryEntry>("historyEntries");
            return col.Find(e => e.Name.ToUpper().Contains(keywordToUpper) || e.Description.ToUpper().Contains(keywordToUpper), (page - 1) * entriesCount, entriesCount);
        }

        public IEnumerable<HistoryEntry> GetHistoryEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy heo)
        {
            var col = db.GetCollection<HistoryEntry>("historyEntries");
            var keywordToLower = keyword.ToLower();
            int skip = (page - 1) * entriesCount;
            Query query;

            if (!string.IsNullOrEmpty(keyword))
            {
                query = Query.And(Query.Or(Query.Contains("Name", keywordToLower), Query.Contains("Description", keywordToLower))
                                , Query.And(Query.GTE("StatusDate", from), Query.LTE("StatusDate", to)));
            }
            else
            {
                query = Query.And(Query.GTE("StatusDate", from), Query.LTE("StatusDate", to));
            }

            switch (heo)
            {
                case EntryOrderBy.StatusDateAscending:

                    return col.Find(
                        Query.And(
                            Query.All("StatusDate")
                            , query
                        )
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.StatusDateDescending:

                    return col.Find(
                        Query.And(
                            Query.All("StatusDate", Query.Descending)
                            , query
                        )
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.WorkflowIdAscending:

                    return col.Find(
                        Query.And(
                            Query.All("WorkflowId")
                            , query
                        )
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.WorkflowIdDescending:

                    return col.Find(
                        Query.And(
                            Query.All("WorkflowId", Query.Descending)
                            , query
                        )
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.NameAscending:

                    return col.Find(
                        Query.And(
                            Query.All("Name")
                            , query
                        )
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.NameDescending:

                    return col.Find(
                        Query.And(
                            Query.All("Name", Query.Descending)
                            , query
                        )
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.LaunchTypeAscending:

                    return col.Find(
                        Query.And(
                            Query.All("LaunchType")
                            , query
                        )
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.LaunchTypeDescending:

                    return col.Find(
                        Query.And(
                            Query.All("LaunchType", Query.Descending)
                            , query
                        )
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.DescriptionAscending:

                    return col.Find(
                        Query.And(
                            Query.All("Description")
                            , query
                        )
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.DescriptionDescending:

                    return col.Find(
                        Query.And(
                            Query.All("Description", Query.Descending)
                            , query
                        )
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.StatusAscending:

                    return col.Find(
                        Query.And(
                            Query.All("Status")
                            , query
                        )
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.StatusDescending:

                    return col.Find(
                        Query.And(
                            Query.All("Status", Query.Descending)
                            , query
                        )
                        , skip
                        , entriesCount
                    );
            }

            return new HistoryEntry[] { };
        }

        public IEnumerable<Entry> GetEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy heo)
        {
            var col = db.GetCollection<Entry>("entries");
            var keywordToLower = keyword.ToLower();
            int skip = (page - 1) * entriesCount;
            Query query;

            if (!string.IsNullOrEmpty(keyword))
            {
                query = Query.And(Query.Or(Query.Contains("Name", keywordToLower), Query.Contains("Description", keywordToLower))
                                , Query.And(Query.GTE("StatusDate", from), Query.LTE("StatusDate", to)));
            }
            else
            {
                query = Query.And(Query.GTE("StatusDate", from), Query.LTE("StatusDate", to));
            }

            switch (heo)
            {
                case EntryOrderBy.StatusDateAscending:

                    return col.Find(
                        Query.And(
                            Query.All("StatusDate")
                            , query
                        )
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.StatusDateDescending:

                    return col.Find(
                        Query.And(
                            Query.All("StatusDate", Query.Descending)
                            , query
                        )
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.WorkflowIdAscending:

                    return col.Find(
                        Query.And(
                            Query.All("WorkflowId")
                            , query
                        )
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.WorkflowIdDescending:

                    return col.Find(
                        Query.And(
                            Query.All("WorkflowId", Query.Descending)
                            , query
                        )
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.NameAscending:

                    return col.Find(
                        Query.And(
                            Query.All("Name")
                            , query
                        )
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.NameDescending:

                    return col.Find(
                        Query.And(
                            Query.All("Name", Query.Descending)
                            , query
                        )
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.LaunchTypeAscending:

                    return col.Find(
                        Query.And(
                            Query.All("LaunchType")
                            , query
                        )
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.LaunchTypeDescending:

                    return col.Find(
                        Query.And(
                            Query.All("LaunchType", Query.Descending)
                            , query
                        )
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.DescriptionAscending:

                    return col.Find(
                        Query.And(
                            Query.All("Description")
                            , query
                        )
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.DescriptionDescending:

                    return col.Find(
                        Query.And(
                            Query.All("Description", Query.Descending)
                            , query
                        )
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.StatusAscending:

                    return col.Find(
                        Query.And(
                            Query.All("Status")
                            , query
                        )
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.StatusDescending:

                    return col.Find(
                        Query.And(
                            Query.All("Status", Query.Descending)
                            , query
                        )
                        , skip
                        , entriesCount
                    );
            }

            return new Entry[] { };
        }

        public long GetHistoryEntriesCount(string keyword)
        {
            var keywordToUpper = keyword.ToUpper();
            var col = db.GetCollection<HistoryEntry>("historyEntries");
            return col.Find(e => e.Name.ToUpper().Contains(keywordToUpper) || e.Description.ToUpper().Contains(keywordToUpper)).LongCount();
        }

        public long GetHistoryEntriesCount(string keyword, DateTime from, DateTime to)
        {
            var keywordToLower = keyword.ToLower();
            var col = db.GetCollection<HistoryEntry>("historyEntries");
            Query query;

            if (!string.IsNullOrEmpty(keyword))
            {
                query = Query.And(Query.Or(Query.Contains("Name", keywordToLower), Query.Contains("Description", keywordToLower))
                    , Query.And(Query.GTE("StatusDate", from), Query.LTE("StatusDate", to)));
            }
            else
            {
                query = Query.And(Query.GTE("StatusDate", from), Query.LTE("StatusDate", to));
            }

            return col.Find(query).LongCount();
        }

        public long GetEntriesCount(string keyword, DateTime from, DateTime to)
        {
            var keywordToLower = keyword.ToLower();
            var col = db.GetCollection<Entry>("entries");
            Query query;

            if (!string.IsNullOrEmpty(keyword))
            {
                query = Query.And(Query.Or(Query.Contains("Name", keywordToLower), Query.Contains("Description", keywordToLower))
                    , Query.And(Query.GTE("StatusDate", from), Query.LTE("StatusDate", to)));
            }
            else
            {
                query = Query.And(Query.GTE("StatusDate", from), Query.LTE("StatusDate", to));
            }

            return col.Find(query).LongCount();
        }

        public DateTime GetHistoryEntryStatusDateMin()
        {
            var col = db.GetCollection<HistoryEntry>("historyEntries");
            var q = col.Find(Query.All("StatusDate"));
            if (q.Any())
            {
                return q.Select(e => e.StatusDate).First();
            }

            return DateTime.Now;
        }

        public DateTime GetHistoryEntryStatusDateMax()
        {
            var col = db.GetCollection<HistoryEntry>("historyEntries");
            var q = col.Find(Query.All("StatusDate", Query.Descending));
            if (q.Any())
            {
                return q.Select(e => e.StatusDate).First();
            }

            return DateTime.Now;
        }

        public DateTime GetEntryStatusDateMin()
        {
            var col = db.GetCollection<HistoryEntry>("entries");
            var q = col.Find(Query.All("StatusDate"));
            if (q.Any())
            {
                return q.Select(e => e.StatusDate).First();
            }

            return DateTime.Now;
        }

        public DateTime GetEntryStatusDateMax()
        {
            var col = db.GetCollection<HistoryEntry>("entries");
            var q = col.Find(Query.All("StatusDate", Query.Descending));
            if (q.Any())
            {
                return q.Select(e => e.StatusDate).First();
            }

            return DateTime.Now;
        }
    }
}
