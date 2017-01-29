using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Configuration;
using Wexflow.Core;
using Nancy.Hosting.Self;

namespace Wexflow.Clients.Cmd
{
    public class Program
    {
        public static string WexflowSettingsFile = ConfigurationManager.AppSettings["WexflowSettingsFile"];
        public static string WexflowServiceUri = ConfigurationManager.AppSettings["WexflowServiceUri"];
        public static WexflowEngine WexflowEngine = new WexflowEngine(WexflowSettingsFile);

        static void Main(string[] args)
        {
            WexflowEngine.Run();
            var nancyHost = new NancyHost(new Uri(WexflowServiceUri));
            nancyHost.Start();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
