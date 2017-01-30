using System.ServiceProcess;
using Wexflow.Core;
using System.Configuration;
using System.ServiceModel;

namespace Wexflow.Clients.WindowsService
{
    public partial class WexflowWindowsService : ServiceBase
    {
        public static string SettingsFile = ConfigurationManager.AppSettings["WexflowSettingsFile"];
        public static WexflowEngine WexflowEngine = new WexflowEngine(SettingsFile);

        ServiceHost serviceHost;
        
        public WexflowWindowsService()
        {
            InitializeComponent();
            ServiceName = "Wexflow";
            WexflowEngine.Run();
        }

        public void OnDebug()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            if (serviceHost != null)
            {
                serviceHost.Close();
            }

            // Create a ServiceHost for the WexflowService type and 
            // provide the base address.
            serviceHost = new ServiceHost(typeof(WexflowService));
                
            // Open the ServiceHostBase to create listeners and start 
            // listening for messages.
            serviceHost.Open();
        }

        protected override void OnStop()
        {
            if (serviceHost != null)
            {
                serviceHost.Close();
                serviceHost = null;
            }
        }
    }
}
