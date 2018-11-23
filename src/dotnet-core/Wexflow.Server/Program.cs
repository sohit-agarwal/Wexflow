using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Wexflow.Core;

namespace Wexflow.Server
{
    public class Program
    {
        public static IConfiguration Config;
        public static WexflowEngine WexflowEngine;
        public static FileSystemWatcher Watcher;

        public static void Main(string[] args)
        {
            Config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            XmlDocument log4NetConfig = new XmlDocument();
            log4NetConfig.Load(File.OpenRead("log4net.config"));
            var repo = LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
            XmlConfigurator.Configure(repo, log4NetConfig["log4net"]);

            string wexflowSettingsFile = Config["WexflowSettingsFile"];
            WexflowEngine = new WexflowEngine(wexflowSettingsFile);
            WexflowEngine.Run();
            InitializeFileSystemWatcher();

            int port = int.Parse(Config["WexflowServicePort"]);

            var host = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseKestrel((context, options) =>
                {
                    options.ListenAnyIP(port);
                })
                .UseStartup<Startup>()
                .Build();

            host.Run();

            Console.WriteLine("Press any key to stop Wexflow workflow server...");
            Console.ReadKey();
        }

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
        }

        public static void InitializeFileSystemWatcher()
        {
            Logger.Info("Initializing FileSystemWatcher...");
            Watcher = new FileSystemWatcher();
            Watcher.Path = WexflowEngine.WorkflowsFolder;
            Watcher.Filter = "*.xml";
            Watcher.IncludeSubdirectories = false;

            // Add event handlers.
            Watcher.Created += OnCreated;
            Watcher.Deleted += OnDeleted;
            Watcher.Changed += OnChanged;

            // Begin watching.
            Watcher.EnableRaisingEvents = true;
            Logger.InfoFormat("FileSystemWatcher.Path={0}", Watcher.Path);
            Logger.InfoFormat("FileSystemWatcher.Filter={0}", Watcher.Filter);
            Logger.InfoFormat("FileSystemWatcher.EnableRaisingEvents={0}", Watcher.EnableRaisingEvents);
            Logger.Info("FileSystemWatcher Initialized.");
        }

        private static void OnCreated(object source, FileSystemEventArgs e)
        {
            Logger.Info("FileSystemWatcher.OnCreated");
            var workflow = WexflowEngine.LoadWorkflowFromFile(e.FullPath);
            if (workflow != null)
            {
                WexflowEngine.Workflows.Add(workflow);
                WexflowEngine.ScheduleWorkflow(workflow);
            }
        }

        private static void OnDeleted(object source, FileSystemEventArgs e)
        {
            Logger.Info("FileSystemWatcher.OnDeleted");
            var removedWorkflow = WexflowEngine.Workflows.SingleOrDefault(wf => wf.WorkflowFilePath == e.FullPath);
            if (removedWorkflow != null)
            {
                Logger.InfoFormat("Workflow {0} is stopped and removed because its definition file {1} was deleted.",
                    removedWorkflow.Name, removedWorkflow.WorkflowFilePath);
                removedWorkflow.Stop();

                WexflowEngine.StopCronJobs(removedWorkflow.Id);
                WexflowEngine.Workflows.Remove(removedWorkflow);
            }
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            Logger.Info("FileSystemWatcher.OnChanged");
            try
            {
                if (WexflowEngine.Workflows != null)
                {
                    var changedWorkflow = WexflowEngine.Workflows.SingleOrDefault(wf => wf.WorkflowFilePath == e.FullPath);

                    if (changedWorkflow != null)
                    {
                        // the existing file might have caused an error during loading, so there may be no corresponding
                        // workflow to the changed file
                        changedWorkflow.Stop();

                        WexflowEngine.StopCronJobs(changedWorkflow.Id);
                        WexflowEngine.Workflows.Remove(changedWorkflow);
                        Logger.InfoFormat("A change in the definition file {0} of workflow {1} has been detected. The workflow will be reloaded.", changedWorkflow.WorkflowFilePath, changedWorkflow.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error during workflow reload", ex);
            }

            var reloaded = WexflowEngine.LoadWorkflowFromFile(e.FullPath);
            if (reloaded != null)
            {
                var duplicateId = WexflowEngine.Workflows.SingleOrDefault(wf => wf.Id == reloaded.Id);
                if (duplicateId != null)
                {
                    Logger.ErrorFormat(
                        "An error occured while loading the workflow : {0}. The workflow Id {1} is already assigned in {2}",
                        e.FullPath, reloaded.Id, duplicateId.WorkflowFilePath);
                }
                else
                {
                    WexflowEngine.Workflows.Add(reloaded);
                    WexflowEngine.ScheduleWorkflow(reloaded);
                }
            }
        }

    }
}
