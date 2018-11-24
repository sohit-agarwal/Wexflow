using NUglify;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.HtmlToText
{
    public class HtmlToText:Task
    {
        public HtmlToText(XElement xe, Workflow wf) : base(xe, wf)
        {
        }

        public override TaskStatus Run()
        {
            Info("Extracting text from HTML files...");

            var status = Status.Success;
            var success = true;
            var atLeastOneSuccess = false;
            var htmlFiles = SelectFiles();

            foreach (var htmlFile in htmlFiles)
            {
                try
                {
                    var source = File.ReadAllText(htmlFile.Path);
                    var result = Uglify.HtmlToText(source);
                    if (result.HasErrors)
                    {
                        ErrorFormat("An error occured while extracting text from the HTML file {0}: {1}", htmlFile.Path, string.Concat(result.Errors.Select(e => e.Message + "\n").ToArray()));
                        success = false;
                        continue;
                    }

                    var destPath = Path.Combine(Workflow.WorkflowTempFolder, Path.GetFileNameWithoutExtension(htmlFile.FileName) + ".txt");
                    File.WriteAllText(destPath, result.Code);
                    Files.Add(new FileInf(destPath, Id));
                    InfoFormat("Text has been extracted from the HTML file {0} -> {1}", htmlFile.Path, destPath);
                    if (!atLeastOneSuccess) atLeastOneSuccess = true;
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("An error occured while extracting text from the HTML file {0}: {1}", htmlFile.Path, e.Message);
                    success = false;
                }
            }

            if (!success && atLeastOneSuccess)
            {
                status = Status.Warning;
            }
            else if (!success)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status);
        }
    }
}
