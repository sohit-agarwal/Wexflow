using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Queries;
using Raven.Client.Exceptions;
using Raven.Client.Exceptions.Database;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Wexflow.Core.Db;

namespace Wexflow.Core.RavenDB
{
    public class Db : Core.Db.Db
    {
        private string _databaseName = "wexflow-dotnet-core";
        private DocumentStore _store;

        public Db(string connectionString) : base(connectionString)
        {
            string ravenUrl = string.Empty;

            var connectionStringParts = ConnectionString.Split(';');
            foreach (var part in connectionStringParts)
            {
                if (!string.IsNullOrEmpty(part.Trim()))
                {
                    string connPart = part.TrimStart(' ').TrimEnd(' ');
                    if (connPart.StartsWith("DatabaseName="))
                    {
                        _databaseName = connPart.Replace("DatabaseName=", string.Empty);
                    }
                    else if (connPart.StartsWith("RavenUrl="))
                    {
                        ravenUrl = connPart.Replace("RavenUrl=", string.Empty);
                    }
                }
            }

            _store = new DocumentStore
            {
                Urls = new string[] { ravenUrl },
                Database = _databaseName
            };

            _store.Initialize();

            // Create database if it does not exist
            try
            {
                _store.Maintenance.ForDatabase(_store.Database).Send(new GetStatisticsOperation());
            }
            catch (DatabaseDoesNotExistException)
            {
                try
                {
                    _store.Maintenance.Server.Send(new CreateDatabaseOperation(new DatabaseRecord(_databaseName)));
                }
                catch (ConcurrencyException)
                {
                    // The database was already created before calling CreateDatabaseOperation
                }
            }
        }

        public override void Init()
        {
            using (var session = _store.OpenSession())
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
                session.Store(statusCount);
                session.SaveChanges();

                // Entries
                ClearEntries();

                // Insert default user if necessary
                var usersCol = session.Query<User>();
                try
                {
                    if (usersCol.Count() == 0)
                    {
                        InsertDefaultUser();
                    }
                }
                catch (Exception) // Create document if it does not exist
                {
                    InsertDefaultUser();
                }

            }

        }

        private void DeleteAll(string documentName)
        {
            _store.Operations
             .Send(new DeleteByQueryOperation(new IndexQuery
             {
                 Query = "from " + documentName
             }));
            Wait();
        }

        private void Wait()
        {
            while (_store.Maintenance.ForDatabase(_store.Database).Send(new GetStatisticsOperation()).StaleIndexes.Length > 0)
            {
                Thread.Sleep(10);
            }
        }

