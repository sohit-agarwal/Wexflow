using Nancy;
using Nancy.Extensions;
using Nancy.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Wexflow.Core;
using Wexflow.Core.Db;
using Wexflow.Core.ExecutionGraph.Flowchart;
using Wexflow.Server.Contracts;
using HistoryEntry = Wexflow.Core.Db.HistoryEntry;
using LaunchType = Wexflow.Server.Contracts.LaunchType;
using StatusCount = Wexflow.Server.Contracts.StatusCount;
using User = Wexflow.Server.Contracts.User;
using UserProfile = Wexflow.Server.Contracts.UserProfile;

namespace Wexflow.Server
{
    public sealed class WexflowService : NancyModule
    {
        private const string Root = "/wexflow/";

        public WexflowService(IAppConfiguration appConfig)
        {
            Hello();

            //
            // Manager
            //
            GetWorkflows();
            Search();
            GetWorkflow();
            StartWorkflow();
            StopWorkflow();
            SuspendWorkflow();
            ResumeWorkflow();

            //
            // Designer
            // 
            GetTasks();
            GetWorkflowXml();
            GetTaskNames();
            GetSettings();
            GetWorkflowsFolder();
            GetTaskXml();
            IsWorkflowIdValid();
            IsCronExpressionValid();
            IsPeriodValid();
            SaveWorkflow();
            DeleteWorkflow();
            GetExecutionGraph();

            //
            // Back office
            //
            GetStatusCount();
            GetUser();
            GetPassword();
            SearchUsers();
            InsertUser();
            UpdateUser();
            UpdateUsernameAndEmailAndUserProfile();
            DeleteUser();
            ResetPassword();
            SearchHistoryEntriesByPageOrderBy();
            SearchEntriesByPageOrderBy();
            GetHistoryEntriesCountByDate();
            GetEntriesCountByDate();
            GetHistoryEntryStatusDateMin();
            GetHistoryEntryStatusDateMax();
            GetEntryStatusDateMin();
            GetEntryStatusDateMax();
        }

        private void Hello()
        {
            Get(Root, args => "Hello from Wexflow workflow engine running on CoreCLR");
        }

