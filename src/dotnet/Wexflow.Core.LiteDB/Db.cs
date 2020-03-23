using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using Wexflow.Core.Db;

namespace Wexflow.Core.LiteDB
{
    public class Db : Core.Db.Db
    {
        private static LiteDatabase _db;

        public Db(string connectionString) : base(connectionString)
        {
            _db = new LiteDatabase(ConnectionString);
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

            statusCountCol.Insert(statusCount);

            // Entries
            ClearEntries();

            // Insert default user if necessary
            var usersCol = _db.GetCollection<User>(Core.Db.User.DocumentName);
            if (usersCol.Count() == 0)
            {
                InsertDefaultUser();
            }
        }

        public override void ClearStatusCount()
        {
            var col = _db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
            var statusCount = col.FindAll();
            col.DeleteMany(s => statusCount.Where(ss => ss.Id == s.Id).Count() > 0);
        }

        public override void ClearEntries()
        {
            var col = _db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
            var entries = col.FindAll();
            col.DeleteMany(e => entries.Where(ee => ee.Id == e.Id).Count() > 0);
        }

        public override Core.Db.StatusCount GetStatusCount()
        {
            var col = _db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
            var statusCount = col.FindAll().FirstOrDefault();
            return statusCount;
        }

        public override void IncrementPendingCount()
        {
            var col = _db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
            var statusCount = col.FindAll().FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.PendingCount++;
                col.Update(statusCount);
            }
        }

