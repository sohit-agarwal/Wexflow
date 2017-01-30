using System;
using System.Collections.Generic;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using System.Threading;

namespace Wexflow.Tasks.XmlToCsv
{
    public class XmlToCsv:Task
    {
        public XmlToCsv(XElement xe, Workflow wf)
            : base(xe, wf)
        {
        }

        public override TaskStatus Run()
        {
            Info("Creating csv files...");

            bool success = true;
            bool atLeastOneSucceed = false;

            foreach (FileInf file in SelectFiles())
            {
                try
                {
                    var csvPath = Path.Combine(Workflow.WorkflowTempFolder,
                        string.Format("{0}_{1:yyyy-MM-dd-HH-mm-ss-fff}.csv", Path.GetFileNameWithoutExtension(file.FileName), DateTime.Now));
                    CreateCsv(file.Path, csvPath);
                    InfoFormat("Csv file {0} created from {1}", csvPath, file.Path);
                    Files.Add(new FileInf(csvPath, Id));
                    
                    if (!atLeastOneSucceed) atLeastOneSucceed = true;
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("An error occured while creating the Csv from the file {0}.", e, file.Path);
                    success = false;
                }
            }

            var status = Status.Success;

            if (!success && atLeastOneSucceed)
            {
                status = Status.Warning;
            }
            else if (!success)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status, false);
        }

        void CreateCsv(string xmlPath, string csvPath)
        {
            var xdoc = XDocument.Load(xmlPath);

            var lines = new List<string>();
            foreach (XElement xLine in xdoc.XPathSelectElements("Lines/Line"))
            {
                var sb = new StringBuilder();
                foreach (XElement xColumn in xLine.XPathSelectElements("Column"))
                {
                    sb.Append(xColumn.Value).Append(";");
                }
                lines.Add(sb.ToString());
            }
            File.WriteAllLines(csvPath, lines.ToArray());
        }
    }
}
