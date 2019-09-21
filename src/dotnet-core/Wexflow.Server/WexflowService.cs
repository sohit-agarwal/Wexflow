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
using System.Threading;
using System.Xml.Linq;
using System.Xml.Schema;
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
            //
            // Index
            //
            Get("/", _ =>
            {
                return Response.AsRedirect(Root);
            });

            //
            // Doc
            //
            Doc();

            //
            // Dashboard
            //
            GetStatusCount();
            GetEntriesCountByDate();
            SearchEntriesByPageOrderBy();
            GetEntryStatusDateMin();
            GetEntryStatusDateMax();

            //
            // Manager
            //
            //GetWorkflows();
            Search();
            GetWorkflow();
            StartWorkflow();
            StopWorkflow();
            SuspendWorkflow();
            ResumeWorkflow();

            //
            // Approval
            //
            SearchApprovalWorkflows();
            ApproveWorkflow();
            DisapproveWorkflow();

            //
            // Designer
            // 
            GetTasks();
            GetWorkflowXml();
            GetTaskNames();
            GetSettings();
            GetTaskXml();
            IsWorkflowIdValid();
            IsCronExpressionValid();
            IsPeriodValid();
            IsXmlWorkflowValid();
            SaveXmlWorkflow();
            SaveWorkflow();
            DeleteWorkflow();
            DeleteWorkflows();
            GetExecutionGraph();

            //
            // History
            //
            GetHistoryEntriesCountByDate();
            SearchHistoryEntriesByPageOrderBy();
            GetHistoryEntryStatusDateMin();
            GetHistoryEntryStatusDateMax();

            //
            // Users
            //
            GetUser();
            //GetPassword();
            SearchUsers();
            InsertUser();
            UpdateUser();
            UpdateUsernameAndEmailAndUserProfile();
            DeleteUser();
            ResetPassword();

            //
            // Profiles
            //
            SearchAdministrators();
            GetUserWorkflows();
            SaveUserWorkflows();
        }

        private string DocH1(string title)
        {
            return "<h1>" + title + "</h1>";
        }

        private string DocH2(string title)
        {
            return "<h2>" + title + "</h2>";
        }

        private string DocGet(string name, string description)
        {
            return "<b>GET</b> " + Root + name + "<br/>" + description + "<br/><br/>";
        }

        private string DocPost(string name, string description)
        {
            return "<b>POST</b> " + Root + name + "<br/>" + description + "<br/><br/>";
        }

        private void Doc()
        {
            Get(Root, args =>
            {
                var doc =
                  DocH1("Wexflow server running on CoreCLR")
                + DocH2("Dashboard")
                + DocGet("statusCount", "Returns status count.")
                + DocGet("entriesCountByDate?s={keyword}&from={date}&to={date}", "Returns entries count by keyword and date filter.")
                + DocGet("searchEntriesByPageOrderBy?s={keyword}&from={date}&to={date}&page={page}&entriesCount={entriesCount}&heo={orderBy}", "Searches for entries.")
                + DocGet("entryStatusDateMin", "Returns entry min date.")
                + DocGet("entryStatusDateMax", "Returns entry max date.")
                + DocH2("Manager")
                //+ DocGet("workflows", "Returns the list of workflows.")
                + DocGet("search?s={keyword}&u={username}&p={password}", "Search for workflows.")
                + DocGet("searchApprovalWorkflows?s={keyword}&u={username}&p={password}", "Search for approval workflows.")
                + DocGet("workflow?u={username}&p={password}&w={id}", "Returns a workflow from its id.")
                + DocPost("start?w={id}&u={username}&p={password}", "Starts a workflow.")
                + DocPost("stop?w={id}&u={username}&p={password}", "Stops a workflow.")
                + DocPost("suspend?w={id}&u={username}&p={password}", "Suspends a workflow.")
                + DocPost("resume?w={id}&u={username}&p={password}", "Resumes a workflow.")
                + DocPost("approve?w={id}&u={username}&p={password}", "Approves a workflow.")
                + DocPost("disapprove?w={id}&u={username}&p={password}", "Disapproves a workflow.")
                + DocH2("Designer")
                + DocGet("tasks/{id}", "Returns workflow's tasks.")
                + DocGet("xml/{id}", "Returns a workflow as XML.")
                + DocGet("taskNames", "Returns task names.")
                + DocGet("settings/{taskName}", "Returns task settings.")
                + DocPost("taskToXml", "Returns a task as XML.")
                + DocGet("isWorkflowIdValid/{id}", "Checks if a workflow id is valid.")
                + DocGet("isCronExpressionValid?e={cronExpression}", "Checks if a cron expression is valid.")
                + DocGet("isPeriodValid/{period}", "Checks if a period is valid.")
                + DocPost("isXmlWorkflowValid", "Checks if the XML of a workflow is valid.")
                + DocPost("saveXml", "Saves a workflow from XML.")
                + DocPost("save", "Saves a workflow from JSON.")
                + DocPost("delete?w={id}&u={username}&p={password}", "Deletes a workflow.")
                + DocPost("delete?w={id}&u={username}&p={password}", "Deletes a workflow.")
                + DocPost("deleteWorkflows", "Deletes workflows.")
                + DocGet("graph/{id}", "Returns the execution graph of the workflow.")
                + DocH2("History")
                + DocGet("historyEntriesCountByDate?s={keyword}&from={date}&to={date}", "Returns history entries count by keyword and date filter.")
                + DocGet("searchHistoryEntriesByPageOrderBy?s={keyword}&from={date}&to={date}&page={page}&entriesCount={entriesCount}&heo={orderBy}", "Searches for history entries.")
                + DocGet("historyEntryStatusDateMin", "Returns history entry min date.")
                + DocGet("historyEntryStatusDateMax", "Returns history entry max date.")
                + DocH2("Users")
                + DocGet("user?qu={username}&qp={password}&username={username}", "Returns a user from his username.")
                //+ DocGet("password?qu={username}&qp={password}&u={username}", "Returns user's password (encrypted).")
                + DocGet("searchUsers?qu={username}&qp={password}&keyword={keyword}&uo={orderBy}", "Searches for users.")
                + DocPost("insertUser?qu={username}&qp={password}&username={username}&password={password}&up={userProfile}&email={email}", "Inserts a user.")
                + DocPost("updateUser?qu={username}&qp={password}&userId={userId}&username={username}&password={password}&up={userProfile}&email={email}", "Inserts a user.")
                + DocPost("updateUsernameAndEmailAndUserProfile?qu={username}&qp={password}&userId={userId}&username={username}&password={password}&up={userProfile}&email={email}", "Updates the username, the email and the user profile of a user.")
                + DocPost("deleteUser?qu={username}&qp={password}&username={username}&password={password}", "Deletes a user.")
                + DocPost("resetPassword?username={username}&email={email}", "Resets a password.")
                + DocH2("Profiles")
                + DocGet("searchAdmins?qu={username}&qp={password}&keyword={keyword}&uo={orderBy}", "Searches for administrators.")
                + DocGet("userWorkflows?qu={username}&qp={password}&u={userId}", "Returns user workflows.")
                + DocPost("saveUserWorkflows", "Saves user workflow relations.");

                var docBytes = Encoding.UTF8.GetBytes(doc);

                return new Response()
                {
                    ContentType = "text/html",
                    Contents = s => s.Write(docBytes, 0, docBytes.Length)
                };
            });
        }

        /// <summary>
        /// Returns the list of workflows.
        /// </summary>
        //private void GetWorkflows()
        //{
        //    Get(Root + "workflows", args =>
        //    {
        //        var workflows = Program.WexflowEngine.Workflows.Select(wf => new WorkflowInfo(wf.DbId, wf.Id, wf.Name,
        //                (LaunchType)wf.LaunchType, wf.IsEnabled, wf.IsApproval, wf.IsWaitingForApproval, wf.Description, wf.IsRunning, wf.IsPaused,
        //                wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression,
        //                wf.IsExecutionGraphEmpty
        //                , wf.LocalVariables.Select(v => new Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()
        //                ))
        //            .ToArray();
        //        var workflowsStr = JsonConvert.SerializeObject(workflows);
        //        var workflowsBytes = Encoding.UTF8.GetBytes(workflowsStr);

        //        return new Response()
        //        {
        //            ContentType = "application/json",
        //            Contents = s => s.Write(workflowsBytes, 0, workflowsBytes.Length)
        //        };
        //    });
        //}

        /// <summary>
        /// Search for workflows.
        /// </summary>
        private void Search()
        {
            Get(Root + "search", args =>
            {
                string keywordToUpper = Request.Query["s"].ToString().ToUpper();
                string username = Request.Query["u"].ToString();
                string password = Request.Query["p"].ToString();

                var workflows = new WorkflowInfo[] { };

                var user = Program.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                    {
                        workflows = Program.WexflowEngine.Workflows
                            .ToList()
                            .Where(wf =>
                                wf.Name.ToUpper().Contains(keywordToUpper) || wf.Description.ToUpper().Contains(keywordToUpper))
                            .Select(wf => new WorkflowInfo(wf.DbId, wf.Id, wf.Name,
                                (LaunchType)wf.LaunchType, wf.IsEnabled, wf.IsApproval, wf.IsWaitingForApproval, wf.Description, wf.IsRunning, wf.IsPaused,
                                wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression,
                                wf.IsExecutionGraphEmpty
                               , wf.LocalVariables.Select(v => new Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()))
                            .ToArray();
                    }
                    else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                    {
                        workflows = Program.WexflowEngine.GetUserWorkflows(user.GetId())
                                                .ToList()
                                                .Where(wf =>
                                                    wf.Name.ToUpper().Contains(keywordToUpper) || wf.Description.ToUpper().Contains(keywordToUpper))
                                                .Select(wf => new WorkflowInfo(wf.DbId, wf.Id, wf.Name,
                                                    (LaunchType)wf.LaunchType, wf.IsEnabled, wf.IsApproval, wf.IsWaitingForApproval, wf.Description, wf.IsRunning, wf.IsPaused,
                                                    wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression,
                                                    wf.IsExecutionGraphEmpty
                                                   , wf.LocalVariables.Select(v => new Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()))
                                                .ToArray();
                    }
                }

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
        /// Search for approval workflows.
        /// </summary>
        private void SearchApprovalWorkflows()
        {
            Get(Root + "searchApprovalWorkflows", args =>
            {
                string keywordToUpper = Request.Query["s"].ToString().ToUpper();
                string username = Request.Query["u"].ToString();
                string password = Request.Query["p"].ToString();

                var workflows = new WorkflowInfo[] { };

                var user = Program.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                    {
                        workflows = Program.WexflowEngine.Workflows
                            .ToList()
                            .Where(wf =>
                                wf.IsApproval &&
                                (wf.Name.ToUpper().Contains(keywordToUpper) || wf.Description.ToUpper().Contains(keywordToUpper)))
                            .Select(wf => new WorkflowInfo(wf.DbId, wf.Id, wf.Name,
                                (LaunchType)wf.LaunchType, wf.IsEnabled, wf.IsApproval, wf.IsWaitingForApproval, wf.Description, wf.IsRunning, wf.IsPaused,
                                wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression,
                                wf.IsExecutionGraphEmpty
                               , wf.LocalVariables.Select(v => new Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()))
                            .ToArray();
                    }
                    else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                    {
                        workflows = Program.WexflowEngine.GetUserWorkflows(user.GetId())
                                                .ToList()
                                                .Where(wf =>
                                                    wf.IsApproval &&
                                                    (wf.Name.ToUpper().Contains(keywordToUpper) || wf.Description.ToUpper().Contains(keywordToUpper)))
                                                .Select(wf => new WorkflowInfo(wf.DbId, wf.Id, wf.Name,
                                                    (LaunchType)wf.LaunchType, wf.IsEnabled, wf.IsApproval, wf.IsWaitingForApproval, wf.Description, wf.IsRunning, wf.IsPaused,
                                                    wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression,
                                                    wf.IsExecutionGraphEmpty
                                                   , wf.LocalVariables.Select(v => new Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()))
                                                .ToArray();
                    }
                }

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
            Get(Root + "workflow", args =>
            {
                var username = Request.Query["u"].ToString();
                var password = Request.Query["p"].ToString();
                var id = int.Parse(Request.Query["w"].ToString());

                Core.Workflow wf = Program.WexflowEngine.GetWorkflow(id);
                if (wf != null)
                {
                    var workflow = new WorkflowInfo(wf.DbId, wf.Id, wf.Name, (LaunchType)wf.LaunchType, wf.IsEnabled, wf.IsApproval, wf.IsWaitingForApproval, wf.Description,
                        wf.IsRunning, wf.IsPaused, wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression,
                        wf.IsExecutionGraphEmpty
                        , wf.LocalVariables.Select(v => new Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()
                        );

                    var user = Program.WexflowEngine.GetUser(username);

                    if (user.Password.Equals(password))
                    {
                        if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                        {
                            var workflowStr = JsonConvert.SerializeObject(workflow);
                            var workflowBytes = Encoding.UTF8.GetBytes(workflowStr);

                            return new Response()
                            {
                                ContentType = "application/json",
                                Contents = s => s.Write(workflowBytes, 0, workflowBytes.Length)
                            };
                        }
                        else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                        {
                            var check = Program.WexflowEngine.CheckUserWorkflow(user.GetId(), wf.DbId);
                            if (check)
                            {
                                var workflowStr = JsonConvert.SerializeObject(workflow);
                                var workflowBytes = Encoding.UTF8.GetBytes(workflowStr);

                                return new Response()
                                {
                                    ContentType = "application/json",
                                    Contents = s => s.Write(workflowBytes, 0, workflowBytes.Length)
                                };
                            }
                        }
                    }

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
            Post(Root + "start", args =>
            {
                int workflowId = int.Parse(Request.Query["w"].ToString());
                string username = Request.Query["u"].ToString();
                string password = Request.Query["p"].ToString();

                var user = Program.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                    {
                        Program.WexflowEngine.StartWorkflow(workflowId);
                    }
                    else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                    {
                        var workflowDbId = Program.WexflowEngine.Workflows.First(w => w.Id == workflowId).DbId;
                        var check = Program.WexflowEngine.CheckUserWorkflow(user.GetId(), workflowDbId);
                        if (check)
                        {
                            Program.WexflowEngine.StartWorkflow(workflowId);
                        }
                    }
                }

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
            Post(Root + "stop", args =>
            {
                var res = false;

                int workflowId = int.Parse(Request.Query["w"].ToString());
                string username = Request.Query["u"].ToString();
                string password = Request.Query["p"].ToString();

                var user = Program.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                    {
                        res = Program.WexflowEngine.StopWorkflow(workflowId);
                    }
                    else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                    {
                        var workflowDbId = Program.WexflowEngine.Workflows.First(w => w.Id == workflowId).DbId;
                        var check = Program.WexflowEngine.CheckUserWorkflow(user.GetId(), workflowDbId);
                        if (check)
                        {
                            res = Program.WexflowEngine.StopWorkflow(workflowId);
                        }
                    }
                }

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
            Post(Root + "suspend", args =>
            {
                bool res = false;

                int workflowId = int.Parse(Request.Query["w"].ToString());
                string username = Request.Query["u"].ToString();
                string password = Request.Query["p"].ToString();

                var user = Program.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                    {
                        res = Program.WexflowEngine.SuspendWorkflow(workflowId);
                    }
                    else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                    {
                        var workflowDbId = Program.WexflowEngine.Workflows.First(w => w.Id == workflowId).DbId;
                        var check = Program.WexflowEngine.CheckUserWorkflow(user.GetId(), workflowDbId);
                        if (check)
                        {
                            res = Program.WexflowEngine.SuspendWorkflow(workflowId);
                        }
                    }
                }

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
            Post(Root + "resume", args =>
            {
                int workflowId = int.Parse(Request.Query["w"].ToString());
                string username = Request.Query["u"].ToString();
                string password = Request.Query["p"].ToString();

                var user = Program.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                    {
                        Program.WexflowEngine.ResumeWorkflow(workflowId);
                    }
                    else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                    {
                        var workflowDbId = Program.WexflowEngine.Workflows.First(w => w.Id == workflowId).DbId;
                        var check = Program.WexflowEngine.CheckUserWorkflow(user.GetId(), workflowDbId);
                        if (check)
                        {
                            Program.WexflowEngine.ResumeWorkflow(workflowId);
                        }
                    }
                }

                return new Response
                {
                    ContentType = "application/json"
                };
            });
        }

        /// <summary>
        /// Approves a workflow.
        /// </summary>
        private void ApproveWorkflow()
        {
            Post(Root + "approve", args =>
            {
                bool res = false;

                int workflowId = int.Parse(Request.Query["w"].ToString());
                string username = Request.Query["u"].ToString();
                string password = Request.Query["p"].ToString();

                var user = Program.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                    {
                        res = Program.WexflowEngine.ApproveWorkflow(workflowId);
                    }
                    else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                    {
                        var workflowDbId = Program.WexflowEngine.Workflows.First(w => w.Id == workflowId).DbId;
                        var check = Program.WexflowEngine.CheckUserWorkflow(user.GetId(), workflowDbId);
                        if (check)
                        {
                            res = Program.WexflowEngine.ApproveWorkflow(workflowId);
                        }
                    }
                }

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
        /// Disapproves a workflow.
        /// </summary>
        private void DisapproveWorkflow()
        {
            Post(Root + "disapprove", args =>
            {
                bool res = false;

                int workflowId = int.Parse(Request.Query["w"].ToString());
                string username = Request.Query["u"].ToString();
                string password = Request.Query["p"].ToString();

                var user = Program.WexflowEngine.GetUser(username);
                if (user.Password.Equals(password))
                {
                    if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                    {
                        res = Program.WexflowEngine.DisapproveWorkflow(workflowId);
                    }
                    else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                    {
                        var workflowDbId = Program.WexflowEngine.Workflows.First(w => w.Id == workflowId).DbId;
                        var check = Program.WexflowEngine.CheckUserWorkflow(user.GetId(), workflowDbId);
                        if (check)
                        {
                            res = Program.WexflowEngine.DisapproveWorkflow(workflowId);
                        }
                    }
                }

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
                Core.Workflow wf = Program.WexflowEngine.GetWorkflow(args.id);
                if (wf != null)
                {
                    //var xmlStr = JsonConvert.SerializeObject(wf.XDoc.ToString());
                    var xmlStr = JsonConvert.SerializeObject(wf.Xml);
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
                    taskNames = new[] { "TasksNames.json is not valid." };
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
        /// Checks if the XML of a workflow is valid.
        /// </summary>
        private void IsXmlWorkflowValid()
        {
            Post(Root + "isXmlWorkflowValid", args =>
            {
                try
                {
                    var xml = RequestStream.FromStream(Request.Body).AsString();
                    xml = CleanupXml(xml);

                    var schemas = new XmlSchemaSet();
                    schemas.Add("urn:wexflow-schema", Program.WexflowEngine.XsdPath);

                    var xdoc = XDocument.Parse(xml);
                    string msg = string.Empty;
                    xdoc.Validate(schemas, (o, e) =>
                    {
                        msg += e.Message + Environment.NewLine;
                    });

                    if (!string.IsNullOrEmpty(msg))
                    {
                        var resFalseStr = JsonConvert.SerializeObject(false);
                        var resFalseBytes = Encoding.UTF8.GetBytes(resFalseStr);

                        return new Response()
                        {
                            ContentType = "application/json",
                            Contents = s => s.Write(resFalseBytes, 0, resFalseBytes.Length)
                        };
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
        /// Saves a workflow from XML.
        /// </summary>
        private void SaveXmlWorkflow()
        {
            Post(Root + "saveXml", args =>
            {
                try
                {
                    var json = RequestStream.FromStream(Request.Body).AsString();
                    var res = false;

                    JObject o = JObject.Parse(json);
                    int workflowId = int.Parse((string)o.SelectToken("workflowId"));
                    string username = o.Value<string>("username");
                    string password = o.Value<string>("password");
                    string xml = (string)o.SelectToken("xml");
                    xml = CleanupXml(xml);

                    var user = Program.WexflowEngine.GetUser(username);
                    if (user.Password.Equals(password))
                    {
                        if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                        {
                            var id = Program.WexflowEngine.SaveWorkflow(user.GetId(), user.UserProfile, xml);
                            if (id == "-1")
                            {
                                res = false;
                            }
                            else
                            {
                                res = true;
                            }
                        }
                        else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                        {
                            var workflowDbId = Program.WexflowEngine.Workflows.First(w => w.Id == workflowId).DbId;
                            var check = Program.WexflowEngine.CheckUserWorkflow(user.GetId(), workflowDbId);
                            if (check)
                            {
                                var id = Program.WexflowEngine.SaveWorkflow(user.GetId(), user.UserProfile, xml);
                                if (id == "-1")
                                {
                                    res = false;
                                }
                                else
                                {
                                    res = true;
                                }
                            }
                        }
                    }

                    var resStr = JsonConvert.SerializeObject(res);
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

        private Core.Workflow GetWorkflowRecursive(int workflowId)
        {
            var wf = Program.WexflowEngine.GetWorkflow(workflowId);
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

        /// <summary>
        /// Saves a workflow from json.
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

                    var username = o.Value<string>("Username");
                    var password = o.Value<string>("Password");

                    var user = Program.WexflowEngine.GetUser(username);

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
                        var workflowDbId = Program.WexflowEngine.Workflows.First(w => w.Id == id).DbId;
                        var check = Program.WexflowEngine.CheckUserWorkflow(user.GetId(), workflowDbId);
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

                        //var path = (string)wi.SelectToken("Path");
                        //xdoc.Save(path);
                        var id = Program.WexflowEngine.SaveWorkflow(user.GetId(), user.UserProfile, xdoc.ToString());
                        //if (user.UserProfile == Core.Db.UserProfile.Administrator)
                        //{
                        //    Program.WexflowEngine.InsertUserWorkflowRelation(user.GetId(), dbId);
                        //}
                        if (id == "-1")
                        {
                            var qFalseStr = JsonConvert.SerializeObject(false);
                            var qFalseBytes = Encoding.UTF8.GetBytes(qFalseStr);

                            return new Response()
                            {
                                ContentType = "application/json",
                                Contents = s => s.Write(qFalseBytes, 0, qFalseBytes.Length)
                            };
                        }
                    }
                    else
                    {
                        XNamespace xn = "urn:wexflow-schema";

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
                            var qid = Program.WexflowEngine.SaveWorkflow(user.GetId(), user.UserProfile, xdoc.ToString());
                            if (qid == "-1")
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
            Post(Root + "delete", args =>
            {
                try
                {
                    var res = false;

                    int workflowId = int.Parse(Request.Query["w"].ToString());
                    string username = Request.Query["u"].ToString();
                    string password = Request.Query["p"].ToString();
                    Core.Workflow wf = Program.WexflowEngine.GetWorkflow(workflowId);

                    if (wf != null)
                    {
                        var user = Program.WexflowEngine.GetUser(username);

                        if (user.Password.Equals(password))
                        {
                            if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                            {
                                Program.WexflowEngine.DeleteWorkflow(wf.DbId);
                                res = true;
                            }
                            else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                            {
                                var check = Program.WexflowEngine.CheckUserWorkflow(user.GetId(), wf.DbId);
                                if (check)
                                {
                                    Program.WexflowEngine.DeleteWorkflow(wf.DbId);
                                    res = true;
                                }
                            }
                        }
                    }

                    var resStr = JsonConvert.SerializeObject(res);
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
                Core.Workflow wf = Program.WexflowEngine.GetWorkflow(args.id);
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
                    DisapprovedCount = statusCount.DisapprovedCount,
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
                string qusername = Request.Query["qu"].ToString();
                string qpassword = Request.Query["qp"].ToString();
                string username = Request.Query["username"].ToString();

                var othuser = Program.WexflowEngine.GetUser(qusername);

                if (othuser.Password.Equals(qpassword))
                {
                    var user = Program.WexflowEngine.GetUser(username);
                    string dateTimeFormat = Program.Config["DateTimeFormat"];

                    if (user != null)
                    {
                        var u = new User
                        {
                            Id = user.GetId(),
                            Username = user.Username,
                            Password = user.Password,
                            UserProfile = (UserProfile)((int)user.UserProfile),
                            Email = user.Email,
                            CreatedOn = user.CreatedOn.ToString(dateTimeFormat),
                            ModifiedOn = user.ModifiedOn.ToString(dateTimeFormat)
                        };

                        var uStr = JsonConvert.SerializeObject(u);
                        var uBytes = Encoding.UTF8.GetBytes(uStr);

                        return new Response
                        {
                            ContentType = "application/json",
                            Contents = s => s.Write(uBytes, 0, uBytes.Length)
                        };

                    }
                }

                return new Response
                {
                    ContentType = "application/json"
                };

            });
        }

        /// <summary>
        /// Returns user's password (encrypted).
        /// </summary>
        //private void GetPassword()
        //{
        //    Get(Root + "password", args =>
        //    {
        //        string qusername = Request.Query["qu"].ToString();
        //        string qpassword = Request.Query["qp"].ToString();
        //        string username = Request.Query["u"].ToString();
        //        string pass = string.Empty;

        //        var user = Program.WexflowEngine.GetUser(qusername);
        //        if (user.Password.Equals(qpassword))
        //        {
        //            pass = Program.WexflowEngine.GetPassword(username);
        //        }

        //        var passStr = JsonConvert.SerializeObject(pass);
        //        var passBytes = Encoding.UTF8.GetBytes(passStr);

        //        return new Response
        //        {
        //            ContentType = "application/json",
        //            Contents = s => s.Write(passBytes, 0, passBytes.Length)
        //        };
        //    });
        //}

        /// <summary>
        /// Searches for users.
        /// </summary>
        private void SearchUsers()
        {
            Get(Root + "searchUsers", args =>
            {
                string qusername = Request.Query["qu"].ToString();
                string qpassword = Request.Query["qp"].ToString();
                string keyword = Request.Query["keyword"].ToString();
                int uo = int.Parse(Request.Query["uo"].ToString());

                var q = new User[] { };
                var user = Program.WexflowEngine.GetUser(qusername);

                if (user.Password.Equals(qpassword) && (user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                {
                    var users = Program.WexflowEngine.GetUsers(keyword, (UserOrderBy)uo);

                    string dateTimeFormat = Program.Config["DateTimeFormat"];

                    q = users.Select(u => new User
                    {
                        Id = u.GetId(),
                        Username = u.Username,
                        Password = u.Password,
                        UserProfile = (UserProfile)((int)u.UserProfile),
                        Email = u.Email,
                        CreatedOn = u.CreatedOn.ToString(dateTimeFormat),
                        ModifiedOn = u.ModifiedOn.ToString(dateTimeFormat)
                    }).ToArray();
                }

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
        /// Searches for administrators.
        /// </summary>
        private void SearchAdministrators()
        {
            Get(Root + "searchAdmins", args =>
            {
                string qusername = Request.Query["qu"].ToString();
                string qpassword = Request.Query["qp"].ToString();
                string keyword = Request.Query["keyword"].ToString();
                int uo = int.Parse(Request.Query["uo"].ToString());

                var q = new User[] { };

                var user = Program.WexflowEngine.GetUser(qusername);

                if (user.Password.Equals(qpassword) && (user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                {
                    var users = Program.WexflowEngine.GetAdministrators(keyword, (UserOrderBy)uo);
                    string dateTimeFormat = Program.Config["DateTimeFormat"];

                    q = users.Select(u => new User
                    {
                        Id = u.GetId(),
                        Username = u.Username,
                        Password = u.Password,
                        UserProfile = (UserProfile)((int)u.UserProfile),
                        Email = u.Email,
                        CreatedOn = u.CreatedOn.ToString(dateTimeFormat),
                        ModifiedOn = u.ModifiedOn.ToString(dateTimeFormat)
                    }).ToArray();
                }

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
        /// Saves user workflow relations.
        /// </summary>
        private void SaveUserWorkflows()
        {
            Post(Root + "saveUserWorkflows", args =>
            {
                try
                {
                    var json = RequestStream.FromStream(Request.Body).AsString();

                    var res = false;
                    JObject o = JObject.Parse(json);

                    var qusername = o.Value<string>("QUsername");
                    var qpassword = o.Value<string>("QPassword");

                    var user = Program.WexflowEngine.GetUser(qusername);

                    if (user.Password.Equals(qpassword) && user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                    {
                        string userId = o.Value<string>("UserId");
                        JArray jArray = o.Value<JArray>("UserWorkflows");
                        Program.WexflowEngine.DeleteUserWorkflowRelations(userId);
                        foreach (JObject item in jArray)
                        {
                            var workflowId = item.Value<string>("WorkflowId");
                            Program.WexflowEngine.InsertUserWorkflowRelation(userId, workflowId);
                        }

                        res = true;
                    }

                    var resStr = JsonConvert.SerializeObject(res);
                    var resBytes = Encoding.UTF8.GetBytes(resStr);

                    return new Response()
                    {
                        ContentType = "application/json",
                        Contents = s => s.Write(resBytes, 0, resBytes.Length)
                    };
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occured while saving workflow relations: {0}", e);

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
        /// Returns user workflows.
        /// </summary>
        private void GetUserWorkflows()
        {
            Get(Root + "userWorkflows", args =>
            {
                string qusername = Request.Query["qu"].ToString();
                string qpassword = Request.Query["qp"].ToString();
                var userId = Request.Query["u"].ToString();

                var res = new WorkflowInfo[] { };

                var user = Program.WexflowEngine.GetUser(qusername);

                if (user.Password.Equals(qpassword) && user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                {
                    try
                    {
                        Core.Workflow[] workflows = Program.WexflowEngine.GetUserWorkflows(userId);
                        res = workflows
                            .ToList()
                            .Select(wf => new WorkflowInfo(wf.DbId, wf.Id, wf.Name,
                            (LaunchType)wf.LaunchType, wf.IsEnabled, wf.IsApproval, wf.IsWaitingForApproval, wf.Description, wf.IsRunning, wf.IsPaused,
                            wf.Period.ToString(@"dd\.hh\:mm\:ss"), wf.CronExpression,
                            wf.IsExecutionGraphEmpty
                           , wf.LocalVariables.Select(v => new Contracts.Variable { Key = v.Key, Value = v.Value }).ToArray()))
                            .ToArray();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("An error occured while retrieving user workflows: ", e);

                    }
                }

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
        /// Inserts a user.
        /// </summary>
        private void InsertUser()
        {
            Post(Root + "insertUser", args =>
            {
                string qusername = Request.Query["qu"].ToString();
                string qpassword = Request.Query["qp"].ToString();
                string username = Request.Query["username"].ToString();
                string password = Request.Query["password"].ToString();
                int userProfile = int.Parse(Request.Query["up"].ToString());
                string email = Request.Query["email"].ToString();

                try
                {
                    var res = false;
                    var user = Program.WexflowEngine.GetUser(qusername);
                    if (user.Password.Equals(qpassword) && user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                    {
                        Program.WexflowEngine.InsertUser(username, password, (Core.Db.UserProfile)userProfile, email);
                        res = true;
                    }

                    var resStr = JsonConvert.SerializeObject(res);
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
                string qusername = Request.Query["qu"].ToString();
                string qpassword = Request.Query["qp"].ToString();
                string userId = Request.Query["userId"].ToString();
                string username = Request.Query["username"].ToString();
                string password = Request.Query["password"].ToString();
                int userProfile = int.Parse(Request.Query["up"].ToString());
                string email = Request.Query["email"].ToString();

                try
                {
                    var res = false;
                    var user = Program.WexflowEngine.GetUser(qusername);
                    if (user.Password.Equals(qpassword) && (user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                    {
                        Program.WexflowEngine.UpdateUser(userId, username, password, (Core.Db.UserProfile)userProfile, email);
                        res = true;
                    }

                    var resStr = JsonConvert.SerializeObject(res);
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
                string qusername = Request.Query["qu"].ToString();
                string qpassword = Request.Query["qp"].ToString();
                string userId = Request.Query["userId"].ToString();
                string username = Request.Query["username"].ToString();
                string email = Request.Query["email"].ToString();
                int up = int.Parse(Request.Query["up"].ToString());

                try
                {
                    var res = false;
                    var user = Program.WexflowEngine.GetUser(qusername);
                    if (user.Password.Equals(qpassword) && (user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                    {
                        Program.WexflowEngine.UpdateUsernameAndEmailAndUserProfile(userId, username, email, up);
                        res = true;
                    }

                    var resStr = JsonConvert.SerializeObject(res);
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
                string qusername = Request.Query["qu"].ToString();
                string qpassword = Request.Query["qp"].ToString();
                string username = Request.Query["username"].ToString();
                string password = Request.Query["password"].ToString();

                try
                {
                    var res = false;
                    var user = Program.WexflowEngine.GetUser(qusername);
                    if (user.Password.Equals(qpassword) && (user.UserProfile == Core.Db.UserProfile.SuperAdministrator || user.UserProfile == Core.Db.UserProfile.Administrator))
                    {
                        Program.WexflowEngine.DeleteUser(username, password);
                        res = true;
                    }

                    var resStr = JsonConvert.SerializeObject(res);
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
                var username = Request.Query["u"].ToString();

                Core.Db.User user = Program.WexflowEngine.GetUser(username);

                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
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

                        Send(host, port, enableSsl, smtpUser, smtpPassword, user.Email, from, subject, body);

                        // Update password
                        Program.WexflowEngine.UpdatePassword(username, newPasswordHash);

                        var resTrueStr = JsonConvert.SerializeObject(true);
                        var resTrueBytes = Encoding.UTF8.GetBytes(resTrueStr);

                        return new Response
                        {
                            ContentType = "application/json",
                            Contents = s => s.Write(resTrueBytes, 0, resTrueBytes.Length)
                        };
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);

                        var resFalseStr = JsonConvert.SerializeObject(false);
                        var resFalseBytes = Encoding.UTF8.GetBytes(resFalseStr);

                        return new Response
                        {
                            ContentType = "application/json",
                            Contents = s => s.Write(resFalseBytes, 0, resFalseBytes.Length)
                        };
                    }
                }

                var resStr = JsonConvert.SerializeObject(false);
                var resBytes = Encoding.UTF8.GetBytes(resStr);

                return new Response
                {
                    ContentType = "application/json",
                    Contents = s => s.Write(resBytes, 0, resBytes.Length)
                };

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

                Contracts.HistoryEntry[] q = entries.Select(e =>
                   new Contracts.HistoryEntry
                   {
                       Id = e.GetDbId(),
                       WorkflowId = e.WorkflowId,
                       Name = e.Name,
                       LaunchType = (LaunchType)((int)e.LaunchType),
                       Description = e.Description,
                       Status = (Contracts.Status)((int)e.Status),
                       //StatusDate = (e.StatusDate - baseDate).TotalMilliseconds
                       StatusDate = e.StatusDate.ToString(Program.Config["DateTimeFormat"])
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
                        Id = e.GetDbId(),
                        WorkflowId = e.WorkflowId,
                        Name = e.Name,
                        LaunchType = (LaunchType)((int)e.LaunchType),
                        Description = e.Description,
                        Status = (Contracts.Status)((int)e.Status),
                        //StatusDate = (e.StatusDate - baseDate).TotalMilliseconds
                        StatusDate = e.StatusDate.ToString(Program.Config["DateTimeFormat"])
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

        /// <summary>
        /// Deletes workflows.
        /// </summary>
        private void DeleteWorkflows()
        {
            Post(Root + "deleteWorkflows", args =>
            {
                try
                {
                    var json = RequestStream.FromStream(Request.Body).AsString();

                    var res = false;

                    var o = JObject.Parse(json);
                    var username = o.Value<string>("Username");
                    var password = o.Value<string>("Password");
                    var workflowDbIds = JsonConvert.DeserializeObject<string[]>(((JArray)o.SelectToken("WorkflowsToDelete")).ToString());

                    var user = Program.WexflowEngine.GetUser(username);
                    if (user.Password.Equals(password))
                    {
                        if (user.UserProfile == Core.Db.UserProfile.SuperAdministrator)
                        {
                            res = Program.WexflowEngine.DeleteWorkflows(workflowDbIds);
                        }
                        else if (user.UserProfile == Core.Db.UserProfile.Administrator)
                        {
                            var tres = true;
                            foreach (var id in workflowDbIds)
                            {
                                var check = Program.WexflowEngine.CheckUserWorkflow(user.GetId(), id);
                                if (check)
                                {
                                    try
                                    {
                                        Program.WexflowEngine.DeleteWorkflow(id);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e);
                                        tres &= false;
                                    }

                                }
                            }
                            res = tres;
                        }
                    }

                    var resStr = JsonConvert.SerializeObject(res);
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
    }
}