using System.Collections.Generic;
using System.Linq;
using IngematicaAngularBase.Model.ViewModels;
using IngematicaAngularBase.Dal;
using IngematicaAngularBase.Model.Entities;

namespace IngematicaAngularBase.Bll
{
    public class EnvioMailBusiness
    {
        public void SendMail(string subject, string mailFrom, string mailTo, string body)
        {
            using (var context = new Entities())
            {
                EnvioMailDataAccess envioMailDataAccess = new EnvioMailDataAccess(context);
                envioMailDataAccess.SendMail(subject, mailFrom, mailTo, body);
            }
        }
    }
}
