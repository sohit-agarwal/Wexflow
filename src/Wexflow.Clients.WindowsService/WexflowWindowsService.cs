using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Wexflow.Core;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace Wexflow.Clients.WindowsService
{
    public partial class WexflowWindowsService : ServiceBase
    {
        public static string SettingsFile = ConfigurationManager.AppSettings["WexflowSettingsFile"];
        public static WexflowEngine WexflowEngine = new WexflowEngine(SettingsFile);

        private ServiceHost serviceHost = null;
        
        public WexflowWindowsService()
        {
            InitializeComponent();
            this.ServiceName = "Wexflow";
            WexflowEngine.Run();
        }

        public void OnDebug()
        {
            this.OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            if (this.serviceHost != null)
            {
                this.serviceHost.Close();
            }

            // Create a ServiceHost for the WexflowService type and 
            // provide the base address.
            this.serviceHost = new ServiceHost(typeof(WexflowService));
                
            // Open the ServiceHostBase to create listeners and start 
            // listening for messages.
            this.serviceHost.Open();
        }

        protected override void OnStop()
        {
            if (this.serviceHost != null)
            {
                this.serviceHost.Close();
                this.serviceHost = null;
            }
        }
    }
}