        public override void DecrementPendingCount()
        {
            var col = _db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
            var statusCount = col.FindAll().FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.PendingCount--;
                col.Update(statusCount);
            }
        }

        public override void IncrementRunningCount()
        {
            var col = _db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
            var statusCount = col.FindAll().FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.RunningCount++;
                col.Update(statusCount);
            }
        }

        public override void DecrementRunningCount()
        {
            var col = _db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
            var statusCount = col.FindAll().FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.RunningCount--;
                col.Update(statusCount);
            }
        }

        public override void IncrementDoneCount()
        {
            var col = _db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
            var statusCount = col.FindAll().FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.DoneCount++;
                col.Update(statusCount);
            }
        }

        public void DecrementDoneCount()
        {
            var col = _db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
            var statusCount = col.FindAll().FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.DoneCount--;
                col.Update(statusCount);
            }
        }

        public override void IncrementFailedCount()
        {
            var col = _db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
            var statusCount = col.FindAll().FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.FailedCount++;
                col.Update(statusCount);
            }
        }

        public override void IncrementDisapprovedCount()
        {
            var col = _db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
            var statusCount = col.FindAll().FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.DisapprovedCount++;
                col.Update(statusCount);
            }
        }

        public void DecrementFailedCount()
        {
            var col = _db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
            var statusCount = col.FindAll().FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.FailedCount--;
                col.Update(statusCount);
            }
        }

        public override void IncrementWarningCount()
        {
            var col = _db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
            var statusCount = col.FindAll().FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.WarningCount++;
                col.Update(statusCount);
            }
        }

        public void DecrementWarningCount()
        {
            var col = _db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
            var statusCount = col.FindAll().FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.WarningCount--;
                col.Update(statusCount);
            }
        }

        public override void IncrementDisabledCount()
        {
            var col = _db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
            var statusCount = col.FindAll().FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.DisabledCount++;
                col.Update(statusCount);
            }
        }

        public void DecrementDisabledCount()
        {
            var col = _db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
            var statusCount = col.FindAll().FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.DisabledCount--;
                col.Update(statusCount);
            }
        }

        public override void IncrementStoppedCount()
        {
            var col = _db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
            var statusCount = col.FindAll().FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.StoppedCount++;
                col.Update(statusCount);
            }
        }

        public void DecrementStoppedCount()
        {
            var col = _db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
            var statusCount = col.FindAll().FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.StoppedCount--;
                col.Update(statusCount);
            }
        }

        public void ResetStatusCount()
        {
            var col = _db.GetCollection<StatusCount>(Core.Db.StatusCount.DocumentName);
            var statusCount = col.FindAll().FirstOrDefault();
            if (statusCount != null)
            {
                statusCount.PendingCount = 0;
                statusCount.RunningCount = 0;
                statusCount.DoneCount = 0;
                statusCount.FailedCount = 0;
                statusCount.WarningCount = 0;
                statusCount.DisabledCount = 0;
                statusCount.DisapprovedCount = 0;
                col.Update(statusCount);
            }
        }

        public override IEnumerable<Core.Db.Entry> GetEntries()
        {
            var col = _db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
            return col.FindAll();
        }

        public override Core.Db.Entry GetEntry(int workflowId)
        {
            var col = _db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
            return col.FindOne(e => e.WorkflowId == workflowId);
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
                WorkflowId = entry.WorkflowId,
                Logs = entry.Logs
            };
            col.Insert(ie);
            col.EnsureIndex(e => e.WorkflowId);
            //col.EnsureIndex(e => e.Name, "LOWER($.Name)");
            col.EnsureIndex(e => e.Name);
            col.EnsureIndex(e => e.LaunchType);
            //col.EnsureIndex(e => e.Description, "LOWER($.Name)");
            col.EnsureIndex(e => e.Description);
            col.EnsureIndex(e => e.Status);
            col.EnsureIndex(e => e.StatusDate);
        }

        public override void UpdateEntry(string id, Core.Db.Entry entry)
        {
            var col = _db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
            var e = new Entry
            {
                Id = int.Parse(id),
                Description = entry.Description,
                LaunchType = entry.LaunchType,
                Name = entry.Name,
                Status = entry.Status,
                StatusDate = entry.StatusDate,
                WorkflowId = entry.WorkflowId,
                Logs = entry.Logs
            };
            col.Update(e);
        }

        public void DeleteEntry(int workflowId)
        {
            var col = _db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
            col.DeleteMany(e => e.WorkflowId == workflowId);
        }

        public override void InsertUser(Core.Db.User user)
        {
            var col = _db.GetCollection<User>(Core.Db.User.DocumentName);
            user.CreatedOn = DateTime.Now;
            col.Insert(new User
            {
                CreatedOn = user.CreatedOn,
                Email = user.Email,
                ModifiedOn = user.ModifiedOn,
                Password = user.Password,
                Username = user.Username,
                UserProfile = user.UserProfile
            });
            //col.EnsureIndex(u => u.Username, "LOWER($.Username)");
            col.EnsureIndex(u => u.Username);
            col.EnsureIndex(u => u.UserProfile);
        }

        public override void UpdatePassword(string username, string password)
        {
            var col = _db.GetCollection<User>(Core.Db.User.DocumentName);
            var dbUser = col.FindOne(u => u.Username == username);
            dbUser.Password = password;
            col.Update(dbUser);
        }

        public override void UpdateUser(string id, Core.Db.User user)
        {
            var col = _db.GetCollection<User>(Core.Db.User.DocumentName);
            var i = int.Parse(id);
            var dbUser = col.FindOne(u => u.Id == i);
            dbUser.ModifiedOn = DateTime.Now;
            dbUser.Username = user.Username;
            dbUser.Password = user.Password;
            dbUser.UserProfile = user.UserProfile;
            dbUser.Email = user.Email;
            col.Update(dbUser);
        }

        public override void UpdateUsernameAndEmailAndUserProfile(string userId, string username, string email, UserProfile up)
        {
            var col = _db.GetCollection<User>(Core.Db.User.DocumentName);
            var i = int.Parse(userId);
            var dbUser = col.FindOne(u => u.Id == i);
            dbUser.ModifiedOn = DateTime.Now;
            dbUser.Username = username;
            dbUser.Email = email;
            dbUser.UserProfile = up;
            col.Update(dbUser);
        }

        public override void DeleteUser(string username, string password)
        {
            var col = _db.GetCollection<User>(Core.Db.User.DocumentName);
            var user = col.FindOne(u => u.Username == username);
            if (user != null && user.Password == password)
            {
                col.DeleteMany(u => u.Username == username);
                DeleteUserWorkflowRelationsByUserId(user.Id.ToString());
            }
            else
            {
                throw new Exception("The password is incorrect.");
            }
        }

        public override Core.Db.User GetUser(string username)
        {
            var col = _db.GetCollection<User>(Core.Db.User.DocumentName);
            User user = col.FindOne(u => u.Username == username);
            return user;
        }

        public override Core.Db.User GetUserByUserId(string userId)
        {
            var col = _db.GetCollection<User>(Core.Db.User.DocumentName);
            var id = int.Parse(userId);
            User user = col.FindOne(u => u.Id == id);
            return user;
        }

        public override string GetPassword(string username)
        {
            var col = _db.GetCollection<User>(Core.Db.User.DocumentName);
            User user = col.FindOne(u => u.Username == username);
            return user.Password;
        }

        public override IEnumerable<Core.Db.User> GetUsers()
        {
            var col = _db.GetCollection<User>(Core.Db.User.DocumentName);
            return col.FindAll();
        }

        public override IEnumerable<Core.Db.User> GetUsers(string keyword, UserOrderBy uo)
        {
            var col = _db.GetCollection<User>(Core.Db.User.DocumentName);
            var keywordToLower = keyword.ToLower();
            BsonExpression query = null;

            if (!string.IsNullOrEmpty(keyword))
            {
                query = Query.Contains("Username", keywordToLower);
            }

            switch (uo)
            {
                case UserOrderBy.UsernameAscending:
                    if (query != null)
                    {
                        //return col.Find(Query.And(Query.All("Username"), query));

                        var q = Query.All("Username");
                        q.Where.Add(query);
                        return col.Find(q);
                    }
                    else
                    {
                        return col.Find(Query.All("Username"));
                    }

                case UserOrderBy.UsernameDescending:

                    if (query != null)
                    {
                        //return col.Find(Query.And(Query.All("Username", Query.Descending), query));

                        var q = Query.All("Username", Query.Descending);
                        q.Where.Add(query);
                        return col.Find(q);
                    }
                    else
                    {
                        return col.Find(Query.All("Username", Query.Descending));
                    }
            }

            return new User[] { };
        }

        public override IEnumerable<Core.Db.User> GetAdministrators(string keyword, UserOrderBy uo)
        {
            var col = _db.GetCollection<User>(Core.Db.User.DocumentName);
            var keywordToLower = keyword.ToLower();
            BsonExpression query = null;

            if (!string.IsNullOrEmpty(keyword))
            {
                query = Query.And(Query.EQ("UserProfile", UserProfile.Administrator.ToString()), Query.Contains("Username", keywordToLower));
            }

            switch (uo)
            {
                case UserOrderBy.UsernameAscending:
                    if (query != null)
                    {
                        //return col.Find(Query.And(Query.All("Username"), query));

                        var q = Query.All("Username");
                        q.Where.Add(query);
                        return col.Find(q);
                    }
                    else
                    {
                        //return col.Find(Query.And(Query.All("Username"), Query.EQ("UserProfile", UserProfile.Administrator.ToString())));

                        var q = Query.All("Username");
                        q.Where.Add(Query.EQ("UserProfile", UserProfile.Administrator.ToString()));
                        return col.Find(q);
                    }

                case UserOrderBy.UsernameDescending:
                    if (query != null)
                    {
                        //return col.Find(Query.And(Query.All("Username", Query.Descending), query));

                        var q = Query.All("Username", Query.Descending);
                        q.Where.Add(query);
                        return col.Find(q);
                    }
                    else
                    {
                        //return col.Find(Query.And(Query.All("Username", Query.Descending), Query.EQ("UserProfile", UserProfile.Administrator.ToString())));

                        var q = Query.All("Username", Query.Descending);
                        q.Where.Add(Query.EQ("UserProfile", UserProfile.Administrator.ToString()));
                        return col.Find(q);
                    }
            }

            return new User[] { };
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
                WorkflowId = entry.WorkflowId,
                Logs = entry.Logs
            };
            col.Insert(he);
            col.EnsureIndex(e => e.WorkflowId);
            //col.EnsureIndex(e => e.Name, "LOWER($.Name)");
            col.EnsureIndex(e => e.Name);
            col.EnsureIndex(e => e.LaunchType);
            //col.EnsureIndex(e => e.Description, "LOWER($.Name)");
            col.EnsureIndex(e => e.Description);
            col.EnsureIndex(e => e.Status);
            col.EnsureIndex(e => e.StatusDate);
        }

        public void UpdateHistoryEntry(HistoryEntry entry)
        {
            var col = _db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
            col.Update(entry);
        }

        public void DeleteHistoryEntries(int workflowId)
        {
            var col = _db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
            col.DeleteMany(e => e.WorkflowId == workflowId);
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries()
        {
            var col = _db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
            return col.FindAll();
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword)
        {
            var keywordToUpper = keyword.ToUpper();
            var col = _db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
            return col.Find(e => e.Name.ToUpper().Contains(keywordToUpper) || e.Description.ToUpper().Contains(keywordToUpper));
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword, int page, int entriesCount)
        {
            var keywordToUpper = keyword.ToUpper();
            var col = _db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
            return col.Find(e => e.Name.ToUpper().Contains(keywordToUpper) || e.Description.ToUpper().Contains(keywordToUpper), (page - 1) * entriesCount, entriesCount);
        }

        public override IEnumerable<Core.Db.HistoryEntry> GetHistoryEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy heo)
        {
            var col = _db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
            var keywordToLower = keyword.ToLower();
            int skip = (page - 1) * entriesCount;
            BsonExpression query;

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

                    //return col.Find(
                    //    Query.And(
                    //        Query.All("StatusDate")
                    //        , query
                    //    )
                    //    , skip
                    //    , entriesCount
                    //);

                    var q1 = Query.All("StatusDate");
                    q1.Where.Add(query);

                    return col.Find(
                        q1
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.StatusDateDescending:

                    //return col.Find(
                    //    Query.And(
                    //        Query.All("StatusDate", Query.Descending)
                    //        , query
                    //    )
                    //    , skip
                    //    , entriesCount
                    //);

                    var q2 = Query.All("StatusDate", Query.Descending);
                    q2.Where.Add(query);

                    return col.Find(
                        q2
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.WorkflowIdAscending:

                    //return col.Find(
                    //    Query.And(
                    //        Query.All("WorkflowId")
                    //        , query
                    //    )
                    //    , skip
                    //    , entriesCount
                    //);

                    var q3 = Query.All("WorkflowId");
                    q3.Where.Add(query);

                    return col.Find(
                       q3
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.WorkflowIdDescending:

                    //return col.Find(
                    //    Query.And(
                    //        Query.All("WorkflowId", Query.Descending)
                    //        , query
                    //    )
                    //    , skip
                    //    , entriesCount
                    //);

                    var q4 = Query.All("WorkflowId", Query.Descending);
                    q4.Where.Add(query);

                    return col.Find(
                       q4
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.NameAscending:

                    //return col.Find(
                    //    Query.And(
                    //        Query.All("Name")
                    //        , query
                    //    )
                    //    , skip
                    //    , entriesCount
                    //);

                    var q5 = Query.All("Name");
                    q5.Where.Add(query);

                    return col.Find(
                        q5
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.NameDescending:

                    //return col.Find(
                    //    Query.And(
                    //        Query.All("Name", Query.Descending)
                    //        , query
                    //    )
                    //    , skip
                    //    , entriesCount
                    //);

                    var q6 = Query.All("Name", Query.Descending);
                    q6.Where.Add(query);

                    return col.Find(
                        q6
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.LaunchTypeAscending:

                    //return col.Find(
                    //    Query.And(
                    //        Query.All("LaunchType")
                    //        , query
                    //    )
                    //    , skip
                    //    , entriesCount
                    //);

                    var q7 = Query.All("LaunchType");
                    q7.Where.Add(query);

                    return col.Find(
                        q7
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.LaunchTypeDescending:

                    //return col.Find(
                    //    Query.And(
                    //        Query.All("LaunchType", Query.Descending)
                    //        , query
                    //    )
                    //    , skip
                    //    , entriesCount
                    //);

                    var q8 = Query.All("LaunchType", Query.Descending);
                    q8.Where.Add(query);

                    return col.Find(
                        q8
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.DescriptionAscending:

                    //return col.Find(
                    //    Query.And(
                    //        Query.All("Description")
                    //        , query
                    //    )
                    //    , skip
                    //    , entriesCount
                    //);

                    var q9 = Query.All("Description");
                    q9.Where.Add(query);

                    return col.Find(
                       q9
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.DescriptionDescending:

                    //return col.Find(
                    //    Query.And(
                    //        Query.All("Description", Query.Descending)
                    //        , query
                    //    )
                    //    , skip
                    //    , entriesCount
                    //);

                    var q10 = Query.All("Description", Query.Descending);
                    q10.Where.Add(query);

                    return col.Find(
                        q10
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.StatusAscending:

                    //return col.Find(
                    //    Query.And(
                    //        Query.All("Status")
                    //        , query
                    //    )
                    //    , skip
                    //    , entriesCount
                    //);

                    var q11 = Query.All("Status");
                    q11.Where.Add(query);

                    return col.Find(
                        q11
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.StatusDescending:

                    //return col.Find(
                    //    Query.And(
                    //        Query.All("Status", Query.Descending)
                    //        , query
                    //    )
                    //    , skip
                    //    , entriesCount
                    //);

                    var q12 = Query.All("Status", Query.Descending);
                    q12.Where.Add(query);

                    return col.Find(
                        q12
                        , skip
                        , entriesCount
                    );
            }

            return new HistoryEntry[] { };
        }

        public override IEnumerable<Core.Db.Entry> GetEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy eo)
        {
            var col = _db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
            var keywordToLower = keyword.ToLower();
            int skip = (page - 1) * entriesCount;
            BsonExpression query;

            if (!string.IsNullOrEmpty(keyword))
            {
                query = Query.And(Query.Or(Query.Contains("Name", keywordToLower), Query.Contains("Description", keywordToLower))
                                , Query.And(Query.GTE("StatusDate", from), Query.LTE("StatusDate", to)));
            }
            else
            {
                query = Query.And(Query.GTE("StatusDate", from), Query.LTE("StatusDate", to));
            }

            switch (eo)
            {
                case EntryOrderBy.StatusDateAscending:

                    //return col.Find(
                    //    Query.And(
                    //        Query.All("StatusDate")
                    //        , query
                    //    )
                    //    , skip
                    //    , entriesCount
                    //);

                    var q1 = Query.All("StatusDate");
                    q1.Where.Add(query);

                    return col.Find(
                        q1
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.StatusDateDescending:

                    //return col.Find(
                    //    Query.And(
                    //        Query.All("StatusDate", Query.Descending)
                    //        , query
                    //    )
                    //    , skip
                    //    , entriesCount
                    //);

                    var q2 = Query.All("StatusDate", Query.Descending);
                    q2.Where.Add(query);

                    return col.Find(
                        q2
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.WorkflowIdAscending:

                    //return col.Find(
                    //    Query.And(
                    //        Query.All("WorkflowId")
                    //        , query
                    //    )
                    //    , skip
                    //    , entriesCount
                    //);

                    var q3 = Query.All("WorkflowId");
                    q3.Where.Add(query);

                    return col.Find(
                       q3
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.WorkflowIdDescending:

                    //return col.Find(
                    //    Query.And(
                    //        Query.All("WorkflowId", Query.Descending)
                    //        , query
                    //    )
                    //    , skip
                    //    , entriesCount
                    //);

                    var q4 = Query.All("WorkflowId", Query.Descending);
                    q4.Where.Add(query);

                    return col.Find(
                       q4
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.NameAscending:

                    //return col.Find(
                    //    Query.And(
                    //        Query.All("Name")
                    //        , query
                    //    )
                    //    , skip
                    //    , entriesCount
                    //);

                    var q5 = Query.All("Name");
                    q5.Where.Add(query);

                    return col.Find(
                        q5
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.NameDescending:

                    //return col.Find(
                    //    Query.And(
                    //        Query.All("Name", Query.Descending)
                    //        , query
                    //    )
                    //    , skip
                    //    , entriesCount
                    //);

                    var q6 = Query.All("Name", Query.Descending);
                    q6.Where.Add(query);

                    return col.Find(
                        q6
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.LaunchTypeAscending:

                    //return col.Find(
                    //    Query.And(
                    //        Query.All("LaunchType")
                    //        , query
                    //    )
                    //    , skip
                    //    , entriesCount
                    //);

                    var q7 = Query.All("LaunchType");
                    q7.Where.Add(query);

                    return col.Find(
                        q7
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.LaunchTypeDescending:

                    //return col.Find(
                    //    Query.And(
                    //        Query.All("LaunchType", Query.Descending)
                    //        , query
                    //    )
                    //    , skip
                    //    , entriesCount
                    //);

                    var q8 = Query.All("LaunchType", Query.Descending);
                    q8.Where.Add(query);

                    return col.Find(
                        q8
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.DescriptionAscending:

                    //return col.Find(
                    //    Query.And(
                    //        Query.All("Description")
                    //        , query
                    //    )
                    //    , skip
                    //    , entriesCount
                    //);

                    var q9 = Query.All("Description");
                    q9.Where.Add(query);

                    return col.Find(
                       q9
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.DescriptionDescending:

                    //return col.Find(
                    //    Query.And(
                    //        Query.All("Description", Query.Descending)
                    //        , query
                    //    )
                    //    , skip
                    //    , entriesCount
                    //);

                    var q10 = Query.All("Description", Query.Descending);
                    q10.Where.Add(query);

                    return col.Find(
                        q10
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.StatusAscending:

                    //return col.Find(
                    //    Query.And(
                    //        Query.All("Status")
                    //        , query
                    //    )
                    //    , skip
                    //    , entriesCount
                    //);

                    var q11 = Query.All("Status");
                    q11.Where.Add(query);

                    return col.Find(
                        q11
                        , skip
                        , entriesCount
                    );

                case EntryOrderBy.StatusDescending:

                    //return col.Find(
                    //    Query.And(
                    //        Query.All("Status", Query.Descending)
                    //        , query
                    //    )
                    //    , skip
                    //    , entriesCount
                    //);

                    var q12 = Query.All("Status", Query.Descending);
                    q12.Where.Add(query);

                    return col.Find(
                        q12
                        , skip
                        , entriesCount
                    );
            }

            return new Entry[] { };
        }

        public override long GetHistoryEntriesCount(string keyword)
        {
            var keywordToUpper = keyword.ToUpper();
            var col = _db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
            return col.Find(e => e.Name.ToUpper().Contains(keywordToUpper) || e.Description.ToUpper().Contains(keywordToUpper)).LongCount();
        }

        public override long GetHistoryEntriesCount(string keyword, DateTime from, DateTime to)
        {
            var keywordToLower = keyword.ToLower();
            var col = _db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
            BsonExpression query;

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

        public override long GetEntriesCount(string keyword, DateTime from, DateTime to)
        {
            var keywordToLower = keyword.ToLower();
            var col = _db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
            BsonExpression query;

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

        public override DateTime GetHistoryEntryStatusDateMin()
        {
            var col = _db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
            var q = col.Find(Query.All("StatusDate"));
            if (q.Any())
            {
                return q.Select(e => e.StatusDate).First();
            }

            return DateTime.Now;
        }

        public override DateTime GetHistoryEntryStatusDateMax()
        {
            var col = _db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
            var q = col.Find(Query.All("StatusDate", Query.Descending));
            if (q.Any())
            {
                return q.Select(e => e.StatusDate).First();
            }

            return DateTime.Now;
        }

        public override DateTime GetEntryStatusDateMin()
        {
            var col = _db.GetCollection<HistoryEntry>(Core.Db.Entry.DocumentName);
            var q = col.Find(Query.All("StatusDate"));
            if (q.Any())
            {
                return q.Select(e => e.StatusDate).First();
            }

            return DateTime.Now;
        }

        public override DateTime GetEntryStatusDateMax()
        {
            var col = _db.GetCollection<HistoryEntry>(Core.Db.Entry.DocumentName);
            var q = col.Find(Query.All("StatusDate", Query.Descending));
            if (q.Any())
            {
                return q.Select(e => e.StatusDate).First();
            }

            return DateTime.Now;
        }

        public override string InsertWorkflow(Core.Db.Workflow workflow)
        {
            var col = _db.GetCollection<Workflow>(Core.Db.Workflow.DocumentName);
            var wf = new Workflow { Xml = workflow.Xml };
            var res = col.Insert(wf);
            return res.AsInt32.ToString();
        }

        public override void UpdateWorkflow(string dbId, Core.Db.Workflow workflow)
        {
            var col = _db.GetCollection<Workflow>(Core.Db.Workflow.DocumentName);
            var wf = new Workflow { Id = int.Parse(dbId), Xml = workflow.Xml };
            col.Update(wf);
        }

        public override void DeleteWorkflow(string id)
        {
            var col = _db.GetCollection<Workflow>(Core.Db.Workflow.DocumentName);
            var i = int.Parse(id);
            col.DeleteMany(e => e.Id == i);
        }

        public override void DeleteWorkflows(string[] ids)
        {
            var col = _db.GetCollection<Workflow>(Core.Db.Workflow.DocumentName);
            col.DeleteMany(e => ids.Contains(e.Id.ToString()));
        }

        public override IEnumerable<Core.Db.Workflow> GetWorkflows()
        {
            var col = _db.GetCollection<Workflow>(Core.Db.Workflow.DocumentName);
            return col.FindAll();
        }

        public override Core.Db.Workflow GetWorkflow(string id)
        {
            var col = _db.GetCollection<Workflow>(Core.Db.Workflow.DocumentName);
            return col.FindById(int.Parse(id));
        }

        public override void InsertUserWorkflowRelation(Core.Db.UserWorkflow userWorkflow)
        {
            var col = _db.GetCollection<UserWorkflow>(Core.Db.UserWorkflow.DocumentName);
            var uw = new UserWorkflow
            {
                UserId = userWorkflow.UserId,
                WorkflowId = userWorkflow.WorkflowId
            };
            col.Insert(uw);
        }

        public override void DeleteUserWorkflowRelationsByUserId(string userId)
        {
            var col = _db.GetCollection<UserWorkflow>(Core.Db.UserWorkflow.DocumentName);
            col.DeleteMany(uw => uw.UserId == userId);
        }

        public override void DeleteUserWorkflowRelationsByWorkflowId(string workflowId)
        {
            var col = _db.GetCollection<UserWorkflow>(Core.Db.UserWorkflow.DocumentName);
            col.DeleteMany(uw => uw.WorkflowId == workflowId);
        }

        public override IEnumerable<string> GetUserWorkflows(string userId)
        {
            var col = _db.GetCollection<UserWorkflow>(Core.Db.UserWorkflow.DocumentName);
            return col.Find(uw => uw.UserId == userId).Select(uw => uw.WorkflowId.ToString());
        }

        public override bool CheckUserWorkflow(string userId, string workflowId)
        {
            var col = _db.GetCollection<UserWorkflow>(Core.Db.UserWorkflow.DocumentName);
            var res = col.FindOne(uw => uw.UserId == userId && uw.WorkflowId == workflowId);
            return res != null;
        }

        public override string GetEntryLogs(string entryId)
        {
            var id = int.Parse(entryId);
            var col = _db.GetCollection<Entry>(Core.Db.Entry.DocumentName);
            var entry = col.FindOne(e => e.Id == id);
            return entry.Logs;
        }

        public override string GetHistoryEntryLogs(string entryId)
        {
            var id = int.Parse(entryId);
            var col = _db.GetCollection<HistoryEntry>(Core.Db.HistoryEntry.DocumentName);
            var entry = col.FindOne(e => e.Id == id);
            return entry.Logs;
        }

        public override void Dispose()
        {
            _db.Dispose();
        }
    }
}
