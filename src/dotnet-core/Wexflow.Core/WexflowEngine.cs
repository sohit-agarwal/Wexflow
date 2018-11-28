using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Wexflow.Core.Db;

namespace Wexflow.Core
{
    /// <summary>
    /// Wexflow engine.
    /// </summary>
    public class WexflowEngine
    {
        /// <summary>
        /// Settings file path.
        /// </summary>
        public string SettingsFile { get; private set; }
        /// <summary>
        /// Workflows folder path.
        /// </summary>
        public string WorkflowsFolder { get; private set; }
        /// <summary>
        /// Trash folder path.
        /// </summary>
        public string TrashFolder { get; private set; }
        /// <summary>
        /// Temp folder path.
        /// </summary>
        public string TempFolder { get; private set; }
        /// <summary>
        /// Tasks folder path.
        /// </summary>
        public string TasksFolder { get; private set; }
        /// <summary>
        /// Workflows temp folder used for global variables parsing.
        /// </summary>
        public string WorkflowsTempFolder { get; private set; }
        /// <summary>
        /// XSD path.
        /// </summary>
        public string XsdPath { get; private set; }
        /// <summary>
        /// Tasks names file path.
        /// </summary>
        public string TasksNamesFile { get; private set; }
        /// <summary>
        /// Tasks settings file path.
        /// </summary>
        public string TasksSettingsFile { get; private set; }
        /// <summary>
        /// List of the Workflows loaded by Wexflow engine.
        /// </summary>
        public IList<Workflow> Workflows { get; private set; }
        /// <summary>
        /// Database connection string.
        /// </summary>
        public string ConnectionString { get; private set; }
        /// <summary>
        /// Global variables file.
        /// </summary>
        public string GlobalVariablesFile { get; private set; }
        /// <summary>
        /// Global variables.
        /// </summary>
        public Variable[] GlobalVariables { get; private set; }

        /// <summary>
        /// Database
        /// </summary>
        public Db.Db Database { get; private set; }

        //
        // Quartz scheduler
        //
        private static readonly NameValueCollection QuartzProperties = new NameValueCollection
        {
            // JSON serialization is the one supported under .NET Core (binary isn't)
            ["quartz.serializer.type"] = "json"
        };

        private static readonly ISchedulerFactory SchedulerFactory = new StdSchedulerFactory(QuartzProperties);
        private static readonly IScheduler QuartzScheduler = SchedulerFactory.GetScheduler().Result;

        /// <summary>
        /// Creates a new instance of Wexflow engine.
        /// </summary>
        /// <param name="settingsFile">Settings file path.</param>
        public WexflowEngine(string settingsFile)
        {
            SettingsFile = settingsFile;
            Workflows = new List<Workflow>();

            Logger.Info("");
            Logger.Info("Starting Wexflow Engine");

            LoadSettings();

            Database = new Db.Db(ConnectionString);
            Database.Init();

            LoadGlobalVariables();

            LoadWorkflows();

            //Task<IScheduler> ischeduler = SchedulerFactory.GetScheduler();
            //ischeduler.Wait();
            //QuartzScheduler = ischeduler.Result;
        }

        /// <summary>
        /// Checks whether a cron expression is valid or not.
        /// </summary>
        /// <param name="expression">Cron expression</param>
        /// <returns></returns>
        public static bool IsCronExpressionValid(string expression)
        {
            bool res = CronExpression.IsValidExpression(expression);
            return res;
        }

