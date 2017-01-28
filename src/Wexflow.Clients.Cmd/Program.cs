using System;
using System.Threading;
using Wexflow.Core;
using System.Configuration;
using System.ServiceModel;

namespace Wexflow.Clients.Cmd
{
	public class Program
	{
		public static bool IsLinux
		{
			get
			{
				int p = (int)Environment.OSVersion.Platform;
				return p == 4 || p == 6 || p == 128;
			}
		}

		public static string SettingsFile = IsLinux ? ConfigurationManager.AppSettings["WexflowSettingsFileLinux"] : ConfigurationManager.AppSettings["WexflowSettingsFileWindows"];
		public static WexflowEngine WexflowEngine = new WexflowEngine(SettingsFile);

		public static void Main(string[] args)
		{
			WexflowEngine.Run();

			// Create a ServiceHost for the WexflowService type and 
			// provide the base address.
			ServiceHost serviceHost = new ServiceHost(typeof(WexflowService));

			// Open the ServiceHostBase to create listeners and start 
			// listening for messages.
			serviceHost.Open();

			Thread.Sleep(Timeout.Infinite);
		}
	}
}
