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
            SmtpClient client = new SmtpClient();
            MailAddress from = new MailAddress(mailFrom);
            MailAddress to = new MailAddress(mailTo);
            MailMessage message = new MailMessage(from, to);
            message.Subject = "Contacto SAO";
            message.Body = body;

            client.UseDefaultCredentials = true;
            //client.Credentials = new System.Net.NetworkCredential("mariano.marchetta1@gmail.com", "tuyu745y98pA");
            //client.Port = 587;
            client.Host = "smtp.gmail.com";
            //client.EnableSsl = true;

            client.Send(message);
        }
    }
}
