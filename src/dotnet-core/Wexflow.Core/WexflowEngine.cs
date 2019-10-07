using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Wexflow.Core.Db;

namespace Wexflow.Core
{
    /// <summary>
    /// Database type
    /// </summary>
    public enum DbType
    {
        /// <summary>
        /// CosmosDB
        /// </summary>
        CosmosDB,
        /// <summary>
        /// LiteDB
        /// </summary>
        LiteDB,
        /// <summary>
        /// MongoDB
        /// </summary>
        MongoDB,
        /// <summary>
        /// RavenDB
        /// </summary>
        RavenDB
    }

    /// <summary>
    /// Wexflow engine.
    /// </summary>
    public class WexflowEngine
    {
        /// <summary>
        /// Maximum retries of loading a workflow.
        /// </summary>
        public static int MaxRetries;
        /// <summary>
        /// Retry timeout.
        /// </summary>
        public static int RetryTimeout;

        /// <summary>
        /// Settings file path.
        /// </summary>
        public string SettingsFile { get; private set; }
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
        /// Approval folder path.
        /// </summary>
        public string ApprovalFolder { get; private set; }
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
        /// Database type.
        /// </summary>
        public DbType DbType { get; private set; }
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

            switch (DbType)
            {
                case DbType.CosmosDB:
                    Database = new CosmosDB.Db(ConnectionString);
                    break;
                case DbType.MongoDB:
                    Database = new MongoDB.Db(ConnectionString);
                    break;
                case DbType.LiteDB:
                    Database = new LiteDB.Db(ConnectionString);
                    break;
                case DbType.RavenDB:
                    Database = new RavenDB.Db(ConnectionString);
                    break;
            }

            if (Database != null)
            {
                Database.Init();
            }

            LoadGlobalVariables();

            LoadWorkflows();
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
            TempFolder = GetWexflowSetting(xdoc, "tempFolder");
            TasksFolder = GetWexflowSetting(xdoc, "tasksFolder");
            if (!Directory.Exists(TempFolder)) Directory.CreateDirectory(TempFolder);
            WorkflowsTempFolder = Path.Combine(TempFolder, "Workflows");
            ApprovalFolder = GetWexflowSetting(xdoc, "approvalFolder");
            if (!Directory.Exists(WorkflowsTempFolder)) Directory.CreateDirectory(WorkflowsTempFolder);
            XsdPath = GetWexflowSetting(xdoc, "xsd");
            TasksNamesFile = GetWexflowSetting(xdoc, "tasksNamesFile");
            TasksSettingsFile = GetWexflowSetting(xdoc, "tasksSettingsFile");
            DbType = (DbType)Enum.Parse(typeof(DbType), GetWexflowSetting(xdoc, "dbType"), true);
            ConnectionString = GetWexflowSetting(xdoc, "connectionString");
            GlobalVariablesFile = GetWexflowSetting(xdoc, "globalVariablesFile");
            MaxRetries = int.Parse(GetWexflowSetting(xdoc, "maxRetries"));
            RetryTimeout = int.Parse(GetWexflowSetting(xdoc, "retryTimeout"));
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
            // C:\Wexflow-dotnet-core\Workflows\prod\windows
            // C:\Wexflow-dotnet-core\Workflows\prod\linux
            // C:\Wexflow-dotnet-core\Workflows\prod\macos
            //foreach (string file in Directory.GetFiles(@"C:\Wexflow-dotnet-core\Workflows\prod\windows"))
            //{
            //    Database.InsertWorkflow(new Db.Workflow { Xml = File.ReadAllText(file) });
            //}

            var workflows = Database.GetWorkflows();

            foreach (var workflow in workflows)
            {
                var wf = LoadWorkflowFromDatabase(workflow);
                Workflows.Add(wf);
            }

        }

        private void StopCronJobs(int workflowId)
        {
            string jobIdentity = "Workflow Job " + workflowId;
            var jobKey = new JobKey(jobIdentity);
            if (QuartzScheduler.CheckExists(jobKey).Result)
            {
                QuartzScheduler.DeleteJob(jobKey);
            }
        }

        private Workflow LoadWorkflowFromDatabase(Db.Workflow workflow)
        {
            try
            {
                var wf = new Workflow(
                      workflow.GetDbId()
                    , workflow.Xml
                    , TempFolder
                    , WorkflowsTempFolder
                    , TasksFolder
                    , ApprovalFolder
                    , XsdPath
                    , Database
                    , GlobalVariables);
                Logger.InfoFormat("Workflow loaded: {0}", wf);
                return wf;
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("An error occured while loading the workflow : {0} Please check the workflow configuration. Error: {1}", workflow.GetDbId(), e.Message);
                return null;
            }
        }