        private void LoadSettings()
        {
            var xdoc = XDocument.Load(SettingsFile);
            WorkflowsFolder = GetWexflowSetting(xdoc, "workflowsFolder");
            TrashFolder = GetWexflowSetting(xdoc, "trashFolder");
            TempFolder = GetWexflowSetting(xdoc, "tempFolder");
            TasksFolder = GetWexflowSetting(xdoc, "tasksFolder");
            if (!Directory.Exists(TempFolder)) Directory.CreateDirectory(TempFolder);
            WorkflowsTempFolder = Path.Combine(TempFolder, "Workflows");
            if (!Directory.Exists(WorkflowsTempFolder)) Directory.CreateDirectory(WorkflowsTempFolder);
            XsdPath = GetWexflowSetting(xdoc, "xsd");
            TasksNamesFile = GetWexflowSetting(xdoc, "tasksNamesFile");
            TasksSettingsFile = GetWexflowSetting(xdoc, "tasksSettingsFile");
            ConnectionString = GetWexflowSetting(xdoc, "connectionString");
            GlobalVariablesFile = GetWexflowSetting(xdoc, "globalVariablesFile");
        }

        private void LoadGlobalVariables()
        {
            List<Variable> variables = new List<Variable>();
            XDocument xdoc = XDocument.Load(GlobalVariablesFile);

            foreach (var xvariable in xdoc.Descendants("Variable"))
            {
                Variable variable = new Variable
                {
                    Key = xvariable.Attribute("name").Value,
                    Value = xvariable.Attribute("value").Value
                };
                variables.Add(variable);
            }

            GlobalVariables = variables.ToArray();
        }

        private string GetWexflowSetting(XDocument xdoc, string name)
        {
            try
            {
                var xValue = xdoc.XPathSelectElement(string.Format("/Wexflow/Setting[@name='{0}']", name)).Attribute("value");
                if (xValue == null) throw new Exception("Wexflow Setting Value attribute not found.");
                return xValue.Value;
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("An error occured when reading Wexflow settings: Setting[@name='{0}']", e, name);
                return string.Empty;
            }
        }

        private void LoadWorkflows()
        {
            foreach (string file in Directory.GetFiles(WorkflowsFolder))
            {
                var workflow = LoadWorkflowFromFile(file);
                if (workflow != null)
                {
                    Workflows.Add(workflow);
                }
            }
        }

        public void StopCronJobs(int workflowId)
        {
            string jobIdentity = "Workflow Job " + workflowId;
            var jobKey = new JobKey(jobIdentity);
            if (QuartzScheduler.CheckExists(jobKey).Result)
            {
                QuartzScheduler.DeleteJob(jobKey);
            }
        }

        public Workflow LoadWorkflowFromFile(string file)
        {
            try
            {
                var wf = new Workflow(file, TempFolder, WorkflowsTempFolder, TasksFolder, XsdPath, Database, GlobalVariables);
                Logger.InfoFormat("Workflow loaded: {0} ({1})", wf, file);
                return wf;
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("An error occured while loading the workflow : {0} Please check the workflow configuration. Error: {1}", file, e.Message);
                return null;
            }
        }

        /// <summary>
        /// Starts Wexflow engine.
        /// </summary>
        public void Run()
        {
            foreach (Workflow workflow in Workflows)
            {
                ScheduleWorkflow(workflow);
            }

            if (!QuartzScheduler.IsStarted)
            {
                QuartzScheduler.Start().Wait();
            }
        }

