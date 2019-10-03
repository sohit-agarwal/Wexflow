using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.FilesJoiner
{
    public class FilesJoiner : Task
    {
        public FilesJoiner(XElement xe, Workflow wf)
            : base(xe, wf)
        {
        }

        private int GetNumberPart(string path)
        {
            var lastIndex = path.LastIndexOf("_");
            if (lastIndex == -1) return int.MaxValue;
            var substring = path.Substring(lastIndex + 1, path.Length - lastIndex - 1);
            return int.TryParse(substring, out var result) ? result : int.MaxValue;
        }

        public override TaskStatus Run()
        {
            Info("Concatenating files...");

            bool success = true;
            bool atLeastOneSucceed = false;

            var files = SelectFiles().OrderBy(i => GetNumberPart(i.Path)).ToArray();

            if (files.Length > 0)
            {
                var fileName = Path.GetFileName(files[0].FileName);
                foreach (var file in files)
                {
                    if (file.FileName.EndsWith("_1"))
                        fileName = file.FileName.Remove(fileName.Length - 2, 2);
                }

                var concatPath = Path.Combine(Workflow.WorkflowTempFolder, fileName);

                if (File.Exists(concatPath))
                {
                    File.Delete(concatPath);
                }

                using (var output = File.Create(concatPath))
                { 
                    foreach (FileInf file in files)
                    {
                        Info("Joiner " + file.Path);
                        try
                        {
                            using (var input = File.OpenRead(file.Path))
                            {
                                input.CopyTo(output);
                            }

                            if (!atLeastOneSucceed) atLeastOneSucceed = true;
                        }
                        catch (ThreadAbortException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            ErrorFormat("An error occured while concatenating the file {0}", e, file.Path);
                            success = false;
                        }
                    }
                }

                if (success)
                {
                    InfoFormat("Concatenation file generated: {0}", fileName);
                }

                Files.Add(new FileInf(concatPath, Id));
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
    }
}