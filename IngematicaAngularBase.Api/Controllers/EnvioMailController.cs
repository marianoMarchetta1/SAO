using AutoMapper;
using IngematicaAngularBase.Api.Common;
using IngematicaAngularBase.Bll;
using IngematicaAngularBase.Model.Common;
using IngematicaAngularBase.Model.Entities;
using IngematicaAngularBase.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace IngematicaAngularBase.Api.Controllers
{
    public class EnvioMailController : ApiController
    {
        [Authorize]
        [HandleApiException]
        public IHttpActionResult Generate(MailViewModel mail)
        {
            string mailFrom = System.Configuration.ConfigurationManager.AppSettings["Mail.From"] != null ? System.Configuration.ConfigurationManager.AppSettings["Mail.From"].ToString() : "sao.arquitectura2017@gmail.com";
            string mailTo = System.Configuration.ConfigurationManager.AppSettings["Mail.To"] != null ? System.Configuration.ConfigurationManager.AppSettings["Mail.To"].ToString() : "mastronardi.romina@gmail.com";
            string body = mail.Comentario;
            string subject = "El usuario " + mail.Nombre + " envio un comentario";

            EnvioMailBusiness envioMailBusiness = new EnvioMailBusiness();
            envioMailBusiness.SendMail(subject, mailFrom, mailTo, body);

            return Ok(1);
        }
    }
}
