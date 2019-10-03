using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;
using Wexflow.Core.Db;
using Wexflow.Core.ExecutionGraph;
using Wexflow.Core.ExecutionGraph.Flowchart;

namespace Wexflow.Core
{
    /// <summary>
    /// Workflow.
    /// </summary>
    public class Workflow
    {
        /// <summary>
        /// This constant is used to determine the key size of the encryption algorithm in bits.
        /// We divide this by 8 within the code below to get the equivalent number of bytes.
        /// </summary>
        public static readonly int KeySize = 128;
        /// <summary>
        /// This constant determines the number of iterations for the password bytes generation function. 
        /// </summary>
        public static readonly int DerivationIterations = 1000;
        /// <summary>
        /// PassPhrase.
        /// </summary>
        public static readonly string PassPhrase = "FHMWW-EORNR-XXF0Q-E8Q#G-YC!RG-KV=TN-M9MQJ-AySDI-LAC5Q-UV==QE-VSVNL-OV1IZ";

        /// <summary>
        /// Default parent node id to start with in the execution graph.
        /// </summary>
        public const int StartId = -1;

        /// <summary>
        /// Database ID.
        /// </summary>
        public string DbId { get; private set; }
        /// <summary>
        /// XML of the workflow.
        /// </summary>
        public string Xml { get; private set; }
        /// <summary>
        /// Wexflow temp folder.
        /// </summary>
        public string WexflowTempFolder { get; private set; }
        /// <summary>
        /// Workflows temp folder used for global variables parsing.
        /// </summary>
        public string WorkflowsTempFolder { get; private set; }
        /// <summary>
        /// Workflow temp folder.
        /// </summary>
        public string WorkflowTempFolder { get; private set; }
        /// <summary>
        /// Approval folder.
        /// </summary>
        public string ApprovalFolder { get; private set; }
        /// <summary>
        /// XSD path.
        /// </summary>
        public string XsdPath { get; private set; }
        /// <summary>
        /// Workflow Id.
        /// </summary>
        public int Id { get; private set; }
        /// <summary>
        /// Workflow name.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Workflow description.
        /// </summary>
        public string Description { get; private set; }
        /// <summary>
        /// Workflow lanch type.
        /// </summary>
        public LaunchType LaunchType { get; private set; }
        /// <summary>
        /// Workflow period.
        /// </summary>
        public TimeSpan Period { get; private set; }
        /// <summary>
        /// Cron expression
        /// </summary>
        public string CronExpression { get; private set; }
        /// <summary>
        /// Shows whether this workflow is enabled or not.
        /// </summary>
        public bool IsEnabled { get; private set; }
        /// <summary>
        /// Shows whether this workflow is an approval workflow or not.
        /// </summary>
        public bool IsApproval { get; private set; }
        /// <summary>
        /// Shows whether this workflow is waiting for approval.
        /// </summary>
        public bool IsWaitingForApproval { get; set; }
        /// <summary>
        /// Shows whether this workflow is disapproved or not.
        /// </summary>
        public bool IsDisapproved { get; private set; }
        /// <summary>
        /// Shows whether this workflow is running or not.
        /// </summary>
        public bool IsRunning { get; private set; }
        /// <summary>
        /// Shows whether this workflow is suspended or not.
        /// </summary>
        public bool IsPaused { get; private set; }
        /// <summary>
        /// Workflow tasks.
        /// </summary>
        public Task[] Tasks { get; private set; }
        /// <summary>
        /// Workflow files.
        /// </summary>
        public Dictionary<int, List<FileInf>> FilesPerTask { get; private set; }
        /// <summary>
        /// Workflow entities.
        /// </summary>
        public Dictionary<int, List<Entity>> EntitiesPerTask { get; private set; }
        /// <summary>
        /// Job Id.
        /// </summary>
        public int JobId { get; private set; }
        /// <summary>
        /// Log tag.
        /// </summary>
        public string LogTag { get { return string.Format("[{0} / {1}]", Name, JobId); } }
        /// <summary>
        /// Xml Namespace Manager.
        /// </summary>
        public XmlNamespaceManager XmlNamespaceManager { get; private set; }
        /// <summary>
        /// Execution graph.
        /// </summary>
        public Graph ExecutionGraph { get; private set; }
        /// <summary>
        /// Workflow XDocument.
        /// </summary>
        public XDocument XDoc { get; private set; }
        /// <summary>
        /// XNamespace.
        /// </summary>
        public XNamespace XNamespaceWf { get; private set; }
        /// <summary>
        /// Shows whether the execution graph is empty or not.
        /// </summary>
        public bool IsExecutionGraphEmpty { get; private set; }
        /// <summary>
        /// Hashtable used as shared memory for tasks.
        /// </summary>
        public Hashtable Hashtable { get; private set; }
        /// <summary>
        /// Database.
        /// </summary>
        public Db.Db Database { get; private set; }
        /// <summary>
        /// Global variables.
        /// </summary>
        public Variable[] GlobalVariables { get; private set; }
        /// <summary>
        /// Local variables.
        /// </summary>
        public Variable[] LocalVariables { get; private set; }
        /// <summary>
        /// Rest variables.
        /// </summary>
        public List<Variable> RestVariables { get; private set; }
        /// <summary>
        /// Tasks folder.
        /// </summary>
        public string TasksFolder { get; private set; }

        private Queue<Job> _jobsQueue;
        private Thread _thread;
        private HistoryEntry _historyEntry;

