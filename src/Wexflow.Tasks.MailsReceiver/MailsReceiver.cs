using System;
using System.IO;
using Wexflow.Core;
using System.Xml.Linq;
using System.Threading;
using OpenPop.Pop3;

namespace Wexflow.Tasks.MailsReceiver
{
    public class MailsReceiver : Task
    {
        public string Host { get; private set; }
        public int Port { get; private set; }
        public bool EnableSsl { get; private set; }
        public string User { get; private set; }
        public string Password { get; private set; }
        public int MessageCount { get; private set; }
        public bool DeleteMessages { get; private set; }

        public MailsReceiver(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            Host = GetSetting("host");
            Port = int.Parse(GetSetting("port","993"));
            EnableSsl = bool.Parse(GetSetting("enableSsl", "true"));
            User = GetSetting("user");
            Password = GetSetting("password");
            MessageCount = int.Parse(GetSetting("messageCount"));
            DeleteMessages = bool.Parse(GetSetting("deleteMessages","false"));
        }

        public override TaskStatus Run()
        {
            Info("Receiving mails...");

            bool success = true;
            bool atLeastOneSucceed = false;

            try
            {
                using (var client = new Pop3Client())
                {
                    client.Connect(Host, Port, EnableSsl);
                    client.Authenticate(User, Password);

                    var count = client.GetMessageCount();

                    // We want to download messages
                    // Messages are numbered in the interval: [1, messageCount]
                    // Ergo: message numbers are 1-based.
                    // Most servers give the latest message the highest number
                    for (int i = Math.Min(MessageCount, count); i > 0; i--)
                    {
                        var message = client.GetMessage(i);
                        string messageFileName = "message_" + i + "_" + string.Format("{0:yyyy-MM-dd-HH-mm-ss-fff}", message.Headers.DateSent);
                        string messagePath = Path.Combine(Workflow.WorkflowTempFolder, messageFileName + ".eml");
                        File.WriteAllBytes(messagePath, message.RawMessage);
                        Files.Add(new FileInf(messagePath, Id));
                        Logger.InfoFormat("Message {0} received. Path: {1}", i, messagePath);

                        // save attachments
                        if (message.MessagePart.MessageParts != null)
                        {
                            foreach (var messagePart in message.MessagePart.MessageParts)
                            {
                                if (messagePart.IsAttachment)
                                {
                                    string attachmentPath = Path.Combine(Workflow.WorkflowTempFolder, messageFileName + "_" + messagePart.FileName);
                                    File.WriteAllBytes(attachmentPath, messagePart.Body);
                                    Files.Add(new FileInf(attachmentPath, Id));
                                    Logger.InfoFormat("Attachment {0} of mail {1} received. Path: {2}", messagePart.FileName, i, attachmentPath);
                                }
                            }
                        }

                        if (DeleteMessages)
                        {
                            client.DeleteMessage(i);
                        }

                        if (!atLeastOneSucceed)
                        {
                            atLeastOneSucceed = true;
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
                ErrorFormat("An error occured while receiving mails.", e);
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
    }
}