        public override bool CheckUserWorkflow(string userId, string workflowId)
        {
            using (var session = _store.OpenSession())
            {
                try
                {
                    var col = session.Query<UserWorkflow>();
                    var res = col.FirstOrDefault(uw => uw.UserId == userId && uw.WorkflowId == workflowId);
                    return res != null;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public override void ClearEntries()
        {
            DeleteAll("entries");
        }

        public override void ClearStatusCount()
        {
            DeleteAll("statusCounts");
        }

        public override void DecrementPendingCount()
        {
            using (var session = _store.OpenSession())
            {
                var col = session.Query<StatusCount>();
                var statusCount = col.FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.PendingCount--;
                    session.SaveChanges();
                    Wait();
                }
            }
        }

        public override void DecrementRunningCount()
        {
            using (var session = _store.OpenSession())
            {
                var col = session.Query<StatusCount>();
                var statusCount = col.FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.RunningCount--;
                    session.SaveChanges();
                    Wait();
                }
            }
        }

        public override void DeleteUser(string username, string password)
        {
            using (var session = _store.OpenSession())
            {
                var col = session.Query<User>();
                var user = col.FirstOrDefault(u => u.Username == username);
                if (user != null && user.Password == password)
                {
                    session.Delete(user);
                    DeleteUserWorkflowRelationsByUserId(user.Id);
                    session.SaveChanges();
                    Wait();
                }
                else
                {
                    throw new Exception("The password is incorrect.");
                }
            }
        }

        public override void DeleteUserWorkflowRelationsByUserId(string userId)
        {
            using (var session = _store.OpenSession())
            {
                var col = session.Query<UserWorkflow>();
                var rels = col.Where(uw => uw.UserId == userId).ToArray();
                foreach (var rel in rels)
                {
                    session.Delete(rel);
                }
                session.SaveChanges();
                Wait();
            }
        }

        public override void DeleteUserWorkflowRelationsByWorkflowId(string workflowDbId)
        {
            using (var session = _store.OpenSession())
            {
                var col = session.Query<UserWorkflow>();
                var rels = col.Where(uw => uw.WorkflowId == workflowDbId).ToArray();
                foreach (var rel in rels)
                {
                    session.Delete(rel);
                }
                session.SaveChanges();
                Wait();
            }
        }

        public override void DeleteWorkflow(string id)
        {
            using (var session = _store.OpenSession())
            {
                var col = session.Query<Workflow>();
                var wf = col.FirstOrDefault(e => e.Id == id);
                if (wf != null)
                {
                    session.Delete(wf);
                }
                session.SaveChanges();
                Wait();
            }
        }

        public override void DeleteWorkflows(string[] ids)
        {
            using (var session = _store.OpenSession())
            {
                var col = session.Query<Workflow>();
 
                foreach(var id in ids)
                {
                    var wf = col.FirstOrDefault(w => w.Id == id);
                    if(wf != null)
                    {
                        session.Delete(wf);
                    }
                }

                session.SaveChanges();
                Wait();
            }
        }

        public override IEnumerable<Core.Db.User> GetAdministrators(string keyword, UserOrderBy uo)
        {
            using (var session = _store.OpenSession())
            {
                try
                {
                    var col = session.Query<User>();
                    var keywordToLower = string.IsNullOrEmpty(keyword) ? "*" : "*" + keyword.ToLower() + "*";

                    switch (uo)
                    {
                        case UserOrderBy.UsernameAscending:
                            return col
                                .Search(u => u.Username, keywordToLower)
                                .Where(u => u.UserProfile == UserProfile.Administrator)
                                .OrderBy(u => u.Username)
                                .ToArray();
                        case UserOrderBy.UsernameDescending:
                            return col
                                .Search(u => u.Username, keywordToLower)
                                .Where(u => u.UserProfile == UserProfile.Administrator)
                                .OrderByDescending(u => u.Username)
                                .ToArray();
                    }

                    return new User[] { };
                }
                catch (Exception)
                {
                    return new User[] { };
                }
            }
        }

        public override IEnumerable<Core.Db.Entry> GetEntries()
        {
            using (var session = _store.OpenSession())
            {
                try
                {
                    var col = session.Query<Entry>().ToArray();
                    return col;
                }
                catch (Exception)
                {
                    return new Entry[] { };
                }
            }
        }

        public override IEnumerable<Core.Db.Entry> GetEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy eo)
        {
            using (var session = _store.OpenSession())
            {
                try
                {
                    var col = session.Query<Entry>();
                    var keywordToLower = string.IsNullOrEmpty(keyword) ? "*" : "*" + keyword.ToLower() + "*";
                    int skip = (page - 1) * entriesCount;

                    switch (eo)
                    {
                        case EntryOrderBy.StatusDateAscending:

                            return col
                                .Search(e => e.Name, keywordToLower, options: SearchOptions.Or)
                                .Search(e => e.Description, keywordToLower)
                                .Where(e => e.StatusDate > from && e.StatusDate < to)
                                .OrderBy(e => e.StatusDate).Skip((page - 1) * entriesCount)
                                .Take(entriesCount)
                                .ToArray();

                        case EntryOrderBy.StatusDateDescending:

                            return col
                                .Search(e => e.Name, keywordToLower, options: SearchOptions.Or)
                                .Search(e => e.Description, keywordToLower)
                                .Where(e => e.StatusDate > from && e.StatusDate < to)
                                .OrderByDescending(e => e.StatusDate)
                                .Skip((page - 1) * entriesCount)
                                .Take(entriesCount)
                                .ToArray();

                        case EntryOrderBy.WorkflowIdAscending:

                            return col
                                .Search(e => e.Name, keywordToLower, options: SearchOptions.Or)
                                .Search(e => e.Description, keywordToLower)
                                .Where(e => e.StatusDate > from && e.StatusDate < to)
                                .OrderBy(e => e.WorkflowId)
                                .Skip((page - 1) * entriesCount)
                                .Take(entriesCount)
                                .ToArray();

                        case EntryOrderBy.WorkflowIdDescending:

                            return col
                                .Search(e => e.Name, keywordToLower, options: SearchOptions.Or)
                                .Search(e => e.Description, keywordToLower)
                                .Where(e => e.StatusDate > from && e.StatusDate < to)
                                .OrderByDescending(e => e.WorkflowId)
                                .Skip((page - 1) * entriesCount)
                                .Take(entriesCount)
                                .ToArray();

                        case EntryOrderBy.NameAscending:

                            return col
                                .Search(e => e.Name, keywordToLower, options: SearchOptions.Or)
                                .Search(e => e.Description, keywordToLower)
                                .Where(e => e.StatusDate > from && e.StatusDate < to)
                                .OrderBy(e => e.Name)
                                .Skip((page - 1) * entriesCount)
                                .Take(entriesCount)
                                .ToArray();

                        case EntryOrderBy.NameDescending:

                            return col
                                .Search(e => e.Name, keywordToLower, options: SearchOptions.Or)
                                .Search(e => e.Description, keywordToLower)
                                .Where(e => e.StatusDate > from && e.StatusDate < to)
                                .OrderByDescending(e => e.Name)
                                .Skip((page - 1) * entriesCount)
                                .Take(entriesCount)
                                .ToArray();

                        case EntryOrderBy.LaunchTypeAscending:

                            return col
                                .Search(e => e.Name, keywordToLower, options: SearchOptions.Or)
                                .Search(e => e.Description, keywordToLower)
                                .Where(e => e.StatusDate > from && e.StatusDate < to)
                                .OrderBy(e => e.LaunchType)
                                .Skip((page - 1) * entriesCount)
                                .Take(entriesCount)
                                .ToArray();

                        case EntryOrderBy.LaunchTypeDescending:

                            return col
                                .Search(e => e.Name, keywordToLower, options: SearchOptions.Or)
                                .Search(e => e.Description, keywordToLower)
                                .Where(e => e.StatusDate > from && e.StatusDate < to)
                                .OrderByDescending(e => e.LaunchType)
                                .Skip((page - 1) * entriesCount)
                                .Take(entriesCount)
                                .ToArray();

                        case EntryOrderBy.DescriptionAscending:

                            return col
                                .Search(e => e.Name, keywordToLower, options: SearchOptions.Or)
                                .Search(e => e.Description, keywordToLower)
                                .Where(e => e.StatusDate > from && e.StatusDate < to)
                                .OrderBy(e => e.Description)
                                .Skip((page - 1) * entriesCount)
                                .Take(entriesCount)
                                .ToArray();

                        case EntryOrderBy.DescriptionDescending:

                            return col
                                .Search(e => e.Name, keywordToLower, options: SearchOptions.Or)
                                .Search(e => e.Description, keywordToLower)
                                .Where(e => e.StatusDate > from && e.StatusDate < to)
                                .OrderByDescending(e => e.Description)
                                .Skip((page - 1) * entriesCount)
                                .Take(entriesCount)
                                .ToArray();

                        case EntryOrderBy.StatusAscending:

                            return col
                                .Search(e => e.Name, keywordToLower, options: SearchOptions.Or)
                                .Search(e => e.Description, keywordToLower)
                                .Where(e => e.StatusDate > from && e.StatusDate < to)
                                .OrderBy(e => e.Status)
                                .Skip((page - 1) * entriesCount)
                                .Take(entriesCount)
                                .ToArray();

                        case EntryOrderBy.StatusDescending:

                            return col
                                .Search(e => e.Name, keywordToLower, options: SearchOptions.Or)
                                .Search(e => e.Description, keywordToLower)
                                .Where(e => e.StatusDate > from && e.StatusDate < to)
                                .OrderByDescending(e => e.Status)
                                .Skip((page - 1) * entriesCount)
                                .Take(entriesCount)
                                .ToArray();
                    }

                    return new Entry[] { };
                }
                catch (Exception)
                {
                    return new Entry[] { };
                }
            }
        }

