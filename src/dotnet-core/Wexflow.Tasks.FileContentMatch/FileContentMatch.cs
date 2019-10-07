using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.FileContentMatch
{
    public class FileContentMatch : Task
    {
        public string File { get;  }
        public string Pattern { get; }

        public FileContentMatch(XElement xe, Workflow wf) : base(xe, wf)
        {
            File = GetSetting("file");
            Pattern = GetSetting("pattern");
        }

        public override TaskStatus Run()
        {
            Info("Checking file...");

            bool success;
            try
            {
                success = Regex.Match(System.IO.File.ReadAllText(File), Pattern, RegexOptions.Multiline).Success;

                if (success)
                {
                    InfoFormat("A content matching the pattern {0} was found in the file {1}.", Pattern, File);
                }
                else
                {
                    InfoFormat("No content matching the pattern {0} was found in the file {1}.", Pattern, File);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while checking the file {0}. Error: {1}", File, e.Message);
                return new TaskStatus(Status.Error, false);
            }

            Info("Task finished");

            return new TaskStatus(Status.Success, success);
        }
    }
}
