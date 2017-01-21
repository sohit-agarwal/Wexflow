using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Net;
using System.Threading;

namespace Wexflow.Tasks.Http
{
    public class Http:Task
    {
        public string[] Urls { get; private set; }

        public Http(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            this.Urls = this.GetSettings("url");
        }

        public override void Run()
        {
            this.Info("Downloading files...");
            WebClient webClient = new WebClient();

            foreach (string url in this.Urls)
            {
                try
                {
                    string destPath = Path.Combine(this.Workflow.WorkflowTempFolder, Path.GetFileName(url));
                    webClient.DownloadFile(url, destPath);
                    this.InfoFormat("File {0} downlaoded as {1}", url, destPath);
                    this.Files.Add(new FileInf(destPath, this.Id));
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    this.ErrorFormat("An error occured while downloading the file: {0}. Error: {1}", url, e.Message);
                }
            }
            this.Info("Task finished.");
        }
    }
}
