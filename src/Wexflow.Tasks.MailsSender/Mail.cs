using System.IO;
using System.Net.Mail;
using System.Net;
using System.Net.Mime;
using System.Xml.Linq;
using System.Xml.XPath;
using Wexflow.Core;

namespace Wexflow.Tasks.MailsSender
{
    public class Mail
    {
        public string From { get; private set; }
        public string[] To { get; private set; }
        public string[] Cc { get; private set; }
        public string[] Bcc { get; private set; }
        public string Subject { get; private set; }
        public string Body { get; private set; }
        public FileInf[] Attachments { get; private set; }

        public Mail(string from, string[] to, string[] cc, string[] bcc, string subject, string body, FileInf[] attachments)
        {
            From = from;
            To = to;
            Cc = cc;
            Bcc = bcc;
            Subject = subject;
            Body = body;
            Attachments = attachments;
        }

        public void Send(string host, int port, bool enableSsl, string user, string password)
        {
            var smtp = new SmtpClient
            {
                Host = host,
                Port = port,
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(user, password)
            };

            using (var msg = new MailMessage())
            {
                msg.From = new MailAddress(From);
                foreach (string to in To) msg.To.Add(new MailAddress(to));
                foreach (string cc in Cc) msg.CC.Add(new MailAddress(cc));
                foreach (string bcc in Bcc) msg.Bcc.Add(new MailAddress(bcc));
                msg.Subject = Subject;
                msg.Body = Body;

                foreach (var attachment in Attachments)
                {
                    // Create  the file attachment for this e-mail message.
                    Attachment data = new Attachment(attachment.Path, MediaTypeNames.Application.Octet);
                    // Add time stamp information for the file.
                    ContentDisposition disposition = data.ContentDisposition;
                    disposition.CreationDate = File.GetCreationTime(attachment.Path);
                    disposition.ModificationDate = File.GetLastWriteTime(attachment.Path);
                    disposition.ReadDate = File.GetLastAccessTime(attachment.Path);
                    // Add the file attachment to this e-mail message.
                    msg.Attachments.Add(data);
                }

                smtp.Send(msg);
            }
        }

        public static Mail Parse(XElement xe, FileInf[] attachments)
        {
            string from = xe.XPathSelectElement("From").Value;
            var to = xe.XPathSelectElement("To").Value.Split(',');

            string[] cc = new string[] { };
            var ccElement = xe.XPathSelectElement("Cc");
            if(ccElement != null) cc = ccElement.Value.Split(',');

            string[] bcc = new string[] { };
            var bccElement = xe.XPathSelectElement("Bcc");
            if (bccElement != null) bcc = bccElement.Value.Split(',');

            string subject = xe.XPathSelectElement("Subject").Value;
            string body = xe.XPathSelectElement("Body").Value;

            return new Mail(from, to, cc, bcc, subject, body, attachments);
        }
    }
}
