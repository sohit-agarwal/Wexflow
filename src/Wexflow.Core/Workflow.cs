using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using System.Threading;
using System.Xml.Schema;
using System.Xml;
using System.Globalization;

namespace Wexflow.Core
{
    public enum LaunchType
    { 
        Startup,
        Trigger,
        Periodic
    }

    public class Workflow
    {
        public const int StartId = -1;

        public string WorkflowFilePath { get; set; }
        public string WexflowTempFolder { get; set; }
        public string WorkflowTempFolder { get; set; }
        public string XsdPath { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public LaunchType LaunchType { get; set; }
        public TimeSpan Period { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsRunning { get; set; }
        public bool IsPaused { get; set; }
        public Task[] Taks { get; set; }
        public Dictionary<int, List<FileInf>> FilesPerTask { get; set; }
        public Dictionary<int, List<Entity>> EntitiesPerTask { get; set; }
        public int JobId { get; set; }
        public string LogTag { get { return string.Format("[{0} / {1}]", Name, JobId); } }
        public XmlNamespaceManager XmlNamespaceManager { get; set; }
        public Graph ExecutionGraph { get; set; }

        Thread _thread;

        public Workflow(string path, string wexflowTempFolder, string xsdPath)
        {
            JobId = 1;
            _thread = null;
            WorkflowFilePath = path;
            WexflowTempFolder = wexflowTempFolder;
            XsdPath = xsdPath;
            FilesPerTask = new Dictionary<int, List<FileInf>>();
            EntitiesPerTask = new Dictionary<int, List<Entity>>();
            Check();
            Load();
        }

        public override string ToString()
        {
            return string.Format("{{id: {0}, name: {1}, enabled: {2}, launchType: {3}}}"
                , Id, Name, IsEnabled, LaunchType);
        }

        void Check()
        {
            var schemas = new XmlSchemaSet();
            schemas.Add("urn:wexflow-schema", XsdPath);

            var doc = XDocument.Load(WorkflowFilePath);
            string msg = string.Empty;
            doc.Validate(schemas, (o, e) =>
            {
                msg += e.Message + Environment.NewLine;
            });

            if (!string.IsNullOrEmpty(msg))
            {
                throw new Exception("The workflow XML document is not valid. Error: " + msg);
            }
        }

        void Load()
        {
            var xmlReader = XmlReader.Create(WorkflowFilePath);
            XmlNameTable xmlNameTable = xmlReader.NameTable;
            XmlNamespaceManager = new XmlNamespaceManager(xmlNameTable);
            XmlNamespaceManager.AddNamespace("wf", "urn:wexflow-schema");

            // Loading settings
            var xdoc = XDocument.Load(WorkflowFilePath);
            Id = int.Parse(GetWorkflowAttribute(xdoc, "id"));
            Name = GetWorkflowAttribute(xdoc, "name");
            Description = GetWorkflowAttribute(xdoc, "description");
            LaunchType = (LaunchType)Enum.Parse(typeof(LaunchType), GetWorkflowSetting(xdoc, "launchType"), true);
            if(LaunchType == LaunchType.Periodic) Period = TimeSpan.Parse(GetWorkflowSetting(xdoc, "period"));
            IsEnabled = bool.Parse(GetWorkflowSetting(xdoc, "enabled"));

            // Loading tasks
            var tasks = new List<Task>();
            foreach (XElement xTask in xdoc.XPathSelectElements("/wf:Workflow/wf:Tasks/wf:Task", XmlNamespaceManager))
            {
                string name = xTask.Attribute("name").Value;
                string assemblyName = "Wexflow.Tasks." + name;
                string typeName = "Wexflow.Tasks." + name + "." + name + ", " + assemblyName;
                var task = (Task)Activator.CreateInstance(Type.GetType(typeName), xTask, this);
                tasks.Add(task);
            }
            Taks = tasks.ToArray();

            // Loading execution graph
            var xExectionGraph = xdoc.XPathSelectElement("/wf:Workflow/wf:ExecutionGraph", XmlNamespaceManager);
            if (xExectionGraph != null)
            {
                var taskNodes = GetTaskNodes(xExectionGraph);

                // Check parallel tasks and infinite loops
                CheckParallelTasks(taskNodes, "Parallel tasks execution detected in ExecutionGraph execution graph.");
                CheckInfiniteLoop(taskNodes, "Infinite loop detected in ExecutionGraph execution graph.");

                // OnSuccess
                GraphEvent onSuccess = null;
                var xOnSuccess = xExectionGraph.XPathSelectElement("wf:OnSuccess", XmlNamespaceManager);
                if (xOnSuccess != null)
                {
                    var onSuccessNodes = GetTaskNodes(xOnSuccess);
                    CheckParallelTasks(onSuccessNodes, "Parallel tasks execution detected in OnSuccess execution graph.");
                    CheckInfiniteLoop(onSuccessNodes, "Infinite loop detected in OnSuccess execution graph.");
                    onSuccess = new GraphEvent(onSuccessNodes);
                }

                // OnWarning
                GraphEvent onWarning = null;
                var xOnWarning = xExectionGraph.XPathSelectElement("wf:OnWarning", XmlNamespaceManager);
                if (xOnWarning != null)
                {
                    var onWarningNodes = GetTaskNodes(xOnWarning);
                    CheckParallelTasks(onWarningNodes, "Parallel tasks execution detected in OnWarning execution graph.");
                    CheckInfiniteLoop(onWarningNodes, "Infinite loop detected in OnWarning execution graph.");
                    onWarning = new GraphEvent(onWarningNodes);
                }

                // OnError
                GraphEvent onError = null;
                var xOnError = xExectionGraph.XPathSelectElement("wf:OnError", XmlNamespaceManager);
                if (xOnError != null)
                {
                    var onErrorNodes = GetTaskNodes(xOnError);
                    CheckParallelTasks(onErrorNodes, "Parallel tasks execution detected in OnError execution graph.");
                    CheckInfiniteLoop(onErrorNodes, "Infinite loop detected in OnError execution graph.");
                    onError = new GraphEvent(onErrorNodes);
                }

                ExecutionGraph = new Graph(taskNodes, onSuccess, onWarning, onError);
            }
        }

        IEnumerable<Node> GetTaskNodes(XElement xExectionGraph)
        {
            var taskNodes = new List<Node>();

            foreach (var xTask in xExectionGraph.Elements().Where(xe => xe.Name != "OnSuccess" && xe.Name != "OnWarning" && xe.Name != "OnError"))
            {
                switch (xTask.Name.LocalName)
                {
                    case "Task":
                        var taskNode = XTaskToNode(xTask);
                        taskNodes.Add(taskNode);
                        break;
                    case "DoIf":
                        var id = int.Parse(xTask.Attribute("id").Value);
                        var ifId = int.Parse(xTask.Attribute("if").Value);
                        var parentId = int.Parse(xTask.XPathSelectElement("wf:Parent", XmlNamespaceManager).Attribute("id").Value);

                        var doIfNodes = XTasksToNodes(xTask.XPathSelectElements("wf:Do/wf:Task", XmlNamespaceManager));
                        CheckParallelTasks(doIfNodes, "Parallel tasks execution detected in DoIf>Do execution graph.");
                        CheckInfiniteLoop(doIfNodes, "Infinite loop detected in DoIf>Do execution graph.");

                        var otherwiseNodes = new List<Node>();
                        var xOtherwise = xTask.XPathSelectElement("wf:Otherwise", XmlNamespaceManager);
                        if (xOtherwise != null)
                        {
                            var otherwiseNodesArray = XTasksToNodes(xOtherwise.XPathSelectElements("wf:Task", XmlNamespaceManager));
                            otherwiseNodes.AddRange(otherwiseNodesArray);
                            CheckParallelTasks(otherwiseNodes, "Parallel tasks execution detected in DoIf>Otherwise execution graph.");
                            CheckInfiniteLoop(otherwiseNodes, "Infinite loop detected in DoIf>Otherwise execution graph.");
                        }

                        taskNodes.Add(new DoIf(id, parentId, ifId, doIfNodes, otherwiseNodes));
                        break;
                    case "DoWhile":
                        var doWhileId = int.Parse(xTask.Attribute("id").Value);
                        var whileId = int.Parse(xTask.Attribute("while").Value);
                        var doWhileParentId = int.Parse(xTask.XPathSelectElement("wf:Parent", XmlNamespaceManager).Attribute("id").Value);

                        var doWhileNodes = XTasksToNodes(xTask.XPathSelectElements("wf:Do/wf:Task", XmlNamespaceManager));
                        CheckParallelTasks(doWhileNodes, "Parallel tasks execution detected in DoWhile>Do execution graph.");
                        CheckInfiniteLoop(doWhileNodes, "Infinite loop detected in DoWhile>Do execution graph.");

                        taskNodes.Add(new DoWhile(doWhileId, doWhileParentId, whileId, doWhileNodes));
                        break;
                }
            }

            return taskNodes;
        }

        Node XTaskToNode(XElement xTask)
        {
            var id = int.Parse(xTask.Attribute("id").Value);
            var parentId = int.Parse(xTask.XPathSelectElement("wf:Parent", XmlNamespaceManager).Attribute("id").Value);
            var node = new Node(id, parentId);
            return node;
        }

        Node[] XTasksToNodes(IEnumerable<XElement> xTasks)
        {
            var taskNodes = new List<Node>();
            foreach (var xTask in xTasks)
            {
                var node = XTaskToNode(xTask);
                taskNodes.Add(node);
            }
            return taskNodes.ToArray();
        }

        void CheckParallelTasks(IEnumerable<Node> taskNodes, string errorMsg)
        {
            bool parallelTasksDetected = false;
            foreach (var taskNode in taskNodes)
            {
                if (taskNodes.Count(n => n.ParentId == taskNode.Id) > 1)
                {
                    parallelTasksDetected = true;
                    break;
                }
            }

            if (parallelTasksDetected)
            {
                throw new Exception(errorMsg);
            }
        }

        void CheckInfiniteLoop(IEnumerable<Node> taskNodes, string errorMsg)
        {
            var startNode = taskNodes.FirstOrDefault(n => n.ParentId == StartId);

            if (startNode != null)
            {
                var infiniteLoopDetected = CheckInfiniteLoop(startNode, taskNodes);

                if (!infiniteLoopDetected)
                {
                    foreach (var taskNode in taskNodes.Where(n => n.Id != startNode.Id))
                    {
                        infiniteLoopDetected |= CheckInfiniteLoop(taskNode, taskNodes);

                        if (infiniteLoopDetected) break;
                    }
                }

                if (infiniteLoopDetected)
                {
                    throw new Exception(errorMsg);
                }
            }
        }

        bool CheckInfiniteLoop(Node startNode, IEnumerable<Node> taskNodes)
        {
            foreach (var taskNode in taskNodes.Where(n => n.ParentId != startNode.ParentId))
            {
                if (taskNode.Id == startNode.Id)
                {
                    return true;
                }
            }

            return false;
        }

        string GetWorkflowAttribute(XDocument xdoc, string attr)
        {
            return xdoc.XPathSelectElement("/wf:Workflow", XmlNamespaceManager).Attribute(attr).Value;
        }

        string GetWorkflowSetting(XDocument xdoc, string name)
        {
            return xdoc.XPathSelectElement(string.Format("/wf:Workflow[@id='{0}']/wf:Settings/wf:Setting[@name='{1}']", Id, name), XmlNamespaceManager).Attribute("value").Value;
        }

        public void Start()
        {
            if (IsRunning) return;

            var thread = new Thread(() =>
                {
                    try
                    {
                        IsRunning = true;
                        Logger.InfoFormat("{0} Workflow started.", LogTag);

                        // Create the temp folder
                        CreateTempFolder();

                        // Run the tasks
                        if (ExecutionGraph == null)
                        {
                            bool success = true;
                            bool warning = false;
                            bool error = false;
                            RunSequentialTasks(Taks, ref success, ref warning, ref error);
                        }
                        else
                        {
                            var status = RunTasks(ExecutionGraph.Nodes, Taks);

                            switch (status)
                            { 
                                case Status.Success:
                                    if (ExecutionGraph.OnSuccess != null)
                                    {
                                        // TODO check DOIf and DoWhile types in NodesToTasks
                                        var successTasks = NodesToTasks(ExecutionGraph.OnSuccess.DoNodes);
                                        RunTasks(ExecutionGraph.OnSuccess.DoNodes, successTasks);
                                    }
                                    break;
                                case Status.Warning:
                                    if (ExecutionGraph.OnWarning != null)
                                    {
                                        var warningTasks = NodesToTasks(ExecutionGraph.OnWarning.DoNodes);
                                        RunTasks(ExecutionGraph.OnWarning.DoNodes, warningTasks);
                                    }
                                    break;
                                case Status.Error:
                                    if (ExecutionGraph.OnError != null)
                                    {
                                        var errorTasks = NodesToTasks(ExecutionGraph.OnError.DoNodes);
                                        RunTasks(ExecutionGraph.OnError.DoNodes, errorTasks);
                                    }
                                    break;
                            }
                        }

                    }
                    catch (ThreadAbortException)
                    {
                    }
                    catch (Exception e)
                    {
                        Logger.ErrorFormat("An error occured while running the workflow. Error: {0}", e.Message, this);
                    }
                    finally
                    {
                        // Cleanup
                        foreach (List<FileInf> files in FilesPerTask.Values) files.Clear();
                        foreach (List<Entity> entities in EntitiesPerTask.Values) entities.Clear();
                        _thread = null;
                        IsRunning = false;
                        GC.Collect();

                        Logger.InfoFormat("{0} Workflow finished.", LogTag);
                        JobId++;
                    }
                });

            _thread = thread;
            thread.Start();
        }

        IEnumerable<Task> NodesToTasks(IEnumerable<Node> nodes)
        {
            var tasks = new List<Task>();

            foreach (var node in nodes)
            {
                if (node is DoIf)
                {
                    var doTasks = NodesToTasks(((DoIf)node).DoNodes);
                    var otherwiseTasks = NodesToTasks(((DoIf)node).OtherwiseNodes);

                    var ifTasks = new List<Task>(doTasks);
                    foreach (var task in otherwiseTasks)
                    { 
                        if(ifTasks.All(t => t.Id != task.Id))
                        {
                            ifTasks.Add(task);
                        }
                    }

                    tasks.AddRange(ifTasks);
                }
                else if (node is DoWhile)
                {
                    tasks.AddRange(NodesToTasks(((DoWhile)node).DoNodes));
                }
                else
                {
                    var task = GetTask(node.Id);

                    if (task != null)
                    {
                        if (tasks.All(t => t.Id != task.Id))
                        {
                            tasks.Add(task);
                        }
                    }
                    else
                    {
                        throw new Exception("Task " + node.Id + " not found.");
                    }
                }
            }

            return tasks;
        }

        Status RunTasks(IEnumerable<Node> nodes, IEnumerable<Task> tasks)
        {
            bool success = true;
            bool warning = false;
            bool atLeastOneSucceed = false;

            if (nodes.Any())
            {
                var startNode = nodes.ElementAt(0);

                if (startNode is DoIf)
                {
                    var doIf = (DoIf)startNode;
                    RunDoIf(tasks, nodes, doIf, ref success, ref warning, ref atLeastOneSucceed);
                }
                else if (startNode is DoWhile)
                {
                    var doWhile = (DoWhile)startNode;
                    RunDoWhile(tasks, nodes, doWhile, ref success, ref warning, ref atLeastOneSucceed);
                }
                else
                {
                    if (startNode.ParentId == StartId)
                    {
                        RunTasks(tasks, nodes, startNode, ref success, ref warning, ref atLeastOneSucceed);
                    }
                }
            }
            else
            {
                RunSequentialTasks(tasks, ref success, ref warning, ref atLeastOneSucceed);
            }

            if (success)
            {
                return Status.Success;
            }

            if (atLeastOneSucceed || warning)
			{
				return Status.Warning;
			}

			return Status.Error;
        }

        void RunSequentialTasks(IEnumerable<Task> tasks, ref bool success, ref bool warning, ref bool atLeastOneSucceed)
        {
            foreach (Task task in tasks)
            {
                if (task.IsEnabled)
                {
                    var status = task.Run();
                    success &= status.Status == Status.Success;
                    warning |= status.Status == Status.Warning;
                    if (!atLeastOneSucceed && status.Status == Status.Success) atLeastOneSucceed = true;
                }
            }
        }

        void RunTasks(IEnumerable<Task> tasks, IEnumerable<Node> nodes, Node node, ref bool success, ref bool warning, ref bool atLeastOneSucceed)
        {
            if(node != null)
            {
                var task = GetTask(tasks, node.Id);
                if (task != null)
                {
                    if (task.IsEnabled)
                    {
                        var status = task.Run();

                        success &= status.Status == Status.Success;
                        warning |= status.Status == Status.Warning;
                        if (!atLeastOneSucceed && status.Status == Status.Success) atLeastOneSucceed = true;

                        var childNode = nodes.FirstOrDefault(n => n.ParentId == node.Id);

                        if (childNode != null)
                        {
                            if (childNode is DoIf)
                            {
                                var doIf = (DoIf)childNode;
                                RunDoIf(tasks, nodes, doIf, ref success, ref warning, ref atLeastOneSucceed);
                            }
                            else if (childNode is DoWhile)
                            {
                                var doWhile = (DoWhile)childNode;
                                RunDoWhile(tasks, nodes, doWhile, ref success, ref warning, ref atLeastOneSucceed);
                            }
                            else
                            {
                                var childTask = GetTask(tasks, childNode.Id);
                                if (childTask != null)
                                {
                                    if (childTask.IsEnabled)
                                    {
                                        var childStatus = childTask.Run();

                                        success &= childStatus.Status == Status.Success;
                                        warning |= childStatus.Status == Status.Warning;
                                        if (!atLeastOneSucceed && status.Status == Status.Success) atLeastOneSucceed = true;

                                        // Recusive call
                                        var ccNode = nodes.FirstOrDefault(n => n.ParentId == childNode.Id);

                                        if (ccNode is DoIf)
                                        {
                                            var doIf = (DoIf)ccNode;
                                            RunDoIf(tasks, nodes, doIf, ref success, ref warning, ref atLeastOneSucceed);
                                        }
                                        else if (ccNode is DoWhile)
                                        {
                                            var doWhile = (DoWhile)ccNode;
                                            RunDoWhile(tasks, nodes, doWhile, ref success, ref warning, ref atLeastOneSucceed);
                                        }
                                        else
                                        {
                                            RunTasks(tasks, nodes, ccNode, ref success, ref warning, ref atLeastOneSucceed);
                                        }
                                    }
                                }
                                else
                                {
                                    throw new Exception("Task " + childNode.Id + " not found.");
                                }
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("Task " + node.Id + " not found.");
                }
            }
        }

        void RunDoIf(IEnumerable<Task> tasks, IEnumerable<Node> nodes, DoIf doIf, ref bool success, ref bool warning, ref bool atLeastOneSucceed)
        {
            var ifTask = GetTask(doIf.If);

            if (ifTask != null)
            {
                if (ifTask.IsEnabled)
                {
                    var status = ifTask.Run();

                    success &= status.Status == Status.Success;
                    warning |= status.Status == Status.Warning;
                    if (!atLeastOneSucceed && status.Status == Status.Success) atLeastOneSucceed = true;

                    if (status.Condition)
                    {
                        if (doIf.DoNodes.Length > 0)
                        {
                            // Build Tasks
                            var doIfTasks = NodesToTasks(doIf.DoNodes);

                            // Run Tasks
                            Node doIfStartNode = doIf.DoNodes[0];

                            if (doIfStartNode.ParentId == StartId)
                            {
                                RunTasks(doIfTasks, doIf.DoNodes, doIfStartNode, ref success, ref warning, ref atLeastOneSucceed);
                            }
                        }
                    }
                    else if(status.Condition == false)
                    {
                        if (doIf.OtherwiseNodes.Length > 0)
                        {
                            // Build Tasks
                            var otherwiseTasks = NodesToTasks(doIf.OtherwiseNodes);

                            // Run Tasks
                            Node otherwiseStartNode = doIf.OtherwiseNodes[0];

                            if (otherwiseStartNode.ParentId == StartId)
                            {
                                RunTasks(otherwiseTasks, doIf.OtherwiseNodes, otherwiseStartNode, ref success, ref warning, ref atLeastOneSucceed);
                            }
                        }
                    }

                    // Child node
                    var childNode = nodes.FirstOrDefault(n => n.ParentId == doIf.Id);

                    if (childNode != null)
                    {
                        RunTasks(tasks, nodes, childNode, ref success, ref warning, ref atLeastOneSucceed);
                    }
                }
            }
            else
            {
                throw new Exception("Task " + doIf.Id + " not found.");
            }
        }

        void RunDoWhile(IEnumerable<Task> tasks, IEnumerable<Node> nodes, DoWhile doWhile, ref bool success, ref bool warning, ref bool atLeastOneSucceed)
        {
            var whileTask = GetTask(doWhile.While);

            if (whileTask != null)
            {
                if (whileTask.IsEnabled)
                {
                    while (true)
                    {
                        var status = whileTask.Run();

                        success &= status.Status == Status.Success;
                        warning |= status.Status == Status.Warning;
                        if (!atLeastOneSucceed && status.Status == Status.Success) atLeastOneSucceed = true;

                        if (status.Condition)
                        {
                            if (doWhile.DoNodes.Length > 0)
                            {
                                // Build Tasks
                                var doWhileTasks = NodesToTasks(doWhile.DoNodes);

                                // Run Tasks
                                Node doWhileStartNode = doWhile.DoNodes[0];

                                if (doWhileStartNode.ParentId == StartId)
                                {
                                    RunTasks(doWhileTasks, doWhile.DoNodes, doWhileStartNode, ref success, ref warning, ref atLeastOneSucceed);
                                }
                            }
                        }
                        else if(status.Condition == false)
                        {
                            break;
                        }
                    }

                    // Child node
                    var childNode = nodes.FirstOrDefault(n => n.ParentId == doWhile.Id);

                    if (childNode != null)
                    {
                        RunTasks(tasks, nodes, childNode, ref success, ref warning, ref atLeastOneSucceed);
                    }
                }
            }
            else
            {
                throw new Exception("Task " + doWhile.Id + " not found.");
            }
        
        }

        public void Stop()
        {
            if (_thread != null && IsRunning && !IsPaused)
            {
                try
                {
                    _thread.Abort();
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("An error occured while stopping the workflow : {0}", e, this);
                }
                finally
                {
                    IsRunning = false;
                    Logger.InfoFormat("{0} Workflow finished.", LogTag);
                    JobId++;
                }
            }
        }

        public void Pause()
        {
            if (_thread != null)
            {
                try
                {
                    _thread.Suspend();
                    IsPaused = true;
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("An error occured while suspending the workflow : {0}", e, this);
                }
            }
        }

        public void Resume()
        {
            if (_thread != null && IsPaused)
            {
                try
                {
                    _thread.Resume();
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("An error occured while resuming the workflow : {0}", e, this);
                }
                finally
                {
                    IsPaused = false;
                }
            }
        }

        void CreateTempFolder()
        { 
            // WorkflowId/dd-MM-yyyy/HH-mm-ss-fff
            var wfTempFolder = Path.Combine(WexflowTempFolder, Id.ToString(CultureInfo.CurrentCulture));
            if (!Directory.Exists(wfTempFolder)) Directory.CreateDirectory(wfTempFolder);
            
            var wfDayTempFolder = Path.Combine(wfTempFolder, string.Format("{0:yyyy-MM-dd}", DateTime.Now));
            if (!Directory.Exists(wfDayTempFolder)) Directory.CreateDirectory(wfDayTempFolder);

            var wfJobTempFolder = Path.Combine(wfDayTempFolder, string.Format("{0:HH-mm-ss-fff}", DateTime.Now));
            if (!Directory.Exists(wfJobTempFolder)) Directory.CreateDirectory(wfJobTempFolder);

            WorkflowTempFolder = wfJobTempFolder;
        }

        Task GetTask(int id)
        {
            return Taks.FirstOrDefault(t => t.Id == id);
        }

        Task GetTask(IEnumerable<Task> tasks, int id)
        {
            return tasks.FirstOrDefault(t => t.Id == id);
        }
    }
}
