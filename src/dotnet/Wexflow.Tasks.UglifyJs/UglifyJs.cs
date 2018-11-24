using NUglify;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.UglifyJs
{
    public class UglifyJs : Task
    {
        public UglifyJs(XElement xe, Workflow wf) : base(xe, wf)
        {
        }

        public override TaskStatus Run()
        {
            Info("Uglifying JavaScript files...");

            var status = Status.Success;
            var success = true;
            var atLeastOneSuccess = false;
            var jsFiles = SelectFiles();

            foreach (var jsFile in jsFiles)
            {
                try
                {
                    var source = File.ReadAllText(jsFile.Path);
                    var result = Uglify.Js(source);
                    if (result.HasErrors)
                    {
                        ErrorFormat("An error occured while uglifying the script {0}: {1}", jsFile.Path, string.Concat(result.Errors.Select(e => e.Message + "\n").ToArray()));
                        success = false;
                        continue;
                    }

                    var destPath = Path.Combine(Workflow.WorkflowTempFolder, Path.GetFileNameWithoutExtension(jsFile.FileName) + ".min.js");
                    File.WriteAllText(destPath, result.Code);
                    Files.Add(new FileInf(destPath, Id));
                    InfoFormat("The script {0} has been uglified -> {1}", jsFile.Path, destPath);
                    if (!atLeastOneSuccess) atLeastOneSuccess = true;
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("An error occured while uglifying the script {0}: {1}", jsFile.Path, e.Message);
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
