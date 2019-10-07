using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using Wexflow.Core.Db;

namespace Wexflow.Core.MongoDB
{
    public class Db : Core.Db.Db
    {
        private static IMongoDatabase _db;

        public Db(string connectionString) : base(connectionString)
        {
            string databaseName = "wexflow";
            string mongoUrl = string.Empty;
            bool enabledSslProtocols = false;
            SslProtocols sslProtocols = SslProtocols.None;

            var connectionStringParts = ConnectionString.Split(';');
            foreach (var part in connectionStringParts)
            {
                if (!string.IsNullOrEmpty(part.Trim()))
                {
                    string connPart = part.TrimStart(' ').TrimEnd(' ');
                    if (connPart.StartsWith("DatabaseName="))
                    {
                        databaseName = connPart.Replace("DatabaseName=", string.Empty);
                    }
                    else if (connPart.StartsWith("MongoUrl="))
                    {
                        mongoUrl = connPart.Replace("MongoUrl=", string.Empty);
                    }
                    else if (connPart.StartsWith("EnabledSslProtocols="))
                    {
                        enabledSslProtocols = bool.Parse(connPart.Replace("EnabledSslProtocols=", string.Empty));
                    }
                    else if (connPart.StartsWith("SslProtocols="))
                    {
                        sslProtocols = (SslProtocols)Enum.Parse(typeof(SslProtocols), connPart.Replace("SslProtocols=", string.Empty), true);
                    }
                }
            }

            var settings = MongoClientSettings.FromUrl(new MongoUrl(mongoUrl));

            if (enabledSslProtocols)
            {
                settings.SslSettings = new SslSettings() { EnabledSslProtocols = sslProtocols };
            }

            var client = new MongoClient(settings);
            _db = client.GetDatabase(databaseName);
        }

        public override void Init()
        {
            // StatusCount
            ClearStatusCount();
            var statusCountCol = _db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);

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

            statusCountCol.InsertOne(statusCount);

            // Entries
            ClearEntries();

            // Insert default user if necessary
            var usersCol = _db.GetCollection<User>(Core.Db.User.DocumentName);
            if (usersCol.CountDocuments(FilterDefinition<User>.Empty) == 0)
            {
                InsertDefaultUser();
            }
        }

        public override IEnumerable<Core.Db.Workflow> GetWorkflows()
        {
            var col = _db.GetCollection<Workflow>(Core.Db.Workflow.DocumentName);
            return col.Find(FilterDefinition<Workflow>.Empty).ToEnumerable();
        }

        public override string InsertWorkflow(Core.Db.Workflow workflow)
        {
            var col = _db.GetCollection<Workflow>(Core.Db.Workflow.DocumentName);
            var wf = new Workflow { Xml = workflow.Xml };
            col.InsertOne(wf);
            return wf.Id;
        }

        public override Core.Db.Workflow GetWorkflow(string id)
        {
            var col = _db.GetCollection<Workflow>(Core.Db.Workflow.DocumentName);
            return col.Find(w => w.Id == id).FirstOrDefault();
        }

        public override void UpdateWorkflow(string dbId, Core.Db.Workflow workflow)
        {
            var col = _db.GetCollection<Workflow>(Core.Db.Workflow.DocumentName);
            var update = Builders<Workflow>.Update
                .Set(w => w.Xml, workflow.Xml);

            col.UpdateOne(w => w.Id == dbId, update);
        }

        public override void DeleteWorkflow(string id)
        {
            var col = _db.GetCollection<Workflow>(Core.Db.Workflow.DocumentName);
            col.DeleteOne(e => e.Id == id);
        }

        public override void DeleteUserWorkflowRelationsByWorkflowId(string workflowDbId)
        {
            var col = _db.GetCollection<UserWorkflow>(Core.Db.UserWorkflow.DocumentName);
            col.DeleteMany(uw => uw.WorkflowId == workflowDbId);
        }

