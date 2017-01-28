using System;
using System.Threading;
using Wexflow.Core;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace Wexflow.Clients.Cmd
{
	public class Program
	{
		public static string SETTINGS_FILE = ConfigurationManager.AppSettings["WexflowSettingsFile"];
		public static WexflowEngine WEXFLOW_ENGINE = new WexflowEngine(SETTINGS_FILE);

		public static void Main(string[] args)
		{
			WEXFLOW_ENGINE.Run();

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
