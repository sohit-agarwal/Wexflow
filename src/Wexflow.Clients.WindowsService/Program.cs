using System.ServiceProcess;
using System.Threading;

namespace Wexflow.Clients.WindowsService
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0].Equals("debug"))
            {
                var service = new WexflowWindowsService();
                service.OnDebug();
                Thread.Sleep(Timeout.Infinite);
            }
            else
            {
                ServiceBase[] servicesToRun = { new WexflowWindowsService() };
                ServiceBase.Run(servicesToRun);
            }
        }
    }
}