        public override void DeleteWorkflows(string[] ids)
        {
            var col = _db.GetCollection<Workflow>(Core.Db.Workflow.DocumentName);
            col.DeleteMany(e => ids.Contains(e.Id));
        }

        public override void InsertUserWorkflowRelation(Core.Db.UserWorkflow userWorkflow)
        {
            var col = _db.GetCollection<UserWorkflow>(Core.Db.UserWorkflow.DocumentName);
            var uw = new UserWorkflow
            {
                UserId = userWorkflow.UserId,
                WorkflowId = userWorkflow.WorkflowId
            };
            col.InsertOne(uw);
        }

        public override void DeleteUserWorkflowRelationsByUserId(string userId)
        {
            var col = _db.GetCollection<UserWorkflow>(Core.Db.UserWorkflow.DocumentName);
            col.DeleteMany(uw => uw.UserId == userId);
        }

        public override IEnumerable<string> GetUserWorkflows(string userId)
        {
            var col = _db.GetCollection<UserWorkflow>(Core.Db.UserWorkflow.DocumentName);
            return col.Find(uw => uw.UserId == userId).ToEnumerable().Select(uw => uw.WorkflowId);
        }

        public override bool CheckUserWorkflow(string userId, string workflowId)
        {
            var col = _db.GetCollection<UserWorkflow>(Core.Db.UserWorkflow.DocumentName);
            var res = col.Find(uw => uw.UserId == userId && uw.WorkflowId == workflowId).FirstOrDefault();
            return res != null;
        }

        public override IEnumerable<Core.Db.User> GetAdministrators(string keyword, UserOrderBy uo)
        {
            var col = _db.GetCollection<User>(Core.Db.User.DocumentName);
            var keywordToLower = keyword.ToLower();

            switch (uo)
            {
                case UserOrderBy.UsernameAscending:
                    return col.Find(u => u.Username.ToLower().Contains(keywordToLower) && u.UserProfile == UserProfile.Administrator).Sort(Builders<User>.Sort.Ascending(u => u.Username)).ToEnumerable();
                case UserOrderBy.UsernameDescending:
                    return col.Find(u => u.Username.ToLower().Contains(keywordToLower) && u.UserProfile == UserProfile.Administrator).Sort(Builders<User>.Sort.Descending(u => u.Username)).ToEnumerable();
            }

            return new User[] { };
        }

        public override void ClearStatusCount()
        {
            var col = _db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
            col.DeleteMany(FilterDefinition<StatusCount>.Empty);
        }

        public override void ClearEntries()
        {
            var col = _db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
            col.DeleteMany(FilterDefinition<Entry>.Empty);
        }

        public override Core.Db.StatusCount GetStatusCount()
        {
            var col = _db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
            var statusCount = col.Find(FilterDefinition<StatusCount>.Empty).FirstOrDefault();
            return statusCount;
        }

        public override IEnumerable<Core.Db.Entry> GetEntries()
        {
            var col = _db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
            return col.Find(FilterDefinition<Entry>.Empty).ToEnumerable();
        }

        public override void InsertUser(Core.Db.User user)
        {
            var col = _db.GetCollection<User>(Core.Db.User.DocumentName);
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
            col.InsertOne(nu);
            col.Indexes.CreateOne(new CreateIndexModel<User>(Builders<User>.IndexKeys.Ascending(u => u.Username)));
            col.Indexes.CreateOne(new CreateIndexModel<User>(Builders<User>.IndexKeys.Ascending(u => u.UserProfile)));
        }

        public override void UpdateUser(string id, Core.Db.User user)
        {
            var col = _db.GetCollection<User>(Core.Db.User.DocumentName);
            var update = Builders<User>.Update
                .Set(u => u.ModifiedOn, DateTime.Now)
                .Set(u => u.Username, user.Username)
                .Set(u => u.Password, user.Password)
                .Set(u => u.UserProfile, user.UserProfile)
                .Set(u => u.Email, user.Email);

            col.UpdateOne(u => u.Id == id, update);
        }

