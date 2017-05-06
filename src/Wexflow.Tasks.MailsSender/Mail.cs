using System.Net.Mail;
using System.Net;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Wexflow.Tasks.MailsSender
{
    public class Mail
    {
        public string From {get; }
        public string[] To { get; }
        public string[] Cc { get; }
        public string Subject { get; }
        public string Body { get; }

        public Mail(string from, string[] to, string[] cc, string subject, string body)
        {
            From = from;
            To = to;
            Cc = cc;
            Subject = subject;
            Body = body;
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
                msg.Subject = Subject;
                msg.Body = Body;

                smtp.Send(msg);
            }
        }

        public static Mail Parse(XElement xe)
        {
            string from = xe.XPathSelectElement("From").Value;
            var to = xe.XPathSelectElement("To").Value.Split(',');
            var cc = xe.XPathSelectElement("Cc").Value.Split(',');
            string subject = xe.XPathSelectElement("Subject").Value;
            string body = xe.XPathSelectElement("Body").Value;

            return new Mail(from, to, cc, subject, body);
        }
    }
}
