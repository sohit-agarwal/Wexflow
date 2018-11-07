using System;
using LiteDB;
using System.Collections.Generic;
using System.Linq;

namespace Wexflow.Core.Db
{
    public enum HistoryEntryOrderBy
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

    public class Db
    {
        public string ConnectionString { get; private set;}

        public Db(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public void Init()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                // StatusCount
                ClearStatusCount();

                var col = db.GetCollection<StatusCount>("statusCount");

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

                col.Insert(statusCount);

                // Entries
                ClearEntries();
            }
        }

        public void ClearStatusCount()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<StatusCount>("statusCount");
                var statusCount = col.FindAll();
                col.Delete(s => statusCount.Where(ss => ss.Id == s.Id).Count() > 0);
            }
        }

        public void ClearEntries()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<StatusCount>("entries");
                var entries = col.FindAll();
                col.Delete(e => entries.Where(ee => ee.Id == e.Id).Count() > 0);
            }
        }

        public StatusCount GetStatusCount()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<StatusCount>("statusCount");
                var statusCount = col.FindAll().FirstOrDefault();
                return statusCount;
            }
        }

        public void IncrementPendingCount()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<StatusCount>("statusCount");
                var statusCount = col.FindAll().FirstOrDefault();
                if(statusCount != null)
                {
                    statusCount.PendingCount++;
                    col.Update(statusCount);
                }   
            }
        }

        public void DecrementPendingCount()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<StatusCount>("statusCount");
                var statusCount = col.FindAll().FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.PendingCount--;
                    col.Update(statusCount);
                }
            }
        }

        public void IncrementRunningCount()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<StatusCount>("statusCount");
                var statusCount = col.FindAll().FirstOrDefault();
                if(statusCount != null)
                {
                    statusCount.RunningCount++;
                    col.Update(statusCount);
                }
                
            }
        }

        public void DecrementRunningCount()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<StatusCount>("statusCount");
                var statusCount = col.FindAll().FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.RunningCount--;
                    col.Update(statusCount);
                }
            }
        }

        public void IncrementDoneCount()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<StatusCount>("statusCount");
                var statusCount = col.FindAll().FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.DoneCount++;
                    col.Update(statusCount);
                }
            }
        }

        public void DecrementDoneCount()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<StatusCount>("statusCount");
                var statusCount = col.FindAll().FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.DoneCount--;
                    col.Update(statusCount);
                }
            }
        }

        public void IncrementFailedCount()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<StatusCount>("statusCount");
                var statusCount = col.FindAll().FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.FailedCount++;
                    col.Update(statusCount);
                }
            }
        }

        public void DecrementFailedCount()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<StatusCount>("statusCount");
                var statusCount = col.FindAll().FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.FailedCount--;
                    col.Update(statusCount);
                }
            }
        }

        public void IncrementWarningCount()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<StatusCount>("statusCount");
                var statusCount = col.FindAll().FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.WarningCount++;
                    col.Update(statusCount);
                }
            }
        }

        public void DecrementWarningCount()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<StatusCount>("statusCount");
                var statusCount = col.FindAll().FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.WarningCount--;
                    col.Update(statusCount);
                }
            }
        }

        public void IncrementDisabledCount()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<StatusCount>("statusCount");
                var statusCount = col.FindAll().FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.DisabledCount++;
                    col.Update(statusCount);
                }
            }
        }

        public void DecrementDisabledCount()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<StatusCount>("statusCount");
                var statusCount = col.FindAll().FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.DisabledCount--;
                    col.Update(statusCount);
                }
            }
        }

        public void IncrementStoppedCount()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<StatusCount>("statusCount");
                var statusCount = col.FindAll().FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.StoppedCount++;
                    col.Update(statusCount);
                }
            }
        }

        public void DecrementStoppedCount()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<StatusCount>("statusCount");
                var statusCount = col.FindAll().FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.StoppedCount--;
                    col.Update(statusCount);
                }
            }
        }

        public void ResetStatusCount()
        {
            using (var db = new LiteDatabase(ConnectionString))
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
        }

        public IEnumerable<Entry> GetEntries()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<Entry>("entries");
                return col.FindAll();
            }
        }

        public Entry GetEntry(int workflowId)
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<Entry>("entries");
                return col.FindOne(e => e.WorkflowId == workflowId);
            }
        }

        public void InsertEntry(Entry entry)
        {
            using (var db = new LiteDatabase(ConnectionString))
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
        }

        public void UpdateEntry(Entry entry)
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<Entry>("entries");
                col.Update(entry);
            }
        }

        public void DeleteEntry(int workflowId)
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<Entry>("entries");
                col.Delete(e => e.WorkflowId == workflowId);
            }
        }

        public void InsertUser(User user)
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<User>("users");
                col.Insert(user);
                col.EnsureIndex(u => u.Username);
            }
        }

        public User GetUser(string username)
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<User>("users");
                User user = col.FindOne(u => u.Username == username);
                return user;
            }
        }

        public string GetPassword(string username)
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<User>("users");
                User user = col.FindOne(u => u.Username == username);
                return user.Password;
            }
        }

        public void InsertHistoryEntry(HistoryEntry entry)
        {
            using (var db = new LiteDatabase(ConnectionString))
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
        }

        public void UpdateHistoryEntry(HistoryEntry entry)
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<HistoryEntry>("historyEntries");
                col.Update(entry);
            }
        }

        public void DeleteHistoryEntries(int workflowId)
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<HistoryEntry>("historyEntries");
                col.Delete(e => e.WorkflowId == workflowId);
            }
        }

        public IEnumerable<HistoryEntry> GetHistoryEntries()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<HistoryEntry>("historyEntries");
                return col.FindAll();
            }
        }

        public IEnumerable<HistoryEntry> GetHistoryEntries(string keyword)
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var keywordToUpper = keyword.ToUpper();
                var col = db.GetCollection<HistoryEntry>("historyEntries");
                return col.Find(e => e.Name.ToUpper().Contains(keywordToUpper) || e.Description.ToUpper().Contains(keywordToUpper));
            }
        }

        public IEnumerable<HistoryEntry> GetHistoryEntries(string keyword, int page, int entriesCount)
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var keywordToUpper = keyword.ToUpper();
                var col = db.GetCollection<HistoryEntry>("historyEntries");
                return col.Find(e => e.Name.ToUpper().Contains(keywordToUpper) || e.Description.ToUpper().Contains(keywordToUpper), (page - 1) * entriesCount, entriesCount);
            }
        }

        public IEnumerable<HistoryEntry> GetHistoryEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, HistoryEntryOrderBy heo)
        {
            using (var db = new LiteDatabase(ConnectionString))
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
                    case HistoryEntryOrderBy.StatusDateAscending:

                        return col.Find(
                            Query.And(
                                Query.All("StatusDate")
                                , query
                            )
                            , skip
                            , entriesCount
                        );

                    case HistoryEntryOrderBy.StatusDateDescending:

                        return col.Find(
                            Query.And(
                                Query.All("StatusDate", Query.Descending)
                                , query
                            )
                            , skip
                            , entriesCount
                        );

                    case HistoryEntryOrderBy.WorkflowIdAscending:

                        return col.Find(
                            Query.And(
                                Query.All("WorkflowId")
                                , query
                            )
                            , skip
                            , entriesCount
                        );

                    case HistoryEntryOrderBy.WorkflowIdDescending:

                        return col.Find(
                            Query.And(
                                Query.All("WorkflowId", Query.Descending)
                                , query
                            )
                            , skip
                            , entriesCount
                        );

                    case HistoryEntryOrderBy.NameAscending:

                        return col.Find(
                            Query.And(
                                Query.All("Name")
                                , query
                            )
                            , skip
                            , entriesCount
                        );

                    case HistoryEntryOrderBy.NameDescending:

                        return col.Find(
                            Query.And(
                                Query.All("Name", Query.Descending)
                                , query
                            )
                            , skip
                            , entriesCount
                        );

                    case HistoryEntryOrderBy.LaunchTypeAscending:

                        return col.Find(
                            Query.And(
                                Query.All("LaunchType")
                                , query
                            )
                            , skip
                            , entriesCount
                        );

                    case HistoryEntryOrderBy.LaunchTypeDescending:

                        return col.Find(
                            Query.And(
                                Query.All("LaunchType", Query.Descending)
                                , query
                            )
                            , skip
                            , entriesCount
                        );

                    case HistoryEntryOrderBy.DescriptionAscending:

                        return col.Find(
                            Query.And(
                                Query.All("Description")
                                , query
                            )
                            , skip
                            , entriesCount
                        );

                    case HistoryEntryOrderBy.DescriptionDescending:

                        return col.Find(
                            Query.And(
                                Query.All("Description", Query.Descending)
                                , query
                            )
                            , skip
                            , entriesCount
                        );

                    case HistoryEntryOrderBy.StatusAscending:

                        return col.Find(
                            Query.And(
                                Query.All("Status")
                                , query
                            )
                            , skip
                            , entriesCount
                        );

                    case HistoryEntryOrderBy.StatusDescending:

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
        }

        public IEnumerable<Entry> GetEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, HistoryEntryOrderBy heo)
        {
            using (var db = new LiteDatabase(ConnectionString))
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
                    case HistoryEntryOrderBy.StatusDateAscending:

                        return col.Find(
                            Query.And(
                                Query.All("StatusDate")
                                , query
                            )
                            , skip
                            , entriesCount
                        );

                    case HistoryEntryOrderBy.StatusDateDescending:

                        return col.Find(
                            Query.And(
                                Query.All("StatusDate", Query.Descending)
                                , query
                            )
                            , skip
                            , entriesCount
                        );

                    case HistoryEntryOrderBy.WorkflowIdAscending:

                        return col.Find(
                            Query.And(
                                Query.All("WorkflowId")
                                , query
                            )
                            , skip
                            , entriesCount
                        );

                    case HistoryEntryOrderBy.WorkflowIdDescending:

                        return col.Find(
                            Query.And(
                                Query.All("WorkflowId", Query.Descending)
                                , query
                            )
                            , skip
                            , entriesCount
                        );

                    case HistoryEntryOrderBy.NameAscending:

                        return col.Find(
                            Query.And(
                                Query.All("Name")
                                , query
                            )
                            , skip
                            , entriesCount
                        );

                    case HistoryEntryOrderBy.NameDescending:

                        return col.Find(
                            Query.And(
                                Query.All("Name", Query.Descending)
                                , query
                            )
                            , skip
                            , entriesCount
                        );

                    case HistoryEntryOrderBy.LaunchTypeAscending:

                        return col.Find(
                            Query.And(
                                Query.All("LaunchType")
                                , query
                            )
                            , skip
                            , entriesCount
                        );

                    case HistoryEntryOrderBy.LaunchTypeDescending:

                        return col.Find(
                            Query.And(
                                Query.All("LaunchType", Query.Descending)
                                , query
                            )
                            , skip
                            , entriesCount
                        );

                    case HistoryEntryOrderBy.DescriptionAscending:

                        return col.Find(
                            Query.And(
                                Query.All("Description")
                                , query
                            )
                            , skip
                            , entriesCount
                        );

                    case HistoryEntryOrderBy.DescriptionDescending:

                        return col.Find(
                            Query.And(
                                Query.All("Description", Query.Descending)
                                , query
                            )
                            , skip
                            , entriesCount
                        );

                    case HistoryEntryOrderBy.StatusAscending:

                        return col.Find(
                            Query.And(
                                Query.All("Status")
                                , query
                            )
                            , skip
                            , entriesCount
                        );

                    case HistoryEntryOrderBy.StatusDescending:

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
        }

        public long GetHistoryEntriesCount(string keyword)
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var keywordToUpper = keyword.ToUpper();
                var col = db.GetCollection<HistoryEntry>("historyEntries");
                return col.Find(e => e.Name.ToUpper().Contains(keywordToUpper) || e.Description.ToUpper().Contains(keywordToUpper)).LongCount();
            }
        }

        public long GetHistoryEntriesCount(string keyword, DateTime from, DateTime to)
        {
            using (var db = new LiteDatabase(ConnectionString))
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
        }

        public long GetEntriesCount(string keyword, DateTime from, DateTime to)
        {
            using (var db = new LiteDatabase(ConnectionString))
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
        }

        public DateTime GetHistoryEntryStatusDateMin()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<HistoryEntry>("historyEntries");
                var q = col.Find(Query.All("StatusDate"));
                if (q.Any())
                {
                    return q.Select(e=>e.StatusDate).First();
                }

                return DateTime.MinValue;
            }
        }

        public DateTime GetHistoryEntryStatusDateMax()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<HistoryEntry>("historyEntries");
                var q = col.Find(Query.All("StatusDate", Query.Descending));
                if (q.Any())
                {
                    return q.Select(e => e.StatusDate).First();
                }

                return DateTime.MaxValue;
            }
        }

        public DateTime GetEntryStatusDateMin()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<HistoryEntry>("entries");
                var q = col.Find(Query.All("StatusDate"));
                if (q.Any())
                {
                    return q.Select(e => e.StatusDate).First();
                }

                return DateTime.MinValue;
            }
        }

        public DateTime GetEntryStatusDateMax()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<HistoryEntry>("entries");
                var q = col.Find(Query.All("StatusDate", Query.Descending));
                if (q.Any())
                {
                    return q.Select(e => e.StatusDate).First();
                }

                return DateTime.MaxValue;
            }
        }
    }
}