        public void ScheduleWorkflow(Workflow wf)
        {
            if (wf.IsEnabled)
            {
                if (wf.LaunchType == LaunchType.Startup)
                {
                    wf.Start();
                }
                else if (wf.LaunchType == LaunchType.Periodic)
                {
                    IDictionary<string, object> map = new Dictionary<string, object>();
                    map.Add("workflow", wf);

                    string jobIdentity = "Workflow Job " + wf.Id;
                    IJobDetail jobDetail = JobBuilder.Create<WorkflowJob>()
                        .WithIdentity(jobIdentity)
                        .SetJobData(new JobDataMap(map))
                        .Build();

                    ITrigger trigger = TriggerBuilder.Create()
                        .ForJob(jobDetail)
                        .WithSimpleSchedule( x => x.WithInterval(wf.Period).RepeatForever())
                        .WithIdentity("Workflow Trigger " + wf.Id)
                        .StartNow()
                        .Build();

                    var jobKey = new JobKey(jobIdentity);
                    if (QuartzScheduler.CheckExists(jobKey).Result)
                    {
                        QuartzScheduler.DeleteJob(jobKey);
                    }

                    QuartzScheduler.ScheduleJob(jobDetail, trigger).Wait();

                }
                else if (wf.LaunchType == LaunchType.Cron)
                {
                    IDictionary<string, object> map = new Dictionary<string, object>();
                    map.Add("workflow", wf);

                    string jobIdentity = "Workflow Job " + wf.Id;
                    IJobDetail jobDetail = JobBuilder.Create<WorkflowJob>()
                        .WithIdentity(jobIdentity)
                        .SetJobData(new JobDataMap(map))
                        .Build();

                    ITrigger trigger = TriggerBuilder.Create()
                        .ForJob(jobDetail)
                        .WithCronSchedule(wf.CronExpression)
                        .WithIdentity("Workflow Trigger " + wf.Id)
                        .StartNow()
                        .Build();

                    var jobKey = new JobKey(jobIdentity);
                    if (QuartzScheduler.CheckExists(jobKey).Result)
                    {
                        QuartzScheduler.DeleteJob(jobKey);
                    }

                    QuartzScheduler.ScheduleJob(jobDetail, trigger).Wait();
                }
            }
        }

        /// <summary>
        /// Stops Wexflow engine.
        /// </summary>
        /// <param name="stopQuartzScheduler">Tells if Quartz scheduler should be stopped or not.</param>
        /// <param name="clearStatusCountAndEntries">Indicates whether to clear statusCount and entries.</param>
        public void Stop(bool stopQuartzScheduler, bool clearStatusCountAndEntries)
        {
            if (stopQuartzScheduler)
            {
                QuartzScheduler.Shutdown().Wait();
            }

            foreach (var wf in Workflows)
            {
                if (wf.IsRunning)
                {
                    wf.Stop();
                }
            }

            if (clearStatusCountAndEntries)
            {
                Database.ClearStatusCount();
                Database.ClearEntries();
            }
        }

        /// <summary>
        /// Gets a workflow.
        /// </summary>
        /// <param name="workflowId">Workflow Id.</param>
        /// <returns></returns>
        public Workflow GetWorkflow(int workflowId)
        {
            return Workflows.FirstOrDefault(wf => wf.Id == workflowId);
        }

        /// <summary>
        /// Starts a workflow.
        /// </summary>
        /// <param name="workflowId">Workflow Id.</param>
        public void StartWorkflow(int workflowId)
        {
            var wf = GetWorkflow(workflowId);

            if (wf == null)
            {
                Logger.ErrorFormat("Workflow {0} not found.", workflowId);
            }
            else
            {
                if (wf.IsEnabled) wf.Start();
            }
        }

        /// <summary>
        /// Stops a workflow.
        /// </summary>
        /// <param name="workflowId">Workflow Id.</param>
        public bool StopWorkflow(int workflowId)
        {
            var wf = GetWorkflow(workflowId);

            if (wf == null)
            {
                Logger.ErrorFormat("Workflow {0} not found.", workflowId);
            }
            else
            {
                if (wf.IsEnabled) return wf.Stop();
            }

            return false;
        }

        /// <summary>
        /// Suspends a workflow.
        /// </summary>
        /// <param name="workflowId">Workflow Id.</param>
        public bool SuspendWorkflow(int workflowId)
        {
            var wf = GetWorkflow(workflowId);

            if (wf == null)
            {
                Logger.ErrorFormat("Workflow {0} not found.", workflowId);
            }
            else
            {
                if (wf.IsEnabled) return wf.Suspend();
            }

            return false;
        }

