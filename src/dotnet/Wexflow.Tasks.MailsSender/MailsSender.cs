using System;
using System.Collections.Generic;
using System.Linq;
using Wexflow.Core;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Threading;

namespace Wexflow.Tasks.MailsSender
{
    public class MailsSender:Task
    {
        public string Host { get; private set; }
        public int Port { get; private set; }
        public bool EnableSsl { get; private set; }
        public string User { get; private set; }
        public string Password { get; private set; }

        public MailsSender(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            Host = GetSetting("host");
            Port = int.Parse(GetSetting("port"));
            EnableSsl = bool.Parse(GetSetting("enableSsl"));
            User = GetSetting("user");
            Password = GetSetting("password");
        }

        public override TaskStatus Run()
        {
            Info("Sending mails...");

            bool success = true;
            bool atLeastOneSucceed = false;

            try
            {
                FileInf[] attachments = SelectAttachments();

                foreach (FileInf mailFile in SelectFiles())
                {
                    var xdoc = XDocument.Load(mailFile.Path);
                    var xMails = xdoc.XPathSelectElements("Mails/Mail");

                    int count = 1;
                    foreach (XElement xMail in xMails)
                    {
                        Mail mail;
                        try
                        {
                            mail = Mail.Parse(xMail, attachments);
                        }
                        catch (ThreadAbortException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
							ErrorFormat("An error occured while parsing the mail {0}. Please check the XML configuration according to the documentation. Error: {1}", count, e.Message);
                            success = false;
                            count++;
                            continue;
                        }

                        try
                        {
                            mail.Send(Host, Port, EnableSsl, User, Password);
                            InfoFormat("Mail {0} sent.", count);
                            count++;
                            
                            if (!atLeastOneSucceed) atLeastOneSucceed = true;
                        }
                        catch (ThreadAbortException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            ErrorFormat("An error occured while sending the mail {0}. Error message: {1}", count, e.Message);
                            success = false;
                        }
                    }
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while sending mails.", e);
                success = false;
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

        public FileInf[] SelectAttachments()
        {
            var files = new List<FileInf>();
            foreach (var xSelectFile in GetXSettings("selectAttachments"))
            {
                var xTaskId = xSelectFile.Attribute("value");
                if (xTaskId != null)
                {
                    var taskId = int.Parse(xTaskId.Value);

                    var qf = QueryFiles(Workflow.FilesPerTask[taskId], xSelectFile).ToArray();

                    files.AddRange(qf);
                }
                else
                {
                    var qf = (from lf in Workflow.FilesPerTask.Values
                        from f in QueryFiles(lf, xSelectFile)
                        select f).Distinct().ToArray();

                    files.AddRange(qf);
                }
            }
            return files.ToArray();
        }
    }
}
