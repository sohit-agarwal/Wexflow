using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            this.Host = this.GetSetting("host");
            this.Port = int.Parse(this.GetSetting("port"));
            this.EnableSsl = bool.Parse(this.GetSetting("enableSsl"));
            this.User = this.GetSetting("user");
            this.Password = this.GetSetting("password");
        }

        public override TaskStatus Run()
        {
            this.Info("Sending mails...");

            bool success = true;
            bool atLeastOneSucceed = false;

            try
            {
                foreach (FileInf mailFile in this.SelectFiles())
                {
                    XDocument xdoc = XDocument.Load(mailFile.Path);
                    IEnumerable<XElement> xMails = xdoc.XPathSelectElements("Mails/Mail");

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
							this.ErrorFormat("An error occured while parsing the mail {0}. Please check the XML configuration according to the documentation. Error: {1}", count, e.Message);
                            success &= false;
                            count++;
                            continue;
                        }

                        try
                        {
                            mail.Send(this.Host, this.Port, this.EnableSsl, this.User, this.Password);
                            this.InfoFormat("Mail {0} sent.", count);
                            count++;
                            success &= true;
                            if (!atLeastOneSucceed) atLeastOneSucceed = true;
                        }
                        catch (ThreadAbortException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            this.ErrorFormat("An error occured while sending the mail {0}. Error message: {1}", count, e.Message);
                            success &= false;
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
                this.ErrorFormat("An error occured while sending mails.", e);
                success &= false;
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

            this.Info("Task finished.");
            return new TaskStatus(status, false);
        }
    }
}
