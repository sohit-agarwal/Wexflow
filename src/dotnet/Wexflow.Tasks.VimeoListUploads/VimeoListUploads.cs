using System;
using System.IO;
using System.Security;
using System.Threading;
using System.Xml.Linq;
using Vimeo;
using Wexflow.Core;

namespace Wexflow.Tasks.VimeoListUploads
{
    public class VimeoListUploads:Task
    {
        public string Token { get; }

        public VimeoListUploads(XElement xe, Workflow wf) : base(xe, wf)
        {
            Token = GetSetting("token");
        }

        public override TaskStatus Run()
        {
            Info("Listing uploads...");

            Status status = Status.Success;

            try
            {
                var xmlPath = Path.Combine(Workflow.WorkflowTempFolder,
                string.Format("{0}_{1:yyyy-MM-dd-HH-mm-ss-fff}.xml", "VimeoListUploads", DateTime.Now));

                var xdoc = new XDocument(new XElement("VimeoListUploads"));
                var xvideos = new XElement("Videos");

                VimeoApi vimeoApi = new VimeoApi(Token);
                var videos = vimeoApi.GetVideos();

                foreach (var d in videos.data)
                {
                    xvideos.Add(new XElement("Video"
                        , new XAttribute("title", SecurityElement.Escape(d.name))
                        , new XAttribute("uri", SecurityElement.Escape(d.uri))
                        , new XAttribute("created_time", SecurityElement.Escape(d.created_time))
                        , new XAttribute("status", SecurityElement.Escape(d.status))
                        ));
                }

                xdoc.Root.Add(xvideos);
                xdoc.Save(xmlPath);
                Files.Add(new FileInf(xmlPath, Id));
                InfoFormat("Results written in {0}", xmlPath);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while listing uploads: {0}", e.Message);
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status);
        }

    }
}