        /// <summary>
        /// Resumes a workflow.
        /// </summary>
        /// <param name="workflowId">Workflow Id.</param>
        public void ResumeWorkflow(int workflowId)
        {
            var wf = GetWorkflow(workflowId);

            if (wf == null)
            {
                Logger.ErrorFormat("Workflow {0} not found.", workflowId);
            }
            else
            {
                if (wf.IsEnabled) wf.Resume();
            }
        }

        /// <summary>
        /// Returns status count
        /// </summary>
        /// <returns>Returns status count</returns>
        public StatusCount GetStatusCount()
        {
            return Database.GetStatusCount();
        }

        /// <summary>
        /// Returns all the entries
        /// </summary>
        /// <returns>Returns all the entries</returns>
        public Entry[] GetEntries()
        {
            return Database.GetEntries().ToArray();
        }

        /// <summary>
        /// Inserts a user.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        /// <param name="userProfile">User profile.</param>
        /// <param name="email">Email.</param>
        public void InsertUser(string username, string password, UserProfile userProfile, string email)
        {
            Database.InsertUser(new User
            {
                Username = username, Password = password, UserProfile = userProfile, Email = email
            });
        }

        /// <summary>
        /// Updates a user.
        /// </summary>
        /// <param name="userId">User's id.</param>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        /// <param name="userProfile">User's profile.</param>
        /// <param name="email">User's email.</param>
        public void UpdateUser(int userId, string username, string password, UserProfile userProfile, string email)
        {
            Database.UpdateUser(new User
            {
                Id =  userId,
                Username = username,
                Password = password,
                UserProfile = userProfile,
                Email = email
            });
        }

        /// <summary>
        /// Updates username and email.
        /// </summary>
        /// <param name="userId">User Id.</param>
        /// <param name="username">New username.</param>
        /// <param name="email">New email.</param>
        /// <param name="up">User profile.</param>
        public void UpdateUsernameAndEmailAndUserProfile(int userId, string username, string email, int up)
        {
            Database.UpdateUsernameAndEmailAndUserProfile(userId, username, email,(UserProfile)up);
        }

        /// <summary>
        /// Deletes a user.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        public void DeleteUser(string username, string password)
        {
            Database.DeleteUser(username, password);
        }

        /// <summary>
        /// Gets a user.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <returns></returns>
        public User GetUser(string username)
        {
            return Database.GetUser(username);
        }

        /// <summary>
        /// Gets a password.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <returns></returns>
        public string GetPassword(string username)
        {
            return Database.GetPassword(username);
        }

        /// <summary>
        /// Returns all the users.
        /// </summary>
        /// <returns>All the users.</returns>
        public User[] GetUsers()
        {
            var q = Database.GetUsers();
            if (q.Any())
            {
                return q.ToArray();
            }

            return new User[]{};
        }

        /// <summary>
        /// Search for users.
        /// </summary>
        /// <returns>All the users.</returns>
        public User[] GetUsers(string keyword, UserOrderBy uo)
        {
            var q = Database.GetUsers(keyword, uo);
            if (q.Any())
            {
                return q.ToArray();
            }

            return new User[] { };
        }

        /// <summary>
        /// Updates user password.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        public void UpdatePassword(string username, string password)
        {
            Database.UpdatePassword(username, password);
        }

        /// <summary>
        /// Returns all the entries.
        /// </summary>
        /// <returns>Returns all the entries</returns>
        public HistoryEntry[] GetHistoryEntries()
        {
            return Database.GetHistoryEntries().ToArray();
        }

        /// <summary>
        /// Returns the entries by a keyword.
        /// </summary>
        /// <param name="keyword">Search keyword.</param>
        /// <returns>Returns all the entries</returns>
        public HistoryEntry[] GetHistoryEntries(string keyword)
        {
            return Database.GetHistoryEntries(keyword).ToArray();
        }

        /// <summary>
        /// Returns the entries by a keyword.
        /// </summary>
        /// <param name="keyword">Search keyword.</param>
        /// <param name="page">Page number.</param>
        /// <param name="entriesCount">Number of entries.</param>
        /// <returns>Returns all the entries</returns>
        public HistoryEntry[] GetHistoryEntries(string keyword, int page, int entriesCount)
        {
            return Database.GetHistoryEntries(keyword, page, entriesCount).ToArray();
        }