        /// <summary>
        /// Returns the list of workflows.
        /// </summary>
        private void GetWorkflows()
        {
            Get(Root + "workflows", args =>
            {
                var workflows = Program.WexflowEngine.Workflows.Select(wf => new WorkflowInfo(wf.Id, wf.Name,
                        (LaunchType) wf.LaunchType, wf.IsEnabled, wf.Description, wf.IsRunning, wf.IsPaused,
                        wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression, wf.WorkflowFilePath,
                        wf.IsExecutionGraphEmpty
                        , wf.LocalVariables.Select(v => new Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()
                        ))
                    .ToArray();
                var workflowsStr = JsonConvert.SerializeObject(workflows);
                var workflowsBytes = Encoding.UTF8.GetBytes(workflowsStr);

                return new Response()
                {
                    ContentType = "application/json",
                    Contents = s => s.Write(workflowsBytes, 0, workflowsBytes.Length)
                };
            });
        }

        /// <summary>
        /// Search for workflows.
        /// </summary>
        private void Search()
        {
            Get(Root + "search", args =>
            {
                string keywordToUpper = Request.Query["s"].ToString().ToUpper();
                var workflows = Program.WexflowEngine.Workflows
                    .Where(wf =>
                        wf.Name.ToUpper().Contains(keywordToUpper) || wf.Description.ToUpper().Contains(keywordToUpper))
                    .Select(wf => new WorkflowInfo(wf.Id, wf.Name,
                        (LaunchType) wf.LaunchType, wf.IsEnabled, wf.Description, wf.IsRunning, wf.IsPaused,
                        wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression, wf.WorkflowFilePath,
                        wf.IsExecutionGraphEmpty
                        , wf.LocalVariables.Select(v => new Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()
                        ))
                    .ToArray();
                var workflowsStr = JsonConvert.SerializeObject(workflows);
                var workflowsBytes = Encoding.UTF8.GetBytes(workflowsStr);

                return new Response()
                {
                    ContentType = "application/json",
                    Contents = s => s.Write(workflowsBytes, 0, workflowsBytes.Length)
                };
            });
        }

        /// <summary>
        /// Returns a workflow from its id.
        /// </summary>
        private void GetWorkflow()
        {
            Get(Root + "workflow/{id}", args =>
            {
                Workflow wf = Program.WexflowEngine.GetWorkflow(args.id);
                if (wf != null)
                {
                    var workflow = new WorkflowInfo(wf.Id, wf.Name, (LaunchType)wf.LaunchType, wf.IsEnabled, wf.Description,
                        wf.IsRunning, wf.IsPaused, wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression,
                        wf.WorkflowFilePath, wf.IsExecutionGraphEmpty
                        , wf.LocalVariables.Select(v => new Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()
                        );
                    var workflowStr = JsonConvert.SerializeObject(workflow);
                    var workflowBytes = Encoding.UTF8.GetBytes(workflowStr);

                    return new Response()
                    {
                        ContentType = "application/json",
                        Contents = s => s.Write(workflowBytes, 0, workflowBytes.Length)
                    };
                }

                return new Response()
                {
                    ContentType = "application/json"
                };
            });
        }

        /// <summary>
        /// Starts a workflow.
        /// </summary>
        private void StartWorkflow()
        {
            Post(Root + "start/{id}", args =>
            {
                Program.WexflowEngine.StartWorkflow(args.id);
                
                return new Response
                {
                    ContentType = "application/json"
                };
            });
        }

        /// <summary>
        /// Stops a workflow.
        /// </summary>
        private void StopWorkflow()
        {
            Post(Root + "stop/{id}", args =>
            {
                bool res = Program.WexflowEngine.StopWorkflow(args.id);

                var resStr = JsonConvert.SerializeObject(res);
                var resBytes = Encoding.UTF8.GetBytes(resStr);

                return new Response()
                {
                    ContentType = "application/json",
                    Contents = s => s.Write(resBytes, 0, resBytes.Length)
                };
            });
        }

        /// <summary>
        /// Suspends a workflow.
        /// </summary>
        private void SuspendWorkflow()
        {
            Post(Root + "suspend/{id}", args =>
            {
                bool res = Program.WexflowEngine.SuspendWorkflow(args.id);

                var resStr = JsonConvert.SerializeObject(res);
                var resBytes = Encoding.UTF8.GetBytes(resStr);

                return new Response()
                {
                    ContentType = "application/json",
                    Contents = s => s.Write(resBytes, 0, resBytes.Length)
                };
            });
        }

        /// <summary>
        /// Resumes a workflow.
        /// </summary>
        private void ResumeWorkflow()
        {
            Post(Root + "resume/{id}", args =>
            {
                Program.WexflowEngine.ResumeWorkflow(args.id);

                return new Response
                {
                    ContentType = "application/json"
                };
            });
        }

        /// <summary>
        /// Returns workflow's tasks.
        /// </summary>
        private void GetTasks()
        {
            Get(Root + "tasks/{id}", args =>
            {
                var wf = Program.WexflowEngine.GetWorkflow(args.id);
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


                    var tasksStr = JsonConvert.SerializeObject(taskInfos);
                    var tasksBytes = Encoding.UTF8.GetBytes(tasksStr);

                    return new Response()
                    {
                        ContentType = "application/json",
                        Contents = s => s.Write(tasksBytes, 0, tasksBytes.Length)
                    };

                }

                return new Response()
                {
                    ContentType = "application/json"
                };
            });
        }

        /// <summary>
        /// Returns a workflow as XML.
        /// </summary>
        private void GetWorkflowXml()
        {
            Get(Root + "xml/{id}", args =>
            {
                var wf = Program.WexflowEngine.GetWorkflow(args.id);
                if (wf != null)
                {
                    var xmlStr = JsonConvert.SerializeObject(wf.XDoc.ToString());
                    var xmlBytes = Encoding.UTF8.GetBytes(xmlStr);

                    return new Response()
                    {
                        ContentType = "application/json",
                        Contents = s => s.Write(xmlBytes, 0, xmlBytes.Length)
                    };
                }

                return new Response()
                {
                    ContentType = "application/json"
                };
            });
        }

        /// <summary>
        /// Returns task names.
        /// </summary>
        private void GetTaskNames()
        {
            Get(Root + "taskNames", args =>
            {
                string[] taskNames;
                try
                {
                    JArray array = JArray.Parse(File.ReadAllText(Program.WexflowEngine.TasksNamesFile));
                    taskNames = array.ToObject<string[]>().OrderBy(x => x).ToArray();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    taskNames = new [] { "TasksNames.json is not valid." };
                }

                var taskNamesStr = JsonConvert.SerializeObject(taskNames);
                var taskNamesBytes = Encoding.UTF8.GetBytes(taskNamesStr);

                return new Response()
                {
                    ContentType = "application/json",
                    Contents = s => s.Write(taskNamesBytes, 0, taskNamesBytes.Length)
                };

            });
        }

        /// <summary>
        /// Returns task settings.
        /// </summary>
        private void GetSettings()
        {
            Get(Root + "settings/{taskName}", args =>
            {
                string[] taskSettings;
                try
                {
                    JObject o = JObject.Parse(File.ReadAllText(Program.WexflowEngine.TasksSettingsFile));
                    var token = o.SelectToken(args.taskName);
                    taskSettings = token != null ? token.ToObject<string[]>() : new string[] { };
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    taskSettings = new[] { "TasksSettings.json is not valid." };
                }

                var taskSettingsStr = JsonConvert.SerializeObject(taskSettings);
                var taskSettingsBytes = Encoding.UTF8.GetBytes(taskSettingsStr);

                return new Response()
                {
                    ContentType = "application/json",
                    Contents = s => s.Write(taskSettingsBytes, 0, taskSettingsBytes.Length)
                };
            });
        }

        /// <summary>
        /// Returns workflows folder.
        /// </summary>
        private void GetWorkflowsFolder()
        {
            Get(Root + "workflowsFolder", args =>
            {
                string workflowsFolder = Program.WexflowEngine.WorkflowsFolder;
                var workflowsFolderStr = JsonConvert.SerializeObject(workflowsFolder);
                var workflowsFolderBytes = Encoding.UTF8.GetBytes(workflowsFolderStr);

                return new Response()
                {
                    ContentType = "application/json",
                    Contents = s => s.Write(workflowsFolderBytes, 0, workflowsFolderBytes.Length)
                };
            });
        }

        /// <summary>
        /// Returns a task as XML.
        /// </summary>
        private void GetTaskXml()
        {
            Post(Root + "taskToXml", args =>
            {
                try
                {
                    var json = RequestStream.FromStream(Request.Body).AsString();

                    JObject task = JObject.Parse(json);

                    int taskId = (int)task.SelectToken("Id");
                    string taskName = (string)task.SelectToken("Name");
                    string taskDesc = (string)task.SelectToken("Description");
                    bool isTaskEnabled = (bool)task.SelectToken("IsEnabled");

                    var xtask = new XElement("Task"
                        , new XAttribute("id", taskId)
                        , new XAttribute("name", taskName)
                        , new XAttribute("description", taskDesc)
                        , new XAttribute("enabled", isTaskEnabled.ToString().ToLower())
                    );

                    var settings = task.SelectToken("Settings");
                    foreach (var setting in settings)
                    {
                        string settingName = (string)setting.SelectToken("Name");
                        string settingValue = (string)setting.SelectToken("Value");

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
                            string attributeName = (string)attribute.SelectToken("Name");
                            string attributeValue = (string)attribute.SelectToken("Value");
                            xsetting.SetAttributeValue(attributeName, attributeValue);
                        }

                        xtask.Add(xsetting);
                    }

                    string xtaskXml = xtask.ToString();
                    var xtaskXmlStr = JsonConvert.SerializeObject(xtaskXml);
                    var xtaskXmlBytes = Encoding.UTF8.GetBytes(xtaskXmlStr);

                    return new Response()
                    {
                        ContentType = "application/json",
                        Contents = s => s.Write(xtaskXmlBytes, 0, xtaskXmlBytes.Length)
                    };
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return new Response()
                    {
                        ContentType = "application/json"
                    };
                }
            });
        }

        /// <summary>
        /// Checks if a workflow id is valid.
        /// </summary>
        private void IsWorkflowIdValid()
        {
            Get(Root + "isWorkflowIdValid/{id}", args =>
            {
                var workflowId = args.id;
                foreach (var workflow in Program.WexflowEngine.Workflows)
                {
                    if (workflow.Id == workflowId)
                    {
                        var falseStr = JsonConvert.SerializeObject(false);
                        var falseBytes = Encoding.UTF8.GetBytes(falseStr);

                        return new Response()
                        {
                            ContentType = "application/json",
                            Contents = s => s.Write(falseBytes, 0, falseBytes.Length)
                        };
                    }
                }

                var trueStr = JsonConvert.SerializeObject(true);
                var trueBytes = Encoding.UTF8.GetBytes(trueStr);

                return new Response()
                {
                    ContentType = "application/json",
                    Contents = s => s.Write(trueBytes, 0, trueBytes.Length)
                };
            });
        }

        /// <summary>
        /// Checks if a cron expression is valid.
        /// </summary>
        private void IsCronExpressionValid()
        {
            Get(Root + "isCronExpressionValid", args =>
            {
                string expression = Request.Query["e"].ToString();
                var res = WexflowEngine.IsCronExpressionValid(expression);
                var resStr = JsonConvert.SerializeObject(res);
                var resBytes = Encoding.UTF8.GetBytes(resStr);

                return new Response()
                {
                    ContentType = "application/json",
                    Contents = s => s.Write(resBytes, 0, resBytes.Length)
                };

            });
        }

        /// <summary>
        /// Checks if a period is valid.
        /// </summary>
        private void IsPeriodValid()
        {
            Get(Root + "isPeriodValid/{period}", args =>
            {
                TimeSpan ts;
                var res = TimeSpan.TryParse(args.period.ToString(), out ts);
                var resStr = JsonConvert.SerializeObject(res);
                var resBytes = Encoding.UTF8.GetBytes(resStr);

                return new Response()
                {
                    ContentType = "application/json",
                    Contents = s => s.Write(resBytes, 0, resBytes.Length)
                };
            });
        }

        /// <summary>
        /// Saves a workflow
        /// </summary>
        private void SaveWorkflow()
        {
            Post(Root + "save", args =>
            {
                try
                {
                    var json = RequestStream.FromStream(Request.Body).AsString();

                    JObject o = JObject.Parse(json);
                    var wi = o.SelectToken("WorkflowInfo");

                    var isNew = (bool)wi.SelectToken("IsNew");
                    if (isNew)
                    {
                        XNamespace xn = "urn:wexflow-schema";
                        var xdoc = new XDocument();

                        int workflowId = (int)wi.SelectToken("Id");
                        string workflowName = (string)wi.SelectToken("Name");
                        LaunchType workflowLaunchType = (LaunchType)((int)wi.SelectToken("LaunchType"));
                        string p = (string)wi.SelectToken("Period");
                        TimeSpan workflowPeriod = TimeSpan.Parse(string.IsNullOrEmpty(p) ? "00.00:00:00" : p);
                        string cronExpression = (string)wi.SelectToken("CronExpression");

                        if (workflowLaunchType == LaunchType.Cron && !WexflowEngine.IsCronExpressionValid(cronExpression))
                        {
                            throw new Exception("The cron expression '" + cronExpression + "' is not valid.");
                        }

                        bool isWorkflowEnabled = (bool)wi.SelectToken("IsEnabled");
                        string workflowDesc = (string)wi.SelectToken("Description");

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
                            int taskId = (int)task.SelectToken("Id");
                            string taskName = (string)task.SelectToken("Name");
                            string taskDesc = (string)task.SelectToken("Description");
                            bool isTaskEnabled = (bool)task.SelectToken("IsEnabled");

                            var xtask = new XElement(xn + "Task"
                                , new XAttribute("id", taskId)
                                , new XAttribute("name", taskName)
                                , new XAttribute("description", taskDesc)
                                , new XAttribute("enabled", isTaskEnabled.ToString().ToLower())
                            );

                            var settings = task.SelectToken("Settings");
                            foreach (var setting in settings)
                            {
                                string settingName = (string)setting.SelectToken("Name");
                                string settingValue = (string)setting.SelectToken("Value");

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
                                    string attributeName = (string)attribute.SelectToken("Name");
                                    string attributeValue = (string)attribute.SelectToken("Value");
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

                        var path = (string)wi.SelectToken("Path");
                        xdoc.Save(path);
                    }
                    else
                    {
                        int id = int.Parse((string)o.SelectToken("Id"));
                        var wf = Program.WexflowEngine.GetWorkflow(id);
                        if (wf != null)
                        {
                            var xdoc = wf.XDoc;

                            int workflowId = (int)wi.SelectToken("Id");
                            string workflowName = (string)wi.SelectToken("Name");
                            LaunchType workflowLaunchType = (LaunchType)((int)wi.SelectToken("LaunchType"));
                            string p = (string)wi.SelectToken("Period");
                            TimeSpan workflowPeriod = TimeSpan.Parse(string.IsNullOrEmpty(p) ? "00.00:00:00" : p);
                            string cronExpression = (string)wi.SelectToken("CronExpression");

                            if (workflowLaunchType == LaunchType.Cron &&
                                !WexflowEngine.IsCronExpressionValid(cronExpression))
                            {
                                throw new Exception("The cron expression '" + cronExpression + "' is not valid.");
                            }

                            bool isWorkflowEnabled = (bool)wi.SelectToken("IsEnabled");
                            string workflowDesc = (string)wi.SelectToken("Description");

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
                                string key = (string)variable.SelectToken("Key");
                                string value = (string)variable.SelectToken("Value");

                                var xVariable = new XElement(wf.XNamespaceWf + "Variable"
                                        , new XAttribute("name", key)
                                        , new XAttribute("value", value)
                                );

                                xLocalVariables.Add(xVariable);
                            }

                            var xtasks = xdoc.Root.Element(wf.XNamespaceWf + "Tasks");
                            var alltasks = xtasks.Elements(wf.XNamespaceWf + "Task");
                            alltasks.Remove();

                            var tasks = o.SelectToken("Tasks");
                            foreach (var task in tasks)
                            {
                                int taskId = (int)task.SelectToken("Id");
                                string taskName = (string)task.SelectToken("Name");
                                string taskDesc = (string)task.SelectToken("Description");
                                bool isTaskEnabled = (bool)task.SelectToken("IsEnabled");

                                var xtask = new XElement(wf.XNamespaceWf + "Task"
                                    , new XAttribute("id", taskId)
                                    , new XAttribute("name", taskName)
                                    , new XAttribute("description", taskDesc)
                                    , new XAttribute("enabled", isTaskEnabled.ToString().ToLower())
                                );

                                var settings = task.SelectToken("Settings");
                                foreach (var setting in settings)
                                {
                                    string settingName = (string)setting.SelectToken("Name");
                                    string settingValue = (string)setting.SelectToken("Value");

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
                                        string attributeName = (string)attribute.SelectToken("Name");
                                        string attributeValue = (string)attribute.SelectToken("Value");
                                        xsetting.SetAttributeValue(attributeName, attributeValue);
                                    }

                                    xtask.Add(xsetting);
                                }

                                xtasks.Add(xtask);
                            }

                            xdoc.Save(wf.WorkflowFilePath);
                        }
                    }

                    var resStr = JsonConvert.SerializeObject(true);
                    var resBytes = Encoding.UTF8.GetBytes(resStr);

                    return new Response()
                    {
                        ContentType = "application/json",
                        Contents = s => s.Write(resBytes, 0, resBytes.Length)
                    };
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);

                    var resStr = JsonConvert.SerializeObject(false);
                    var resBytes = Encoding.UTF8.GetBytes(resStr);

                    return new Response()
                    {
                        ContentType = "application/json",
                        Contents = s => s.Write(resBytes, 0, resBytes.Length)
                    };
                }
            });
        }