        public override void UpdateUsernameAndEmailAndUserProfile(string userId, string username, string email, UserProfile up)
        {
            var col = _db.GetCollection<User>(Core.Db.User.DocumentName);

            var update = Builders<User>.Update
                .Set(u => u.ModifiedOn, DateTime.Now)
                .Set(u => u.Username, username)
                .Set(u => u.UserProfile, up)
                .Set(u => u.Email, email);

            col.UpdateOne(u => u.Id == userId, update);

        }

        public override Core.Db.User GetUser(string username)
        {
            var col = _db.GetCollection<User>(Core.Db.User.DocumentName);
            var user = col.Find(u => u.Username == username).FirstOrDefault();
            return user;
        }

        public override void DeleteUser(string username, string password)
        {
            var col = _db.GetCollection<User>(Core.Db.User.DocumentName);
            var user = col.Find(u => u.Username == username).FirstOrDefault();
            if (user != null && user.Password == password)
            {
                col.DeleteMany(u => u.Username == username);
                DeleteUserWorkflowRelationsByUserId(user.Id);
            }
            else
            {
                throw new Exception("The password is incorrect.");
            }
        }

        public override string GetPassword(string username)
        {
            var col = _db.GetCollection<User>(Core.Db.User.DocumentName);
            User user = col.Find(u => u.Username == username).First();
            return user.Password;
        }

        public override IEnumerable<Core.Db.User> GetUsers()
        {
            var col = _db.GetCollection<User>(Core.Db.User.DocumentName);
            return col.Find(FilterDefinition<User>.Empty).ToEnumerable();
        }

        public override IEnumerable<Core.Db.User> GetUsers(string keyword, UserOrderBy uo)
        {
            var col = _db.GetCollection<User>(Core.Db.User.DocumentName);
            var keywordToLower = keyword.ToLower();

            switch (uo)
            {
                case UserOrderBy.UsernameAscending:
                    return col.Find(u => u.Username.ToLower().Contains(keywordToLower)).Sort(Builders<User>.Sort.Ascending(u => u.Username)).ToEnumerable();
                case UserOrderBy.UsernameDescending:
                    return col.Find(u => u.Username.ToLower().Contains(keywordToLower)).Sort(Builders<User>.Sort.Descending(u => u.Username)).ToEnumerable();
            }

            return new User[] { };
        }

