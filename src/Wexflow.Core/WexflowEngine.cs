using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using System.Threading;

namespace Wexflow.Core
{
    public class WexflowEngine
    {
        public string SettingsFile { get; private set; }
        public string WorkflowsFolder { get; private set; }
        public string TrashFolder { get; private set; }
        public string TempFolder { get; private set; }
        public string XsdPath { get; private set; }
        public string TasksNamesFile { get; private set; }
        public string TasksSettingsFile { get; private set; }
        public IList<Workflow> Workflows { get; private set; }

        private readonly Dictionary<int, WexflowTimer> _wexflowTimers;

        public WexflowEngine(string settingsFile)
        {
            SettingsFile = settingsFile;
            Workflows = new List<Workflow>();
            _wexflowTimers = new Dictionary<int, WexflowTimer>();

            Logger.Info("");
            Logger.Info("Starting Wexflow Engine");

            LoadSettings();
            LoadWorkflows();
        }

        void LoadSettings()
        {
            var xdoc = XDocument.Load(SettingsFile);
            WorkflowsFolder = GetWexflowSetting(xdoc, "workflowsFolder");
            TrashFolder = GetWexflowSetting(xdoc, "trashFolder");
            TempFolder = GetWexflowSetting(xdoc, "tempFolder");
            if (!Directory.Exists(TempFolder)) Directory.CreateDirectory(TempFolder);
            XsdPath = GetWexflowSetting(xdoc, "xsd");
            TasksNamesFile = GetWexflowSetting(xdoc, "tasksNamesFile");
            TasksSettingsFile = GetWexflowSetting(xdoc, "tasksSettingsFile");
        }

        string GetWexflowSetting(XDocument xdoc, string name)
        {
            var xValue = xdoc.XPathSelectElement(string.Format("/Wexflow/Setting[@name='{0}']", name)).Attribute("value");
            if (xValue == null) throw new Exception("Wexflow Setting Value attribute not found.");
            return xValue.Value;
        }

        void LoadWorkflows()
        {
            foreach (string file in Directory.GetFiles(WorkflowsFolder))
            {
                var workflow = LoadWorkflowFromFile(file);
                if (workflow != null)
                {
                    Workflows.Add(workflow);
                }
            }

            var watcher = new FileSystemWatcher(WorkflowsFolder, "*.xml")
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = false
            };

            watcher.Created += (_, args) =>
            {
                var workflow = LoadWorkflowFromFile(args.FullPath);
                if (workflow != null)
                {
                    Workflows.Add(workflow);
                    ScheduleWorkflow(workflow);
                }
            };

            watcher.Deleted += (_, args) =>
            {
                var removedWorkflow = Workflows.SingleOrDefault(wf => wf.WorkflowFilePath == args.FullPath);
                if (removedWorkflow != null)
                {
                    Logger.InfoFormat("Workflow {0} is stopped and removed because its definition file {1} was deleted",
                        removedWorkflow.Name, removedWorkflow.WorkflowFilePath);
                    removedWorkflow.Stop();
                    Workflows.Remove(removedWorkflow);
                }
            };

            watcher.Changed += (_, args) =>
            {
                try
                {
                    //Logger.Debug($"Workflows: {Workflows?.Select(wf => wf.WorkflowFilePath).Aggregate((wf1, wf2) => wf1 + " " + wf2) ?? "'Workflows' is null"}");
                    if (Workflows != null)
                    {
                        var changedWorkflow = Workflows.SingleOrDefault(wf => wf.WorkflowFilePath == args.FullPath);

                        if (changedWorkflow != null)
                        {
                            // the existing file might have caused an error during loading, so there may be no corresponding
                            // workflow to the changed file
                            changedWorkflow.Stop();
                            Workflows.Remove(changedWorkflow);
                            Logger.InfoFormat("A change in the definition file {0} of workflow {1} has been detected. The workflow will be reloaded", changedWorkflow.WorkflowFilePath, changedWorkflow.Name);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("Error during workflow reload", e);
                }

                var reloaded = LoadWorkflowFromFile(args.FullPath);
                if (reloaded != null)
                {
                    var duplicateId = Workflows.SingleOrDefault(wf => wf.Id == reloaded.Id);
                    if (duplicateId != null)
                    {
                        Logger.ErrorFormat(
                            "An error occured while loading the workflow : {0}. The workflow Id {1} is already assgined in {2}",
                            args.FullPath, reloaded.Id, duplicateId.WorkflowFilePath);
                    }
                    else
                    {
                        Workflows.Add(reloaded);
                        ScheduleWorkflow(reloaded);
                    }
                }
            };
        }

        Workflow LoadWorkflowFromFile(string file)
        {
            try
            {
                var wf = new Workflow(file, TempFolder, XsdPath);
                Logger.InfoFormat("Workflow loaded: {0} ({1})", wf, file);
                return wf;
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("An error occured while loading the workflow : {0} Please check the workflow configuration. Error: {1}", file, e.Message);
                return null;
            }
        }

        public void Run()
        {
            foreach (Workflow workflow in Workflows)
            {
                ScheduleWorkflow(workflow);
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
                    Action<object> callback = o =>
                    {
                        var workflow = o as Workflow;
                        if (workflow != null && !workflow.IsRunning) workflow.Start();
                    };

                    var timer = new WexflowTimer(new TimerCallback(callback), wf, wf.Period);
                    _wexflowTimers.Add(wf.Id, timer);
                    timer.Start();
                }
            }
        }

        public void Stop()
        {
            foreach (var wt in _wexflowTimers.Values)
            {
               wt.Stop();
            }

            foreach (var wf in Workflows)
            {
                if (wf.IsRunning)
                {
                    wf.Stop();
                }
            }
        }

        public Workflow GetWorkflow(int workflowId)
        {
            return Workflows.FirstOrDefault(wf => wf.Id == workflowId);
        }

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

        public void StopWorkflow(int workflowId)
        {
            var wf = GetWorkflow(workflowId);

            if (wf == null)
            {
                Logger.ErrorFormat("Workflow {0} not found.", workflowId);
            }
            else
            {
                if (wf.IsEnabled) wf.Stop();
            }
        }

        public void PauseWorkflow(int workflowId)
        {
            var wf = GetWorkflow(workflowId);

            if (wf == null)
            {
                Logger.ErrorFormat("Workflow {0} not found.", workflowId);
            }
            else
            {
                if (wf.IsEnabled) wf.Pause();
            }
        }

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
    }
}