        /// <summary>
        /// Returns the entries by a keyword.
        /// </summary>
        /// <param name="keyword">Search keyword.</param>
        /// <param name="from">Date From.</param>
        /// <param name="to">Date To.</param>
        /// <param name="page">Page number.</param>
        /// <param name="entriesCount">Number of entries.</param>
        /// <param name="heo">EntryOrderBy</param>
        /// <returns>Returns all the entries</returns>
        public HistoryEntry[] GetHistoryEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy heo)
        {
            var col = Database.GetHistoryEntries(keyword, from, to, page, entriesCount, heo);

            if (!col.Any())
            {
                return new HistoryEntry[] { };
            }
            else
            {
                return col.ToArray();
            }
        }

        /// <summary>
        /// Returns the entries by a keyword.
        /// </summary>
        /// <param name="keyword">Search keyword.</param>
        /// <param name="from">Date From.</param>
        /// <param name="to">Date To.</param>
        /// <param name="page">Page number.</param>
        /// <param name="entriesCount">Number of entries.</param>
        /// <param name="heo">EntryOrderBy</param>
        /// <returns>Returns all the entries</returns>
        public Entry[] GetEntries(string keyword, DateTime from, DateTime to, int page, int entriesCount, EntryOrderBy heo)
        {
            var col = Database.GetEntries(keyword, from, to, page, entriesCount, heo);

            if (!col.Any())
            {
                return new Entry[] { };
            }
            else
            {
                return col.ToArray();
            }
        }

        /// <summary>
        /// Gets the number of history entries by search keyword.
        /// </summary>
        /// <param name="keyword">Search keyword.</param>
        /// <returns>The number of history entries by search keyword.</returns>
        public long GetHistoryEntriesCount(string keyword)
        {
            return Database.GetHistoryEntriesCount(keyword);
        }

        /// <summary>
        /// Gets the number of history entries by search keyword and date filter.
        /// </summary>
        /// <param name="keyword">Search keyword.</param>
        /// <param name="from">Date from.</param>
        /// <param name="to">Date to.</param>
        /// <returns></returns>
        public long GetHistoryEntriesCount(string keyword, DateTime from, DateTime to)
        {
            return Database.GetHistoryEntriesCount(keyword, from, to);
        }

        /// <summary>
        /// Gets the number of entries by search keyword and date filter.
        /// </summary>
        /// <param name="keyword">Search keyword.</param>
        /// <param name="from">Date from.</param>
        /// <param name="to">Date to.</param>
        /// <returns></returns>
        public long GetEntriesCount(string keyword, DateTime from, DateTime to)
        {
            return Database.GetEntriesCount(keyword, from, to);
        }

        /// <summary>
        /// Returns Status Date Min value.
        /// </summary>
        /// <returns>Status Date Min value.</returns>
        public DateTime GetHistoryEntryStatusDateMin()
        {
            return Database.GetHistoryEntryStatusDateMin();
        }

        /// <summary>
        /// Returns Status Date Max value.
        /// </summary>
        /// <returns>Status Date Max value.</returns>
        public DateTime GetHistoryEntryStatusDateMax()
        {
            return Database.GetHistoryEntryStatusDateMax();
        }

        /// <summary>
        /// Returns Status Date Min value.
        /// </summary>
        /// <returns>Status Date Min value.</returns>
        public DateTime GetEntryStatusDateMin()
        {
            return Database.GetEntryStatusDateMin();
        }

        /// <summary>
        /// Returns Status Date Max value.
        /// </summary>
        /// <returns>Status Date Max value.</returns>
        public DateTime GetEntryStatusDateMax()
        {
            return Database.GetEntryStatusDateMax();
        }
    }
}