        public override void UpdatePassword(string username, string password)
        {
            var col = _db.GetCollection<User>(Core.Db.User.DocumentName);
            var dbUser = col.Find(u => u.Username == username).First();
            dbUser.Password = password;

            var update = Builders<User>.Update
                .Set(u => u.Password, dbUser.Password);

            col.UpdateOne(u => u.Id == dbUser.Id, update);
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries()
        {
            var col = _db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
            return col.Find(FilterDefinition<HistoryEntry>.Empty).ToEnumerable();
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword)
        {
            var keywordToUpper = keyword.ToUpper();
            var col = _db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
            return col.Find(e => e.Name.ToUpper().Contains(keywordToUpper) || e.Description.ToUpper().Contains(keywordToUpper)).ToEnumerable();
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword, int page, int entriesCount)
        {
            var keywordToUpper = keyword.ToUpper();
            var col = _db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
            return col.Find(e => e.Name.ToUpper().Contains(keywordToUpper) || e.Description.ToUpper().Contains(keywordToUpper)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy heo)
        {
            var col = _db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
            var keywordToLower = keyword.ToLower();
            int skip = (page - 1) * entriesCount;

            switch (heo)
            {
                case EntryOrderBy.StatusDateAscending:

                    return col.Find(he => (he.Name.ToLower().Contains(keywordToLower) || he.Description.ToLower().Contains(keywordToLower)) && he.StatusDate > from && he.StatusDate < to).Sort(Builders<HistoryEntry>.Sort.Ascending(he => he.StatusDate)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                case EntryOrderBy.StatusDateDescending:

                    return col.Find(he => (he.Name.ToLower().Contains(keywordToLower) || he.Description.ToLower().Contains(keywordToLower)) && he.StatusDate > from && he.StatusDate < to).Sort(Builders<HistoryEntry>.Sort.Descending(he => he.StatusDate)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                case EntryOrderBy.WorkflowIdAscending:

                    return col.Find(he => (he.Name.ToLower().Contains(keywordToLower) || he.Description.ToLower().Contains(keywordToLower)) && he.StatusDate > from && he.StatusDate < to).Sort(Builders<HistoryEntry>.Sort.Ascending(he => he.WorkflowId)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                case EntryOrderBy.WorkflowIdDescending:

                    return col.Find(he => (he.Name.ToLower().Contains(keywordToLower) || he.Description.ToLower().Contains(keywordToLower)) && he.StatusDate > from && he.StatusDate < to).Sort(Builders<HistoryEntry>.Sort.Descending(he => he.WorkflowId)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                case EntryOrderBy.NameAscending:

                    return col.Find(he => (he.Name.ToLower().Contains(keywordToLower) || he.Description.ToLower().Contains(keywordToLower)) && he.StatusDate > from && he.StatusDate < to).Sort(Builders<HistoryEntry>.Sort.Ascending(he => he.Name)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                case EntryOrderBy.NameDescending:

                    return col.Find(he => (he.Name.ToLower().Contains(keywordToLower) || he.Description.ToLower().Contains(keywordToLower)) && he.StatusDate > from && he.StatusDate < to).Sort(Builders<HistoryEntry>.Sort.Descending(he => he.Name)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                case EntryOrderBy.LaunchTypeAscending:

                    return col.Find(he => (he.Name.ToLower().Contains(keywordToLower) || he.Description.ToLower().Contains(keywordToLower)) && he.StatusDate > from && he.StatusDate < to).Sort(Builders<HistoryEntry>.Sort.Ascending(he => he.LaunchType)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                case EntryOrderBy.LaunchTypeDescending:

                    return col.Find(he => (he.Name.ToLower().Contains(keywordToLower) || he.Description.ToLower().Contains(keywordToLower)) && he.StatusDate > from && he.StatusDate < to).Sort(Builders<HistoryEntry>.Sort.Descending(he => he.LaunchType)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                case EntryOrderBy.DescriptionAscending:

                    return col.Find(he => (he.Name.ToLower().Contains(keywordToLower) || he.Description.ToLower().Contains(keywordToLower)) && he.StatusDate > from && he.StatusDate < to).Sort(Builders<HistoryEntry>.Sort.Ascending(he => he.Description)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                case EntryOrderBy.DescriptionDescending:

                    return col.Find(he => (he.Name.ToLower().Contains(keywordToLower) || he.Description.ToLower().Contains(keywordToLower)) && he.StatusDate > from && he.StatusDate < to).Sort(Builders<HistoryEntry>.Sort.Descending(he => he.Description)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                case EntryOrderBy.StatusAscending:

                    return col.Find(he => (he.Name.ToLower().Contains(keywordToLower) || he.Description.ToLower().Contains(keywordToLower)) && he.StatusDate > from && he.StatusDate < to).Sort(Builders<HistoryEntry>.Sort.Ascending(he => he.Status)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                case EntryOrderBy.StatusDescending:

                    return col.Find(he => (he.Name.ToLower().Contains(keywordToLower) || he.Description.ToLower().Contains(keywordToLower)) && he.StatusDate > from && he.StatusDate < to).Sort(Builders<HistoryEntry>.Sort.Descending(he => he.Status)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);
            }

            return new HistoryEntry[] { };
        }

        public override IEnumerable<Core.Db.Entry> GetEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy eo)
        {
            var col = _db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
            var keywordToLower = keyword.ToLower();
            int skip = (page - 1) * entriesCount;

            switch (eo)
            {
                case EntryOrderBy.StatusDateAscending:

                    return col.Find(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to).Sort(Builders<Entry>.Sort.Ascending(e => e.StatusDate)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                case EntryOrderBy.StatusDateDescending:

                    return col.Find(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to).Sort(Builders<Entry>.Sort.Descending(e => e.StatusDate)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                case EntryOrderBy.WorkflowIdAscending:

                    return col.Find(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to).Sort(Builders<Entry>.Sort.Ascending(e => e.WorkflowId)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                case EntryOrderBy.WorkflowIdDescending:

                    return col.Find(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to).Sort(Builders<Entry>.Sort.Descending(e => e.WorkflowId)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                case EntryOrderBy.NameAscending:

                    return col.Find(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to).Sort(Builders<Entry>.Sort.Ascending(e => e.Name)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                case EntryOrderBy.NameDescending:

                    return col.Find(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to).Sort(Builders<Entry>.Sort.Descending(e => e.Name)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                case EntryOrderBy.LaunchTypeAscending:

                    return col.Find(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to).Sort(Builders<Entry>.Sort.Ascending(e => e.LaunchType)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                case EntryOrderBy.LaunchTypeDescending:

                    return col.Find(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to).Sort(Builders<Entry>.Sort.Descending(e => e.LaunchType)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                case EntryOrderBy.DescriptionAscending:

                    return col.Find(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to).Sort(Builders<Entry>.Sort.Ascending(e => e.Description)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                case EntryOrderBy.DescriptionDescending:

                    return col.Find(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to).Sort(Builders<Entry>.Sort.Descending(e => e.Description)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                case EntryOrderBy.StatusAscending:

                    return col.Find(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to).Sort(Builders<Entry>.Sort.Ascending(e => e.Status)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);

                case EntryOrderBy.StatusDescending:

                    return col.Find(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to).Sort(Builders<Entry>.Sort.Descending(e => e.Status)).ToEnumerable().Skip((page - 1) * entriesCount).Take(entriesCount);
            }

            return new Entry[] { };
        }

        public override long GetHistoryEntriesCount(string keyword)
        {
            var keywordToUpper = keyword.ToUpper();
            var col = _db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
            return col.Find(e => e.Name.ToUpper().Contains(keywordToUpper) || e.Description.ToUpper().Contains(keywordToUpper)).CountDocuments();
        }

        public override long GetHistoryEntriesCount(string keyword, DateTime from, DateTime to)
        {
            var keywordToLower = keyword.ToLower();
            var col = _db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);

            return col.Find(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to).CountDocuments();
        }

        public override long GetEntriesCount(string keyword, DateTime from, DateTime to)
        {
            var keywordToLower = keyword.ToLower();
            var col = _db.GetCollection<HistoryEntry>(Core.Db.Entry.DocumentName);

            return col.Find(e => (e.Name.ToLower().Contains(keywordToLower) || e.Description.ToLower().Contains(keywordToLower)) && e.StatusDate > from && e.StatusDate < to).CountDocuments();
        }

        public override DateTime GetHistoryEntryStatusDateMin()
        {
            var col = _db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
            var q = col.Find(FilterDefinition<HistoryEntry>.Empty).Sort(Builders<HistoryEntry>.Sort.Ascending(e => e.StatusDate)).ToEnumerable();
            if (q.Any())
            {
                return q.Select(e => e.StatusDate).First();
            }

            return DateTime.Now;
        }

        public override DateTime GetHistoryEntryStatusDateMax()
        {
            var col = _db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
            var q = col.Find(FilterDefinition<HistoryEntry>.Empty).Sort(Builders<HistoryEntry>.Sort.Descending(e => e.StatusDate)).ToEnumerable();
            if (q.Any())
            {
                return q.Select(e => e.StatusDate).First();
            }

            return DateTime.Now;
        }

        public override DateTime GetEntryStatusDateMin()
        {
            var col = _db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
            var q = col.Find(FilterDefinition<Entry>.Empty).Sort(Builders<Entry>.Sort.Ascending(e => e.StatusDate)).ToEnumerable();
            if (q.Any())
            {
                return q.Select(e => e.StatusDate).First();
            }

            return DateTime.Now;
        }

        public override DateTime GetEntryStatusDateMax()
        {
            var col = _db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
            var q = col.Find(FilterDefinition<Entry>.Empty).Sort(Builders<Entry>.Sort.Descending(e => e.StatusDate)).ToEnumerable();
            if (q.Any())
            {
                return q.Select(e => e.StatusDate).First();
            }

            return DateTime.Now;
        }

        public override void IncrementDisabledCount()
        {
            var col = _db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
            var statusCount = col.Find(FilterDefinition<StatusCount>.Empty).FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.DisabledCount++;

                var update = Builders<StatusCount>.Update
                    .Set(sc => sc.DisabledCount, statusCount.DisabledCount);

                col.UpdateOne(sc => sc.Id == statusCount.Id, update);
            }
        }

        public override void IncrementRunningCount()
        {
            var col = _db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
            var statusCount = col.Find(FilterDefinition<StatusCount>.Empty).FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.RunningCount++;

                var update = Builders<StatusCount>.Update
                    .Set(sc => sc.RunningCount, statusCount.RunningCount);

                col.UpdateOne(sc => sc.Id == statusCount.Id, update);
            }
        }

        public override Core.Db.Entry GetEntry(int workflowId)
        {
            var col = _db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
            return col.Find(e => e.WorkflowId == workflowId).FirstOrDefault();
        }

        public override void InsertEntry(Core.Db.Entry entry)
        {
            var col = _db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
            var ie = new Entry
            {
                Description = entry.Description,
                LaunchType = entry.LaunchType,
                Name = entry.Name,
                Status = entry.Status,
                StatusDate = entry.StatusDate,
                WorkflowId = entry.WorkflowId
            };
            col.InsertOne(ie);
            col.Indexes.CreateOne(new CreateIndexModel<Entry>(Builders<Entry>.IndexKeys.Ascending(e => e.WorkflowId)));
            col.Indexes.CreateOne(new CreateIndexModel<Entry>(Builders<Entry>.IndexKeys.Ascending(e => e.Name)));
            col.Indexes.CreateOne(new CreateIndexModel<Entry>(Builders<Entry>.IndexKeys.Ascending(e => e.LaunchType)));
            col.Indexes.CreateOne(new CreateIndexModel<Entry>(Builders<Entry>.IndexKeys.Ascending(e => e.Description)));
            col.Indexes.CreateOne(new CreateIndexModel<Entry>(Builders<Entry>.IndexKeys.Ascending(e => e.Status)));
            col.Indexes.CreateOne(new CreateIndexModel<Entry>(Builders<Entry>.IndexKeys.Ascending(e => e.StatusDate)));
        }

        public override void UpdateEntry(string id, Core.Db.Entry entry)
        {
            var col = _db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
            var update = Builders<Entry>.Update
                .Set(e => e.Name, entry.Name)
                .Set(e => e.Description, entry.Description)
                .Set(e => e.LaunchType, entry.LaunchType)
                .Set(e => e.Status, entry.Status)
                .Set(e => e.StatusDate, entry.StatusDate)
                .Set(e => e.WorkflowId, entry.WorkflowId);

            col.UpdateOne(e => e.Id == id, update);
        }

        public override void IncrementDisapprovedCount()
        {
            var col = _db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
            var statusCount = col.Find(FilterDefinition<StatusCount>.Empty).FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.DisapprovedCount++;

                var update = Builders<StatusCount>.Update
                    .Set(sc => sc.DisapprovedCount, statusCount.DisapprovedCount);

                col.UpdateOne(sc => sc.Id == statusCount.Id, update);
            }
        }

        public override void IncrementDoneCount()
        {
            var col = _db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
            var statusCount = col.Find(FilterDefinition<StatusCount>.Empty).FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.DoneCount++;

                var update = Builders<StatusCount>.Update
                    .Set(sc => sc.DoneCount, statusCount.DoneCount);

                col.UpdateOne(sc => sc.Id == statusCount.Id, update);
            }
        }

        public override void IncrementWarningCount()
        {
            var col = _db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
            var statusCount = col.Find(FilterDefinition<StatusCount>.Empty).FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.WarningCount++;

                var update = Builders<StatusCount>.Update
                    .Set(sc => sc.WarningCount, statusCount.WarningCount);

                col.UpdateOne(sc => sc.Id == statusCount.Id, update);
            }
        }

        public override void IncrementFailedCount()
        {
            var col = _db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
            var statusCount = col.Find(FilterDefinition<StatusCount>.Empty).FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.FailedCount++;

                var update = Builders<StatusCount>.Update
                    .Set(sc => sc.FailedCount, statusCount.FailedCount);

                col.UpdateOne(sc => sc.Id == statusCount.Id, update);
            }
        }

        public override void InsertHistoryEntry(Core.Db.HistoryEntry entry)
        {
            var col = _db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
            var he = new HistoryEntry
            {
                Description = entry.Description,
                LaunchType = entry.LaunchType,
                Name = entry.Name,
                Status = entry.Status,
                StatusDate = entry.StatusDate,
                WorkflowId = entry.WorkflowId
            };
            col.InsertOne(he);
            col.Indexes.CreateOne(new CreateIndexModel<HistoryEntry>(Builders<HistoryEntry>.IndexKeys.Ascending(e => e.WorkflowId)));
            col.Indexes.CreateOne(new CreateIndexModel<HistoryEntry>(Builders<HistoryEntry>.IndexKeys.Ascending(e => e.Name)));
            col.Indexes.CreateOne(new CreateIndexModel<HistoryEntry>(Builders<HistoryEntry>.IndexKeys.Ascending(e => e.LaunchType)));
            col.Indexes.CreateOne(new CreateIndexModel<HistoryEntry>(Builders<HistoryEntry>.IndexKeys.Ascending(e => e.Description)));
            col.Indexes.CreateOne(new CreateIndexModel<HistoryEntry>(Builders<HistoryEntry>.IndexKeys.Ascending(e => e.Status)));
            col.Indexes.CreateOne(new CreateIndexModel<HistoryEntry>(Builders<HistoryEntry>.IndexKeys.Ascending(e => e.StatusDate)));

        }

        public override void DecrementRunningCount()
        {
            var col = _db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
            var statusCount = col.Find(FilterDefinition<StatusCount>.Empty).FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.RunningCount--;

                var update = Builders<StatusCount>.Update
                    .Set(sc => sc.RunningCount, statusCount.RunningCount);

                col.UpdateOne(sc => sc.Id == statusCount.Id, update);
            }
        }

        public override void IncrementStoppedCount()
        {
            var col = _db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
            var statusCount = col.Find(FilterDefinition<StatusCount>.Empty).FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.StoppedCount++;

                var update = Builders<StatusCount>.Update
                    .Set(sc => sc.StoppedCount, statusCount.StoppedCount);

                col.UpdateOne(sc => sc.Id == statusCount.Id, update);
            }
        }

        public override void IncrementPendingCount()
        {
            var col = _db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
            var statusCount = col.Find(FilterDefinition<StatusCount>.Empty).FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.PendingCount++;

                var update = Builders<StatusCount>.Update
                    .Set(sc => sc.PendingCount, statusCount.PendingCount);

                col.UpdateOne(sc => sc.Id == statusCount.Id, update);
            }
        }

        public override void DecrementPendingCount()
        {
            var col = _db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
            var statusCount = col.Find(FilterDefinition<StatusCount>.Empty).FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.PendingCount--;

                var update = Builders<StatusCount>.Update
                    .Set(sc => sc.PendingCount, statusCount.PendingCount);

                col.UpdateOne(sc => sc.Id == statusCount.Id, update);
            }
        }

    }
}