        /// <summary>
        /// Creates a new workflow.
        /// </summary>
        /// <param name="dbId">Database ID.</param>
        /// <param name="xml">XML of the workflow.</param>
        /// <param name="wexflowTempFolder">Wexflow temp folder.</param>
        /// <param name="workflowsTempFolder">Workflows temp folder.</param>
        /// <param name="tasksFolder">Tasks folder.</param>
        /// <param name="approvalFolder">Approval folder.</param>
        /// <param name="xsdPath">XSD path.</param>
        /// <param name="database">Database.</param>
        /// <param name="globalVariables">Global variables.</param>
        public Workflow(
              string dbId
            , string xml
            , string wexflowTempFolder
            , string workflowsTempFolder
            , string tasksFolder
            , string approvalFolder
            , string xsdPath
            , Db.Db database
            , Variable[] globalVariables)
        {
            JobId = 1;
            _jobsQueue = new Queue<Job>();
            _thread = null;
            DbId = dbId;
            Xml = xml;
            WexflowTempFolder = wexflowTempFolder;
            WorkflowsTempFolder = workflowsTempFolder;
            TasksFolder = tasksFolder;
            ApprovalFolder = approvalFolder;
            XsdPath = xsdPath;
            Database = database;
            FilesPerTask = new Dictionary<int, List<FileInf>>();
            EntitiesPerTask = new Dictionary<int, List<Entity>>();
            Hashtable = new Hashtable();
            GlobalVariables = globalVariables;
            RestVariables = new List<Variable>();
            Check();
            LoadLocalVariables();
            Load(Xml);

            if (!IsEnabled)
            {
                Database.IncrementDisabledCount();
            }
        }

        /// <summary>
        /// Returns informations about this workflow.
        /// </summary>
        /// <returns>Informations about this workflow.</returns>
        public override string ToString()
        {
            return string.Format("{{id: {0}, name: {1}, enabled: {2}, launchType: {3}}}", Id, Name, IsEnabled, LaunchType);
        }

        private void Check()
        {
            var schemas = new XmlSchemaSet();
            schemas.Add("urn:wexflow-schema", XsdPath);

            var doc = XDocument.Parse(Xml);
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

        private void LoadLocalVariables()
        {
            using (var xmlReader = XmlReader.Create(new StringReader(Xml)))
            {
                var xmlNameTable = xmlReader.NameTable;
                if (xmlNameTable != null)
                {
                    XmlNamespaceManager = new XmlNamespaceManager(xmlNameTable);
                    XmlNamespaceManager.AddNamespace("wf", "urn:wexflow-schema");
                }
                else
                {
                    throw new Exception("xmlNameTable of " + Id + " is null");
                }

                var xdoc = XDocument.Parse(Xml);
                List<Variable> localVariables = new List<Variable>();

                foreach (var xvariable in xdoc.XPathSelectElements("/wf:Workflow/wf:LocalVariables/wf:Variable",
                    XmlNamespaceManager))
                {
                    string key = xvariable.Attribute("name").Value;
                    string value = xvariable.Attribute("value").Value;

                    Variable variable = new Variable
                    {
                        Key = key,
                        Value = value
                    };

                    localVariables.Add(variable);
                }

                LocalVariables = localVariables.ToArray();
            }
        }

        private string Parse(string src)
        {
            var dest = string.Empty;

            //
            // Parse global variables.
            //
            using (StringReader sr = new StringReader(src))
            using (StringWriter sw = new StringWriter())
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    foreach (var variable in GlobalVariables)
                    {
                        line = line.Replace("$" + variable.Key, variable.Value);
                    }
                    sw.WriteLine(line);
                }
                dest = sw.ToString();
            }

            //
            // Load local variables with their final values (parsed)
            //
            List<Variable> localVariablesParsed = new List<Variable>();
            using (var xmlReader = XmlReader.Create(new StringReader(dest)))
            {
                var xmlNameTable = xmlReader.NameTable;
                if (xmlNameTable != null)
                {
                    XmlNamespaceManager = new XmlNamespaceManager(xmlNameTable);
                    XmlNamespaceManager.AddNamespace("wf", "urn:wexflow-schema");
                }
                else
                {
                    throw new Exception("xmlNameTable of " + Id + " is null");
                }

                var xdoc = XDocument.Parse(dest);

                foreach (var xvariable in xdoc.XPathSelectElements("/wf:Workflow/wf:LocalVariables/wf:Variable",
                    XmlNamespaceManager))
                {
                    string key = xvariable.Attribute("name").Value;
                    string value = xvariable.Attribute("value").Value;

                    Variable variable = new Variable
                    {
                        Key = key,
                        Value = value
                    };

                    localVariablesParsed.Add(variable);
                }
            }