        /// <summary>
        /// Saves a workflow in the database.
        /// </summary>
        /// <param name="xml">XML of the workflow.</param>
        /// <param name="userId">User id.</param>
        /// <param name="userProfile">User profile.</param>
        /// <returns>Workflow db id.</returns>
        public string SaveWorkflow(string userId, UserProfile userProfile, string xml)
        {
            try
            {
                using (var xmlReader = XmlReader.Create(new StringReader(xml)))
                {
                    XmlNamespaceManager xmlNamespaceManager = null;
                    var xmlNameTable = xmlReader.NameTable;
                    if (xmlNameTable != null)
                    {
                        xmlNamespaceManager = new XmlNamespaceManager(xmlNameTable);
                        xmlNamespaceManager.AddNamespace("wf", "urn:wexflow-schema");
                    }

                    var xdoc = XDocument.Parse(xml);
                    var id = int.Parse(xdoc.XPathSelectElement("/wf:Workflow", xmlNamespaceManager).Attribute("id").Value);
                    var workflow = Workflows.FirstOrDefault(w => w.Id == id);

                    if (workflow == null) // insert
                    {
                        // check the workflow before to save it
                        try
                        {
                            new Workflow("-1"
                            , xml
                            , TempFolder
                            , WorkflowsTempFolder
                            , TasksFolder
                            , ApprovalFolder
                            , XsdPath
                            , Database
                            , GlobalVariables
                            );
                        }
                        catch (Exception e)
                        {
                            Logger.ErrorFormat("An error occured while saving the workflow {0}:", e, xml);
                            return "-1";
                        }
                        string dbId = Database.InsertWorkflow(new Db.Workflow { Xml = xml });

                        if (userProfile == UserProfile.Administrator)
                        {
                            InsertUserWorkflowRelation(userId, dbId);
                        }

                        var wfFromDb = Database.GetWorkflow(dbId);
                        var newWorkflow = LoadWorkflowFromDatabase(wfFromDb);

                        Logger.InfoFormat("New workflow {0} has been created. The workflow will be loaded.", newWorkflow.Name);
                        Workflows.Add(newWorkflow);
                        ScheduleWorkflow(newWorkflow);
                        return dbId;
                    }
                    else // update
                    {
                        // check the workflow before to save it
                        try
                        {
                            new Workflow("-1"
                            , xml
                            , TempFolder
                            , WorkflowsTempFolder
                            , TasksFolder
                            , ApprovalFolder
                            , XsdPath
                            , Database
                            , GlobalVariables
                            );
                        }
                        catch (Exception e)
                        {
                            Logger.ErrorFormat("An error occured while saving the workflow {0}:", e, xml);
                            return "-1";
                        }

                        var workflowFromDb = Database.GetWorkflow(workflow.DbId);
                        workflowFromDb.Xml = xml;
                        Database.UpdateWorkflow(workflow.DbId, workflowFromDb);

                        var changedWorkflow = Workflows.SingleOrDefault(wf => wf.DbId == workflowFromDb.GetDbId());

                        if (changedWorkflow != null)
                        {
                            changedWorkflow.Stop();

                            StopCronJobs(changedWorkflow.Id);
                            Workflows.Remove(changedWorkflow);
                            Logger.InfoFormat("A change in the workflow {0} has been detected. The workflow will be reloaded.", changedWorkflow.Name);

                            var updatedWorkflow = LoadWorkflowFromDatabase(workflowFromDb);
                            Workflows.Add(updatedWorkflow);
                            ScheduleWorkflow(updatedWorkflow);

                            return changedWorkflow.DbId;
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error while saving a workflow: {0}", e.Message);
            }

            return "-1";
        }

        /// <summary>
        /// Deletes a workflow from the database.
        /// </summary>
        /// <param name="dbId">DB ID.</param>
        public void DeleteWorkflow(string dbId)
        {
            try
            {
                Database.DeleteWorkflow(dbId);
                Database.DeleteUserWorkflowRelationsByWorkflowId(dbId);

                var removedWorkflow = Workflows.SingleOrDefault(wf => wf.DbId == dbId);
                if (removedWorkflow != null)
                {
                    Logger.InfoFormat("Workflow {0} is stopped and removed.", removedWorkflow.Name);
                    removedWorkflow.Stop();

                    StopCronJobs(removedWorkflow.Id);
                    Workflows.Remove(removedWorkflow);
                }

            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error while deleting a workflow: {0}", e.Message);
            }
        }

        /// <summary>
        /// Deletes workflows from the database.
        /// </summary>
        /// <param name="dbIds">DB IDs</param>
        public bool DeleteWorkflows(string[] dbIds)
        {
            try
            {
                Database.DeleteWorkflows(dbIds);

                foreach (var dbId in dbIds)
                {
                    var removedWorkflow = Workflows.SingleOrDefault(wf => wf.DbId == dbId);
                    if (removedWorkflow != null)
                    {
                        Logger.InfoFormat("Workflow {0} is stopped and removed.", removedWorkflow.Name);
                        removedWorkflow.Stop();

                        StopCronJobs(removedWorkflow.Id);
                        Workflows.Remove(removedWorkflow);
                        Database.DeleteUserWorkflowRelationsByWorkflowId(removedWorkflow.DbId);
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error while deleting workflows: {0}", e.Message);
                return false;
            }
        }

        /// <summary>
        /// Inserts a user workflow relation in DB.
        /// </summary>
        /// <param name="userId">User DB ID.</param>
        /// <param name="workflowId">Workflow DB ID.</param>
        public void InsertUserWorkflowRelation(string userId, string workflowId)
        {
            try
            {
                Database.InsertUserWorkflowRelation(new UserWorkflow { UserId = userId, WorkflowId = workflowId });
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error while inserting user workflow relation: {0}", e.Message);
            }
        }

        /// <summary>
        /// Deletes user workflow relations.
        /// </summary>
        /// <param name="userId">User DB ID.</param>
        public void DeleteUserWorkflowRelations(string userId)
        {
            try
            {
                Database.DeleteUserWorkflowRelationsByUserId(userId);
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error while deleting user workflow relations of user {0}: {1}", userId, e.Message);
            }
        }

        /// <summary>
        /// Returns user workflows.
        /// </summary>
        /// <param name="userId">User DB ID.</param>
        /// <returns>User worklofws.</returns>
        public Workflow[] GetUserWorkflows(string userId)
        {
            try
            {
                var userWorkflows = Database.GetUserWorkflows(userId).ToArray();
                var workflows = Workflows.Where(w => userWorkflows.Contains(w.DbId)).ToArray();
                return workflows;
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error while retrieving user workflows of user {0}: {1}", userId, e.Message);
                return new Workflow[] { };
            }
        }

        /// <summary>
        /// Checks whether a user have access to a workflow.
        /// </summary>
        /// <param name="userId">User id.</param>
        /// <param name="workflowId">Workflow db id.</param>
        /// <returns>true/false.</returns>
        public bool CheckUserWorkflow(string userId, string workflowId)
        {
            try
            {
                return Database.CheckUserWorkflow(userId, workflowId);
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error while checking user workflows of user {0}: {1}", userId, e.Message);
                return false;
            }
        }

        /// <summary>
        /// Returns administrators search result.
        /// </summary>
        /// <param name="keyword">Keyword.</param>
        /// <param name="uo">User Order By.</param>
        /// <returns>Administrators search result.</returns>
        public User[] GetAdministrators(string keyword, UserOrderBy uo)
        {
            try
            {
                var admins = Database.GetAdministrators(keyword, uo);
                return admins.ToArray();
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error while retrieving administrators: {0}", e.Message);
                return new User[] { };
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

        private void ScheduleWorkflow(Workflow wf)
        {
            try
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
                            .WithSimpleSchedule(x => x.WithInterval(wf.Period).RepeatForever())
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
            catch (Exception e)
            {
                Logger.ErrorFormat("An error occured while scheduling the workflow {0}: ", e, wf);
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
        /// Resumes a workflow.
        /// </summary>
        /// <param name="workflowId">Workflow Id.</param>
        public bool ApproveWorkflow(int workflowId)
        {
            try
            {
                var wf = GetWorkflow(workflowId);

                if (wf == null)
                {
                    Logger.ErrorFormat("Workflow {0} not found.", workflowId);
                    return false;
                }

                if (wf.IsApproval)
                {
                    wf.Approve();
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("An error occured while approving the workflow {0}.", e, workflowId);
                return false;
            }
        }

        /// <summary>
        /// Resumes a workflow.
        /// </summary>
        /// <param name="workflowId">Workflow Id.</param>
        public bool DisapproveWorkflow(int workflowId)
        {
            try
            {
                var wf = GetWorkflow(workflowId);

                if (wf == null)
                {
                    Logger.ErrorFormat("Workflow {0} not found.", workflowId);
                    return false;
                }

                if (wf.IsApproval)
                {
                    wf.Disapprove();
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("An error occured while approving the workflow {0}.", e, workflowId);
                return false;
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
                Username = username,
                Password = password,
                UserProfile = userProfile,
                Email = email
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
        public void UpdateUser(string userId, string username, string password, UserProfile userProfile, string email)
        {
            Database.UpdateUser(userId, new User
            {
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
        public void UpdateUsernameAndEmailAndUserProfile(string userId, string username, string email, int up)
        {
            Database.UpdateUsernameAndEmailAndUserProfile(userId, username, email, (UserProfile)up);
        }

        /// <summary>
        /// Deletes a user.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        public void DeleteUser(string username, string password)
        {
            var user = Database.GetUser(username);
            Database.DeleteUser(username, password);
            Database.DeleteUserWorkflowRelationsByUserId(user.GetId());
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

            return new User[] { };
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
