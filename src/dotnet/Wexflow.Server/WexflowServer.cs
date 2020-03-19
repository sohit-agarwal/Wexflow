using Microsoft.Owin.Hosting;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.ServiceProcess;
using System.Threading;
using Wexflow.Core;

namespace Wexflow.Server
{
    public partial class WexflowServer : ServiceBase
    {
        public static NameValueCollection Config = ConfigurationManager.AppSettings;
        private static string settingsFile = Config["WexflowSettingsFile"];
        public static WexflowEngine WexflowEngine = new WexflowEngine(settingsFile);

        private IDisposable _webApp;

        public WexflowServer()
        {
            InitializeComponent();
            ServiceName = "Wexflow";
            Thread startThread = new Thread(StartThread) { IsBackground = true };
            startThread.Start();
        }

        private void StartThread()
        {
            WexflowEngine.Run();
        }

        protected override void OnStart(string[] args)
        {
            if (_webApp != null)
            {
                _webApp.Dispose();
            }

            var port = int.Parse(ConfigurationManager.AppSettings["WexflowServicePort"]);
            var url = "http://+:" + port;
            _webApp = WebApp.Start<Startup>(url);
        }

        protected override void OnStop()
        {
            WexflowEngine.Stop(true, true);

            if (_webApp != null)
            {
                _webApp.Dispose();
                _webApp = null;
            }
        }
    }
}