        /// <summary>
        /// Deletes a workflow.
        /// </summary>
        private void DeleteWorkflow()
        {
            Post(Root + "delete/{id}", args =>
            {
                try
                {
                    var wf = Program.WexflowEngine.GetWorkflow(args.id);
                    if (wf != null)
                    {
                        string destPath = Path.Combine(Program.WexflowEngine.TrashFolder, Path.GetFileName(wf.WorkflowFilePath));
                        if (File.Exists(destPath))
                        {
                            destPath = Path.Combine(Program.WexflowEngine.TrashFolder
                                , Path.GetFileNameWithoutExtension(destPath) + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(destPath));
                        }

                        File.Move(wf.WorkflowFilePath, destPath);
                    }

                    var resStr = JsonConvert.SerializeObject(true);
                    var resBytes = Encoding.UTF8.GetBytes(resStr);

                    return new Response
                    {
                        ContentType = "application/json",
                        Contents = s => s.Write(resBytes, 0, resBytes.Length)
                    };
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    
                    var resStr = JsonConvert.SerializeObject(false);
                    var resBytes = Encoding.UTF8.GetBytes(resStr);

                    return new Response
                    {
                        ContentType = "application/json",
                        Contents = s => s.Write(resBytes, 0, resBytes.Length)
                    };
                }
            });
        }

