using IngematicaAngularBase.Dal.Specification;
using IngematicaAngularBase.Model.Common;
using IngematicaAngularBase.Model.Entities;
using IngematicaAngularBase.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace IngematicaAngularBase.Dal
{
    public class EnvioMailDataAccess
    {
        public EnvioMailDataAccess(Entities context)
        {
            this.context = context;
        }

        private readonly Entities context;

        public void SendMail(string subject, string mailFrom, string mailTo, string body)
        {
            MailAddress from = new MailAddress(mailFrom);
            MailAddress to = new MailAddress(mailTo);
            MailMessage message = new MailMessage(from, to);
            message.Body = body;
            message.BodyEncoding = UTF8Encoding.UTF8;
            message.Subject = subject;
            message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            SmtpClient client = new SmtpClient();
            client.Port = 587;
            client.Host = "smtp.gmail.com";
            client.Timeout = 10000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("sao.arquitectura2017@gmail.com", "sao.arquitectura");
            client.Send(message);

        }
    }
}
