using System;
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
                foreach (FileInf mailFile in SelectFiles())
                {
                    var xdoc = XDocument.Load(mailFile.Path);
                    var xMails = xdoc.XPathSelectElements("Mails/Mail");

                    int count = 1;
                    foreach (XElement xMail in xMails)
                    {
                        Mail mail = null;
                        try
                        {
                            mail = Mail.Parse(xMail);
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

            Status status = Status.Success;

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