        /// <summary>
        /// Returns the execution graph of the workflow.
        /// </summary>
        private void GetExecutionGraph()
        {
            Get(Root + "graph/{id}", args =>
            {
                Workflow wf = Program.WexflowEngine.GetWorkflow(args.id);
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

                    //return nodes.ToArray();

                    var nodesStr = JsonConvert.SerializeObject(nodes);
                    var nodesBytes = Encoding.UTF8.GetBytes(nodesStr);

                    return new Response
                    {
                        ContentType = "application/json",
                        Contents = s => s.Write(nodesBytes, 0, nodesBytes.Length)
                    };
                }

                return new Response
                {
                    ContentType = "application/json"
                };
            });
        }

        /// <summary>
        /// Returns status count.
        /// </summary>
        private void GetStatusCount()
        {
            Get(Root + "statusCount", args =>
            {
                var statusCount = Program.WexflowEngine.GetStatusCount();
                StatusCount sc = new StatusCount
                {
                    PendingCount = statusCount.PendingCount,
                    RunningCount = statusCount.RunningCount,
                    DoneCount = statusCount.DoneCount,
                    FailedCount = statusCount.FailedCount,
                    WarningCount = statusCount.WarningCount,
                    DisabledCount = statusCount.DisabledCount,
                    StoppedCount = statusCount.StoppedCount
                };

                var scStr = JsonConvert.SerializeObject(sc);
                var scBytes = Encoding.UTF8.GetBytes(scStr);

                return new Response
                {
                    ContentType = "application/json",
                    Contents = s => s.Write(scBytes, 0, scBytes.Length)
                };

            });
        }

        /// <summary>
        /// Returns a user from his username.
        /// </summary>
        private void GetUser()
        {
            Get(Root + "user", args =>
            {
                string username = Request.Query["username"].ToString();

                var user = Program.WexflowEngine.GetUser(username);
                DateTime baseDate = new DateTime(1970, 1, 1);
                if (user != null)
                {
                    User u = new User
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Password = user.Password,
                        UserProfile = (UserProfile)((int)user.UserProfile),
                        Email = user.Email,
                        CreatedOn = (user.CreatedOn - baseDate).TotalMilliseconds,
                        ModifiedOn = (user.ModifiedOn - baseDate).TotalMilliseconds
                    };

                    var uStr = JsonConvert.SerializeObject(u);
                    var uBytes = Encoding.UTF8.GetBytes(uStr);

                    return new Response
                    {
                        ContentType = "application/json",
                        Contents = s => s.Write(uBytes, 0, uBytes.Length)
                    };
                }

                return new Response
                {
                    ContentType = "application/json"
                };

            });
        }

        /// <summary>
        /// Returns user's password (crypted).
        /// </summary>
        private void GetPassword()
        {
            Get(Root + "password", args =>
            {
                string username = Request.Query["u"].ToString();
                string pass = Program.WexflowEngine.GetPassword(username);

                var passStr = JsonConvert.SerializeObject(pass);
                var passBytes = Encoding.UTF8.GetBytes(passStr);

                return new Response
                {
                    ContentType = "application/json",
                    Contents = s => s.Write(passBytes, 0, passBytes.Length)
                };
            });
        }

        /// <summary>
        /// Searches for users.
        /// </summary>
        private void SearchUsers()
        {
            Get(Root + "searchUsers", args =>
            {
                string keyword = Request.Query["keyword"].ToString();
                int uo = int.Parse(Request.Query["uo"].ToString());

                var users = Program.WexflowEngine.GetUsers(keyword, (UserOrderBy)uo);
                DateTime baseDate = new DateTime(1970, 1, 1);

                var q = users.Select(u => new User
                {
                    Id = u.Id,
                    Username = u.Username,
                    Password = u.Password,
                    UserProfile = (UserProfile)((int)u.UserProfile),
                    Email = u.Email,
                    CreatedOn = (u.CreatedOn - baseDate).TotalMilliseconds,
                    ModifiedOn = (u.ModifiedOn - baseDate).TotalMilliseconds
                }).ToArray();

                var qStr = JsonConvert.SerializeObject(q);
                var qBytes = Encoding.UTF8.GetBytes(qStr);

                return new Response
                {
                    ContentType = "application/json",
                    Contents = s => s.Write(qBytes, 0, qBytes.Length)
                };

            });
        }

        /// <summary>
        /// Inserts a user.
        /// </summary>
        private void InsertUser()
        {
            Post(Root + "insertUser", args =>
            {
                string username = Request.Query["username"].ToString();
                string password = Request.Query["password"].ToString();
                int userProfile = int.Parse(Request.Query["up"].ToString());
                string email = Request.Query["email"].ToString();

                try
                {
                    Program.WexflowEngine.InsertUser(username, password, (Core.Db.UserProfile)userProfile, email);

                    var resStr = JsonConvert.SerializeObject(true);
                    var resBytes = Encoding.UTF8.GetBytes(resStr);

                    return new Response
                    {
                        ContentType = "application/json",
                        Contents = s => s.Write(resBytes, 0, resBytes.Length)
                    };
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);

                    var resStr = JsonConvert.SerializeObject(false);
                    var resBytes = Encoding.UTF8.GetBytes(resStr);

                    return new Response
                    {
                        ContentType = "application/json",
                        Contents = s => s.Write(resBytes, 0, resBytes.Length)
                    };
                }
            });
        }

        /// <summary>
        /// Updates a user.
        /// </summary>
        private void UpdateUser()
        {
            Post(Root + "updateUser", args =>
            {
                int userId = int.Parse(Request.Query["userId"].ToString());
                string username = Request.Query["username"].ToString();
                string password = Request.Query["password"].ToString();
                int userProfile = int.Parse(Request.Query["up"].ToString());
                string email = Request.Query["email"].ToString();

                try
                {
                    Program.WexflowEngine.UpdateUser(userId, username, password, (Core.Db.UserProfile)userProfile, email);

                    var resStr = JsonConvert.SerializeObject(true);
                    var resBytes = Encoding.UTF8.GetBytes(resStr);

                    return new Response
                    {
                        ContentType = "application/json",
                        Contents = s => s.Write(resBytes, 0, resBytes.Length)
                    };
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);

                    var resStr = JsonConvert.SerializeObject(false);
                    var resBytes = Encoding.UTF8.GetBytes(resStr);

                    return new Response
                    {
                        ContentType = "application/json",
                        Contents = s => s.Write(resBytes, 0, resBytes.Length)
                    };
                }

            });
        }

        /// <summary>
        /// Updates the username, the email and the user profile of a user.
        /// </summary>
        private void UpdateUsernameAndEmailAndUserProfile()
        {
            Post(Root + "updateUsernameAndEmailAndUserProfile", args =>
            {
                int userId = int.Parse(Request.Query["userId"].ToString());
                string username = Request.Query["username"].ToString();
                string email = Request.Query["email"].ToString();
                int up = int.Parse(Request.Query["up"].ToString());
                
                try
                {
                    Program.WexflowEngine.UpdateUsernameAndEmailAndUserProfile(userId, username, email, up);

                    var resStr = JsonConvert.SerializeObject(true);
                    var resBytes = Encoding.UTF8.GetBytes(resStr);

                    return new Response
                    {
                        ContentType = "application/json",
                        Contents = s => s.Write(resBytes, 0, resBytes.Length)
                    };
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);

                    var resStr = JsonConvert.SerializeObject(false);
                    var resBytes = Encoding.UTF8.GetBytes(resStr);

                    return new Response
                    {
                        ContentType = "application/json",
                        Contents = s => s.Write(resBytes, 0, resBytes.Length)
                    };
                }

            });
        }

        /// <summary>
        /// Deletes a user.
        /// </summary>
        private void DeleteUser()
        {
            Post(Root + "deleteUser", args =>
            {
                string username = Request.Query["username"].ToString();
                string password = Request.Query["password"].ToString();

                try
                {
                    Program.WexflowEngine.DeleteUser(username, password);

                    var resStr = JsonConvert.SerializeObject(true);
                    var resBytes = Encoding.UTF8.GetBytes(resStr);

                    return new Response
                    {
                        ContentType = "application/json",
                        Contents = s => s.Write(resBytes, 0, resBytes.Length)
                    };
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);

                    var resStr = JsonConvert.SerializeObject(false);
                    var resBytes = Encoding.UTF8.GetBytes(resStr);

                    return new Response
                    {
                        ContentType = "application/json",
                        Contents = s => s.Write(resBytes, 0, resBytes.Length)
                    };
                }

            });
        }

        /// <summary>
        /// Resets a password.
        /// </summary>
        private void ResetPassword()
        {
            Post(Root + "resetPassword", args =>
            {
                string username = Request.Query["username"].ToString();
                string email = Request.Query["email"].ToString();

                try
                {
                    string newPassword = "wexflow" + GenerateRandomNumber();
                    string newPasswordHash = Db.GetMd5(newPassword);

                    // Send email
                    string subject = "Wexflow - Password reset of user " + username;
                    string body = "Your new password is: " + newPassword;

                    string host = Program.Config["Smtp.Host"];
                    int port = int.Parse(Program.Config["Smtp.Port"]);
                    bool enableSsl = bool.Parse(Program.Config["Smtp.EnableSsl"]);
                    string smtpUser = Program.Config["Smtp.User"];
                    string smtpPassword = Program.Config["Smtp.Password"];
                    string from = Program.Config["Smtp.From"];

                    Send(host, port, enableSsl, smtpUser, smtpPassword, email, from, subject, body);

                    // Update password
                    Program.WexflowEngine.UpdatePassword(username, newPasswordHash);

                    var resStr = JsonConvert.SerializeObject(true);
                    var resBytes = Encoding.UTF8.GetBytes(resStr);

                    return new Response
                    {
                        ContentType = "application/json",
                        Contents = s => s.Write(resBytes, 0, resBytes.Length)
                    };
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);

                    var resStr = JsonConvert.SerializeObject(false);
                    var resBytes = Encoding.UTF8.GetBytes(resStr);

                    return new Response
                    {
                        ContentType = "application/json",
                        Contents = s => s.Write(resBytes, 0, resBytes.Length)
                    };
                }

            });
        }

        /// <summary>
        /// Generates a random number of 4 digits.
        /// </summary>
        /// <returns></returns>
        private int GenerateRandomNumber()
        {
            int _min = 1000;
            int _max = 9999;
            Random _rdm = new Random();
            return _rdm.Next(_min, _max);
        }

        /// <summary>
        /// Sends an email.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="enableSsl"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="to"></param>
        /// <param name="from"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
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

        /// <summary>
        /// Searches for history entries.
        /// </summary>
        private void SearchHistoryEntriesByPageOrderBy()
        {
            Get(Root + "searchHistoryEntriesByPageOrderBy", args =>
            {
                string keyword = Request.Query["s"].ToString();
                double from = double.Parse(Request.Query["from"].ToString());
                double to = double.Parse(Request.Query["to"].ToString());
                int page = int.Parse(Request.Query["page"].ToString());
                int entriesCount = int.Parse(Request.Query["entriesCount"].ToString());
                int heo = int.Parse(Request.Query["heo"].ToString());

                DateTime baseDate = new DateTime(1970, 1, 1);
                DateTime fromDate = baseDate.AddMilliseconds(from);
                DateTime toDate = baseDate.AddMilliseconds(to);

                HistoryEntry[] entries = Program.WexflowEngine.GetHistoryEntries(keyword, fromDate, toDate, page,
                    entriesCount, (EntryOrderBy)heo);

                Contracts.HistoryEntry[] q =  entries.Select(e =>
                    new Contracts.HistoryEntry
                    {
                        Id = e.Id,
                        WorkflowId = e.WorkflowId,
                        Name = e.Name,
                        LaunchType = (LaunchType)((int)e.LaunchType),
                        Description = e.Description,
                        Status = (Contracts.Status)((int)e.Status),
                        StatusDate = (e.StatusDate - baseDate).TotalMilliseconds
                    }).ToArray();

                var qStr = JsonConvert.SerializeObject(q);
                var qBytes = Encoding.UTF8.GetBytes(qStr);

                return new Response
                {
                    ContentType = "application/json",
                    Contents = s => s.Write(qBytes, 0, qBytes.Length)
                };

            });
        }

        /// <summary>
        /// Searches for entries.
        /// </summary>
        private void SearchEntriesByPageOrderBy()
        {
            Get(Root + "searchEntriesByPageOrderBy", args =>
            {
                string keyword = Request.Query["s"].ToString();
                double from = double.Parse(Request.Query["from"].ToString());
                double to = double.Parse(Request.Query["to"].ToString());
                int page = int.Parse(Request.Query["page"].ToString());
                int entriesCount = int.Parse(Request.Query["entriesCount"].ToString());
                int heo = int.Parse(Request.Query["heo"].ToString());

                DateTime baseDate = new DateTime(1970, 1, 1);
                DateTime fromDate = baseDate.AddMilliseconds(from);
                DateTime toDate = baseDate.AddMilliseconds(to);

                Core.Db.Entry[] entries = Program.WexflowEngine.GetEntries(keyword, fromDate, toDate, page, entriesCount, (EntryOrderBy)heo);

                Contracts.Entry[] q = entries.Select(e =>
                    new Contracts.Entry
                    {
                        Id = e.Id,
                        WorkflowId = e.WorkflowId,
                        Name = e.Name,
                        LaunchType = (LaunchType)((int)e.LaunchType),
                        Description = e.Description,
                        Status = (Contracts.Status)((int)e.Status),
                        StatusDate = (e.StatusDate - baseDate).TotalMilliseconds
                    }).ToArray();

                var qStr = JsonConvert.SerializeObject(q);
                var qBytes = Encoding.UTF8.GetBytes(qStr);

                return new Response
                {
                    ContentType = "application/json",
                    Contents = s => s.Write(qBytes, 0, qBytes.Length)
                };

            });
        }

        /// <summary>
        /// Returns history entries count by keyword and date filter.
        /// </summary>
        private void GetHistoryEntriesCountByDate()
        {
            Get(Root + "historyEntriesCountByDate", args =>
            {
                string keyword = Request.Query["s"].ToString();
                double from = double.Parse(Request.Query["from"].ToString());
                double to = double.Parse(Request.Query["to"].ToString());

                DateTime baseDate = new DateTime(1970, 1, 1);
                DateTime fromDate = baseDate.AddMilliseconds(from);
                DateTime toDate = baseDate.AddMilliseconds(to);
                long count = Program.WexflowEngine.GetHistoryEntriesCount(keyword, fromDate, toDate);

                var countStr = JsonConvert.SerializeObject(count);
                var countBytes = Encoding.UTF8.GetBytes(countStr);

                return new Response
                {
                    ContentType = "application/json",
                    Contents = s => s.Write(countBytes, 0, countBytes.Length)
                };

            });
        }

        /// <summary>
        /// Returns entries count by keyword and date filter.
        /// </summary>
        private void GetEntriesCountByDate()
        {
            Get(Root + "entriesCountByDate", args =>
            {
                string keyword = Request.Query["s"].ToString();
                double from = double.Parse(Request.Query["from"].ToString());
                double to = double.Parse(Request.Query["to"].ToString());

                DateTime baseDate = new DateTime(1970, 1, 1);
                DateTime fromDate = baseDate.AddMilliseconds(from);
                DateTime toDate = baseDate.AddMilliseconds(to);
                long count = Program.WexflowEngine.GetEntriesCount(keyword, fromDate, toDate);

                var countStr = JsonConvert.SerializeObject(count);
                var countBytes = Encoding.UTF8.GetBytes(countStr);

                return new Response
                {
                    ContentType = "application/json",
                    Contents = s => s.Write(countBytes, 0, countBytes.Length)
                };

            });
        }

        /// <summary>
        /// Returns history entry min date.
        /// </summary>
        private void GetHistoryEntryStatusDateMin()
        {
            Get(Root + "historyEntryStatusDateMin", args =>
            {
                var date = Program.WexflowEngine.GetHistoryEntryStatusDateMin();
                DateTime baseDate = new DateTime(1970, 1, 1);
                double d = (date - baseDate).TotalMilliseconds;

                var dStr = JsonConvert.SerializeObject(d);
                var dBytes = Encoding.UTF8.GetBytes(dStr);

                return new Response
                {
                    ContentType = "application/json",
                    Contents = s => s.Write(dBytes, 0, dBytes.Length)
                };
            });
        }

        /// <summary>
        /// Returns history entry max date.
        /// </summary>
        private void GetHistoryEntryStatusDateMax()
        {
            Get(Root + "historyEntryStatusDateMax", args =>
            {
                var date = Program.WexflowEngine.GetHistoryEntryStatusDateMax();
                DateTime baseDate = new DateTime(1970, 1, 1);
                double d = (date - baseDate).TotalMilliseconds;

                var dStr = JsonConvert.SerializeObject(d);
                var dBytes = Encoding.UTF8.GetBytes(dStr);

                return new Response
                {
                    ContentType = "application/json",
                    Contents = s => s.Write(dBytes, 0, dBytes.Length)
                };
            });
        }

        /// <summary>
        /// Returns entry min date.
        /// </summary>
        private void GetEntryStatusDateMin()
        {
            Get(Root + "entryStatusDateMin", args =>
            {
                var date = Program.WexflowEngine.GetEntryStatusDateMin();
                DateTime baseDate = new DateTime(1970, 1, 1);
                double d = (date - baseDate).TotalMilliseconds;

                var dStr = JsonConvert.SerializeObject(d);
                var dBytes = Encoding.UTF8.GetBytes(dStr);

                return new Response
                {
                    ContentType = "application/json",
                    Contents = s => s.Write(dBytes, 0, dBytes.Length)
                };
            });
        }

        /// <summary>
        /// Returns entry max date.
        /// </summary>
        private void GetEntryStatusDateMax()
        {
            Get(Root + "entryStatusDateMax", args =>
            {
                var date = Program.WexflowEngine.GetEntryStatusDateMax();
                DateTime baseDate = new DateTime(1970, 1, 1);
                double d = (date - baseDate).TotalMilliseconds;

                var dStr = JsonConvert.SerializeObject(d);
                var dBytes = Encoding.UTF8.GetBytes(dStr);

                return new Response
                {
                    ContentType = "application/json",
                    Contents = s => s.Write(dBytes, 0, dBytes.Length)
                };
            });
        }
    }
}