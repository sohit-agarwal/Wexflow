using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Threading;

namespace Wexflow.Tasks.ListFiles
{
    public class ListFiles:Task
    {
        public ListFiles(XElement xe, Workflow wf)
            : base(xe, wf)
        {}

        public override TaskStatus Run()
        {
            this.Info("Listing files...");
            //System.Threading.Thread.Sleep(10 * 1000);

            bool success = true;

            try
            {
                string xmlPath = Path.Combine(this.Workflow.WorkflowTempFolder,
                    string.Format("ListFiles_{0:yyyy-MM-dd-HH-mm-ss-fff}.xml", DateTime.Now));

                XDocument xdoc = new XDocument(new XElement("WexflowProcessing"));

                XElement xWorkflow = new XElement("Workflow",
                    new XAttribute("id", this.Workflow.Id),
                    new XAttribute("name", this.Workflow.Name),
                    new XAttribute("description", this.Workflow.Description));

                XElement xFiles = new XElement("Files");
                foreach (List<FileInf> files in this.Workflow.FilesPerTask.Values)
                {
                    foreach (FileInf file in files)
                    {
                        xFiles.Add(file.ToXElement());
                        this.Info(file.ToString());
                    }
                }

                xWorkflow.Add(xFiles);
                xdoc.Root.Add(xWorkflow);
                xdoc.Save(xmlPath);

                FileInf xmlFile = new FileInf(xmlPath, this.Id);
                this.Files.Add(xmlFile);
                this.Info(xmlFile.ToString());
                success &= true;
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                this.ErrorFormat("An error occured while listing files. Error: {0}", e.Message);
                success &= false;
            }

            Status status = Status.Success;

            if (!success)
            {
                status = Status.Error;
            }

            this.Info("Task finished.");
            return new TaskStatus(status, false);
        }
    }
}
