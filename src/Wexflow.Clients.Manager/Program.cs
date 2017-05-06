using System;
using System.Windows.Forms;
using System.ServiceProcess;
using System.Configuration;

namespace Wexflow.Clients.Manager
{
    public static class Program
    {
        public static string WexflowServiceName = ConfigurationManager.AppSettings["WexflowServiceName"];
        public static bool DebugMode;

        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0].Equals("debug"))
            {
                DebugMode = true;
                RunForm1();
            }
            else
            {
                if (IsWexflowWindowsServiceRunning())
                {
                    RunForm1();
                }
                else
                {
                    MessageBox.Show(@"Wexflow Windows Service is not running. Please run it to start Wexflow Manager.");
                }
            }
        }

        static void RunForm1()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        public static bool IsWexflowWindowsServiceRunning()
        {
            var sc = new ServiceController(WexflowServiceName);
            return sc.Status == ServiceControllerStatus.Running;
        }
    }
}
