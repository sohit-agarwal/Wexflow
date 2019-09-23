using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using Wexflow.Core.Db;

namespace Wexflow.Core.CosmosDB
{
    public class Db : Core.Db.Db
    {
        private string _databaseName = "wexflow-dotnet-core";

        private string _endpointUrl;
        private string _authorizationKey;
        private Helper _helper;

        public Db(string connectionString) : base(connectionString)
        {
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
                    else if (connPart.StartsWith("EndpointUrl="))
                    {
                        _endpointUrl = connPart.Replace("EndpointUrl=", string.Empty);
                    }
                    else if (connPart.StartsWith("AuthorizationKey="))
                    {
                        _authorizationKey = connPart.Replace("AuthorizationKey=", string.Empty);
                    }
                }
            }

            _helper = new Helper(_endpointUrl, _authorizationKey);

            _helper.CreateDatabaseIfNotExists(_databaseName);
            _helper.CreateDocumentCollectionIfNotExists(_databaseName, Core.Db.Entry.DocumentName);
            _helper.CreateDocumentCollectionIfNotExists(_databaseName, Core.Db.HistoryEntry.DocumentName);
            _helper.CreateDocumentCollectionIfNotExists(_databaseName, Core.Db.StatusCount.DocumentName);
            _helper.CreateDocumentCollectionIfNotExists(_databaseName, Core.Db.User.DocumentName);
            _helper.CreateDocumentCollectionIfNotExists(_databaseName, Core.Db.UserWorkflow.DocumentName);
            _helper.CreateDocumentCollectionIfNotExists(_databaseName, Core.Db.Workflow.DocumentName);
        }

        public override void Init()
        {
            // StatusCount
            ClearStatusCount();

            var statusCount = new StatusCount
            {
                //Id = Guid.NewGuid().ToString(),
                PendingCount = 0,
                RunningCount = 0,
                DoneCount = 0,
                FailedCount = 0,
                WarningCount = 0,
                DisabledCount = 0,
                StoppedCount = 0
            };

            //_helper.
            _helper.CreateDocument(_databaseName, Core.Db.StatusCount.DocumentName, statusCount);

            // Entries
            ClearEntries();

            // Insert default user if necessary
            var usersCol = _helper.ReadDocumentFeed(_databaseName, Core.Db.User.DocumentName);
            if (usersCol.Length == 0)
            {
                InsertDefaultUser();
            }
        }

        public override bool CheckUserWorkflow(string userId, string workflowId)
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                UserWorkflow userWorkflow = client.CreateDocumentQuery<UserWorkflow>(
                UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.UserWorkflow.DocumentName))
                .Where(uw => uw.UserId == userId && uw.WorkflowId == workflowId)
                .AsEnumerable().ToArray()
                .FirstOrDefault();

                return userWorkflow != null;
            }
        }

        public override void ClearEntries()
        {
            _helper.DeleteAllDocuments(_databaseName, Core.Db.Entry.DocumentName);
        }

        public override void ClearStatusCount()
        {
            _helper.DeleteAllDocuments(_databaseName, Core.Db.StatusCount.DocumentName);
        }

        public override void DecrementPendingCount()
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                StatusCount statusCount = client.CreateDocumentQuery<StatusCount>(
                UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.StatusCount.DocumentName))
                .AsEnumerable().ToArray()
                .First();

                statusCount.PendingCount--;

                _helper.ReplaceDocument(_databaseName, Core.Db.StatusCount.DocumentName, statusCount, statusCount.Id);
            }
        }

        public override void DecrementRunningCount()
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                StatusCount statusCount = client.CreateDocumentQuery<StatusCount>(
                UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.StatusCount.DocumentName))
                .AsEnumerable().ToArray()
                .First();

                statusCount.RunningCount--;

                _helper.ReplaceDocument(_databaseName, Core.Db.StatusCount.DocumentName, statusCount, statusCount.Id);
            }
        }

        public override void DeleteUser(string username, string password)
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                var user = client.CreateDocumentQuery<User>(
                UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.User.DocumentName))
                .Where(u => u.Username == username && u.Password == password)
                .AsEnumerable().ToArray()
                .FirstOrDefault();

                if (user != null)
                {
                    _helper.DeleteDocument(_databaseName, Core.Db.User.DocumentName, user.Id);
                    DeleteUserWorkflowRelationsByUserId(user.Id);
                }
            }
        }

        public override void DeleteUserWorkflowRelationsByUserId(string userId)
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                var rels = client.CreateDocumentQuery<UserWorkflow>(
                UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.UserWorkflow.DocumentName))
                .Where(uw => uw.UserId == userId)
                .AsEnumerable().ToArray()
                .ToArray();

                foreach (var rel in rels)
                {
                    _helper.DeleteDocument(_databaseName, Core.Db.UserWorkflow.DocumentName, rel.Id);
                }
            }
        }

        public override void DeleteUserWorkflowRelationsByWorkflowId(string workflowDbId)
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                var rels = client.CreateDocumentQuery<UserWorkflow>(
                UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.UserWorkflow.DocumentName))
                .Where(uw => uw.WorkflowId == workflowDbId)
                .AsEnumerable().ToArray()
                .ToArray();

                foreach (var rel in rels)
                {
                    _helper.DeleteDocument(_databaseName, Core.Db.UserWorkflow.DocumentName, rel.Id);
                }
            }
        }

        public override void DeleteWorkflow(string id)
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                var workflow = client.CreateDocumentQuery<Workflow>(
                UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.Workflow.DocumentName))
                .Where(w => w.Id == id)
                .AsEnumerable().ToArray()
                .FirstOrDefault();

                if (workflow != null)
                {
                    _helper.DeleteDocument(_databaseName, Core.Db.Workflow.DocumentName, workflow.Id);
                }
            }
        }

        public override void DeleteWorkflows(string[] ids)
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                var workflows = client.CreateDocumentQuery<Workflow>(
                UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.Workflow.DocumentName))
                .Where(w => ids.Contains(w.Id))
                .AsEnumerable().ToArray()
                .ToArray();

                foreach (var workflow in workflows)
                {
                    _helper.DeleteDocument(_databaseName, Core.Db.Workflow.DocumentName, workflow.Id);
                }
            }
        }

        public override IEnumerable<Core.Db.User> GetAdministrators(string keyword, UserOrderBy uo)
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                var keywordToLower = keyword.ToLower();

                switch (uo)
                {
                    case UserOrderBy.UsernameAscending:
                        return client.CreateDocumentQuery<User>(
                               UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.User.DocumentName))
                               .Where(u => u.Username.ToLower().Contains(keywordToLower) && u.UserProfile == UserProfile.Administrator)
                               .OrderBy(u => u.Username)
                               .AsEnumerable().ToArray();

                    case UserOrderBy.UsernameDescending:
                        return client.CreateDocumentQuery<User>(
                               UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.User.DocumentName))
                               .Where(u => u.Username.ToLower().Contains(keywordToLower) && u.UserProfile == UserProfile.Administrator)
                               .OrderByDescending(u => u.Username)
                               .AsEnumerable().ToArray();
                }

                return new User[] { };
            }
        }

        public override IEnumerable<Core.Db.Entry> GetEntries()
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                return client.CreateDocumentQuery<Entry>(
                              UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.Entry.DocumentName))
                              .AsEnumerable().ToArray()
                              .ToArray();
            }
        }

        public override IEnumerable<Core.Db.Entry> GetEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy eo)
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                var keywordToLower = keyword.ToLower();
                int skip = (page - 1) * entriesCount;

                switch (eo)
                {
                    case EntryOrderBy.StatusDateAscending:

                        return
                            client.CreateDocumentQuery<Entry>(
                              UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.Entry.DocumentName))
                            .Where(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to)
                            .OrderBy(e => e.StatusDate)
                            .Skip((page - 1) * entriesCount).Take(entriesCount)
                            .AsEnumerable().ToArray();

                    case EntryOrderBy.StatusDateDescending:

                        return
                           client.CreateDocumentQuery<Entry>(
                             UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.Entry.DocumentName))
                           .Where(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to)
                           .OrderByDescending(e => e.StatusDate)
                           .Skip((page - 1) * entriesCount).Take(entriesCount)
                           .AsEnumerable().ToArray();

                    case EntryOrderBy.WorkflowIdAscending:

                        return
                           client.CreateDocumentQuery<Entry>(
                             UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.Entry.DocumentName))
                           .Where(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to)
                           .OrderBy(e => e.WorkflowId)
                           .Skip((page - 1) * entriesCount).Take(entriesCount)
                           .AsEnumerable().ToArray();

                    case EntryOrderBy.WorkflowIdDescending:

                        return
                           client.CreateDocumentQuery<Entry>(
                             UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.Entry.DocumentName))
                           .Where(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to)
                           .OrderByDescending(e => e.WorkflowId)
                           .Skip((page - 1) * entriesCount).Take(entriesCount)
                           .AsEnumerable().ToArray();

                    case EntryOrderBy.NameAscending:

                        return
                           client.CreateDocumentQuery<Entry>(
                             UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.Entry.DocumentName))
                           .Where(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to)
                           .OrderBy(e => e.Name)
                           .Skip((page - 1) * entriesCount).Take(entriesCount)
                           .AsEnumerable().ToArray();

                    case EntryOrderBy.NameDescending:

                        return
                           client.CreateDocumentQuery<Entry>(
                             UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.Entry.DocumentName))
                           .Where(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to)
                           .OrderByDescending(e => e.Name)
                           .Skip((page - 1) * entriesCount).Take(entriesCount)
                           .AsEnumerable().ToArray();

                    case EntryOrderBy.LaunchTypeAscending:

                        return
                           client.CreateDocumentQuery<Entry>(
                             UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.Entry.DocumentName))
                           .Where(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to)
                           .OrderBy(e => e.LaunchType)
                           .Skip((page - 1) * entriesCount).Take(entriesCount)
                           .AsEnumerable().ToArray();

                    case EntryOrderBy.LaunchTypeDescending:

                        return
                           client.CreateDocumentQuery<Entry>(
                             UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.Entry.DocumentName))
                           .Where(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to)
                           .OrderByDescending(e => e.LaunchType)
                           .Skip((page - 1) * entriesCount).Take(entriesCount)
                           .AsEnumerable().ToArray();

                    case EntryOrderBy.DescriptionAscending:

                        return
                           client.CreateDocumentQuery<Entry>(
                             UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.Entry.DocumentName))
                           .Where(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to)
                           .OrderBy(e => e.Description)
                           .Skip((page - 1) * entriesCount).Take(entriesCount)
                           .AsEnumerable().ToArray();

                    case EntryOrderBy.DescriptionDescending:

                        return
                           client.CreateDocumentQuery<Entry>(
                             UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.Entry.DocumentName))
                           .Where(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to)
                           .OrderByDescending(e => e.Description)
                           .Skip((page - 1) * entriesCount).Take(entriesCount)
                           .AsEnumerable().ToArray();

                    case EntryOrderBy.StatusAscending:

                        return
                           client.CreateDocumentQuery<Entry>(
                             UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.Entry.DocumentName))
                           .Where(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to)
                           .OrderBy(e => e.Status)
                           .Skip((page - 1) * entriesCount).Take(entriesCount)
                           .AsEnumerable().ToArray();

                    case EntryOrderBy.StatusDescending:

                        return
                           client.CreateDocumentQuery<Entry>(
                             UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.Entry.DocumentName))
                           .Where(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to)
                           .OrderByDescending(e => e.Status)
                           .Skip((page - 1) * entriesCount).Take(entriesCount)
                           .AsEnumerable().ToArray();
                }
            }

            return new Entry[] { };
        }

        public override long GetEntriesCount(string keyword, DateTime from, DateTime to)
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                return
                        client.CreateDocumentQuery<Entry>(
                            UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.Entry.DocumentName))
                        .Where(e => e.StatusDate > from && e.StatusDate < to)
                        .AsEnumerable().ToArray()
                        .Count();
            }
        }

        public override Core.Db.Entry GetEntry(int workflowId)
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                return
                        client.CreateDocumentQuery<Entry>(
                            UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.Entry.DocumentName))
                        .Where(e => e.WorkflowId == workflowId)
                        .AsEnumerable().ToArray()
                        .FirstOrDefault();
            }
        }

        public override DateTime GetEntryStatusDateMax()
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                var q =
                        client.CreateDocumentQuery<Entry>(
                            UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.Entry.DocumentName))
                        .OrderByDescending(e => e.StatusDate)
                        .AsEnumerable().ToArray();

                if (q.Any())
                {
                    return q.Select(e => e.StatusDate).First();
                }

                return DateTime.Now;
            }
        }

        public override DateTime GetEntryStatusDateMin()
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                var q =
                        client.CreateDocumentQuery<Entry>(
                            UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.Entry.DocumentName))
                        .OrderBy(e => e.StatusDate)
                        .AsEnumerable().ToArray();

                if (q.Any())
                {
                    return q.Select(e => e.StatusDate).First();
                }

                return DateTime.Now;
            }
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries()
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                return
                        client.CreateDocumentQuery<HistoryEntry>(
                            UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.HistoryEntry.DocumentName))
                        .AsEnumerable().ToArray();

            }
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword)
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                var keywordToLower = keyword.ToLower();

                return
                        client.CreateDocumentQuery<HistoryEntry>(
                            UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.HistoryEntry.DocumentName))
                        .Where(e => e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower))
                        .AsEnumerable().ToArray();

            }
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword, int page, int entriesCount)
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                var keywordToLower = keyword.ToLower();
                int skip = (page - 1) * entriesCount;
                return
                        client.CreateDocumentQuery<HistoryEntry>(
                            UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.HistoryEntry.DocumentName))
                        .Where(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)))
                        .Skip((page - 1) * entriesCount).Take(entriesCount)
                        .AsEnumerable().ToArray();

            }
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy heo)
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                var keywordToLower = keyword.ToLower();
                int skip = (page - 1) * entriesCount;

                switch (heo)
                {
                    case EntryOrderBy.StatusDateAscending:

                        return
                            client.CreateDocumentQuery<HistoryEntry>(
                              UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.HistoryEntry.DocumentName))
                            .Where(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to)
                            .OrderBy(e => e.StatusDate)
                            .Skip((page - 1) * entriesCount).Take(entriesCount)
                            .AsEnumerable().ToArray();

                    case EntryOrderBy.StatusDateDescending:

                        return
                           client.CreateDocumentQuery<HistoryEntry>(
                             UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.HistoryEntry.DocumentName))
                           .Where(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to)
                           .OrderByDescending(e => e.StatusDate)
                           .Skip((page - 1) * entriesCount).Take(entriesCount)
                           .AsEnumerable().ToArray();

                    case EntryOrderBy.WorkflowIdAscending:

                        return
                           client.CreateDocumentQuery<HistoryEntry>(
                             UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.HistoryEntry.DocumentName))
                           .Where(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to)
                           .OrderBy(e => e.WorkflowId)
                           .Skip((page - 1) * entriesCount).Take(entriesCount)
                           .AsEnumerable().ToArray();

                    case EntryOrderBy.WorkflowIdDescending:

                        return
                           client.CreateDocumentQuery<HistoryEntry>(
                             UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.HistoryEntry.DocumentName))
                           .Where(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to)
                           .OrderByDescending(e => e.WorkflowId)
                           .Skip((page - 1) * entriesCount).Take(entriesCount)
                           .AsEnumerable().ToArray();

                    case EntryOrderBy.NameAscending:

                        return
                           client.CreateDocumentQuery<HistoryEntry>(
                             UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.HistoryEntry.DocumentName))
                           .Where(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to)
                           .OrderBy(e => e.Name)
                           .Skip((page - 1) * entriesCount).Take(entriesCount)
                           .AsEnumerable().ToArray();

                    case EntryOrderBy.NameDescending:

                        return
                           client.CreateDocumentQuery<HistoryEntry>(
                             UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.HistoryEntry.DocumentName))
                           .Where(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to)
                           .OrderByDescending(e => e.Name)
                           .Skip((page - 1) * entriesCount).Take(entriesCount)
                           .AsEnumerable().ToArray();

                    case EntryOrderBy.LaunchTypeAscending:

                        return
                           client.CreateDocumentQuery<HistoryEntry>(
                             UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.HistoryEntry.DocumentName))
                           .Where(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to)
                           .OrderBy(e => e.LaunchType)
                           .Skip((page - 1) * entriesCount).Take(entriesCount)
                           .AsEnumerable().ToArray();

                    case EntryOrderBy.LaunchTypeDescending:

                        return
                           client.CreateDocumentQuery<HistoryEntry>(
                             UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.HistoryEntry.DocumentName))
                           .Where(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to)
                           .OrderByDescending(e => e.LaunchType)
                           .Skip((page - 1) * entriesCount).Take(entriesCount)
                           .AsEnumerable().ToArray();

                    case EntryOrderBy.DescriptionAscending:

                        return
                           client.CreateDocumentQuery<HistoryEntry>(
                             UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.HistoryEntry.DocumentName))
                           .Where(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to)
                           .OrderBy(e => e.Description)
                           .Skip((page - 1) * entriesCount).Take(entriesCount)
                           .AsEnumerable().ToArray();

                    case EntryOrderBy.DescriptionDescending:

                        return
                           client.CreateDocumentQuery<HistoryEntry>(
                             UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.HistoryEntry.DocumentName))
                           .Where(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to)
                           .OrderByDescending(e => e.Description)
                           .Skip((page - 1) * entriesCount).Take(entriesCount)
                           .AsEnumerable().ToArray();

                    case EntryOrderBy.StatusAscending:

                        return
                           client.CreateDocumentQuery<HistoryEntry>(
                             UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.HistoryEntry.DocumentName))
                           .Where(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to)
                           .OrderBy(e => e.Status)
                           .Skip((page - 1) * entriesCount).Take(entriesCount)
                           .AsEnumerable().ToArray();

                    case EntryOrderBy.StatusDescending:

                        return
                           client.CreateDocumentQuery<HistoryEntry>(
                             UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.HistoryEntry.DocumentName))
                           .Where(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to)
                           .OrderByDescending(e => e.Status)
                           .Skip((page - 1) * entriesCount).Take(entriesCount)
                           .AsEnumerable().ToArray();
                }
            }

            return new HistoryEntry[] { };
        }

        public override long GetHistoryEntriesCount(string keyword)
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                var keywordToLower = keyword.ToLower();
                return
                        client.CreateDocumentQuery<HistoryEntry>(
                            UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.HistoryEntry.DocumentName))
                        .Where(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)))
                        .AsEnumerable().ToArray()
                        .Count();

            }
        }

        public override long GetHistoryEntriesCount(string keyword, DateTime from, DateTime to)
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {

                var keywordToLower = keyword.ToLower();
                return
                        client.CreateDocumentQuery<HistoryEntry>(
                            UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.HistoryEntry.DocumentName))
                        .Where(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to)
                        .AsEnumerable().ToArray()
                        .Count();

            }
        }

        public override DateTime GetHistoryEntryStatusDateMax()
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                var q =
                        client.CreateDocumentQuery<HistoryEntry>(
                            UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.HistoryEntry.DocumentName))
                        .OrderByDescending(e => e.StatusDate)
                        .AsEnumerable().ToArray();

                if (q.Any())
                {
                    return q.Select(e => e.StatusDate).First();
                }

                return DateTime.Now;
            }
        }

        public override DateTime GetHistoryEntryStatusDateMin()
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                var q =
                        client.CreateDocumentQuery<HistoryEntry>(
                            UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.HistoryEntry.DocumentName))
                        .OrderBy(e => e.StatusDate)
                        .AsEnumerable().ToArray();

                if (q.Any())
                {
                    return q.Select(e => e.StatusDate).First();
                }

                return DateTime.Now;
            }
        }

        public override string GetPassword(string username)
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                return
                        client.CreateDocumentQuery<User>(
                            UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.User.DocumentName))
                        .Where(u => u.Username == username)
                        .AsEnumerable().ToArray()
                        .First()
                        .Password;
            }
        }

        public override Core.Db.StatusCount GetStatusCount()
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                return
                        client.CreateDocumentQuery<StatusCount>(
                            UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.StatusCount.DocumentName))
                        .AsEnumerable().ToArray()
                        .FirstOrDefault();
            }
        }

        public override Core.Db.User GetUser(string username)
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                return
                        client.CreateDocumentQuery<User>(
                            UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.User.DocumentName))
                        .Where(u => u.Username == username)
                        .AsEnumerable().ToArray()
                        .FirstOrDefault();
            }
        }

        public override IEnumerable<Core.Db.User> GetUsers()
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                var col = _helper.ReadDocumentFeed(_databaseName, Core.Db.User.DocumentName);
                if (col.Length == 0)
                {
                    return new User[] { };
                }

                return
                        client.CreateDocumentQuery<User>(
                            UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.User.DocumentName))
                        .AsEnumerable().ToArray();
            }
        }

        public override IEnumerable<Core.Db.User> GetUsers(string keyword, UserOrderBy uo)
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                var keywordToLower = keyword.ToLower();

                switch (uo)
                {
                    case UserOrderBy.UsernameAscending:
                        return client.CreateDocumentQuery<User>(
                            UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.User.DocumentName))
                            .Where(u => u.Username.ToLower().Contains(keywordToLower))
                            .OrderBy(u => u.Username)
                            .AsEnumerable().ToArray();
                    case UserOrderBy.UsernameDescending:
                        return client.CreateDocumentQuery<User>(
                           UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.User.DocumentName))
                           .Where(u => u.Username.ToLower().Contains(keywordToLower))
                           .OrderByDescending(u => u.Username)
                           .AsEnumerable().ToArray();
                }
            }

            return new User[] { };
        }

        public override IEnumerable<string> GetUserWorkflows(string userId)
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                return
                        client.CreateDocumentQuery<UserWorkflow>(
                            UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.UserWorkflow.DocumentName))
                        .Where(uw => uw.UserId == userId)
                        .AsEnumerable().ToArray()
                        .Select(uw => uw.WorkflowId);
            }
        }

        public override Core.Db.Workflow GetWorkflow(string id)
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                return
                        client.CreateDocumentQuery<Workflow>(
                            UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.Workflow.DocumentName))
                        .Where(w => w.Id == id)
                        .AsEnumerable().ToArray()
                        .FirstOrDefault();
            }
        }

        public override IEnumerable<Core.Db.Workflow> GetWorkflows()
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                return client.CreateDocumentQuery<Workflow>(
                                UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.Workflow.DocumentName))
                            .AsEnumerable().ToArray();
            }
        }

        public override void IncrementDisabledCount()
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                StatusCount statusCount = client.CreateDocumentQuery<StatusCount>(
                UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.StatusCount.DocumentName))
                .AsEnumerable().ToArray()
                .First();

                statusCount.DisabledCount++;

                _helper.ReplaceDocument(_databaseName, Core.Db.StatusCount.DocumentName, statusCount, statusCount.Id);
            }
        }

        public override void IncrementDisapprovedCount()
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                StatusCount statusCount = client.CreateDocumentQuery<StatusCount>(
                UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.StatusCount.DocumentName))
                .AsEnumerable().ToArray()
                .First();

                statusCount.DisapprovedCount++;

                _helper.ReplaceDocument(_databaseName, Core.Db.StatusCount.DocumentName, statusCount, statusCount.Id);
            }
        }

        public override void IncrementDoneCount()
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                StatusCount statusCount = client.CreateDocumentQuery<StatusCount>(
                UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.StatusCount.DocumentName))
                .AsEnumerable().ToArray()
                .First();

                statusCount.DoneCount++;

                _helper.ReplaceDocument(_databaseName, Core.Db.StatusCount.DocumentName, statusCount, statusCount.Id);
            }
        }

        public override void IncrementFailedCount()
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                StatusCount statusCount = client.CreateDocumentQuery<StatusCount>(
                UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.StatusCount.DocumentName))
                .AsEnumerable().ToArray()
                .First();

                statusCount.FailedCount++;

                _helper.ReplaceDocument(_databaseName, Core.Db.StatusCount.DocumentName, statusCount, statusCount.Id);
            }
        }

        public override void IncrementPendingCount()
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                StatusCount statusCount = client.CreateDocumentQuery<StatusCount>(
                UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.StatusCount.DocumentName))
                .AsEnumerable().ToArray()
                .First();

                statusCount.PendingCount++;

                _helper.ReplaceDocument(_databaseName, Core.Db.StatusCount.DocumentName, statusCount, statusCount.Id);
            }
        }

        public override void IncrementRunningCount()
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                StatusCount statusCount = client.CreateDocumentQuery<StatusCount>(
                UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.StatusCount.DocumentName))
                .AsEnumerable().ToArray()
                .First();

                statusCount.RunningCount++;

                _helper.ReplaceDocument(_databaseName, Core.Db.StatusCount.DocumentName, statusCount, statusCount.Id);
            }
        }

        public override void IncrementStoppedCount()
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                StatusCount statusCount = client.CreateDocumentQuery<StatusCount>(
                UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.StatusCount.DocumentName))
                .AsEnumerable().ToArray()
                .First();

                statusCount.StoppedCount++;

                _helper.ReplaceDocument(_databaseName, Core.Db.StatusCount.DocumentName, statusCount, statusCount.Id);
            }
        }

        public override void IncrementWarningCount()
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                StatusCount statusCount = client.CreateDocumentQuery<StatusCount>(
                UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.StatusCount.DocumentName))
                .AsEnumerable().ToArray()
                .First();

                statusCount.WarningCount++;

                _helper.ReplaceDocument(_databaseName, Core.Db.StatusCount.DocumentName, statusCount, statusCount.Id);
            }
        }

        public override void InsertEntry(Core.Db.Entry entry)
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                _helper.CreateDocument(_databaseName,
                    Core.Db.Entry.DocumentName,
                    new Entry
                    {
                        //Id = Guid.NewGuid().ToString(),
                        Name = entry.Name,
                        Description = entry.Description,
                        LaunchType = entry.LaunchType,
                        Status = entry.Status,
                        StatusDate = entry.StatusDate,
                        WorkflowId = entry.WorkflowId
                    });
            }
        }

        public override void InsertHistoryEntry(Core.Db.HistoryEntry entry)
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                _helper.CreateDocument(_databaseName,
                    Core.Db.HistoryEntry.DocumentName,
                    new HistoryEntry
                    {
                        //Id = Guid.NewGuid().ToString(),
                        Name = entry.Name,
                        Description = entry.Description,
                        LaunchType = entry.LaunchType,
                        Status = entry.Status,
                        StatusDate = entry.StatusDate,
                        WorkflowId = entry.WorkflowId
                    });
            }
        }

        public override void InsertUser(Core.Db.User user)
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                _helper.CreateDocument(_databaseName,
                    Core.Db.User.DocumentName,
                    new User
                    {
                        //Id = Guid.NewGuid().ToString(),
                        Username = user.Username,
                        Password = user.Password,
                        Email = user.Email,
                        CreatedOn = DateTime.Now,
                        ModifiedOn = user.ModifiedOn,
                        UserProfile = user.UserProfile

                    });
            }
        }

        public override void InsertUserWorkflowRelation(Core.Db.UserWorkflow userWorkflow)
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                _helper.CreateDocument(_databaseName,
                    Core.Db.UserWorkflow.DocumentName,
                    new UserWorkflow
                    {
                        //Id = Guid.NewGuid().ToString(),
                        UserId = userWorkflow.UserId,
                        WorkflowId = userWorkflow.WorkflowId

                    });
            }
        }

        public override string InsertWorkflow(Core.Db.Workflow workflow)
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                var response = _helper.CreateDocument(_databaseName,
                    Core.Db.Workflow.DocumentName,
                    new Workflow
                    {
                        Xml = workflow.Xml
                    });

                var wf = (Workflow)(dynamic)response.Resource;
                return wf.Id;
            }
        }

        public override void UpdateEntry(string id, Core.Db.Entry entry)
        {
            _helper.ReplaceDocument(_databaseName,
                Core.Db.Entry.DocumentName,
                new Entry
                {
                    Id = id,
                    Name = entry.Name,
                    Description = entry.Description,
                    LaunchType = entry.LaunchType,
                    Status = entry.Status,
                    StatusDate = entry.StatusDate,
                    WorkflowId = entry.WorkflowId
                },
                id);
        }

        public override void UpdatePassword(string username, string password)
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                var user = client.CreateDocumentQuery<User>(
                         UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.User.DocumentName))
                    .Where(u => u.Username == username && u.Password == password)
                    .AsEnumerable().ToArray()
                    .FirstOrDefault();

                if (user != null)
                {
                    _helper.ReplaceDocument(_databaseName,
                        Core.Db.User.DocumentName,
                        new User
                        {
                            Id = user.Id,
                            Username = user.Username,
                            Password = password,
                            Email = user.Email,
                            CreatedOn = user.CreatedOn,
                            ModifiedOn = DateTime.Now,
                            UserProfile = user.UserProfile

                        },
                        user.Id);
                }
            }
        }

        public override void UpdateUser(string id, Core.Db.User user)
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                var uu = client.CreateDocumentQuery<User>(
                        UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.User.DocumentName))
                   .Where(u => u.Id == id)
                   .AsEnumerable().ToArray()
                   .FirstOrDefault();

                if (uu != null)
                {
                    _helper.ReplaceDocument(_databaseName, Core.Db.User.DocumentName,
                    new User
                    {
                        Id = id,
                        Username = user.Username,
                        Password = user.Password,
                        Email = user.Email,
                        CreatedOn = uu.CreatedOn,
                        ModifiedOn = DateTime.Now,
                        UserProfile = user.UserProfile

                    },
                    id);
                }
            }
        }

        public override void UpdateUsernameAndEmailAndUserProfile(string userId, string username, string email, UserProfile up)
        {
            using (var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey))
            {
                var user = client.CreateDocumentQuery<User>(
                         UriFactory.CreateDocumentCollectionUri(_databaseName, Core.Db.User.DocumentName))
                    .Where(u => u.Id == userId)
                    .AsEnumerable().ToArray()
                    .FirstOrDefault();

                if (user != null)
                {
                    _helper.ReplaceDocument(_databaseName,
                        Core.Db.User.DocumentName,
                        new User
                        {
                            Id = userId,
                            Username = username,
                            Password = user.Password,
                            Email = email,
                            CreatedOn = user.CreatedOn,
                            ModifiedOn = DateTime.Now,
                            UserProfile = up

                        },
                        userId);
                }
            }
        }

        public override void UpdateWorkflow(string dbId, Core.Db.Workflow workflow)
        {
            _helper.ReplaceDocument(_databaseName,
                        Core.Db.Workflow.DocumentName,
                        new Workflow
                        {
                            Id = dbId,
                            Xml = workflow.Xml
                        },
                        dbId);
        }
    }
}
