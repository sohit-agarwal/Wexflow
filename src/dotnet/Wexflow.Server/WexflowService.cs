using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using Wexflow.Core;
using Wexflow.Core.Db;
using Wexflow.Core.ExecutionGraph.Flowchart;
using Wexflow.Core.Service.Contracts;
using Entry = Wexflow.Core.Service.Contracts.Entry;
using HistoryEntry = Wexflow.Core.Service.Contracts.HistoryEntry;
using LaunchType = Wexflow.Core.Service.Contracts.LaunchType;
using StatusCount = Wexflow.Core.Service.Contracts.StatusCount;
using User = Wexflow.Core.Service.Contracts.User;
using UserProfile = Wexflow.Core.Service.Contracts.UserProfile;
using System.Configuration;

namespace Wexflow.Server
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class WexflowService : IWexflowService
    {
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "workflows")]
        public WorkflowInfo[] GetWorkflows()
        {
            return WexflowWindowsService.WexflowEngine.Workflows.Select(wf => new WorkflowInfo(wf.Id, wf.Name,
                    (LaunchType) wf.LaunchType, wf.IsEnabled, wf.Description, wf.IsRunning, wf.IsPaused,
                    wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression, wf.WorkflowFilePath,
                    wf.IsExecutionGraphEmpty
                    , wf.LocalVariables.Select(v => new Core.Service.Contracts.Variable{Key = v.Key, Value = v.Value }).ToArray()
                    ))
                .ToArray();
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "search?s={keyword}")]
        public WorkflowInfo[] Search(string keyword)
        {
            var keywordToUpper = keyword.ToUpper();
            return WexflowWindowsService.WexflowEngine.Workflows
                .Where(wf =>
                    wf.Name.ToUpper().Contains(keywordToUpper) || wf.Description.ToUpper().Contains(keywordToUpper))
                .Select(wf => new WorkflowInfo(wf.Id, wf.Name,
                    (LaunchType) wf.LaunchType, wf.IsEnabled, wf.Description, wf.IsRunning, wf.IsPaused,
                    wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression, wf.WorkflowFilePath,
                    wf.IsExecutionGraphEmpty
                   , wf.LocalVariables.Select(v => new Core.Service.Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()))
                .ToArray();
        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "start/{id}")]
        public void StartWorkflow(string id)
        {
            WexflowWindowsService.WexflowEngine.StartWorkflow(int.Parse(id));
        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "stop/{id}")]
        public bool StopWorkflow(string id)
        {
            return WexflowWindowsService.WexflowEngine.StopWorkflow(int.Parse(id));
        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "suspend/{id}")]
        public bool SuspendWorkflow(string id)
        {
            return WexflowWindowsService.WexflowEngine.SuspendWorkflow(int.Parse(id));
        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "resume/{id}")]
        public void ResumeWorkflow(string id)
        {
            WexflowWindowsService.WexflowEngine.ResumeWorkflow(int.Parse(id));
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "workflow/{id}")]
        public WorkflowInfo GetWorkflow(string id)
        {
            var wf = WexflowWindowsService.WexflowEngine.GetWorkflow(int.Parse(id));
            if (wf != null)
            {
                return new WorkflowInfo(wf.Id, wf.Name, (LaunchType) wf.LaunchType, wf.IsEnabled, wf.Description,
                    wf.IsRunning, wf.IsPaused, wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression,
                    wf.WorkflowFilePath, wf.IsExecutionGraphEmpty
                        , wf.LocalVariables.Select(v => new Core.Service.Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()
                    );
            }

            return null;
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "tasks/{id}")]
        public TaskInfo[] GetTasks(string id)
        {
            var wf = WexflowWindowsService.WexflowEngine.GetWorkflow(int.Parse(id));
            if (wf != null)
            {
                IList<TaskInfo> taskInfos = new List<TaskInfo>();

                foreach (var task in wf.Taks)
                {
                    IList<SettingInfo> settingInfos = new List<SettingInfo>();

                    foreach (var setting in task.Settings)
                    {
                        IList<AttributeInfo> attributeInfos = new List<AttributeInfo>();

                        foreach (var attribute in setting.Attributes)
                        {
                            AttributeInfo attributeInfo = new AttributeInfo(attribute.Name, attribute.Value);
                            attributeInfos.Add(attributeInfo);
                        }

                        SettingInfo settingInfo = new SettingInfo(setting.Name, setting.Value, attributeInfos.ToArray());
                        settingInfos.Add(settingInfo);
                    }

                    TaskInfo taskInfo = new TaskInfo(task.Id, task.Name, task.Description, task.IsEnabled, settingInfos.ToArray());

                    taskInfos.Add(taskInfo);
                }

                return taskInfos.ToArray();
            }

            return null;
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "xml/{id}")]
        public string GetWorkflowXml(string id)
        {
            var wf = WexflowWindowsService.WexflowEngine.GetWorkflow(int.Parse(id));
            if (wf != null)
            {
                return wf.XDoc.ToString();
            }

            return string.Empty;
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "taskNames")]
        public string[] GetTaskNames()
        {
            try
            {
                JArray array = JArray.Parse(File.ReadAllText(WexflowWindowsService.WexflowEngine.TasksNamesFile));
                return array.ToObject<string[]>().OrderBy(x => x).ToArray();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new [] { "TasksNames.json is not valid." };
            }
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "settings/{taskName}")]
        public string[] GetSettings(string taskName)
        {
            try
            {
                JObject o = JObject.Parse(File.ReadAllText(WexflowWindowsService.WexflowEngine.TasksSettingsFile));
                var token = o.SelectToken(taskName);
                return token != null ? token.ToObject<string[]>().OrderBy(x => x).ToArray() : new string[] { };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new [] { "TasksSettings.json is not valid." };
            }
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "workflowsFolder")]
        public string GetWorkflowsFolder()
        {
            return WexflowWindowsService.WexflowEngine.WorkflowsFolder;
        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "taskToXml")]
        public string GetTaskXml(Stream streamdata)
        {
            try
            {
                StreamReader reader = new StreamReader(streamdata);
                string json = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();

                JObject task = JObject.Parse(json);

                int taskId = (int) task.SelectToken("Id");
                string taskName = (string) task.SelectToken("Name");
                string taskDesc = (string) task.SelectToken("Description");
                bool isTaskEnabled = (bool) task.SelectToken("IsEnabled");

                var xtask = new XElement("Task"
                    , new XAttribute("id", taskId)
                    , new XAttribute("name", taskName)
                    , new XAttribute("description", taskDesc)
                    , new XAttribute("enabled", isTaskEnabled.ToString().ToLower())
                );

                var settings = task.SelectToken("Settings");
                foreach (var setting in settings)
                {
                    string settingName = (string) setting.SelectToken("Name");
                    string settingValue = (string) setting.SelectToken("Value");

                    var xsetting = new XElement("Setting"
                        , new XAttribute("name", settingName)
                    );

                    if (!string.IsNullOrEmpty(settingValue))
                    {
                        xsetting.SetAttributeValue("value", settingValue);
                    }

                    if (settingName == "selectFiles" || settingName == "selectAttachments")
                    {
                        if (!string.IsNullOrEmpty(settingValue))
                        {
                            xsetting.SetAttributeValue("value", settingValue);
                        }
                    }
                    else
                    {
                        xsetting.SetAttributeValue("value", settingValue);
                    }

                    var attributes = setting.SelectToken("Attributes");
                    foreach (var attribute in attributes)
                    {
                        string attributeName = (string) attribute.SelectToken("Name");
                        string attributeValue = (string) attribute.SelectToken("Value");
                        xsetting.SetAttributeValue(attributeName, attributeValue);
                    }

                    xtask.Add(xsetting);
                }

                return xtask.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return string.Empty;
            }
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "isWorkflowIdValid/{id}")]
        public bool IsWorkflowIdValid(string id)
        {
            var workflowId = int.Parse(id);
            foreach (var workflow in WexflowWindowsService.WexflowEngine.Workflows)
            {
                if (workflow.Id == workflowId) return false;
            }

            return true;
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "isCronExpressionValid?e={expression}")]
        public bool IsCronExpressionValid(string expression)
        {
            var res = WexflowEngine.IsCronExpressionValid(expression);
            return res;
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "isPeriodValid/{period}")]
        public bool IsPeriodValid(string period)
        {
            TimeSpan ts;
            var res = TimeSpan.TryParse(period, out ts);
            return res;
        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "save")]
        public bool SaveWorkflow(Stream streamdata)
        {
            try
            {
                StreamReader reader = new StreamReader(streamdata);
                string json = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();

                JObject o = JObject.Parse(json);
                var wi = o.SelectToken("WorkflowInfo");

                var isNew = (bool) wi.SelectToken("IsNew");
                if (isNew)
                {
                    XNamespace xn = "urn:wexflow-schema";
                    var xdoc = new XDocument();

                    int workflowId = (int) wi.SelectToken("Id");
                    string workflowName = (string) wi.SelectToken("Name");
                    LaunchType workflowLaunchType = (LaunchType) ((int) wi.SelectToken("LaunchType"));
                    string p = (string) wi.SelectToken("Period");
                    TimeSpan workflowPeriod = TimeSpan.Parse(string.IsNullOrEmpty(p) ? "00.00:00:00" : p);
                    string cronExpression = (string) wi.SelectToken("CronExpression");

                    if (workflowLaunchType == LaunchType.Cron && !WexflowEngine.IsCronExpressionValid(cronExpression))
                    {
                        throw new Exception("The cron expression '" + cronExpression + "' is not valid.");
                    }

                    bool isWorkflowEnabled = (bool) wi.SelectToken("IsEnabled");
                    string workflowDesc = (string) wi.SelectToken("Description");

                    // Local variables
                    var xLocalVariables = new XElement(xn + "LocalVariables");
                    var variables = wi.SelectToken("LocalVariables");
                    foreach (var variable in variables)
                    {
                        string key = (string)variable.SelectToken("Key");
                        string value = (string)variable.SelectToken("Value");

                        var xVariable = new XElement(xn + "Variable"
                                , new XAttribute("name", key)
                                , new XAttribute("value", value)
                        );

                        xLocalVariables.Add(xVariable);
                    }

                    // tasks
                    var xtasks = new XElement(xn + "Tasks");
                    var tasks = o.SelectToken("Tasks");
                    foreach (var task in tasks)
                    {
                        int taskId = (int) task.SelectToken("Id");
                        string taskName = (string) task.SelectToken("Name");
                        string taskDesc = (string) task.SelectToken("Description");
                        bool isTaskEnabled = (bool) task.SelectToken("IsEnabled");

                        var xtask = new XElement(xn + "Task"
                            , new XAttribute("id", taskId)
                            , new XAttribute("name", taskName)
                            , new XAttribute("description", taskDesc)
                            , new XAttribute("enabled", isTaskEnabled.ToString().ToLower())
                        );

                        var settings = task.SelectToken("Settings");
                        foreach (var setting in settings)
                        {
                            string settingName = (string) setting.SelectToken("Name");
                            string settingValue = (string) setting.SelectToken("Value");

                            var xsetting = new XElement(xn + "Setting"
                                , new XAttribute("name", settingName)
                            );

                            if (!string.IsNullOrEmpty(settingValue))
                            {
                                xsetting.SetAttributeValue("value", settingValue);
                            }

                            if (settingName == "selectFiles" || settingName == "selectAttachments")
                            {
                                if (!string.IsNullOrEmpty(settingValue))
                                {
                                    xsetting.SetAttributeValue("value", settingValue);
                                }
                            }
                            else
                            {
                                xsetting.SetAttributeValue("value", settingValue);
                            }

                            var attributes = setting.SelectToken("Attributes");
                            foreach (var attribute in attributes)
                            {
                                string attributeName = (string) attribute.SelectToken("Name");
                                string attributeValue = (string) attribute.SelectToken("Value");
                                xsetting.SetAttributeValue(attributeName, attributeValue);
                            }

                            xtask.Add(xsetting);
                        }

                        xtasks.Add(xtask);
                    }

                    // root
                    var xwf = new XElement(xn + "Workflow"
                        , new XAttribute("id", workflowId)
                        , new XAttribute("name", workflowName)
                        , new XAttribute("description", workflowDesc)
                        , new XElement(xn + "Settings"
                            , new XElement(xn + "Setting"
                                , new XAttribute("name", "launchType")
                                , new XAttribute("value", workflowLaunchType.ToString().ToLower()))
                            , new XElement(xn + "Setting"
                                , new XAttribute("name", "enabled")
                                , new XAttribute("value", isWorkflowEnabled.ToString().ToLower()))
                            , new XElement(xn + "Setting"
                                , new XAttribute("name", "period")
                                , new XAttribute("value", workflowPeriod.ToString(@"dd\.hh\:mm\:ss")))
                            , new XElement(xn + "Setting"
                                , new XAttribute("name", "cronExpression")
                                , new XAttribute("value", cronExpression))
                        )
                        , xLocalVariables
                        , xtasks
                    );

                    xdoc.Add(xwf);

                    var path = (string) wi.SelectToken("Path");
                    xdoc.Save(path);
                }
                else
                {
                    int id = int.Parse((string) o.SelectToken("Id"));
                    var wf = WexflowWindowsService.WexflowEngine.GetWorkflow(id);
                    if (wf != null)
                    {
                        var xdoc = wf.XDoc;

                        int workflowId = (int) wi.SelectToken("Id");
                        string workflowName = (string) wi.SelectToken("Name");
                        LaunchType workflowLaunchType = (LaunchType) ((int) wi.SelectToken("LaunchType"));
                        string p = (string) wi.SelectToken("Period");
                        TimeSpan workflowPeriod = TimeSpan.Parse(string.IsNullOrEmpty(p) ? "00.00:00:00" : p);
                        string cronExpression = (string) wi.SelectToken("CronExpression");

                        if (workflowLaunchType == LaunchType.Cron &&
                            !WexflowEngine.IsCronExpressionValid(cronExpression))
                        {
                            throw new Exception("The cron expression '" + cronExpression + "' is not valid.");
                        }

                        bool isWorkflowEnabled = (bool) wi.SelectToken("IsEnabled");
                        string workflowDesc = (string) wi.SelectToken("Description");

                        //if(xdoc.Root == null) throw new Exception("Root is null");
                        xdoc.Root.Attribute("id").Value = workflowId.ToString();
                        xdoc.Root.Attribute("name").Value = workflowName;
                        xdoc.Root.Attribute("description").Value = workflowDesc;

                        var xwfEnabled = xdoc.Root.XPathSelectElement("wf:Settings/wf:Setting[@name='enabled']",
                            wf.XmlNamespaceManager);
                        xwfEnabled.Attribute("value").Value = isWorkflowEnabled.ToString().ToLower();
                        var xwfLaunchType = xdoc.Root.XPathSelectElement("wf:Settings/wf:Setting[@name='launchType']",
                            wf.XmlNamespaceManager);
                        xwfLaunchType.Attribute("value").Value = workflowLaunchType.ToString().ToLower();

                        var xwfPeriod = xdoc.Root.XPathSelectElement("wf:Settings/wf:Setting[@name='period']",
                            wf.XmlNamespaceManager);
                        //if (workflowLaunchType == LaunchType.Periodic)
                        //{
                        if (xwfPeriod != null)
                        {
                            xwfPeriod.Attribute("value").Value = workflowPeriod.ToString(@"dd\.hh\:mm\:ss");
                        }
                        else
                        {
                            xdoc.Root.XPathSelectElement("wf:Settings", wf.XmlNamespaceManager)
                                .Add(new XElement(wf.XNamespaceWf + "Setting", new XAttribute("name", "period"),
                                    new XAttribute("value", workflowPeriod.ToString())));
                        }
                        //}

                        var xwfCronExpression = xdoc.Root.XPathSelectElement(
                            "wf:Settings/wf:Setting[@name='cronExpression']",
                            wf.XmlNamespaceManager);

                        if (xwfCronExpression != null)
                        {
                            xwfCronExpression.Attribute("value").Value = cronExpression ?? string.Empty;
                        }
                        else if (!string.IsNullOrEmpty(cronExpression))
                        {
                            xdoc.Root.XPathSelectElement("wf:Settings", wf.XmlNamespaceManager)
                                .Add(new XElement(wf.XNamespaceWf + "Setting", new XAttribute("name", "cronExpression"),
                                    new XAttribute("value", cronExpression)));
                        }

                        // Local variables
                        var xLocalVariables = xdoc.Root.Element(wf.XNamespaceWf + "LocalVariables");
                        if (xLocalVariables != null)
                        {
                            var allVariables = xLocalVariables.Elements(wf.XNamespaceWf + "Variable");
                            allVariables.Remove();
                        }
                        else
                        {
                            xLocalVariables = new XElement(wf.XNamespaceWf + "LocalVariables");
                            xdoc.Root.Element(wf.XNamespaceWf + "Tasks").AddBeforeSelf(xLocalVariables);
                        }

                        var variables = wi.SelectToken("LocalVariables");
                        foreach (var variable in variables)
                        {
                            string key = (string) variable.SelectToken("Key");
                            string value = (string) variable.SelectToken("Value");

                            var xVariable = new XElement(wf.XNamespaceWf + "Variable"
                                    , new XAttribute("name", key)
                                    , new XAttribute("value", value)
                            );

                            xLocalVariables.Add(xVariable);
                        }

                        // Tasks
                        var xtasks = xdoc.Root.Element(wf.XNamespaceWf + "Tasks");
                        var alltasks = xtasks.Elements(wf.XNamespaceWf + "Task");
                        alltasks.Remove();

                        var tasks = o.SelectToken("Tasks");
                        foreach (var task in tasks)
                        {
                            int taskId = (int) task.SelectToken("Id");
                            string taskName = (string) task.SelectToken("Name");
                            string taskDesc = (string) task.SelectToken("Description");
                            bool isTaskEnabled = (bool) task.SelectToken("IsEnabled");

                            var xtask = new XElement(wf.XNamespaceWf + "Task"
                                , new XAttribute("id", taskId)
                                , new XAttribute("name", taskName)
                                , new XAttribute("description", taskDesc)
                                , new XAttribute("enabled", isTaskEnabled.ToString().ToLower())
                            );

                            var settings = task.SelectToken("Settings");
                            foreach (var setting in settings)
                            {
                                string settingName = (string) setting.SelectToken("Name");
                                string settingValue = (string) setting.SelectToken("Value");

                                var xsetting = new XElement(wf.XNamespaceWf + "Setting"
                                    , new XAttribute("name", settingName)
                                );

                                if (settingName == "selectFiles" || settingName == "selectAttachments")
                                {
                                    if (!string.IsNullOrEmpty(settingValue))
                                    {
                                        xsetting.SetAttributeValue("value", settingValue);
                                    }
                                }
                                else
                                {
                                    xsetting.SetAttributeValue("value", settingValue);
                                }

                                var attributes = setting.SelectToken("Attributes");
                                foreach (var attribute in attributes)
                                {
                                    string attributeName = (string) attribute.SelectToken("Name");
                                    string attributeValue = (string) attribute.SelectToken("Value");
                                    xsetting.SetAttributeValue(attributeName, attributeValue);
                                }

                                xtask.Add(xsetting);
                            }

                            xtasks.Add(xtask);
                        }

                        xdoc.Save(wf.WorkflowFilePath);
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "delete/{id}")]
        public bool DeleteWorkflow(string id)
        {
            try
            {
                var wf = WexflowWindowsService.WexflowEngine.GetWorkflow(int.Parse(id));
                if (wf != null)
                {
                    string destPath = Path.Combine(WexflowWindowsService.WexflowEngine.TrashFolder, Path.GetFileName(wf.WorkflowFilePath));
                    if (File.Exists(destPath))
                    {
                        destPath = Path.Combine(WexflowWindowsService.WexflowEngine.TrashFolder
                            , Path.GetFileNameWithoutExtension(destPath) + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(destPath));
                    }

                    File.Move(wf.WorkflowFilePath, destPath);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "graph/{id}")]
        public Node[] GetExecutionGraph(string id)
        {
            var wf = WexflowWindowsService.WexflowEngine.GetWorkflow(int.Parse(id));
            if (wf != null)
            {
                IList<Node> nodes = new List<Node>();

                foreach (var node in wf.ExecutionGraph.Nodes)
                {
                    var task = wf.Taks.FirstOrDefault(t => t.Id == node.Id);
                    string nodeName = "Task " + node.Id + (task != null ? ": " + task.Description : "");

                    if (node is If)
                    {
                        nodeName = "If...EndIf";
                    }
                    else if (node is While)
                    {
                        nodeName = "While...EndWhile";
                    }
                    else if (node is Switch)
                    {
                        nodeName = "Switch...EndSwitch";
                    }

                    string nodeId = "n" + node.Id;
                    string parentId = "n" + node.ParentId;

                    nodes.Add(new Node(nodeId, nodeName, parentId));
                }

                return nodes.ToArray();
            }

            return null;
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "statusCount")]
        public StatusCount GetStatusCount()
        {
            var statusCount = WexflowWindowsService.WexflowEngine.GetStatusCount();
            return new StatusCount
            {
                PendingCount = statusCount.PendingCount,
                RunningCount = statusCount.RunningCount,
                DoneCount = statusCount.DoneCount,
                FailedCount = statusCount.FailedCount,
                WarningCount = statusCount.WarningCount,
                DisabledCount = statusCount.DisabledCount,
                StoppedCount = statusCount.StoppedCount
            };
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "entries")]
        public Entry[] GetEntries()
        {
            var entries = WexflowWindowsService.WexflowEngine.GetEntries();
            return entries.Select(e =>
                new Entry
                {
                    Id = e.Id,
                    WorkflowId = e.WorkflowId,
                    Name = e.Name,
                    LaunchType = (LaunchType) ((int) e.LaunchType),
                    Description = e.Description,
                    Status = (Core.Service.Contracts.Status) ((int) e.Status)
                }).ToArray();
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "user?username={username}")]
        public User GetUser(string username)
        {
            var user = WexflowWindowsService.WexflowEngine.GetUser(username);
            DateTime baseDate = new DateTime(1970, 1, 1);
            if (user != null)
            {
                return new User
                {
                    Id = user.Id,
                    Username = user.Username,
                    Password = user.Password,
                    UserProfile = (UserProfile)((int)user.UserProfile),
                    Email = user.Email,
                    CreatedOn = (user.CreatedOn - baseDate).TotalMilliseconds,
                    ModifiedOn = (user.ModifiedOn - baseDate).TotalMilliseconds
                };
            }

            return null;
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "users")]
        public User[] GetUsers()
        {
            var users = WexflowWindowsService.WexflowEngine.GetUsers();
            DateTime baseDate = new DateTime(1970, 1, 1);

            return users.Select(u => new User
            {
                Id = u.Id,
                Username = u.Username,
                Password = u.Password,
                UserProfile = (UserProfile)((int)u.UserProfile),
                Email = u.Email,
                CreatedOn = (u.CreatedOn - baseDate).TotalMilliseconds,
                ModifiedOn = (u.ModifiedOn - baseDate).TotalMilliseconds
            }).ToArray();
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "searchUsers?keyword={keyword}&uo={uo}")]
        public User[] SearchUsers(string keyword, int uo)
        {
            var users = WexflowWindowsService.WexflowEngine.GetUsers(keyword, (UserOrderBy)uo);
            DateTime baseDate = new DateTime(1970, 1, 1);

            return users.Select(u => new User
            {
                Id = u.Id,
                Username = u.Username,
                Password = u.Password,
                UserProfile = (UserProfile)((int)u.UserProfile),
                Email = u.Email,
                CreatedOn = (u.CreatedOn - baseDate).TotalMilliseconds,
                ModifiedOn = (u.ModifiedOn - baseDate).TotalMilliseconds
            }).ToArray();
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "password?u={username}")]
        public string GetPassword(string username)
        {
            return WexflowWindowsService.WexflowEngine.GetPassword(username);
        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "insertUser?username={username}&password={password}&up={userProfile}&email={email}")]
        public bool InsertUser(string username, string password, int userProfile, string email)
        {
            try
            {
                WexflowWindowsService.WexflowEngine.InsertUser(username, password, (Core.Db.UserProfile)userProfile, email);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "updateUser?userId={userId}&username={username}&password={password}&up={userProfile}&email={email}")]
        public bool UpdateUser(int userId, string username, string password, int userProfile, string email)
        {
            try
            {
                WexflowWindowsService.WexflowEngine.UpdateUser(userId, username, password, (Core.Db.UserProfile)userProfile, email);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "updateUsernameAndEmailAndUserProfile?userId={userId}&username={username}&email={email}&up={up}")]
        public bool UpdateUsernameAndEmailAndUserProfile(int userId, string username, string email, int up)
        {
            try
            {
                WexflowWindowsService.WexflowEngine.UpdateUsernameAndEmailAndUserProfile(userId, username, email, up);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "deleteUser?username={username}&password={password}")]
        public bool DeleteUser(string username, string password)
        {
            try
            {
                WexflowWindowsService.WexflowEngine.DeleteUser(username, password);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "resetPassword?username={username}&email={email}")]
        public bool ResetPassword(string username, string email)
        {
            try
            {
                string newPassword = "wexflow" + GenerateRandomNumber();
                string newPasswordHash = Db.GetMd5(newPassword);

                // Send email
                string subject = "Wexflow - Password reset of user " + username;
                string body = "Your new password is: " + newPassword;

                string host = ConfigurationManager.AppSettings["Smtp.Host"];
                int port = int.Parse(ConfigurationManager.AppSettings["Smtp.Port"]);
                bool enableSsl = bool.Parse(ConfigurationManager.AppSettings["Smtp.EnableSsl"]);
                string smtpUser = ConfigurationManager.AppSettings["Smtp.User"];
                string smtpPassword = ConfigurationManager.AppSettings["Smtp.Password"];
                string from = ConfigurationManager.AppSettings["Smtp.From"];

                Send(host, port, enableSsl, smtpUser, smtpPassword, email, from, subject, body);

                // Update password
                WexflowWindowsService.WexflowEngine.UpdatePassword(username, newPasswordHash);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        private int GenerateRandomNumber()
        {
            int _min = 1000;
            int _max = 9999;
            Random _rdm = new Random();
            return _rdm.Next(_min, _max);
        }

        private void Send(string host, int port, bool enableSsl, string user, string password, string to, string from, string subject, string body)
        {
            var smtp = new SmtpClient
            {
                Host = host,
                Port = port,
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(user, password)
            };

            using (var msg = new MailMessage())
            {
                msg.From = new MailAddress(from);
                msg.To.Add(new MailAddress(to));
                msg.Subject = subject;
                msg.Body = body;

                smtp.Send(msg);
            }
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "historyEntries")]
        public HistoryEntry[] GetHistoryEntries()
        {
            var entries = WexflowWindowsService.WexflowEngine.GetHistoryEntries();
            DateTime baseDate = new DateTime(1970, 1, 1);
            return entries.Select(e =>
                new HistoryEntry
                {
                    Id = e.Id,
                    WorkflowId = e.WorkflowId,
                    Name = e.Name,
                    LaunchType = (LaunchType) ((int) e.LaunchType),
                    Description = e.Description,
                    Status = (Core.Service.Contracts.Status) ((int) e.Status),
                    StatusDate = (e.StatusDate - baseDate).TotalMilliseconds
                }).ToArray();
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "searchHistoryEntries?s={keyword}")]
        public HistoryEntry[] SearchHistoryEntries(string keyword)
        {
            var entries = WexflowWindowsService.WexflowEngine.GetHistoryEntries(keyword);
            DateTime baseDate = new DateTime(1970, 1, 1);
            return entries.Select(e =>
                new HistoryEntry
                {
                    Id = e.Id,
                    WorkflowId = e.WorkflowId,
                    Name = e.Name,
                    LaunchType = (LaunchType) ((int) e.LaunchType),
                    Description = e.Description,
                    Status = (Core.Service.Contracts.Status) ((int) e.Status),
                    StatusDate = (e.StatusDate - baseDate).TotalMilliseconds
                }).ToArray();
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "searchHistoryEntriesByPage?s={keyword}&page={page}&entriesCount={entriesCount}")]
        public HistoryEntry[] SearchHistoryEntriesByPage(string keyword, int page, int entriesCount)
        {
            var entries = WexflowWindowsService.WexflowEngine.GetHistoryEntries(keyword, page, entriesCount);
            DateTime baseDate = new DateTime(1970, 1, 1);
            return entries.Select(e =>
                new HistoryEntry
                {
                    Id = e.Id,
                    WorkflowId = e.WorkflowId,
                    Name = e.Name,
                    LaunchType = (LaunchType) ((int) e.LaunchType),
                    Description = e.Description,
                    Status = (Core.Service.Contracts.Status) ((int) e.Status),
                    StatusDate = (e.StatusDate - baseDate).TotalMilliseconds
                }).ToArray();
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate =
                "searchHistoryEntriesByPageOrderBy?s={keyword}&from={from}&to={to}&page={page}&entriesCount={entriesCount}&heo={heo}")]
        public HistoryEntry[] SearchHistoryEntriesByPageOrderBy(string keyword, double from, double to, int page,
            int entriesCount, int heo)
        {
            DateTime baseDate = new DateTime(1970, 1, 1);
            DateTime fromDate = baseDate.AddMilliseconds(from);
            DateTime toDate = baseDate.AddMilliseconds(to);

            var entries = WexflowWindowsService.WexflowEngine.GetHistoryEntries(keyword, fromDate, toDate, page,
                entriesCount, (Core.Db.EntryOrderBy) heo);

            return entries.Select(e =>
                new HistoryEntry
                {
                    Id = e.Id,
                    WorkflowId = e.WorkflowId,
                    Name = e.Name,
                    LaunchType = (LaunchType) ((int) e.LaunchType),
                    Description = e.Description,
                    Status = (Core.Service.Contracts.Status) ((int) e.Status),
                    StatusDate = (e.StatusDate - baseDate).TotalMilliseconds
                }).ToArray();
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate =
                "searchEntriesByPageOrderBy?s={keyword}&from={from}&to={to}&page={page}&entriesCount={entriesCount}&heo={heo}")]
        public Entry[] SearchEntriesByPageOrderBy(string keyword, double from, double to, int page, int entriesCount,
            int heo)
        {
            DateTime baseDate = new DateTime(1970, 1, 1);
            DateTime fromDate = baseDate.AddMilliseconds(from);
            DateTime toDate = baseDate.AddMilliseconds(to);

            var entries = WexflowWindowsService.WexflowEngine.GetEntries(keyword, fromDate, toDate, page, entriesCount, (EntryOrderBy) heo);

            return entries.Select(e =>
                new Entry
                {
                    Id = e.Id,
                    WorkflowId = e.WorkflowId,
                    Name = e.Name,
                    LaunchType = (LaunchType) ((int) e.LaunchType),
                    Description = e.Description,
                    Status = (Core.Service.Contracts.Status) ((int) e.Status),
                    StatusDate = (e.StatusDate - baseDate).TotalMilliseconds
                }).ToArray();
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "historyEntriesCount?s={keyword}")]
        public long GetHistoryEntriesCount(string keyword)
        {
            long count = WexflowWindowsService.WexflowEngine.GetHistoryEntriesCount(keyword);
            return count;
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "historyEntriesCountByDate?s={keyword}&from={from}&to={to}")]
        public long GetHistoryEntriesCountByDate(string keyword, double from, double to)
        {
            DateTime baseDate = new DateTime(1970, 1, 1);
            DateTime fromDate = baseDate.AddMilliseconds(from);
            DateTime toDate = baseDate.AddMilliseconds(to);
            long count = WexflowWindowsService.WexflowEngine.GetHistoryEntriesCount(keyword, fromDate, toDate);
            return count;
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "entriesCountByDate?s={keyword}&from={from}&to={to}")]
        public long GetEntriesCountByDate(string keyword, double from, double to)
        {
            DateTime baseDate = new DateTime(1970, 1, 1);
            DateTime fromDate = baseDate.AddMilliseconds(from);
            DateTime toDate = baseDate.AddMilliseconds(to);
            long count = WexflowWindowsService.WexflowEngine.GetEntriesCount(keyword, fromDate, toDate);
            return count;
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "historyEntryStatusDateMin")]
        public double GetHistoryEntryStatusDateMin()
        {
            var date = WexflowWindowsService.WexflowEngine.GetHistoryEntryStatusDateMin();
            DateTime baseDate = new DateTime(1970, 1, 1);
            return (date - baseDate).TotalMilliseconds;
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "historyEntryStatusDateMax")]
        public double GetHistoryEntryStatusDateMax()
        {
            var date = WexflowWindowsService.WexflowEngine.GetHistoryEntryStatusDateMax();
            DateTime baseDate = new DateTime(1970, 1, 1);
            return (date - baseDate).TotalMilliseconds;
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "entryStatusDateMin")]
        public double GetEntryStatusDateMin()
        {
            var date = WexflowWindowsService.WexflowEngine.GetEntryStatusDateMin();
            DateTime baseDate = new DateTime(1970, 1, 1);
            return (date - baseDate).TotalMilliseconds;
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "entryStatusDateMax")]
        public double GetEntryStatusDateMax()
        {
            var date = WexflowWindowsService.WexflowEngine.GetEntryStatusDateMax();
            DateTime baseDate = new DateTime(1970, 1, 1);
            return (date - baseDate).TotalMilliseconds;
        }

    }
}
