using IngematicaAngularBase.Dal.Specification;
using IngematicaAngularBase.Model.Common;
using IngematicaAngularBase.Model.Entities;
using IngematicaAngularBase.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
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
           // SecurityDataAccess.SendMail(subject, mailFrom, mailTo, body);


        }
    }
}
