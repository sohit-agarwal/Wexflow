using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using System.Threading;
using System.Management;
using System.IO;

namespace Wexflow.Tasks.Wmi
{
    public class Wmi:Task
    {
        public string Query { get; private set; }

        public Wmi(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            this.Query = this.GetSetting("query");
        }

        public override TaskStatus Run()
        {
            this.Info("Running WMI query...");

            bool success = true;

            try
            {
                string xmlPath = Path.Combine(this.Workflow.WorkflowTempFolder, string.Format("WMI_{0:yyyy-MM-dd-HH-mm-ss-fff}.xml", DateTime.Now));

                XDocument xdoc = new XDocument(new XElement("Objects"));
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(this.Query);
                ManagementObjectCollection collection = searcher.Get();

                foreach (ManagementObject obj in collection)
                {
                    XElement xObj = new XElement("Object");
                    foreach (PropertyData prop in obj.Properties)
                    {
                        XElement xProp = new XElement("Property", new XAttribute("name", prop.Name), new XAttribute("value", prop.Value ?? string.Empty));
                        xObj.Add(xProp);
                    }
                    xdoc.Root.Add(xObj);
                }
                xdoc.Save(xmlPath);
                this.Files.Add(new FileInf(xmlPath, this.Id));
                this.InfoFormat("The query {0} has been executed.", this.Query);
                success &= true;
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                this.ErrorFormat("An error occured while executing the query {0}. Error: {1}", this.Query, e.Message);
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
