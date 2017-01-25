using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Reflection;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Xml.Schema;
using System.Xml;

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
        public const int START_ID = -1;

        public string WorkflowFilePath { get; private set; }
        public string WexflowTempFolder { get; private set; }
        public string WorkflowTempFolder { get; private set; }
        public string XsdPath { get; private set; }
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public LaunchType LaunchType { get; private set; }
        public TimeSpan Period { get; private set; }
        public bool IsEnabled { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsPaused { get; private set; }
        public Task[] Taks { get; private set; }
        public Dictionary<int, List<FileInf>> FilesPerTask { get; private set; }
        public Dictionary<int, List<Entity>> EntitiesPerTask { get; private set; }
        public int JobId { get; private set; }
        public string LogTag { get { return string.Format("[{0} / {1}]", this.Name, this.JobId); } }
        public XmlNamespaceManager XmlNamespaceManager { get; private set; }
        public Graph ExecutionGraph { get; private set; }

        private Thread _thread;

        public Workflow(string path, string wexflowTempFolder, string xsdPath)
        {
            this.JobId = 1;
            this._thread = null;
            this.WorkflowFilePath = path;
            this.WexflowTempFolder = wexflowTempFolder;
            this.XsdPath = xsdPath;
            this.FilesPerTask = new Dictionary<int, List<FileInf>>();
            this.EntitiesPerTask = new Dictionary<int, List<Entity>>();
            Check();
            Load();
        }

        public override string ToString()
        {
            return string.Format("{{id: {0}, name: {1}, enabled: {2}, launchType: {3}}}"
                , this.Id, this.Name, this.IsEnabled, this.LaunchType);
        }

        private void Check()
        {
            XmlSchemaSet schemas = new XmlSchemaSet();
            schemas.Add("urn:wexflow-schema", this.XsdPath);

            XDocument doc = XDocument.Load(this.WorkflowFilePath);
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

        private void Load()
        {
            XmlReader xmlReader = XmlReader.Create(this.WorkflowFilePath);
            XmlNameTable xmlNameTable = xmlReader.NameTable;
            XmlNamespaceManager = new XmlNamespaceManager(xmlNameTable);
            XmlNamespaceManager.AddNamespace("wf", "urn:wexflow-schema");

            // Loading settings
            XDocument xdoc = XDocument.Load(this.WorkflowFilePath);
            this.Id = int.Parse(GetWorkflowAttribute(xdoc, "id"));
            this.Name = GetWorkflowAttribute(xdoc, "name");
            this.Description = GetWorkflowAttribute(xdoc, "description");
            this.LaunchType = (LaunchType)Enum.Parse(typeof(LaunchType), GetWorkflowSetting(xdoc, "launchType"), true);
            if(this.LaunchType == Core.LaunchType.Periodic) this.Period = TimeSpan.Parse(GetWorkflowSetting(xdoc, "period"));
            this.IsEnabled = bool.Parse(GetWorkflowSetting(xdoc, "enabled"));

            // Loading tasks
            List<Task> tasks = new List<Task>();
            foreach (XElement xTask in xdoc.XPathSelectElements("/wf:Workflow/wf:Tasks/wf:Task", this.XmlNamespaceManager))
            {
                string name = xTask.Attribute("name").Value;
                string assemblyName = "Wexflow.Tasks." + name;
                string typeName = "Wexflow.Tasks." + name + "." + name + ", " + assemblyName;
                Task task = (Task)Activator.CreateInstance(Type.GetType(typeName), xTask, this);
                tasks.Add(task);
            }
            this.Taks = tasks.ToArray();

            // Loading execution graph
            XElement xExectionGraph = xdoc.XPathSelectElement("/wf:Workflow/wf:ExecutionGraph", this.XmlNamespaceManager);
            if (xExectionGraph != null)
            {
                var taskNodes = GetTaskNodes(xExectionGraph);

                // Check parallel tasks and infinite loops
                CheckParallelTasks(taskNodes, "Parallel tasks execution detected in ExecutionGraph execution graph.");
                CheckInfiniteLoop(taskNodes, "Infinite loop detected in ExecutionGraph execution graph.");

                // OnSuccess
                GraphEvent onSuccess = null;
                XElement xOnSuccess = xExectionGraph.XPathSelectElement("wf:OnSuccess", this.XmlNamespaceManager);
                if (xOnSuccess != null)
                {
                    var onSuccessNodes = GetTaskNodes(xOnSuccess);
                    CheckParallelTasks(onSuccessNodes, "Parallel tasks execution detected in OnSuccess execution graph.");
                    CheckInfiniteLoop(onSuccessNodes, "Infinite loop detected in OnSuccess execution graph.");
                    onSuccess = new GraphEvent(onSuccessNodes);
                }

                // OnWarning
                GraphEvent onWarning = null;
                XElement xOnWarning = xExectionGraph.XPathSelectElement("wf:OnWarning", this.XmlNamespaceManager);
                if (xOnWarning != null)
                {
                    var onWarningNodes = GetTaskNodes(xOnWarning);
                    CheckParallelTasks(onWarningNodes, "Parallel tasks execution detected in OnWarning execution graph.");
                    CheckInfiniteLoop(onWarningNodes, "Infinite loop detected in OnWarning execution graph.");
                    onWarning = new GraphEvent(onWarningNodes);
                }

                // OnError
                GraphEvent onError = null;
                XElement xOnError = xExectionGraph.XPathSelectElement("wf:OnError", this.XmlNamespaceManager);
                if (xOnError != null)
                {
                    var onErrorNodes = GetTaskNodes(xOnError);
                    CheckParallelTasks(onErrorNodes, "Parallel tasks execution detected in OnError execution graph.");
                    CheckInfiniteLoop(onErrorNodes, "Infinite loop detected in OnError execution graph.");
                    onError = new GraphEvent(onErrorNodes);
                }

                this.ExecutionGraph = new Graph(taskNodes, onSuccess, onWarning, onError);
            }
        }

        private IEnumerable<Node> GetTaskNodes(XElement xExectionGraph)
        {
            List<Node> taskNodes = new List<Node>();

            foreach (var xTask in xExectionGraph.Elements().Where(xe => xe.Name != "OnSuccess" && xe.Name != "OnWarning" && xe.Name != "OnError"))
            {
                switch (xTask.Name.LocalName)
                {
                    case "Task":
                        Node taskNode = XTaskToNode(xTask);
                        taskNodes.Add(taskNode);
                        break;
                    case "DoIf":
                        int id = int.Parse(xTask.Attribute("id").Value);
                        int ifId = int.Parse(xTask.Attribute("if").Value);
                        int parentId = int.Parse(xTask.XPathSelectElement("wf:Parent", this.XmlNamespaceManager).Attribute("id").Value);

                        Node[] doIfNodes = XTasksToNodes(xTask.XPathSelectElements("wf:Do/wf:Task", this.XmlNamespaceManager));
                        CheckParallelTasks(doIfNodes, "Parallel tasks execution detected in DoIf>Do execution graph.");
                        CheckInfiniteLoop(doIfNodes, "Infinite loop detected in DoIf>Do execution graph.");

                        List<Node> otherwiseNodes = new List<Node>();
                        XElement xOtherwise = xTask.XPathSelectElement("wf:Otherwise", this.XmlNamespaceManager);
                        if (xOtherwise != null)
                        {
                            var otherwiseNodesArray = XTasksToNodes(xOtherwise.XPathSelectElements("wf:Task", this.XmlNamespaceManager));
                            otherwiseNodes.AddRange(otherwiseNodesArray);
                            CheckParallelTasks(otherwiseNodes, "Parallel tasks execution detected in DoIf>Otherwise execution graph.");
                            CheckInfiniteLoop(otherwiseNodes, "Infinite loop detected in DoIf>Otherwise execution graph.");
                        }

                        taskNodes.Add(new DoIf(id, parentId, ifId, doIfNodes, otherwiseNodes));
                        break;
                    case "DoWhile":
                        int doWhileId = int.Parse(xTask.Attribute("id").Value);
                        int whileId = int.Parse(xTask.Attribute("while").Value);
                        int doWhileParentId = int.Parse(xTask.XPathSelectElement("wf:Parent", this.XmlNamespaceManager).Attribute("id").Value);

                        Node[] doWhileNodes = XTasksToNodes(xTask.XPathSelectElements("wf:Do/wf:Task", this.XmlNamespaceManager));
                        CheckParallelTasks(doWhileNodes, "Parallel tasks execution detected in DoWhile>Do execution graph.");
                        CheckInfiniteLoop(doWhileNodes, "Infinite loop detected in DoWhile>Do execution graph.");

                        taskNodes.Add(new DoWhile(doWhileId, doWhileParentId, whileId, doWhileNodes));
                        break;
                }
            }

            return taskNodes;
        }

        private Node XTaskToNode(XElement xTask)
        {
            int id = int.Parse(xTask.Attribute("id").Value);
            int parentId = int.Parse(xTask.XPathSelectElement("wf:Parent", this.XmlNamespaceManager).Attribute("id").Value);
            Node node = new Node(id, parentId);
            return node;
        }

        private Node[] XTasksToNodes(IEnumerable<XElement> xTasks)
        {
            List<Node> taskNodes = new List<Node>();
            foreach (var xTask in xTasks)
            {
                Node node = XTaskToNode(xTask);
                taskNodes.Add(node);
            }
            return taskNodes.ToArray();
        }

        private void CheckParallelTasks(IEnumerable<Node> taskNodes, string errorMsg)
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

        private void CheckInfiniteLoop(IEnumerable<Node> taskNodes, string errorMsg)
        {
            Node startNode = taskNodes.FirstOrDefault(n => n.ParentId == START_ID);

            if (startNode != null)
            {
                bool infiniteLoopDetected = CheckInfiniteLoop(startNode, taskNodes);

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

        private bool CheckInfiniteLoop(Node startNode, IEnumerable<Node> taskNodes)
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

        private string GetWorkflowAttribute(XDocument xdoc, string attr)
        {
            return xdoc.XPathSelectElement("/wf:Workflow", this.XmlNamespaceManager).Attribute(attr).Value;
        }

        private string GetWorkflowSetting(XDocument xdoc, string name)
        {
            return xdoc.XPathSelectElement(string.Format("/wf:Workflow[@id='{0}']/wf:Settings/wf:Setting[@name='{1}']", this.Id, name), this.XmlNamespaceManager).Attribute("value").Value;
        }

        public void Start()
        {
            if (this.IsRunning) return;

            Thread thread = new Thread(new ThreadStart(() =>
                {
                    try
                    {
                        this.IsRunning = true;
                        Logger.InfoFormat("{0} Workflow started.", this.LogTag);

                        // Create the temp folder
                        CreateTempFolder();

                        // Run the tasks
                        if (this.ExecutionGraph == null)
                        {
                            bool success = true;
                            bool warning = false;
                            bool error = false;
                            RunSequentialTasks(this.Taks, ref success, ref warning, ref error);
                        }
                        else
                        {
                            Status status = RunTasks(this.ExecutionGraph.Nodes, this.Taks);

                            switch (status)
                            { 
                                case Status.Success:
                                    if (this.ExecutionGraph.OnSuccess != null)
                                    {
                                        // TODO check DOIf and DoWhile types in NodesToTasks
                                        var successTasks = NodesToTasks(this.ExecutionGraph.OnSuccess.DoNodes);
                                        RunTasks(this.ExecutionGraph.OnSuccess.DoNodes, successTasks);
                                    }
                                    break;
                                case Status.Warning:
                                    if (this.ExecutionGraph.OnWarning != null)
                                    {
                                        var warningTasks = NodesToTasks(this.ExecutionGraph.OnWarning.DoNodes);
                                        RunTasks(this.ExecutionGraph.OnWarning.DoNodes, warningTasks);
                                    }
                                    break;
                                case Status.Error:
                                    if (this.ExecutionGraph.OnError != null)
                                    {
                                        var errorTasks = NodesToTasks(this.ExecutionGraph.OnError.DoNodes);
                                        RunTasks(this.ExecutionGraph.OnError.DoNodes, errorTasks);
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
                        foreach (List<FileInf> files in this.FilesPerTask.Values) files.Clear();
                        foreach (List<Entity> entities in this.EntitiesPerTask.Values) entities.Clear();
                        this._thread = null;
                        this.IsRunning = false;
                        GC.Collect();

                        Logger.InfoFormat("{0} Workflow finished.", this.LogTag);
                        this.JobId++;
                    }
                }));

            this._thread = thread;
            thread.Start();
        }

        private IEnumerable<Task> NodesToTasks(IEnumerable<Node> nodes)
        {
            List<Task> tasks = new List<Task>();

            foreach (var node in nodes)
            {
                if (node is DoIf)
                {
                    var DoTasks = NodesToTasks(((DoIf)node).DoNodes);
                    var OtherwiseTasks = NodesToTasks(((DoIf)node).OtherwiseNodes);

                    List<Task> ifTasks = new List<Task>(DoTasks);
                    foreach (var task in OtherwiseTasks)
                    { 
                        if(!ifTasks.Any(t => t.Id == task.Id))
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
                    Task task = GetTask(node.Id);

                    if (task != null)
                    {
                        if (!tasks.Any(t => t.Id == task.Id))
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

        private Status RunTasks(IEnumerable<Node> nodes, IEnumerable<Task> tasks)
        {
            bool success = true;
            bool warning = false;
            bool atLeastOneSucceed = false;

            if (nodes.Count() > 0)
            {
                Node startNode = nodes.ElementAt(0);

                if (startNode is DoIf)
                {
                    DoIf doIf = (DoIf)startNode;
                    RunDoIf(tasks, nodes, doIf, ref success, ref warning, ref atLeastOneSucceed);
                }
                else if (startNode is DoWhile)
                {
                    DoWhile doWhile = (DoWhile)startNode;
                    RunDoWhile(tasks, nodes, doWhile, ref success, ref warning, ref atLeastOneSucceed);
                }
                else
                {
                    if (startNode.ParentId == START_ID)
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
            else
            {
                if (atLeastOneSucceed || warning)
                {
                    return Status.Warning;
                }
                else
                {
                    return Status.Error;
                }
            }
        }

        private void RunSequentialTasks(IEnumerable<Task> tasks, ref bool success, ref bool warning, ref bool atLeastOneSucceed)
        {
            foreach (Task task in tasks)
            {
                if (task.IsEnabled)
                {
                    TaskStatus status = task.Run();
                    success &= status.Status == Status.Success;
                    warning |= status.Status == Status.Warning;
                    if (!atLeastOneSucceed && status.Status == Status.Success) atLeastOneSucceed = true;
                }
            }
        }

        private void RunTasks(IEnumerable<Task> tasks, IEnumerable<Node> nodes, Node node, ref bool success, ref bool warning, ref bool atLeastOneSucceed)
        {
            if(node != null)
            {
                Task task = GetTask(tasks, node.Id);
                if (task != null)
                {
                    if (task.IsEnabled)
                    {
                        TaskStatus status = task.Run();

                        success &= status.Status == Status.Success;
                        warning |= status.Status == Status.Warning;
                        if (!atLeastOneSucceed && status.Status == Status.Success) atLeastOneSucceed = true;

                        Node childNode = nodes.Where(n => n.ParentId == node.Id).FirstOrDefault();

                        if (childNode != null)
                        {
                            if (childNode is DoIf)
                            {
                                DoIf doIf = (DoIf)childNode;
                                RunDoIf(tasks, nodes, doIf, ref success, ref warning, ref atLeastOneSucceed);
                            }
                            else if (childNode is DoWhile)
                            {
                                DoWhile doWhile = (DoWhile)childNode;
                                RunDoWhile(tasks, nodes, doWhile, ref success, ref warning, ref atLeastOneSucceed);
                            }
                            else
                            {
                                Task childTask = GetTask(tasks, childNode.Id);
                                if (childTask != null)
                                {
                                    if (childTask.IsEnabled)
                                    {
                                        TaskStatus childStatus = childTask.Run();

                                        success &= childStatus.Status == Status.Success;
                                        warning |= childStatus.Status == Status.Warning;
                                        if (!atLeastOneSucceed && status.Status == Status.Success) atLeastOneSucceed = true;

                                        // Recusive call
                                        Node ccNode = nodes.Where(n => n.ParentId == childNode.Id).FirstOrDefault();

                                        if (ccNode is DoIf)
                                        {
                                            DoIf doIf = (DoIf)ccNode;
                                            RunDoIf(tasks, nodes, doIf, ref success, ref warning, ref atLeastOneSucceed);
                                        }
                                        else if (ccNode is DoWhile)
                                        {
                                            DoWhile doWhile = (DoWhile)ccNode;
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

        private void RunDoIf(IEnumerable<Task> tasks, IEnumerable<Node> nodes, DoIf doIf, ref bool success, ref bool warning, ref bool atLeastOneSucceed)
        {
            Task ifTask = GetTask(doIf.If);

            if (ifTask != null)
            {
                if (ifTask.IsEnabled)
                {
                    TaskStatus status = ifTask.Run();

                    success &= status.Status == Status.Success;
                    warning |= status.Status == Status.Warning;
                    if (!atLeastOneSucceed && status.Status == Status.Success) atLeastOneSucceed = true;

                    if (status.Condition == true)
                    {
                        if (doIf.DoNodes.Length > 0)
                        {
                            // Build Tasks
                            var doIfTasks = NodesToTasks(doIf.DoNodes);

                            // Run Tasks
                            Node doIfStartNode = doIf.DoNodes[0];

                            if (doIfStartNode.ParentId == START_ID)
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

                            if (otherwiseStartNode.ParentId == START_ID)
                            {
                                RunTasks(otherwiseTasks, doIf.OtherwiseNodes, otherwiseStartNode, ref success, ref warning, ref atLeastOneSucceed);
                            }
                        }
                    }

                    // Child node
                    Node childNode = nodes.Where(n => n.ParentId == doIf.Id).FirstOrDefault();

                    if (childNode != null)
                    {
                        RunTasks(this.Taks, nodes, childNode, ref success, ref warning, ref atLeastOneSucceed);
                    }
                }
            }
            else
            {
                throw new Exception("Task " + doIf.Id + " not found.");
            }
        }

        private void RunDoWhile(IEnumerable<Task> tasks, IEnumerable<Node> nodes, DoWhile doWhile, ref bool success, ref bool warning, ref bool atLeastOneSucceed)
        {
            Task whileTask = GetTask(doWhile.While);

            if (whileTask != null)
            {
                if (whileTask.IsEnabled)
                {
                    while (true)
                    {
                        TaskStatus status = whileTask.Run();

                        success &= status.Status == Status.Success;
                        warning |= status.Status == Status.Warning;
                        if (!atLeastOneSucceed && status.Status == Status.Success) atLeastOneSucceed = true;

                        if (status.Condition == true)
                        {
                            if (doWhile.DoNodes.Length > 0)
                            {
                                // Build Tasks
                                var doWhileTasks = NodesToTasks(doWhile.DoNodes);

                                // Run Tasks
                                Node doWhileStartNode = doWhile.DoNodes[0];

                                if (doWhileStartNode.ParentId == START_ID)
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
                    Node childNode = nodes.Where(n => n.ParentId == doWhile.Id).FirstOrDefault();

                    if (childNode != null)
                    {
                        RunTasks(this.Taks, nodes, childNode, ref success, ref warning, ref atLeastOneSucceed);
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
            if (this._thread != null && this.IsRunning && !this.IsPaused)
            {
                try
                {
                    this._thread.Abort();
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("An error occured while stopping the workflow : {0}", e, this);
                }
                finally
                {
                    this.IsRunning = false;
                    Logger.InfoFormat("{0} Workflow finished.", this.LogTag);
                    this.JobId++;
                }
            }
        }

        public void Pause()
        {
            if (this._thread != null)
            {
                try
                {
                    this._thread.Suspend();
                    this.IsPaused = true;
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("An error occured while suspending the workflow : {0}", e, this);
                }
            }
        }

        public void Resume()
        {
            if (this._thread != null && this.IsPaused)
            {
                try
                {
                    this._thread.Resume();
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("An error occured while resuming the workflow : {0}", e, this);
                }
                finally
                {
                    this.IsPaused = false;
                }
            }
        }

        private void CreateTempFolder()
        { 
            // WorkflowId/dd-MM-yyyy/HH-mm-ss-fff
            string wfTempFolder = Path.Combine(this.WexflowTempFolder, this.Id.ToString());
            if (!Directory.Exists(wfTempFolder)) Directory.CreateDirectory(wfTempFolder);
            
            string wfDayTempFolder = Path.Combine(wfTempFolder, string.Format("{0:yyyy-MM-dd}", DateTime.Now));
            if (!Directory.Exists(wfDayTempFolder)) Directory.CreateDirectory(wfDayTempFolder);

            
            string wfJobTempFolder = Path.Combine(wfDayTempFolder, string.Format("{0:HH-mm-ss-fff}", DateTime.Now));
            if (!Directory.Exists(wfJobTempFolder)) Directory.CreateDirectory(wfJobTempFolder);

            this.WorkflowTempFolder = wfJobTempFolder;
        }

        private Task GetTask(int id)
        {
            return this.Taks.FirstOrDefault(t => t.Id == id);
        }

        private Task GetTask(IEnumerable<Task> tasks, int id)
        {
            return tasks.FirstOrDefault(t => t.Id == id);
        }
    }
}
