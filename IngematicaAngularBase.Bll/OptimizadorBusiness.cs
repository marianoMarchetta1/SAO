using IngematicaAngularBase.Dal;
using IngematicaAngularBase.Model;
using IngematicaAngularBase.Model.Common;
using IngematicaAngularBase.Model.Entities;
using IngematicaAngularBase.Model.ViewModels;
using Ingematica.MailService.Common;
using Ingematica.MailService.Contract;
using Ingematica.MailService.Service;
using Ingematica.MailService.Service.WCF;
using OAuth.Api;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace IngematicaAngularBase.Bll
{
    public class OptimizadorBusiness
    {
        public List<SelectionListSimple> GetMuebleList(bool activo)
        {
            using (var context = new Entities())
            {
                OptimizadorDataAccess optimizadorDataAccess = new OptimizadorDataAccess(context);
                return optimizadorDataAccess.GetMuebleList(activo);
            }
        }
    }
}