            //
            // Parse local variables.
            //
            var res = string.Empty;
            using (StringReader sr = new StringReader(dest))
            using (StringWriter sw = new StringWriter())
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    foreach (var variable in localVariablesParsed)
                    {
                        line = line.Replace("$" + variable.Key, variable.Value);
                    }
                    sw.WriteLine(line);
                }
                res = sw.ToString();
            }

            //
            // Parse Rest variables.
            //
            var res2 = string.Empty;
            using (StringReader sr = new StringReader(res))
            using (StringWriter sw = new StringWriter())
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    foreach (var variable in RestVariables)
                    {
                        line = line.Replace("$" + variable.Key, variable.Value);
                    }
                    sw.WriteLine(line);
                }
                res2 = sw.ToString();
            }

            return res2;
        }

        private void Load(string xml)
        {
            FilesPerTask.Clear();
            EntitiesPerTask.Clear();

            using (var xmlReader = XmlReader.Create(new StringReader(xml)))
            {
                var xmlNameTable = xmlReader.NameTable;
                if (xmlNameTable != null)
                {
                    XmlNamespaceManager = new XmlNamespaceManager(xmlNameTable);
                    XmlNamespaceManager.AddNamespace("wf", "urn:wexflow-schema");
                }
                else
                {
                    throw new Exception("xmlNameTable of " + Id + " is null");
                }

                // Loading settings
                var xdoc = XDocument.Parse(xml);
                XDoc = xdoc;
                XNamespaceWf = "urn:wexflow-schema";

                Id = int.Parse(GetWorkflowAttribute(xdoc, "id"));
                Name = GetWorkflowAttribute(xdoc, "name");
                Description = GetWorkflowAttribute(xdoc, "description");
                LaunchType = (LaunchType)Enum.Parse(typeof(LaunchType), GetWorkflowSetting(xdoc, "launchType", true), true);

                string period = GetWorkflowSetting(xdoc, "period", false);
                if (LaunchType == LaunchType.Periodic || !string.IsNullOrEmpty(period))
                {
                    Period = TimeSpan.Parse(period);
                }

                string cronexp = GetWorkflowSetting(xdoc, "cronExpression", false);
                if (LaunchType == LaunchType.Cron || !string.IsNullOrEmpty(cronexp))
                {
                    CronExpression = cronexp;
                    if (!WexflowEngine.IsCronExpressionValid(CronExpression))
                    {
                        throw new Exception("The cron expression '" + CronExpression + "' is not valid.");
                    }
                }
                IsEnabled = bool.Parse(GetWorkflowSetting(xdoc, "enabled", true));
                var isApprovalStr = GetWorkflowSetting(xdoc, "approval", false);
                IsApproval = bool.Parse(string.IsNullOrEmpty(isApprovalStr) ? "false" : isApprovalStr);

                if (xdoc.Root != null)
                {
                    var xExecutionGraph = xdoc.Root.Element(XNamespaceWf + "ExecutionGraph");
                    IsExecutionGraphEmpty = xExecutionGraph == null || !xExecutionGraph.Elements().Any();
                }

                // Loading tasks
                var tasks = new List<Task>();
                foreach (var xTask in xdoc.XPathSelectElements("/wf:Workflow/wf:Tasks/wf:Task", XmlNamespaceManager))
                {
                    var xAttribute = xTask.Attribute("name");
                    if (xAttribute != null)
                    {
                        Type type = null;
                        var name = xAttribute.Value;
                        var assemblyName = "Wexflow.Tasks." + name;
                        var typeName = "Wexflow.Tasks." + name + "." + name + ", " + assemblyName;

                        // Try to load from root
                        type = Type.GetType(typeName);

                        if (type == null) // Try to load from Tasks folder
                        {
                            var taskAssemblyFile = Path.Combine(TasksFolder, assemblyName + ".dll");
                            if (File.Exists(taskAssemblyFile))
                            {
                                var taskAssembly = Assembly.LoadFile(taskAssemblyFile);
                                var typeFullName = "Wexflow.Tasks." + name + "." + name;
                                type = taskAssembly.GetType(typeFullName);
                            }
                        }

                        if (type != null)
                        {
                            var task = (Task)Activator.CreateInstance(type, xTask, this);
                            tasks.Add(task);
                        }
                        else
                        {
                            throw new Exception("The type of the task " + name + " could not be loaded.");
                        }
                    }
                    else
                    {
                        throw new Exception("Name attribute of the task " + xTask + " does not exist.");
                    }
                }
                Tasks = tasks.ToArray();

                // Loading execution graph
                var xExectionGraph = xdoc.XPathSelectElement("/wf:Workflow/wf:ExecutionGraph", XmlNamespaceManager);
                if (xExectionGraph != null)
                {
                    var taskNodes = GetTaskNodes(xExectionGraph);

                    // Check startup node, parallel tasks and infinite loops
                    if (taskNodes.Any()) CheckStartupNode(taskNodes, "Startup node with parentId=-1 not found in ExecutionGraph execution graph.");
                    CheckParallelTasks(taskNodes, "Parallel tasks execution detected in ExecutionGraph execution graph.");
                    CheckInfiniteLoop(taskNodes, "Infinite loop detected in ExecutionGraph execution graph.");

                    // OnSuccess
                    GraphEvent onSuccess = null;
                    var xOnSuccess = xExectionGraph.XPathSelectElement("wf:OnSuccess", XmlNamespaceManager);
                    if (xOnSuccess != null)
                    {
                        var onSuccessNodes = GetTaskNodes(xOnSuccess);
                        CheckStartupNode(onSuccessNodes, "Startup node with parentId=-1 not found in OnSuccess execution graph.");
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
                        CheckStartupNode(onWarningNodes, "Startup node with parentId=-1 not found in OnWarning execution graph.");
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
                        CheckStartupNode(onErrorNodes, "Startup node with parentId=-1 not found in OnError execution graph.");
                        CheckParallelTasks(onErrorNodes, "Parallel tasks execution detected in OnError execution graph.");
                        CheckInfiniteLoop(onErrorNodes, "Infinite loop detected in OnError execution graph.");
                        onError = new GraphEvent(onErrorNodes);
                    }

                    // OnDisapproved
                    GraphEvent onDisapproved = null;
                    var xOnDispproved = xExectionGraph.XPathSelectElement("wf:OnDisapproved", XmlNamespaceManager);
                    if (xOnDispproved != null)
                    {
                        var onDisapproveNodes = GetTaskNodes(xOnDispproved);
                        CheckStartupNode(onDisapproveNodes, "Startup node with parentId=-1 not found in OnError execution graph.");
                        CheckParallelTasks(onDisapproveNodes, "Parallel tasks execution detected in OnError execution graph.");
                        CheckInfiniteLoop(onDisapproveNodes, "Infinite loop detected in OnError execution graph.");
                        onDisapproved = new GraphEvent(onDisapproveNodes);
                    }


                    ExecutionGraph = new Graph(taskNodes, onSuccess, onWarning, onError, onDisapproved);
                }
            }
        }

        private Node[] GetTaskNodes(XElement xExectionGraph)
        {
            var nodes = xExectionGraph
                .Elements()
                .Where(xe => xe.Name.LocalName != "OnSuccess" && xe.Name.LocalName != "OnWarning" && xe.Name.LocalName != "OnError" && xe.Name.LocalName != "OnDisapproved")
                .Select(XNodeToNode)
                .ToArray();

            return nodes;
        }

        private If XIfToIf(XElement xIf)
        {
            var xId = xIf.Attribute("id");
            if (xId == null) throw new Exception("If id attribute not found.");
            var id = int.Parse(xId.Value);
            var xParent = xIf.Attribute("parent");
            if (xParent == null) throw new Exception("If parent attribute not found.");
            var parentId = int.Parse(xParent.Value);
            var xIfId = xIf.Attribute("if");
            if (xIfId == null) throw new Exception("If attribute not found.");
            var ifId = int.Parse(xIfId.Value);

            // Do nodes
            var doNodes = xIf.XPathSelectElement("wf:Do", XmlNamespaceManager)
                .Elements()
                .Select(XNodeToNode)
                .ToArray();

            CheckStartupNode(doNodes, "Startup node with parentId=-1 not found in DoIf>Do execution graph.");
            CheckParallelTasks(doNodes, "Parallel tasks execution detected in DoIf>Do execution graph.");
            CheckInfiniteLoop(doNodes, "Infinite loop detected in DoIf>Do execution graph.");

            // Otherwise nodes
            Node[] elseNodes = null;
            var xElse = xIf.XPathSelectElement("wf:Else", XmlNamespaceManager);
            if (xElse != null)
            {
                elseNodes = xElse
                    .Elements()
                    .Select(XNodeToNode)
                    .ToArray();

                CheckStartupNode(elseNodes, "Startup node with parentId=-1 not found in DoIf>Otherwise execution graph.");
                CheckParallelTasks(elseNodes, "Parallel tasks execution detected in DoIf>Otherwise execution graph.");
                CheckInfiniteLoop(elseNodes, "Infinite loop detected in DoIf>Otherwise execution graph.");
            }

            return new If(id, parentId, ifId, doNodes, elseNodes);
        }

        private While XWhileToWhile(XElement xWhile)
        {
            var xId = xWhile.Attribute("id");
            if (xId == null) throw new Exception("While Id attribute not found.");
            var id = int.Parse(xId.Value);
            var xParent = xWhile.Attribute("parent");
            if (xParent == null) throw new Exception("While parent attribute not found.");
            var parentId = int.Parse(xParent.Value);
            var xWhileId = xWhile.Attribute("while");
            if (xWhileId == null) throw new Exception("While attribute not found.");
            var whileId = int.Parse(xWhileId.Value);

            var doNodes = xWhile
                .Elements()
                .Select(XNodeToNode)
                .ToArray();

            CheckStartupNode(doNodes, "Startup node with parentId=-1 not found in DoWhile>Do execution graph.");
            CheckParallelTasks(doNodes, "Parallel tasks execution detected in DoWhile>Do execution graph.");
            CheckInfiniteLoop(doNodes, "Infinite loop detected in DoWhile>Do execution graph.");

            return new While(id, parentId, whileId, doNodes);
        }

        private Switch XSwitchToSwitch(XElement xSwitch)
        {
            var xId = xSwitch.Attribute("id");
            if (xId == null) throw new Exception("Switch Id attribute not found.");
            var id = int.Parse(xId.Value);
            var xParent = xSwitch.Attribute("parent");
            if (xParent == null) throw new Exception("Switch parent attribute not found.");
            var parentId = int.Parse(xParent.Value);
            var xSwitchId = xSwitch.Attribute("switch");
            if (xSwitchId == null) throw new Exception("Switch attribute not found.");
            var switchId = int.Parse(xSwitchId.Value);

            var cases = xSwitch
                .XPathSelectElements("wf:Case", XmlNamespaceManager)
                .Select(xCase =>
                {
                    var xValue = xCase.Attribute("value");
                    if (xValue == null) throw new Exception("Case value attribute not found.");
                    var val = xValue.Value;

                    var nodes = xCase
                        .Elements()
                        .Select(XNodeToNode)
                        .ToArray();

                    var nodeName = string.Format("Switch>Case(value={0})", val);
                    CheckStartupNode(nodes, "Startup node with parentId=-1 not found in " + nodeName + " execution graph.");
                    CheckParallelTasks(nodes, "Parallel tasks execution detected in " + nodeName + " execution graph.");
                    CheckInfiniteLoop(nodes, "Infinite loop detected in " + nodeName + " execution graph.");

                    return new Case(val, nodes);
                });

            var xDefault = xSwitch.XPathSelectElement("wf:Default", XmlNamespaceManager);
            if (xDefault == null) return new Switch(id, parentId, switchId, cases, null);
            var @default = xDefault
                .Elements()
                .Select(XNodeToNode)
                .ToArray();

            if (@default.Length > 0)
            {
                CheckStartupNode(@default,
                    "Startup node with parentId=-1 not found in Switch>Default execution graph.");
                CheckParallelTasks(@default, "Parallel tasks execution detected in Switch>Default execution graph.");
                CheckInfiniteLoop(@default, "Infinite loop detected in Switch>Default execution graph.");
            }

            return new Switch(id, parentId, switchId, cases, @default);
        }

        private Node XNodeToNode(XElement xNode)
        {
            switch (xNode.Name.LocalName)
            {
                case "Task":
                    var xId = xNode.Attribute("id");
                    if (xId == null) throw new Exception("Task id not found.");
                    var id = int.Parse(xId.Value);

                    var xParentId = xNode.XPathSelectElement("wf:Parent", XmlNamespaceManager)
                        .Attribute("id");

                    if (xParentId == null) throw new Exception("Parent id not found.");
                    var parentId = int.Parse(xParentId.Value);

                    var node = new Node(id, parentId);
                    return node;
                case "If":
                    return XIfToIf(xNode);
                case "While":
                    return XWhileToWhile(xNode);
                case "Switch":
                    return XSwitchToSwitch(xNode);
                default:
                    throw new Exception(xNode.Name.LocalName + " is not supported.");
            }
        }

        private void CheckStartupNode(Node[] nodes, string errorMsg)
        {
            if (nodes == null) throw new ArgumentNullException(); // new ArgumentNullException(nameof(nodes))
            if (nodes.All(n => n.ParentId != StartId)) throw new Exception(errorMsg);
        }

        private void CheckParallelTasks(Node[] taskNodes, string errorMsg)
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

        private void CheckInfiniteLoop(Node[] taskNodes, string errorMsg)
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

        private bool CheckInfiniteLoop(Node startNode, Node[] taskNodes)
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
            var xAttribute = xdoc.XPathSelectElement("/wf:Workflow", XmlNamespaceManager).Attribute(attr);
            if (xAttribute != null)
            {
                return xAttribute.Value;
            }

            throw new Exception("Workflow attribute " + attr + "not found.");
        }

        private string GetWorkflowSetting(XDocument xdoc, string name, bool throwExceptionIfNotFound)
        {
            var xSetting = xdoc
                .XPathSelectElement(
                    string.Format("/wf:Workflow[@id='{0}']/wf:Settings/wf:Setting[@name='{1}']", Id, name),
                    XmlNamespaceManager);

            if (xSetting != null)
            {
                var xAttribute = xSetting.Attribute("value");
                if (xAttribute != null)
                {
                    return xAttribute.Value;
                }
                else if (throwExceptionIfNotFound)
                {
                    throw new Exception("Workflow setting " + name + " not found.");
                }
            }
            else if (throwExceptionIfNotFound)
            {
                throw new Exception("Workflow setting " + name + " not found.");
            }

            return string.Empty;
        }

        /// <summary>
        /// Starts this workflow.
        /// </summary>
        public void Start()
        {
            if (IsRunning)
            {
                var job = new Job { Workflow = this, QueuedOn = DateTime.Now };
                _jobsQueue.Enqueue(job);
                return;
            }

            //
            // Parse the workflow file (Global variables and local variables.)
            //
            var dest = Parse(Xml);
            Load(dest);

            Database.IncrementRunningCount();

            var entry = Database.GetEntry(Id);
            if (entry == null)
            {
                var newEntry = new Entry
                {
                    WorkflowId = Id,
                    Name = Name,
                    LaunchType = ((Db.LaunchType)(int)LaunchType),
                    Description = Description,
                    Status = Db.Status.Running,
                    StatusDate = DateTime.Now
                };
                Database.InsertEntry(newEntry);
            }
            else
            {
                entry.Status = Db.Status.Running;
                entry.StatusDate = DateTime.Now;
                Database.UpdateEntry(entry.GetDbId(), entry);
            }
            entry = Database.GetEntry(Id);

            _historyEntry = new HistoryEntry
            {
                WorkflowId = Id,
                Name = Name,
                LaunchType = ((Db.LaunchType)(int)LaunchType),
                Description = Description
            };

            var thread = new Thread(() =>
                {
                    try
                    {
                        IsRunning = true;
                        IsDisapproved = false;
                        Logger.InfoFormat("{0} Workflow started.", LogTag);

                        // Create the temp folder
                        CreateTempFolder();

                        // Run the tasks
                        if (ExecutionGraph == null)
                        {
                            bool success = true;
                            bool warning = false;
                            bool error = true;
                            RunSequentialTasks(Tasks, ref success, ref warning, ref error);

                            if (IsDisapproved)
                            {
                                Database.IncrementDisapprovedCount();
                                entry.Status = Db.Status.Disapproved;
                                entry.StatusDate = DateTime.Now;
                                Database.UpdateEntry(entry.GetDbId(), entry);
                                _historyEntry.Status = Db.Status.Disapproved;
                            }
                            else
                            {
                                if (success)
                                {
                                    Database.IncrementDoneCount();
                                    entry.Status = Db.Status.Done;
                                    entry.StatusDate = DateTime.Now;
                                    Database.UpdateEntry(entry.GetDbId(), entry);
                                    _historyEntry.Status = Db.Status.Done;

                                }
                                else if (warning)
                                {
                                    Database.IncrementWarningCount();
                                    entry.Status = Db.Status.Warning;
                                    entry.StatusDate = DateTime.Now;
                                    Database.UpdateEntry(entry.GetDbId(), entry);
                                    _historyEntry.Status = Db.Status.Warning;
                                }
                                else if (error)
                                {
                                    Database.IncrementFailedCount();
                                    entry.Status = Db.Status.Failed;
                                    entry.StatusDate = DateTime.Now;
                                    Database.UpdateEntry(entry.GetDbId(), entry);
                                    _historyEntry.Status = Db.Status.Failed;
                                }
                            }
                        }
                        else
                        {
                            var status = RunTasks(ExecutionGraph.Nodes, Tasks, false);

                            switch (status)
                            {
                                case Status.Success:
                                    if (ExecutionGraph.OnSuccess != null)
                                    {
                                        var successTasks = NodesToTasks(ExecutionGraph.OnSuccess.Nodes);
                                        RunTasks(ExecutionGraph.OnSuccess.Nodes, successTasks, false);
                                    }
                                    Database.IncrementDoneCount();
                                    entry.Status = Db.Status.Done;
                                    entry.StatusDate = DateTime.Now;
                                    Database.UpdateEntry(entry.GetDbId(), entry);
                                    _historyEntry.Status = Db.Status.Done;
                                    break;
                                case Status.Warning:
                                    if (ExecutionGraph.OnWarning != null)
                                    {
                                        var warningTasks = NodesToTasks(ExecutionGraph.OnWarning.Nodes);
                                        RunTasks(ExecutionGraph.OnWarning.Nodes, warningTasks, false);
                                    }
                                    Database.IncrementWarningCount();
                                    entry.Status = Db.Status.Warning;
                                    entry.StatusDate = DateTime.Now;
                                    Database.UpdateEntry(entry.GetDbId(), entry);
                                    _historyEntry.Status = Db.Status.Warning;
                                    break;
                                case Status.Error:
                                    if (ExecutionGraph.OnError != null)
                                    {
                                        var errorTasks = NodesToTasks(ExecutionGraph.OnError.Nodes);
                                        RunTasks(ExecutionGraph.OnError.Nodes, errorTasks, false);
                                    }
                                    Database.IncrementFailedCount();
                                    entry.Status = Db.Status.Failed;
                                    entry.StatusDate = DateTime.Now;
                                    Database.UpdateEntry(entry.GetDbId(), entry);
                                    _historyEntry.Status = Db.Status.Failed;
                                    break;
                                case Status.Disapproved:
                                    if (ExecutionGraph.OnDisapproved != null)
                                    {
                                        var disapprovedTasks = NodesToTasks(ExecutionGraph.OnDisapproved.Nodes);
                                        RunTasks(ExecutionGraph.OnDisapproved.Nodes, disapprovedTasks, true);
                                    }
                                    Database.IncrementDisapprovedCount();
                                    entry.Status = Db.Status.Disapproved;
                                    entry.StatusDate = DateTime.Now;
                                    Database.UpdateEntry(entry.GetDbId(), entry);
                                    _historyEntry.Status = Db.Status.Disapproved;
                                    break;
                            }
                        }

                        _historyEntry.StatusDate = DateTime.Now;
                        Database.InsertHistoryEntry(_historyEntry);

                        Database.DecrementRunningCount();
                    }
                    catch (ThreadAbortException)
                    {
                    }
                    catch (Exception e)
                    {
                        Logger.ErrorFormat("An error occured while running the workflow. Error: {0}", e, this);
                    }
                    finally
                    {
                        Load(Xml); // Reload the original workflow
                        RestVariables.Clear();

                        // Cleanup
                        foreach (List<FileInf> files in FilesPerTask.Values) files.Clear();
                        foreach (List<Entity> entities in EntitiesPerTask.Values) entities.Clear();
                        _thread = null;
                        IsRunning = false;
                        IsDisapproved = false;
                        GC.Collect();

                        Logger.InfoFormat("{0} Workflow finished.", LogTag);
                        JobId++;

                        if (_jobsQueue.Count > 0)
                        {
                            var job = _jobsQueue.Dequeue();
                            job.Workflow.Start();
                        }
                    }
                });

            _thread = thread;
            thread.Start();
        }

        private Task[] NodesToTasks(Node[] nodes)
        {
            var tasks = new List<Task>();

            if (nodes == null) return tasks.ToArray();

            foreach (var node in nodes)
            {
                var @if = node as If;
                if (@if != null)
                {
                    var doTasks = NodesToTasks(@if.DoNodes);
                    var otherwiseTasks = NodesToTasks(@if.ElseNodes);

                    var ifTasks = new List<Task>(doTasks);
                    foreach (var task in otherwiseTasks)
                    {
                        if (ifTasks.All(t => t.Id != task.Id))
                        {
                            ifTasks.Add(task);
                        }
                    }

                    tasks.AddRange(ifTasks);
                }
                else if (node is While)
                {
                    tasks.AddRange(NodesToTasks(((While)node).Nodes));
                }
                else if (node is Switch)
                {
                    var @switch = (Switch)node;
                    tasks.AddRange(NodesToTasks(@switch.Default).Where(task => tasks.All(t => t.Id != task.Id)));
                    tasks.AddRange(NodesToTasks(@switch.Cases.SelectMany(@case => @case.Nodes).ToArray()).Where(task => tasks.All(t => t.Id != task.Id)));
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

            return tasks.ToArray();
        }

        private Status RunTasks(Node[] nodes, Task[] tasks, bool force)
        {
            var success = true;
            var warning = false;
            var atLeastOneSucceed = false;

            if (nodes.Any())
            {
                var startNode = GetStartupNode(nodes);

                var @if = startNode as If;
                if (@if != null)
                {
                    var doIf = @if;
                    RunIf(tasks, nodes, doIf, force, ref success, ref warning, ref atLeastOneSucceed);
                }
                else if (startNode is While)
                {
                    var doWhile = (While)startNode;
                    RunWhile(tasks, nodes, doWhile, force, ref success, ref warning, ref atLeastOneSucceed);
                }
                else
                {
                    if (startNode.ParentId == StartId)
                    {
                        RunTasks(tasks, nodes, startNode, force, ref success, ref warning, ref atLeastOneSucceed);
                    }
                }
            }

            if (IsDisapproved)
            {
                return Status.Disapproved;
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

        private void RunSequentialTasks(IEnumerable<Task> tasks, ref bool success, ref bool warning, ref bool error)
        {
            var atLeastOneSucceed = false;
            foreach (var task in tasks)
            {
                if (!task.IsEnabled) continue;
                if (IsApproval && IsDisapproved) break;
                var status = task.Run();
                success &= status.Status == Status.Success;
                warning |= status.Status == Status.Warning;
                error &= status.Status == Status.Error;
                if (!atLeastOneSucceed && status.Status == Status.Success) atLeastOneSucceed = true;
            }

            if (tasks.Count() > 0 && !success && atLeastOneSucceed)
            {
                warning = true;
            }
        }

        private void RunTasks(Task[] tasks, Node[] nodes, Node node, bool force, ref bool success, ref bool warning, ref bool atLeastOneSucceed)
        {
            if (node != null)
            {
                if (node is If || node is While || node is Switch)
                {
                    var if1 = node as If;
                    if (if1 != null)
                    {
                        var @if = if1;
                        RunIf(tasks, nodes, @if, force, ref success, ref warning, ref atLeastOneSucceed);
                    }
                    else if (node is While)
                    {
                        var @while = (While)node;
                        RunWhile(tasks, nodes, @while, force, ref success, ref warning, ref atLeastOneSucceed);
                    }
                    else
                    {
                        var @switch = (Switch)node;
                        RunSwitch(tasks, nodes, @switch, force, ref success, ref warning, ref atLeastOneSucceed);
                    }
                }
                else
                {
                    var task = GetTask(tasks, node.Id);
                    if (task != null)
                    {
                        if (task.IsEnabled && ((!IsApproval || (IsApproval && !IsDisapproved)) || force))
                        {
                            var status = task.Run();

                            success &= status.Status == Status.Success;
                            warning |= status.Status == Status.Warning;
                            if (!atLeastOneSucceed && status.Status == Status.Success) atLeastOneSucceed = true;

                            var childNode = nodes.FirstOrDefault(n => n.ParentId == node.Id);

                            if (childNode != null)
                            {
                                var if1 = childNode as If;
                                if (if1 != null)
                                {
                                    var @if = if1;
                                    RunIf(tasks, nodes, @if, force, ref success, ref warning, ref atLeastOneSucceed);
                                }
                                else if (childNode is While)
                                {
                                    var @while = (While)childNode;
                                    RunWhile(tasks, nodes, @while, force, ref success, ref warning, ref atLeastOneSucceed);
                                }
                                else if (childNode is Switch)
                                {
                                    var @switch = (Switch)childNode;
                                    RunSwitch(tasks, nodes, @switch, force, ref success, ref warning, ref atLeastOneSucceed);
                                }
                                else
                                {
                                    var childTask = GetTask(tasks, childNode.Id);
                                    if (childTask != null)
                                    {
                                        if (childTask.IsEnabled && ((!IsApproval || (IsApproval && !IsDisapproved)) || force))
                                        {
                                            var childStatus = childTask.Run();

                                            success &= childStatus.Status == Status.Success;
                                            warning |= childStatus.Status == Status.Warning;
                                            if (!atLeastOneSucceed && status.Status == Status.Success) atLeastOneSucceed = true;

                                            // Recusive call
                                            var ccNode = nodes.FirstOrDefault(n => n.ParentId == childNode.Id);

                                            var node1 = ccNode as If;
                                            if (node1 != null)
                                            {
                                                var @if = node1;
                                                RunIf(tasks, nodes, @if, force, ref success, ref warning, ref atLeastOneSucceed);
                                            }
                                            else if (ccNode is While)
                                            {
                                                var @while = (While)ccNode;
                                                RunWhile(tasks, nodes, @while, force, ref success, ref warning, ref atLeastOneSucceed);
                                            }
                                            else if (ccNode is Switch)
                                            {
                                                var @switch = (Switch)ccNode;
                                                RunSwitch(tasks, nodes, @switch, force, ref success, ref warning, ref atLeastOneSucceed);
                                            }
                                            else
                                            {
                                                RunTasks(tasks, nodes, ccNode, force, ref success, ref warning, ref atLeastOneSucceed);
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
        }

        private void RunIf(Task[] tasks, Node[] nodes, If @if, bool force, ref bool success, ref bool warning, ref bool atLeastOneSucceed)
        {
            var ifTask = GetTask(@if.IfId);

            if (ifTask != null)
            {
                if (ifTask.IsEnabled && (!IsApproval || (IsApproval && !IsDisapproved)))
                {
                    var status = ifTask.Run();

                    success &= status.Status == Status.Success;
                    warning |= status.Status == Status.Warning;
                    if (!atLeastOneSucceed && status.Status == Status.Success) atLeastOneSucceed = true;

                    if (status.Status == Status.Success && status.Condition)
                    {
                        if (@if.DoNodes.Length > 0)
                        {
                            // Build Tasks
                            var doIfTasks = NodesToTasks(@if.DoNodes);

                            // Run Tasks
                            var doIfStartNode = GetStartupNode(@if.DoNodes);

                            if (doIfStartNode.ParentId == StartId)
                            {
                                RunTasks(doIfTasks, @if.DoNodes, doIfStartNode, force, ref success, ref warning, ref atLeastOneSucceed);
                            }
                        }
                    }
                    else if (status.Condition == false)
                    {
                        if (@if.ElseNodes.Length > 0)
                        {
                            // Build Tasks
                            var elseTasks = NodesToTasks(@if.ElseNodes);

                            // Run Tasks
                            var elseStartNode = GetStartupNode(@if.ElseNodes);

                            RunTasks(elseTasks, @if.ElseNodes, elseStartNode, force, ref success, ref warning, ref atLeastOneSucceed);
                        }
                    }

                    // Child node
                    var childNode = nodes.FirstOrDefault(n => n.ParentId == @if.Id);

                    if (childNode != null)
                    {
                        RunTasks(tasks, nodes, childNode, force, ref success, ref warning, ref atLeastOneSucceed);
                    }
                }
            }
            else
            {
                throw new Exception("Task " + @if.Id + " not found.");
            }
        }

        private void RunWhile(Task[] tasks, Node[] nodes, While @while, bool force, ref bool success, ref bool warning, ref bool atLeastOneSucceed)
        {
            var whileTask = GetTask(@while.WhileId);

            if (whileTask != null)
            {
                if (whileTask.IsEnabled && (!IsApproval || (IsApproval && !IsDisapproved)))
                {
                    while (true)
                    {
                        var status = whileTask.Run();

                        success &= status.Status == Status.Success;
                        warning |= status.Status == Status.Warning;
                        if (!atLeastOneSucceed && status.Status == Status.Success) atLeastOneSucceed = true;

                        if (status.Status == Status.Success && status.Condition)
                        {
                            if (@while.Nodes.Length > 0)
                            {
                                // Build Tasks
                                var doWhileTasks = NodesToTasks(@while.Nodes);

                                // Run Tasks
                                var doWhileStartNode = GetStartupNode(@while.Nodes);

                                RunTasks(doWhileTasks, @while.Nodes, doWhileStartNode, force, ref success, ref warning, ref atLeastOneSucceed);
                            }
                        }
                        else if (status.Condition == false)
                        {
                            break;
                        }
                    }

                    // Child node
                    var childNode = nodes.FirstOrDefault(n => n.ParentId == @while.Id);

                    if (childNode != null)
                    {
                        RunTasks(tasks, nodes, childNode, force, ref success, ref warning, ref atLeastOneSucceed);
                    }
                }
            }
            else
            {
                throw new Exception("Task " + @while.Id + " not found.");
            }
        }

        private void RunSwitch(Task[] tasks, Node[] nodes, Switch @switch, bool force, ref bool success, ref bool warning, ref bool atLeastOneSucceed)
        {
            var switchTask = GetTask(@switch.SwitchId);

            if (switchTask != null)
            {
                if (switchTask.IsEnabled && (!IsApproval || (IsApproval && !IsDisapproved)))
                {
                    var status = switchTask.Run();

                    success &= status.Status == Status.Success;
                    warning |= status.Status == Status.Warning;
                    if (!atLeastOneSucceed && status.Status == Status.Success) atLeastOneSucceed = true;

                    if (status.Status == Status.Success)
                    {
                        bool aCaseHasBeenExecuted = false;
                        foreach (var @case in @switch.Cases)
                        {
                            if (@case.Value == status.SwitchValue)
                            {
                                if (@case.Nodes.Length > 0)
                                {
                                    // Build Tasks
                                    var switchTasks = NodesToTasks(@case.Nodes);

                                    // Run Tasks
                                    var switchStartNode = GetStartupNode(@case.Nodes);

                                    RunTasks(switchTasks, @case.Nodes, switchStartNode, force, ref success, ref warning, ref atLeastOneSucceed);
                                }
                                aCaseHasBeenExecuted = true;
                                break;
                            }
                        }

                        if (!aCaseHasBeenExecuted && @switch.Default != null && @switch.Default.Any())
                        {
                            // Build Tasks
                            var defalutTasks = NodesToTasks(@switch.Default);

                            // Run Tasks
                            var defaultStartNode = GetStartupNode(@switch.Default);

                            RunTasks(defalutTasks, @switch.Default, defaultStartNode, force, ref success, ref warning, ref atLeastOneSucceed);
                        }

                        // Child node
                        var childNode = nodes.FirstOrDefault(n => n.ParentId == @switch.Id);

                        if (childNode != null)
                        {
                            RunTasks(tasks, nodes, childNode, force, ref success, ref warning, ref atLeastOneSucceed);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Stops this workflow.
        /// </summary>
        public bool Stop()
        {
            if (IsRunning)
            {
                try
                {
                    _thread.Abort();
                    IsWaitingForApproval = false;
                    Database.DecrementRunningCount();
                    Database.IncrementStoppedCount();
                    var entry = Database.GetEntry(Id);
                    entry.Status = Db.Status.Stopped;
                    entry.StatusDate = DateTime.Now;
                    Database.UpdateEntry(entry.GetDbId(), entry);
                    _historyEntry.Status = Db.Status.Stopped;
                    _historyEntry.StatusDate = DateTime.Now;
                    Database.InsertHistoryEntry(_historyEntry);
                    IsDisapproved = false;

                    if (_jobsQueue.Count > 0)
                    {
                        var job = _jobsQueue.Dequeue();
                        job.Workflow.Start();
                    }

                    return true;
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("An error occured while stopping the workflow : {0}", e, this);
                }
            }

            return false;
        }

        /// <summary>
        /// Suspends this workflow.
        /// </summary>
        public bool Suspend()
        {
            if (IsRunning)
            {
                try
                {
#pragma warning disable 618
                    _thread.Suspend();
#pragma warning restore 618
                    IsPaused = true;
                    Database.IncrementPendingCount();
                    Database.DecrementRunningCount();
                    var entry = Database.GetEntry(Id);
                    entry.Status = Db.Status.Pending;
                    entry.StatusDate = DateTime.Now;
                    Database.UpdateEntry(entry.GetDbId(), entry);
                    return true;
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("An error occured while suspending the workflow : {0}", e, this);
                }
            }

            return false;
        }

        /// <summary>
        /// Resumes this workflow.
        /// </summary>
        public void Resume()
        {
            if (IsPaused)
            {
                try
                {
#pragma warning disable 618
                    _thread.Resume();
#pragma warning restore 618
                    Database.IncrementRunningCount();
                    Database.DecrementPendingCount();
                    var entry = Database.GetEntry(Id);
                    entry.Status = Db.Status.Running;
                    entry.StatusDate = DateTime.Now;
                    Database.UpdateEntry(entry.GetDbId(), entry);
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

        /// <summary>
        /// Approves the current workflow.
        /// </summary>
        public void Approve()
        {
            if (IsApproval)
            {
                var task = Tasks.Where(t => t.IsWaitingForApproval).First();
                var dir = Path.Combine(ApprovalFolder, Id.ToString(), task.Id.ToString());
                Directory.CreateDirectory(dir);
                File.WriteAllText(Path.Combine(dir, "task.approved"), "Task " + task.Id + " of the workflow " + Id + " approved.");
                IsDisapproved = false;
            }
        }

        /// <summary>
        /// Disapproves the current workflow.
        /// </summary>
        public void Disapprove()
        {
            if (IsApproval)
            {
                IsDisapproved = true;
            }
        }

        private void CreateTempFolder()
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

        private Node GetStartupNode(IEnumerable<Node> nodes)
        {
            return nodes.First(n => n.ParentId == StartId);
        }

        private Task GetTask(int id)
        {
            return Tasks.FirstOrDefault(t => t.Id == id);
        }

        private Task GetTask(IEnumerable<Task> tasks, int id)
        {
            return tasks.FirstOrDefault(t => t.Id == id);
        }
    }
}
