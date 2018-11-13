using System;
using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.HttpGet
{
    public class HttpGet:Task
    {
        private const SslProtocols _Tls12 = (SslProtocols)0x00000C00;
        private const SecurityProtocolType Tls12 = (SecurityProtocolType)_Tls12;

        public string Url { get; private set; }

        public HttpGet(XElement xe, Workflow wf) : base(xe, wf)
        {
            Url = GetSetting("url");
        }

        public override TaskStatus Run()
        {
            Info("Executing GET request...");
            var status = Status.Success;
            try
            {
                using (var client = new WebClient())
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = Tls12;

                    var response = client.DownloadData(Url);
                    var destFile = Path.Combine(Workflow.WorkflowTempFolder, string.Format("HttpGet_{0:yyyy-MM-dd-HH-mm-ss-fff}", DateTime.Now));
                    File.WriteAllBytes(destFile, response);
                    Files.Add(new FileInf(destFile, Id));
                    InfoFormat("GET request {0} executed whith success -> {1}", Url, destFile);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while executing the GET request {0}: {1}", Url, e.Message);
                status = Status.Error;
            }
            Info("Task finished.");
            return new TaskStatus(status);
        }
    }
}
