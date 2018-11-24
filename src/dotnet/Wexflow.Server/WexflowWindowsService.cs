using System.ServiceProcess;
using Wexflow.Core;
using System.Configuration;
using System.ServiceModel;

namespace Wexflow.Server
{
    public partial class WexflowWindowsService : ServiceBase
    {
        public static string SettingsFile = ConfigurationManager.AppSettings["WexflowSettingsFile"];
        public static WexflowEngine WexflowEngine = new WexflowEngine(SettingsFile);

        private ServiceHost _serviceHost;
        
        public WexflowWindowsService()
        {
            InitializeComponent();
            ServiceName = "Wexflow";
            WexflowEngine.Run();
        }

        protected override void OnStart(string[] args)
        {
            if (_serviceHost != null)
            {
                _serviceHost.Close();
            }

            // Create a ServiceHost for the WexflowService type and 
            // provide the base address.
            _serviceHost = new ServiceHost(typeof(WexflowService));
                
            // Open the ServiceHostBase to create listeners and start 
            // listening for messages.
            _serviceHost.Open();
        }

        protected override void OnStop()
        {
            WexflowEngine.Stop(true, true);

            if (_serviceHost != null)
            {
                _serviceHost.Close();
                _serviceHost = null;
            }
        }
    }
}
