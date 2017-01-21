using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using System.Threading;
using System.IO;

namespace Wexflow.Tasks.FilesExist
{
    public class FilesExist:Task
    {
        public string[] FFiles { get; private set; }
        public string[] Folders { get; private set; }

        public FilesExist(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            this.FFiles = this.GetSettings("file");
            this.Folders = this.GetSettings("folder");
        }

        public override void Run()
        {
            this.Info("Checking...");
            try
            {
                string xmlPath = Path.Combine(this.Workflow.WorkflowTempFolder,
                       string.Format("FilesExist_{0:yyyy-MM-dd-HH-mm-ss-fff}.xml", DateTime.Now));
                XDocument xdoc = new XDocument(new XElement("Root"));
                XElement xFiles = new XElement("Files");
                XElement xFolders = new XElement("Folders");

                foreach (var file in this.FFiles)
                {
                    xFiles.Add(new XElement("File",
                        new XAttribute("path", file),
                        new XAttribute("name", Path.GetFileName(file)),
                        new XAttribute("exists", File.Exists(file))));
                }

                foreach (var folder in this.Folders)
                {
                    xFolders.Add(new XElement("Folder",
                        new XAttribute("path", folder),
                        new XAttribute("name", Path.GetFileName(folder)),
                        new XAttribute("exists", Directory.Exists(folder))));
                }

                xdoc.Root.Add(xFiles);
                xdoc.Root.Add(xFolders);
                xdoc.Save(xmlPath);
                this.Files.Add(new FileInf(xmlPath, this.Id));
                this.InfoFormat("The result has been written in: {0}", xmlPath);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                this.ErrorFormat("An error occured while checking files and folders. Error: {0}", e.Message);
            }
            this.Info("Task finished.");
        }
    }
}
