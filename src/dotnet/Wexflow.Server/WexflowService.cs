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
using System.Xml.Schema;
using System.Threading;
using Newtonsoft.Json;

namespace Wexflow.Server
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class WexflowService : IWexflowService
    {
        //[WebInvoke(Method = "GET",
        //    ResponseFormat = WebMessageFormat.Json,
        //    UriTemplate = "workflows")]
        //public WorkflowInfo[] GetWorkflows()
        //{
        //    return WexflowWindowsService.WexflowEngine.Workflows.Select(wf => new WorkflowInfo(wf.DbId, wf.Id, wf.Name,
        //            (LaunchType)wf.LaunchType, wf.IsEnabled, wf.IsApproval, wf.IsWaitingForApproval, wf.Description, wf.IsRunning, wf.IsPaused,
        //            wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression,
        //            wf.IsExecutionGraphEmpty
        //            , wf.LocalVariables.Select(v => new Core.Service.Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()
        //            ))
        //        .ToArray();
        //}

        //[WebInvoke(Method = "GET",
        //    ResponseFormat = WebMessageFormat.Json,
        //    UriTemplate = "approvalWorkflows")]
        //public WorkflowInfo[] GetApprovalWorkflows()
        //{
        //    return WexflowWindowsService.WexflowEngine.Workflows
        //            .Where(w => w.IsApproval)
        //            .Select(wf => new WorkflowInfo(wf.DbId, wf.Id, wf.Name,
        //                        (LaunchType)wf.LaunchType, wf.IsEnabled, wf.IsApproval, wf.IsWaitingForApproval, wf.Description, wf.IsRunning, wf.IsPaused,
        //                        wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression,
        //                        wf.IsExecutionGraphEmpty
        //                        , wf.LocalVariables.Select(v => new Core.Service.Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()
        //            ))
        //        .ToArray();
        //}

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "search?s={keyword}&u={username}&p={password}")]
        public WorkflowInfo[] Search(string keyword, string username, string password)
        {
            var keywordToUpper = keyword.ToUpper();

            var user = WexflowWindowsService.WexflowEngine.GetUser(username);
            if (user.Password.Equals(password))
            {
                if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                {
                    return WexflowWindowsService.WexflowEngine.Workflows
                        .ToList()
                        .Where(wf =>
                            wf.Name.ToUpper().Contains(keywordToUpper) || wf.Description.ToUpper().Contains(keywordToUpper))
                        .Select(wf => new WorkflowInfo(wf.DbId, wf.Id, wf.Name,
                            (LaunchType)wf.LaunchType, wf.IsEnabled, wf.IsApproval, wf.IsWaitingForApproval, wf.Description, wf.IsRunning, wf.IsPaused,
                            wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression,
                            wf.IsExecutionGraphEmpty
                           , wf.LocalVariables.Select(v => new Core.Service.Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()))
                        .ToArray();
                }
                else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                {
                    return WexflowWindowsService.WexflowEngine.GetUserWorkflows(user.Id)
                                            .ToList()
                                            .Where(wf =>
                                                wf.Name.ToUpper().Contains(keywordToUpper) || wf.Description.ToUpper().Contains(keywordToUpper))
                                            .Select(wf => new WorkflowInfo(wf.DbId, wf.Id, wf.Name,
                                                (LaunchType)wf.LaunchType, wf.IsEnabled, wf.IsApproval, wf.IsWaitingForApproval, wf.Description, wf.IsRunning, wf.IsPaused,
                                                wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression,
                                                wf.IsExecutionGraphEmpty
                                               , wf.LocalVariables.Select(v => new Core.Service.Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()))
                                            .ToArray();
                }
            }

            return new WorkflowInfo[] { };
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "searchApprovalWorkflows?s={keyword}&u={username}&p={password}")]
        public WorkflowInfo[] SearchApprovalWorkflows(string keyword, string username, string password)
        {
            var keywordToUpper = keyword.ToUpper();

            var user = WexflowWindowsService.WexflowEngine.GetUser(username);
            if (user.Password.Equals(password))
            {
                if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                {
                    return WexflowWindowsService.WexflowEngine.Workflows
                        .ToList()
                        .Where(wf =>
                            wf.IsApproval &&
                            (wf.Name.ToUpper().Contains(keywordToUpper) || wf.Description.ToUpper().Contains(keywordToUpper)))
                        .Select(wf => new WorkflowInfo(wf.DbId, wf.Id, wf.Name,
                            (LaunchType)wf.LaunchType, wf.IsEnabled, wf.IsApproval, wf.IsWaitingForApproval, wf.Description, wf.IsRunning, wf.IsPaused,
                            wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression,
                            wf.IsExecutionGraphEmpty
                           , wf.LocalVariables.Select(v => new Core.Service.Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()))
                        .ToArray();
                }
                else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                {
                    return WexflowWindowsService.WexflowEngine.GetUserWorkflows(user.Id)
                                            .ToList()
                                            .Where(wf =>
                                                wf.IsApproval &&
                                                (wf.Name.ToUpper().Contains(keywordToUpper) || wf.Description.ToUpper().Contains(keywordToUpper)))
                                            .Select(wf => new WorkflowInfo(wf.DbId, wf.Id, wf.Name,
                                                (LaunchType)wf.LaunchType, wf.IsEnabled, wf.IsApproval, wf.IsWaitingForApproval, wf.Description, wf.IsRunning, wf.IsPaused,
                                                wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression,
                                                wf.IsExecutionGraphEmpty
                                               , wf.LocalVariables.Select(v => new Core.Service.Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()))
                                            .ToArray();
                }
            }

            return new WorkflowInfo[] { };

        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "start?w={id}&u={username}&p={password}")]
        public void StartWorkflow(string id, string username, string password)
        {
            int workflowId = int.Parse(id);
            var user = WexflowWindowsService.WexflowEngine.GetUser(username);
            if (user.Password.Equals(password))
            {
                if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                {
                    WexflowWindowsService.WexflowEngine.StartWorkflow(workflowId);
                }
                else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                {
                    var workflowDbId = WexflowWindowsService.WexflowEngine.Workflows.First(w => w.Id == workflowId).DbId;
                    var check = WexflowWindowsService.WexflowEngine.CheckUserWorkflow(user.Id, workflowDbId);
                    if (check)
                    {
                        WexflowWindowsService.WexflowEngine.StartWorkflow(workflowId);
                    }
                }
            }
        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "stop?w={id}&u={username}&p={password}")]
        public bool StopWorkflow(string id, string username, string password)
        {
            int workflowId = int.Parse(id);
            var user = WexflowWindowsService.WexflowEngine.GetUser(username);
            if (user.Password.Equals(password))
            {
                if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                {
                    return WexflowWindowsService.WexflowEngine.StopWorkflow(workflowId);
                }
                else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                {
                    var workflowDbId = WexflowWindowsService.WexflowEngine.Workflows.First(w => w.Id == workflowId).DbId;
                    var check = WexflowWindowsService.WexflowEngine.CheckUserWorkflow(user.Id, workflowDbId);
                    if (check)
                    {
                        return WexflowWindowsService.WexflowEngine.StopWorkflow(workflowId);
                    }
                }
            }

            return false;
        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "suspend?w={id}&u={username}&p={password}")]
        public bool SuspendWorkflow(string id, string username, string password)
        {
            int workflowId = int.Parse(id);
            var user = WexflowWindowsService.WexflowEngine.GetUser(username);
            if (user.Password.Equals(password))
            {
                if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                {
                    return WexflowWindowsService.WexflowEngine.SuspendWorkflow(workflowId);
                }
                else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                {
                    var workflowDbId = WexflowWindowsService.WexflowEngine.Workflows.First(w => w.Id == workflowId).DbId;
                    var check = WexflowWindowsService.WexflowEngine.CheckUserWorkflow(user.Id, workflowDbId);
                    if (check)
                    {
                        return WexflowWindowsService.WexflowEngine.SuspendWorkflow(workflowId);
                    }
                }
            }

            return false;
        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "resume?w={id}&u={username}&p={password}")]
        public void ResumeWorkflow(string id, string username, string password)
        {
            int workflowId = int.Parse(id);
            var user = WexflowWindowsService.WexflowEngine.GetUser(username);
            if (user.Password.Equals(password))
            {
                if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                {
                    WexflowWindowsService.WexflowEngine.ResumeWorkflow(workflowId);
                }
                else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                {
                    var workflowDbId = WexflowWindowsService.WexflowEngine.Workflows.First(w => w.Id == workflowId).DbId;
                    var check = WexflowWindowsService.WexflowEngine.CheckUserWorkflow(user.Id, workflowDbId);
                    if (check)
                    {
                        WexflowWindowsService.WexflowEngine.ResumeWorkflow(workflowId);
                    }
                }
            }
        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "approve?w={id}&u={username}&p={password}")]
        public bool ApproveWorkflow(string id, string username, string password)
        {
            int workflowId = int.Parse(id);
            var user = WexflowWindowsService.WexflowEngine.GetUser(username);
            if (user.Password.Equals(password))
            {
                if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                {
                    return WexflowWindowsService.WexflowEngine.ApproveWorkflow(workflowId);
                }
                else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                {
                    var workflowDbId = WexflowWindowsService.WexflowEngine.Workflows.First(w => w.Id == workflowId).DbId;
                    var check = WexflowWindowsService.WexflowEngine.CheckUserWorkflow(user.Id, workflowDbId);
                    if (check)
                    {
                        return WexflowWindowsService.WexflowEngine.ApproveWorkflow(workflowId);
                    }
                }
            }

            return false;
        }

        [WebInvoke(Method = "POST",
           ResponseFormat = WebMessageFormat.Json,
           UriTemplate = "disapprove?w={id}&u={username}&p={password}")]
        public bool DisapproveWorkflow(string id, string username, string password)
        {
            int workflowId = int.Parse(id);
            var user = WexflowWindowsService.WexflowEngine.GetUser(username);
            if (user.Password.Equals(password))
            {
                if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                {
                    return WexflowWindowsService.WexflowEngine.DisapproveWorkflow(workflowId);
                }
                else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                {
                    var workflowDbId = WexflowWindowsService.WexflowEngine.Workflows.First(w => w.Id == workflowId).DbId;
                    var check = WexflowWindowsService.WexflowEngine.CheckUserWorkflow(user.Id, workflowDbId);
                    if (check)
                    {
                        return WexflowWindowsService.WexflowEngine.DisapproveWorkflow(workflowId);
                    }
                }
            }

            return false;
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "workflow?u={username}&p={password}&w={id}")]
        public WorkflowInfo GetWorkflow(string username, string password, int id)
        {
            WorkflowInfo wi = null;
            var wf = WexflowWindowsService.WexflowEngine.GetWorkflow(id);
            if (wf != null)
            {
                wi = new WorkflowInfo(wf.DbId, wf.Id, wf.Name, (LaunchType)wf.LaunchType, wf.IsEnabled, wf.IsApproval, wf.IsWaitingForApproval, wf.Description,
                    wf.IsRunning, wf.IsPaused, wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression
                    , wf.IsExecutionGraphEmpty
                    , wf.LocalVariables.Select(v => new Core.Service.Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()
                    );

                var user = WexflowWindowsService.WexflowEngine.GetUser(username);

                if (user.Password.Equals(password))
                {
                    if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                    {
                        return wi;
                    }
                    else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                    {
                        var check = WexflowWindowsService.WexflowEngine.CheckUserWorkflow(user.Id, wf.DbId);
                        if (check)
                        {
                            return wi;
                        }
                    }
                }
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

                foreach (var task in wf.Tasks)
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
                return new[] { "TasksNames.json is not valid." };
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
                return new[] { "TasksSettings.json is not valid." };
            }
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
            UriTemplate = "isXmlWorkflowValid")]
        public bool IsXmlWorkflowValid(Stream streamdata)
        {
            try
            {
                StreamReader reader = new StreamReader(streamdata);
                string xml = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();

                xml = CleanupXml(xml);

                var schemas = new XmlSchemaSet();
                schemas.Add("urn:wexflow-schema", WexflowWindowsService.WexflowEngine.XsdPath);

                var xdoc = XDocument.Parse(xml);
                string msg = string.Empty;
                xdoc.Validate(schemas, (o, e) =>
                {
                    msg += e.Message + Environment.NewLine;
                });

                if (!string.IsNullOrEmpty(msg))
                {
                    return false;
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
            UriTemplate = "saveXml")]
        public bool SaveXmlWorkflow(Stream streamdata)
        {
            try
            {
                StreamReader reader = new StreamReader(streamdata);
                string json = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();

                JObject o = JObject.Parse(json);
                int workflowId = int.Parse((string)o.SelectToken("workflowId"));
                string username = o.Value<string>("username");
                string password = o.Value<string>("password");
                string xml = (string)o.SelectToken("xml");
                xml = CleanupXml(xml);

                var user = WexflowWindowsService.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                    {
                        WexflowWindowsService.WexflowEngine.SaveWorkflow(user.Id, user.UserProfile, xml);
                        return true;
                    }
                    else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                    {
                        var workflowDbId = WexflowWindowsService.WexflowEngine.Workflows.First(w => w.Id == workflowId).DbId;
                        var check = WexflowWindowsService.WexflowEngine.CheckUserWorkflow(user.Id, workflowDbId);
                        if (check)
                        {
                            WexflowWindowsService.WexflowEngine.SaveWorkflow(user.Id, user.UserProfile, xml);
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        private Core.Workflow GetWorkflowRecursive(int workflowId)
        {
            var wf = WexflowWindowsService.WexflowEngine.GetWorkflow(workflowId);
            if (wf != null)
            {
                return wf;
            }
            else
            {
                Thread.Sleep(500);
                return GetWorkflowRecursive(workflowId);
            }
        }

        private string CleanupXml(string xml)
        {
            var trimChars = new char[] { '\r', '\n', '"', '\'' };
            return xml
                .TrimStart(trimChars)
                .TrimEnd(trimChars)
                .Replace("\\r", string.Empty)
                .Replace("\\n", string.Empty)
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\");
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
                var isNew = (bool)wi.SelectToken("IsNew");

                var username = o.Value<string>("Username");
                var password = o.Value<string>("Password");

                var user = WexflowWindowsService.WexflowEngine.GetUser(username);

                if (!user.Password.Equals(password))
                {
                    return false;
                }

                if (user.UserProfile == Core.Db.UserProfile.Restricted)
                {
                    return false;
                }

                if (user.UserProfile == Core.Db.UserProfile.Administrator && !isNew)
                {
                    var id = o.Value<int>("Id");
                    var workflowDbId = WexflowWindowsService.WexflowEngine.Workflows.First(w => w.Id == id).DbId;
                    var check = WexflowWindowsService.WexflowEngine.CheckUserWorkflow(user.Id, workflowDbId);
                    if (!check)
                    {
                        return false;
                    }
                }


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
                    bool isWorkflowApproval = (bool)wi.SelectToken("IsApproval");
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
                                , new XAttribute("name", "approval")
                                , new XAttribute("value", isWorkflowApproval.ToString().ToLower()))
                        //, new XElement(xn + "Setting"
                        //    , new XAttribute("name", "period")
                        //    , new XAttribute("value", workflowPeriod.ToString(@"dd\.hh\:mm\:ss")))
                        //, new XElement(xn + "Setting"
                        //    , new XAttribute("name", "cronExpression")
                        //    , new XAttribute("value", cronExpression))
                        )
                        , xLocalVariables
                        , xtasks
                    );

                    if (workflowLaunchType == LaunchType.Periodic)
                    {
                        xwf.Element(xn + "Settings").Add(
                             new XElement(xn + "Setting"
                                , new XAttribute("name", "period")
                                , new XAttribute("value", workflowPeriod.ToString(@"dd\.hh\:mm\:ss")))
                            );
                    }

                    if (workflowLaunchType == LaunchType.Cron)
                    {
                        xwf.Element(xn + "Settings").Add(
                             new XElement(xn + "Setting"
                                , new XAttribute("name", "cronExpression")
                                , new XAttribute("value", cronExpression))
                            );
                    }

                    xdoc.Add(xwf);

                    var path = (string)wi.SelectToken("Path");
                    WexflowWindowsService.WexflowEngine.SaveWorkflow(user.Id, user.UserProfile, xdoc.ToString());
                    //if (user.UserProfile == Core.Db.UserProfile.Administrator)
                    //{
                    //    WexflowWindowsService.WexflowEngine.InsertUserWorkflowRelation(user.Id, dbId);
                    //}
                }
                else
                {
                    XNamespace xn = "urn:wexflow-schema";

                    int id = int.Parse((string)o.SelectToken("Id"));
                    var wf = WexflowWindowsService.WexflowEngine.GetWorkflow(id);
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
                        bool isWorkflowApproval = (bool)(wi.SelectToken("IsApproval") ?? false);
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

                        var xwfApproval = xdoc.Root.XPathSelectElement("wf:Settings/wf:Setting[@name='approval']",
                            wf.XmlNamespaceManager);
                        if (xwfApproval == null)
                        {
                            xdoc.Root.XPathSelectElement("wf:Settings", wf.XmlNamespaceManager)
                                .Add(new XElement(xn + "Setting"
                                        , new XAttribute("name", "approval")
                                        , new XAttribute("value", isWorkflowApproval.ToString().ToLower())));
                        }
                        else
                        {
                            xwfApproval.Attribute("value").Value = isWorkflowApproval.ToString().ToLower();
                        }

                        var xwfPeriod = xdoc.Root.XPathSelectElement("wf:Settings/wf:Setting[@name='period']",
                            wf.XmlNamespaceManager);
                        if (workflowLaunchType == LaunchType.Periodic)
                        {
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
                        }
                        //else
                        //{
                        //    if (xwfPeriod != null)
                        //    {
                        //        xwfPeriod.Remove();
                        //    }
                        //}

                        var xwfCronExpression = xdoc.Root.XPathSelectElement(
                            "wf:Settings/wf:Setting[@name='cronExpression']",
                            wf.XmlNamespaceManager);

                        if (workflowLaunchType == LaunchType.Cron)
                        {
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
                        }
                        //else
                        //{
                        //    if(xwfCronExpression != null)
                        //    {
                        //        xwfCronExpression.Remove();
                        //    }
                        //}

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

                        // Tasks
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

                        //xdoc.Save(wf.WorkflowFilePath);
                        WexflowWindowsService.WexflowEngine.SaveWorkflow(user.Id, user.UserProfile, xdoc.ToString());
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
            UriTemplate = "delete?w={id}&u={username}&p={password}")]
        public bool DeleteWorkflow(string id, string username, string password)
        {
            try
            {
                var wf = WexflowWindowsService.WexflowEngine.GetWorkflow(int.Parse(id));
                if (wf != null)
                {
                    var user = WexflowWindowsService.WexflowEngine.GetUser(username);

                    if (user.Password.Equals(password))
                    {
                        if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                        {
                            WexflowWindowsService.WexflowEngine.DeleteWorkflow(wf.DbId);
                            return true;
                        }
                        else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                        {
                            var check = WexflowWindowsService.WexflowEngine.CheckUserWorkflow(user.Id, wf.DbId);
                            if (check)
                            {
                                WexflowWindowsService.WexflowEngine.DeleteWorkflow(wf.DbId);
                                return true;
                            }
                        }
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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
                    var task = wf.Tasks.FirstOrDefault(t => t.Id == node.Id);
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
                DisapprovedCount = statusCount.DisapprovedCount,
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
                    LaunchType = (LaunchType)((int)e.LaunchType),
                    Description = e.Description,
                    Status = (Core.Service.Contracts.Status)((int)e.Status)
                }).ToArray();
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "user?qu={qusername}&qp={qpassword}&username={username}")]
        public User GetUser(string qusername, string qpassword, string username)
        {
            var othuser = WexflowWindowsService.WexflowEngine.GetUser(qusername);

            if (othuser.Password.Equals(qpassword))
            {

                var user = WexflowWindowsService.WexflowEngine.GetUser(username);
                string dateTimeFormat = ConfigurationManager.AppSettings["DateTimeFormat"];

                if (user != null)
                {
                    return new User
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Password = user.Password,
                        UserProfile = (UserProfile)((int)user.UserProfile),
                        Email = user.Email,
                        CreatedOn = user.CreatedOn.ToString(dateTimeFormat),
                        ModifiedOn = user.ModifiedOn.ToString(dateTimeFormat)
                    };
                }
            }

            return null;
        }

        //[WebInvoke(Method = "GET",
        //    ResponseFormat = WebMessageFormat.Json,
        //    UriTemplate = "users")]
        //public User[] GetUsers()
        //{
        //    var users = WexflowWindowsService.WexflowEngine.GetUsers();
        //    //DateTime baseDate = new DateTime(1970, 1, 1);
        //    string dateTimeFormat = ConfigurationManager.AppSettings["DateTimeFormat"];

        //    return users.Select(u => new User
        //    {
        //        Id = u.Id,
        //        Username = u.Username,
        //        Password = u.Password,
        //        UserProfile = (UserProfile)((int)u.UserProfile),
        //        Email = u.Email,
        //        //CreatedOn = (u.CreatedOn - baseDate).TotalMilliseconds,
        //        CreatedOn = u.CreatedOn.ToString(dateTimeFormat),
        //        //ModifiedOn = (u.ModifiedOn - baseDate).TotalMilliseconds
        //        ModifiedOn = u.ModifiedOn.ToString(dateTimeFormat)
        //    }).ToArray();
        //}

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "searchUsers?qu={qusername}&qp={qpassword}&keyword={keyword}&uo={uo}")]
        public User[] SearchUsers(string qusername, string qpassword, string keyword, int uo)
        {
            var user = WexflowWindowsService.WexflowEngine.GetUser(qusername);

            if (user.Password.Equals(qpassword) && (user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
            {
                var users = WexflowWindowsService.WexflowEngine.GetUsers(keyword, (UserOrderBy)uo);
                string dateTimeFormat = ConfigurationManager.AppSettings["DateTimeFormat"];

                return users.Select(u => new User
                {
                    Id = u.Id,
                    Username = u.Username,
                    Password = u.Password,
                    UserProfile = (UserProfile)((int)u.UserProfile),
                    Email = u.Email,
                    CreatedOn = u.CreatedOn.ToString(dateTimeFormat),
                    ModifiedOn = u.ModifiedOn.ToString(dateTimeFormat)
                }).ToArray();
            }

            return new User[] { };
        }

        [WebInvoke(Method = "GET",
           ResponseFormat = WebMessageFormat.Json,
           UriTemplate = "searchAdmins?qu={qusername}&qp={qpassword}&keyword={keyword}&uo={uo}")]
        public User[] SearchAdministrators(string qusername, string qpassword, string keyword, int uo)
        {
            var user = WexflowWindowsService.WexflowEngine.GetUser(qusername);

            if (user.Password.Equals(qpassword) && (user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
            {
                var users = WexflowWindowsService.WexflowEngine.GetAdministrators(keyword, (UserOrderBy)uo);
                string dateTimeFormat = ConfigurationManager.AppSettings["DateTimeFormat"];

                return users.Select(u => new User
                {
                    Id = u.Id,
                    Username = u.Username,
                    Password = u.Password,
                    UserProfile = (UserProfile)((int)u.UserProfile),
                    Email = u.Email,
                    CreatedOn = u.CreatedOn.ToString(dateTimeFormat),
                    ModifiedOn = u.ModifiedOn.ToString(dateTimeFormat)
                }).ToArray();
            }

            return new User[] { };
        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "saveUserWorkflows")]
        public bool SaveUserWorkflows(Stream streamdata)
        {
            try
            {
                StreamReader reader = new StreamReader(streamdata);
                string json = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();

                JObject o = JObject.Parse(json);
                var qusername = o.Value<string>("QUsername");
                var qpassword = o.Value<string>("QPassword");

                var user = WexflowWindowsService.WexflowEngine.GetUser(qusername);

                if (user.Password.Equals(qpassword) && user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                {
                    int userId = o.Value<int>("UserId");
                    JArray jArray = o.Value<JArray>("UserWorkflows");
                    WexflowWindowsService.WexflowEngine.DeleteUserWorkflowRelations(userId);
                    foreach (JObject item in jArray)
                    {
                        var workflowId = item.Value<int>("WorkflowId");
                        WexflowWindowsService.WexflowEngine.InsertUserWorkflowRelation(userId, workflowId);
                    }

                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured while saving workflow relations: {0}", e);
                return false;
            }

        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "userWorkflows?qu={qusername}&qp={qpassword}&u={userId}")]
        public WorkflowInfo[] GetUserWorkflows(string qusername, string qpassword, int userId)
        {
            try
            {
                var user = WexflowWindowsService.WexflowEngine.GetUser(qusername);

                if (user.Password.Equals(qpassword) && user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                {
                    var workflows = WexflowWindowsService.WexflowEngine.GetUserWorkflows(userId);
                    var res = workflows
                        .ToList()
                        .Select(wf => new WorkflowInfo(wf.DbId, wf.Id, wf.Name,
                        (LaunchType)wf.LaunchType, wf.IsEnabled, wf.IsApproval, wf.IsWaitingForApproval, wf.Description, wf.IsRunning, wf.IsPaused,
                        wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression,
                        wf.IsExecutionGraphEmpty
                       , wf.LocalVariables.Select(v => new Core.Service.Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()))
                        .ToArray();
                    return res;
                }

                return new WorkflowInfo[] { };
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured while retrieving user workflows: ", e);
                return new WorkflowInfo[] { };
            }
        }

        //[WebInvoke(Method = "GET",
        //    ResponseFormat = WebMessageFormat.Json,
        //    UriTemplate = "password?qu={qusername}&qp={qpassword}&u={username}")]
        //public string GetPassword(string qusername, string qpassword, string username)
        //{
        //    var user = WexflowWindowsService.WexflowEngine.GetUser(qusername);
        //    if (user.Password.Equals(qpassword))
        //    {
        //        return WexflowWindowsService.WexflowEngine.GetPassword(username);
        //    }

        //    return string.Empty;
        //}

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "insertUser?qu={qusername}&qp={qpassword}&username={username}&password={password}&up={userProfile}&email={email}")]
        public bool InsertUser(string qusername, string qpassword, string username, string password, int userProfile, string email)
        {
            try
            {
                var user = WexflowWindowsService.WexflowEngine.GetUser(qusername);
                if (user.Password.Equals(qpassword) && user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                {
                    WexflowWindowsService.WexflowEngine.InsertUser(username, password, (Core.Db.UserProfile)userProfile, email);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "updateUser?qu={qusername}&qp={qpassword}&userId={userId}&username={username}&password={password}&up={userProfile}&email={email}")]
        public bool UpdateUser(string qusername, string qpassword, int userId, string username, string password, int userProfile, string email)
        {
            try
            {
                var user = WexflowWindowsService.WexflowEngine.GetUser(qusername);
                if (user.Password.Equals(qpassword) && (user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                {
                    WexflowWindowsService.WexflowEngine.UpdateUser(userId, username, password, (Core.Db.UserProfile)userProfile, email);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "updateUsernameAndEmailAndUserProfile?qu={qusername}&qp={qpassword}&userId={userId}&username={username}&email={email}&up={up}")]
        public bool UpdateUsernameAndEmailAndUserProfile(string qusername, string qpassword, int userId, string username, string email, int up)
        {
            try
            {
                var user = WexflowWindowsService.WexflowEngine.GetUser(qusername);
                if (user.Password.Equals(qpassword) && (user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                {
                    WexflowWindowsService.WexflowEngine.UpdateUsernameAndEmailAndUserProfile(userId, username, email, up);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "deleteUser?qu={qusername}&qp={qpassword}&username={username}&password={password}")]
        public bool DeleteUser(string qusername, string qpassword, string username, string password)
        {
            try
            {
                var user = WexflowWindowsService.WexflowEngine.GetUser(qusername);
                if (user.Password.Equals(qpassword) && (user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                {
                    WexflowWindowsService.WexflowEngine.DeleteUser(username, password);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "resetPassword?u={username}")]
        public bool ResetPassword(string username)
        {
            try
            {
                var user = WexflowWindowsService.WexflowEngine.GetUser(username);

                if (user != null && !string.IsNullOrEmpty(user.Email))
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

                    Send(host, port, enableSsl, smtpUser, smtpPassword, user.Email, from, subject, body);

                    // Update password
                    WexflowWindowsService.WexflowEngine.UpdatePassword(username, newPasswordHash);
                }

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
                    LaunchType = (LaunchType)((int)e.LaunchType),
                    Description = e.Description,
                    Status = (Core.Service.Contracts.Status)((int)e.Status),
                    //StatusDate = (e.StatusDate - baseDate).TotalMilliseconds
                    StatusDate = e.StatusDate.ToString(ConfigurationManager.AppSettings["DateTimeFormat"])
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
                    LaunchType = (LaunchType)((int)e.LaunchType),
                    Description = e.Description,
                    Status = (Core.Service.Contracts.Status)((int)e.Status),
                    //StatusDate = (e.StatusDate - baseDate).TotalMilliseconds
                    StatusDate = e.StatusDate.ToString(ConfigurationManager.AppSettings["DateTimeFormat"])
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
                    LaunchType = (LaunchType)((int)e.LaunchType),
                    Description = e.Description,
                    Status = (Core.Service.Contracts.Status)((int)e.Status),
                    //StatusDate = (e.StatusDate - baseDate).TotalMilliseconds
                    StatusDate = e.StatusDate.ToString(ConfigurationManager.AppSettings["DateTimeFormat"])
                }).ToArray();
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate =
                "searchHistoryEntriesByPageOrderBy?s={keyword}&from={from}&to={to}&page={page}&entriesCount={entriesCount}&heo={heo}")]
        public HistoryEntry[] SearchHistoryEntriesByPageOrderBy(string keyword, double from, double to, int page,
            int entriesCount, int heo)
        {
            DateTime baseDate = new DateTime(1970, 1, 1, 0, 0, 0);
            DateTime fromDate = baseDate.AddMilliseconds(from);
            DateTime toDate = baseDate.AddMilliseconds(to);

            var entries = WexflowWindowsService.WexflowEngine.GetHistoryEntries(keyword, fromDate, toDate, page,
                entriesCount, (EntryOrderBy)heo);

            return entries.Select(e =>
                new HistoryEntry
                {
                    Id = e.Id,
                    WorkflowId = e.WorkflowId,
                    Name = e.Name,
                    LaunchType = (LaunchType)((int)e.LaunchType),
                    Description = e.Description,
                    Status = (Core.Service.Contracts.Status)((int)e.Status),
                    //StatusDate = e.StatusDate.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds
                    StatusDate = e.StatusDate.ToString(ConfigurationManager.AppSettings["DateTimeFormat"])
                }).ToArray();
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate =
                "searchEntriesByPageOrderBy?s={keyword}&from={from}&to={to}&page={page}&entriesCount={entriesCount}&heo={heo}")]
        public Entry[] SearchEntriesByPageOrderBy(string keyword, double from, double to, int page, int entriesCount, int heo)
        {
            DateTime baseDate = new DateTime(1970, 1, 1, 0, 0, 0);
            DateTime fromDate = baseDate.AddMilliseconds(from);
            DateTime toDate = baseDate.AddMilliseconds(to);

            var entries = WexflowWindowsService.WexflowEngine.GetEntries(keyword, fromDate, toDate, page, entriesCount, (EntryOrderBy)heo);

            var q = entries.Select(e =>
                new Entry
                {
                    Id = e.Id,
                    WorkflowId = e.WorkflowId,
                    Name = e.Name,
                    LaunchType = (LaunchType)((int)e.LaunchType),
                    Description = e.Description,
                    Status = (Core.Service.Contracts.Status)((int)e.Status),
                    //StatusDate = e.StatusDate.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds
                    StatusDate = e.StatusDate.ToString(ConfigurationManager.AppSettings["DateTimeFormat"])
                }).ToArray();

            return q;
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

        [WebInvoke(Method = "POST",
           ResponseFormat = WebMessageFormat.Json,
           UriTemplate = "deleteWorkflows")]
        public bool DeleteWorkflows(Stream streamdata)
        {
            try
            {
                StreamReader reader = new StreamReader(streamdata);
                string json = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();

                var o = JObject.Parse(json);
                var username = o.Value<string>("Username");
                var password = o.Value<string>("Password");
                var workflowDbIds = JsonConvert.DeserializeObject<int[]>(((JArray)o.SelectToken("WorkflowsToDelete")).ToString());

                var user = WexflowWindowsService.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                    {
                        var res = WexflowWindowsService.WexflowEngine.DeleteWorkflows(workflowDbIds);
                        return res;
                    }
                    else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                    {
                        var res = true;
                        foreach (var id in workflowDbIds)
                        {
                            var check = WexflowWindowsService.WexflowEngine.CheckUserWorkflow(user.Id, id);
                            if (check)
                            {
                                try
                                {
                                    WexflowWindowsService.WexflowEngine.DeleteWorkflow(id);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                    res &= false;
                                }

                            }
                        }
                        return res;
                    }
                }

                return false;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

    }
}