        public override long GetEntriesCount(string keyword, DateTime from, DateTime to)
        {
            using (var session = _store.OpenSession())
            {
                try
                {
                    var keywordToLower = string.IsNullOrEmpty(keyword) ? "*" : "*" + keyword.ToLower() + "*";
                    var col = session.Query<Entry>();

                    return col
                        .Search(e => e.Name, keywordToLower, options: SearchOptions.Or)
                        .Search(e => e.Description, keywordToLower)
                        .Count(e => e.StatusDate > from && e.StatusDate < to);
                }
                catch (Exception)
                {
                    return 0;
                }
            }

        }

        public override Core.Db.Entry GetEntry(int workflowId)
        {
            using (var session = _store.OpenSession())
            {
                try
                {
                    var col = session.Query<Entry>();
                    return col.FirstOrDefault(e => e.WorkflowId == workflowId);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public override DateTime GetEntryStatusDateMax()
        {
            using (var session = _store.OpenSession())
            {
                try
                {
                    var col = session.Query<Entry>();
                    var q = col.OrderByDescending(e => e.StatusDate);
                    if (q.Any())
                    {
                        return q.Select(e => e.StatusDate).First();
                    }

                    return DateTime.Now;
                }
                catch (Exception)
                {
                    return DateTime.Now;
                }
            }
        }

        public override DateTime GetEntryStatusDateMin()
        {
            using (var session = _store.OpenSession())
            {
                try
                {
                    var col = session.Query<Entry>();
                    var q = col.OrderBy(e => e.StatusDate);
                    if (q.Any())
                    {
                        return q.Select(e => e.StatusDate).First();
                    }

                    return DateTime.Now;
                }
                catch (Exception)
                {
                    return DateTime.Now;
                }
            }
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries()
        {
            using (var session = _store.OpenSession())
            {
                try
                {
                    var col = session.Query<HistoryEntry>().ToArray();
                    return col;
                }
                catch (Exception)
                {
                    return new HistoryEntry[] { };
                }
            }
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword)
        {
            using (var session = _store.OpenSession())
            {
                try
                {
                    var keywordToLower = string.IsNullOrEmpty(keyword) ? "*" : "*" + keyword.ToLower() + "*";
                    var col = session.Query<HistoryEntry>();
                    return col
                        .Search(e => e.Name, keywordToLower, options: SearchOptions.Or)
                        .Search(e => e.Description, keywordToLower)
                        .ToArray();
                }
                catch (Exception)
                {
                    return new HistoryEntry[] { };
                }
            }
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword, int page, int entriesCount)
        {
            using (var session = _store.OpenSession())
            {
                try
                {
                    var keywordToLower = string.IsNullOrEmpty(keyword) ? "*" : "*" + keyword.ToLower() + "*";
                    var col = session.Query<HistoryEntry>();
                    return col
                        .Search(e => e.Name, keywordToLower, options: SearchOptions.Or)
                        .Search(e => e.Description, keywordToLower)
                        .Skip((page - 1) * entriesCount).Take(entriesCount).ToArray();
                }
                catch (Exception)
                {
                    return new HistoryEntry[] { };
                }
            }
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy heo)
        {
            using (var session = _store.OpenSession())
            {
                try
                {
                    var col = session.Query<HistoryEntry>();
                    var keywordToLower = string.IsNullOrEmpty(keyword) ? "*" : "*" + keyword.ToLower() + "*";
                    int skip = (page - 1) * entriesCount;

                    switch (heo)
                    {
                        case EntryOrderBy.StatusDateAscending:

                            return col
                                .Search(e => e.Name, keywordToLower, options: SearchOptions.Or)
                                .Search(e => e.Description, keywordToLower)
                                .Where(e => e.StatusDate > from && e.StatusDate < to)
                                .OrderBy(e => e.StatusDate).Skip((page - 1) * entriesCount)
                                .Take(entriesCount)
                                .ToArray();

                        case EntryOrderBy.StatusDateDescending:

                            return col
                                .Search(e => e.Name, keywordToLower, options: SearchOptions.Or)
                                .Search(e => e.Description, keywordToLower)
                                .Where(e => e.StatusDate > from && e.StatusDate < to)
                                .OrderByDescending(e => e.StatusDate)
                                .Skip((page - 1) * entriesCount)
                                .Take(entriesCount)
                                .ToArray();

                        case EntryOrderBy.WorkflowIdAscending:

                            return col
                                .Search(e => e.Name, keywordToLower, options: SearchOptions.Or)
                                .Search(e => e.Description, keywordToLower)
                                .Where(e => e.StatusDate > from && e.StatusDate < to)
                                .OrderBy(e => e.WorkflowId)
                                .Skip((page - 1) * entriesCount)
                                .Take(entriesCount)
                                .ToArray();

                        case EntryOrderBy.WorkflowIdDescending:

                            return col
                                .Search(e => e.Name, keywordToLower, options: SearchOptions.Or)
                                .Search(e => e.Description, keywordToLower)
                                .Where(e => e.StatusDate > from && e.StatusDate < to)
                                .OrderByDescending(e => e.WorkflowId)
                                .Skip((page - 1) * entriesCount)
                                .Take(entriesCount)
                                .ToArray();

                        case EntryOrderBy.NameAscending:

                            return col
                                .Search(e => e.Name, keywordToLower, options: SearchOptions.Or)
                                .Search(e => e.Description, keywordToLower)
                                .Where(e => e.StatusDate > from && e.StatusDate < to)
                                .OrderBy(e => e.Name)
                                .Skip((page - 1) * entriesCount)
                                .Take(entriesCount)
                                .ToArray();

                        case EntryOrderBy.NameDescending:

                            return col
                                .Search(e => e.Name, keywordToLower, options: SearchOptions.Or)
                                .Search(e => e.Description, keywordToLower)
                                .Where(e => e.StatusDate > from && e.StatusDate < to)
                                .OrderByDescending(e => e.Name)
                                .Skip((page - 1) * entriesCount)
                                .Take(entriesCount)
                                .ToArray();

                        case EntryOrderBy.LaunchTypeAscending:

                            return col
                                .Search(e => e.Name, keywordToLower, options: SearchOptions.Or)
                                .Search(e => e.Description, keywordToLower)
                                .Where(e => e.StatusDate > from && e.StatusDate < to)
                                .OrderBy(e => e.LaunchType)
                                .Skip((page - 1) * entriesCount)
                                .Take(entriesCount)
                                .ToArray();

                        case EntryOrderBy.LaunchTypeDescending:

                            return col
                                .Search(e => e.Name, keywordToLower, options: SearchOptions.Or)
                                .Search(e => e.Description, keywordToLower)
                                .Where(e => e.StatusDate > from && e.StatusDate < to)
                                .OrderByDescending(e => e.LaunchType)
                                .Skip((page - 1) * entriesCount)
                                .Take(entriesCount)
                                .ToArray();

                        case EntryOrderBy.DescriptionAscending:

                            return col
                                .Search(e => e.Name, keywordToLower, options: SearchOptions.Or)
                                .Search(e => e.Description, keywordToLower)
                                .Where(e => e.StatusDate > from && e.StatusDate < to)
                                .OrderBy(e => e.Description)
                                .Skip((page - 1) * entriesCount)
                                .Take(entriesCount)
                                .ToArray();

                        case EntryOrderBy.DescriptionDescending:

                            return col
                                .Search(e => e.Name, keywordToLower, options: SearchOptions.Or)
                                .Search(e => e.Description, keywordToLower)
                                .Where(e => e.StatusDate > from && e.StatusDate < to)
                                .OrderByDescending(e => e.Description)
                                .Skip((page - 1) * entriesCount)
                                .Take(entriesCount)
                                .ToArray();

                        case EntryOrderBy.StatusAscending:

                            return col
                                .Search(e => e.Name, keywordToLower, options: SearchOptions.Or)
                                .Search(e => e.Description, keywordToLower)
                                .Where(e => e.StatusDate > from && e.StatusDate < to)
                                .OrderBy(e => e.Status)
                                .Skip((page - 1) * entriesCount)
                                .Take(entriesCount)
                                .ToArray();

                        case EntryOrderBy.StatusDescending:

                            return col
                                .Search(e => e.Name, keywordToLower, options: SearchOptions.Or)
                                .Search(e => e.Description, keywordToLower)
                                .Where(e => e.StatusDate > from && e.StatusDate < to)
                                .OrderByDescending(e => e.Status)
                                .Skip((page - 1) * entriesCount)
                                .Take(entriesCount)
                                .ToArray();
                    }

                    return new HistoryEntry[] { };
                }
                catch (Exception)
                {
                    return new HistoryEntry[] { };
                }
            }
        }

        public override long GetHistoryEntriesCount(string keyword)
        {
            using (var session = _store.OpenSession())
            {
                try
                {
                    var keywordToLower = string.IsNullOrEmpty(keyword) ? "*" : "*" + keyword.ToLower() + "*";
                    var col = session.Query<HistoryEntry>();
                    return col
                        .Search(e => e.Name, keywordToLower, options: SearchOptions.Or)
                        .Search(e => e.Description, keywordToLower)
                        .Count();
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        public override long GetHistoryEntriesCount(string keyword, DateTime from, DateTime to)
        {
            using (var session = _store.OpenSession())
            {
                try
                {
                    var keywordToLower = string.IsNullOrEmpty(keyword) ? "*" : "*" + keyword.ToLower() + "*";
                    var col = session.Query<HistoryEntry>();

                    return col
                        .Search(e => e.Name, keywordToLower, options: SearchOptions.Or)
                        .Search(e => e.Description, keywordToLower)
                        .Count(e => e.StatusDate > from && e.StatusDate < to);
                }
                catch (Exception)
                {
                    return 0;
                }
            }

        }

        public override DateTime GetHistoryEntryStatusDateMax()
        {
            using (var session = _store.OpenSession())
            {
                try
                {
                    var col = session.Query<HistoryEntry>();
                    var q = col.OrderByDescending(e => e.StatusDate);
                    if (q.Any())
                    {
                        return q.Select(e => e.StatusDate).First();
                    }

                    return DateTime.Now;
                }
                catch (Exception)
                {
                    return DateTime.Now;
                }
            }
        }

        public override DateTime GetHistoryEntryStatusDateMin()
        {
            using (var session = _store.OpenSession())
            {
                try
                {
                    var col = session.Query<HistoryEntry>();
                    var q = col.OrderBy(e => e.StatusDate);
                    if (q.Any())
                    {
                        return q.Select(e => e.StatusDate).First();
                    }

                    return DateTime.Now;
                }
                catch (Exception)
                {
                    return DateTime.Now;
                }
            }
        }

        public override string GetPassword(string username)
        {
            using (var session = _store.OpenSession())
            {
                try
                {
                    var col = session.Query<User>();
                    User user = col.First(u => u.Username == username);
                    return user.Password;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public override Core.Db.StatusCount GetStatusCount()
        {
            using (var session = _store.OpenSession())
            {
                try
                {
                    var col = session.Query<StatusCount>();
                    var statusCount = col.FirstOrDefault();
                    return statusCount;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public override Core.Db.User GetUser(string username)
        {
            using (var session = _store.OpenSession())
            {
                try
                {
                    var col = session.Query<User>();
                    var user = col.FirstOrDefault(u => u.Username == username);
                    return user;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public override IEnumerable<Core.Db.User> GetUsers()
        {
            using (var session = _store.OpenSession())
            {
                try
                {
                    var col = session.Query<User>();
                    return col.ToArray();
                }
                catch (Exception)
                {
                    return new User[] { };
                }
            }
        }

        public override IEnumerable<Core.Db.User> GetUsers(string keyword, UserOrderBy uo)
        {
            using (var session = _store.OpenSession())
            {
                try
                {
                    var col = session.Query<User>();
                    var keywordToLower = string.IsNullOrEmpty(keyword) ? "*" : "*" + keyword.ToLower() + "*";

                    switch (uo)
                    {
                        case UserOrderBy.UsernameAscending:
                            return col.Search(u => u.Username, keywordToLower).OrderBy(u => u.Username).ToArray();
                        case UserOrderBy.UsernameDescending:
                            return col.Search(u => u.Username, keywordToLower).OrderByDescending(u => u.Username).ToArray();
                    }

                    return new User[] { };
                }
                catch (Exception)
                {
                    return new User[] { };
                }
            }
        }

        public override IEnumerable<string> GetUserWorkflows(string userId)
        {
            using (var session = _store.OpenSession())
            {
                try
                {
                    var col = session.Query<UserWorkflow>();
                    return col.Where(uw => uw.UserId == userId).Select(uw => uw.WorkflowId).ToArray();
                }
                catch (Exception)
                {
                    return new string[] { };
                }
            }
        }

        public override Core.Db.Workflow GetWorkflow(string id)
        {
            using (var session = _store.OpenSession())
            {
                try
                {
                    var col = session.Query<Workflow>();
                    return col.FirstOrDefault(w => w.Id == id);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public override IEnumerable<Core.Db.Workflow> GetWorkflows()
        {
            using (var session = _store.OpenSession())
            {
                try
                {
                    var col = session.Query<Workflow>();
                    return col.ToArray();
                }
                catch (Exception)
                {
                    return new Workflow[] { };
                }
            }
        }

        public override void IncrementDisabledCount()
        {
            using (var session = _store.OpenSession())
            {
                var col = session.Query<StatusCount>();
                var statusCount = col.FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.DisabledCount++;
                    session.SaveChanges();
                    Wait();
                }
            }
        }

        public override void IncrementDisapprovedCount()
        {
            using (var session = _store.OpenSession())
            {
                var col = session.Query<StatusCount>();
                var statusCount = col.FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.DisapprovedCount++;
                    session.SaveChanges();
                    Wait();
                }
            }
        }

        public override void IncrementDoneCount()
        {
            using (var session = _store.OpenSession())
            {
                var col = session.Query<StatusCount>();
                var statusCount = col.FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.DoneCount++;
                    session.SaveChanges();
                    Wait();
                }
            }
        }

        public override void IncrementFailedCount()
        {
            using (var session = _store.OpenSession())
            {
                var col = session.Query<StatusCount>();
                var statusCount = col.FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.FailedCount++;
                    session.SaveChanges();
                    Wait();
                }
            }
        }

        public override void IncrementPendingCount()
        {
            using (var session = _store.OpenSession())
            {
                var col = session.Query<StatusCount>();
                var statusCount = col.FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.PendingCount++;
                    session.SaveChanges();
                    Wait();
                }
            }
        }

        public override void IncrementRunningCount()
        {
            using (var session = _store.OpenSession())
            {
                var col = session.Query<StatusCount>();
                var statusCount = col.FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.RunningCount++;
                    session.SaveChanges();
                    Wait();
                }
            }
        }

        public override void IncrementStoppedCount()
        {
            using (var session = _store.OpenSession())
            {
                var col = session.Query<StatusCount>();
                var statusCount = col.FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.StoppedCount++;
                    session.SaveChanges();
                    Wait();
                }
            }
        }

        public override void IncrementWarningCount()
        {
            using (var session = _store.OpenSession())
            {
                var col = session.Query<StatusCount>();
                var statusCount = col.FirstOrDefault();
                if (statusCount != null)
                {
                    statusCount.WarningCount++;
                    session.SaveChanges();
                    Wait();
                }
            }
        }

        public override void InsertEntry(Core.Db.Entry entry)
        {
            using (var session = _store.OpenSession())
            {
                var ie = new Entry
                {
                    Description = entry.Description,
                    LaunchType = entry.LaunchType,
                    Name = entry.Name,
                    Status = entry.Status,
                    StatusDate = entry.StatusDate,
                    WorkflowId = entry.WorkflowId
                };
                session.Store(ie);
                session.SaveChanges();
                Wait();
            }
        }

        public override void InsertHistoryEntry(Core.Db.HistoryEntry entry)
        {
            using (var session = _store.OpenSession())
            {
                var he = new HistoryEntry
                {
                    Description = entry.Description,
                    LaunchType = entry.LaunchType,
                    Name = entry.Name,
                    Status = entry.Status,
                    StatusDate = entry.StatusDate,
                    WorkflowId = entry.WorkflowId
                };
                session.Store(he);
                session.SaveChanges();
                Wait();
            }
        }

        public override void InsertUser(Core.Db.User user)
        {
            using (var session = _store.OpenSession())
            {
                user.CreatedOn = DateTime.Now;
                var nu = new User
                {
                    CreatedOn = user.CreatedOn,
                    Email = user.Email,
                    ModifiedOn = user.ModifiedOn,
                    Password = user.Password,
                    Username = user.Username,
                    UserProfile = user.UserProfile
                };
                session.Store(nu);
                session.SaveChanges();
                Wait();
            }
        }

        public override void InsertUserWorkflowRelation(Core.Db.UserWorkflow userWorkflow)
        {
            using (var session = _store.OpenSession())
            {
                var uw = new UserWorkflow
                {
                    UserId = userWorkflow.UserId,
                    WorkflowId = userWorkflow.WorkflowId
                };
                session.Store(uw);
                session.SaveChanges();
                Wait();
            }
        }

        public override string InsertWorkflow(Core.Db.Workflow workflow)
        {
            using (var session = _store.OpenSession())
            {
                var wf = new Workflow { Xml = workflow.Xml };
                session.Store(wf);
                session.SaveChanges();
                Wait();
                return wf.Id;
            }
        }

        public override void UpdateEntry(string id, Core.Db.Entry entry)
        {
            using (var session = _store.OpenSession())
            {
                var col = session.Query<Entry>();
                var ue = col.First(e => e.Id == id);
                ue.Name = entry.Name;
                ue.Description = entry.Description;
                ue.LaunchType = entry.LaunchType;
                ue.Status = entry.Status;
                ue.StatusDate = entry.StatusDate;
                ue.WorkflowId = entry.WorkflowId;

                session.SaveChanges();
                Wait();
            }
        }

        public override void UpdatePassword(string username, string password)
        {
            using (var session = _store.OpenSession())
            {
                var col = session.Query<User>();
                var dbUser = col.First(u => u.Username == username);
                dbUser.Password = password;

                session.SaveChanges();
                Wait();
            }
        }

        public override void UpdateUser(string id, Core.Db.User user)
        {
            using (var session = _store.OpenSession())
            {
                var col = session.Query<User>();
                var uu = col.First(u => u.Id == id);
                uu.ModifiedOn = DateTime.Now;
                uu.Username = user.Username;
                uu.Password = user.Password;
                uu.UserProfile = user.UserProfile;
                uu.Email = user.Email;

                session.SaveChanges();
                Wait();
            }
        }

        public override void UpdateUsernameAndEmailAndUserProfile(string userId, string username, string email, UserProfile up)
        {
            using (var session = _store.OpenSession())
            {
                var col = session.Query<User>();
                var uu = col.First(u => u.Id == userId);
                uu.ModifiedOn = DateTime.Now;
                uu.Username = username;
                uu.UserProfile = up;
                uu.Email = email;

                session.SaveChanges();
                Wait();
            }
        }

        public override void UpdateWorkflow(string dbId, Core.Db.Workflow workflow)
        {
            using (var session = _store.OpenSession())
            {
                var col = session.Query<Workflow>();
                var wf = col.First(w => w.Id == dbId);
                wf.Xml = workflow.Xml;

                session.SaveChanges();
                Wait();
            }
        }
    }
}
