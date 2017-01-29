using System;
using System.Threading;
using System.Configuration;
using Wexflow.Core;
using Nancy.Hosting.Self;

namespace Wexflow.Clients.Cmd
{
    public class Program
    {
		public static bool IsLinux
		{
			get
			{
				int p = (int)Environment.OSVersion.Platform;
				return (p == 4) || (p == 6) || (p == 128);
			}
		}

        public static string WexflowSettingsFileWindows = ConfigurationManager.AppSettings["WexflowSettingsFileWindows"];
		public static string WexflowSettingsFileLinux = ConfigurationManager.AppSettings["WexflowSettingsFileLinux"];
        public static string WexflowServiceUri = ConfigurationManager.AppSettings["WexflowServiceUri"];
		public static WexflowEngine WexflowEngine = new WexflowEngine(IsLinux ? WexflowSettingsFileLinux : WexflowSettingsFileWindows);

        static void Main()
        {
            WexflowEngine.Run();
			var nancyHost = new NancyHost(new Uri(WexflowServiceUri));
            nancyHost.Start();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
