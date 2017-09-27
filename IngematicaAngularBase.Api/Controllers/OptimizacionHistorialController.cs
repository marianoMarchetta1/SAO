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
    [Authorize]
    [HandleApiException]
    public class OptimizacionHistorialController : ApiController
    {
        [Route("api/optimizacionHistorial/list")]
        [AuthorizeRule(Rule = "OptimizacionHistorial_CanList")]
        public IHttpActionResult Post(OptimizacionHistorialQuery query)
        {
            if (!ModelState.IsValid)
                throw new CustomApplicationException("ModelState Invalid.");
            OptimizacionHistorialBusiness bs = new OptimizacionHistorialBusiness();
            return Ok(bs.GetList(query));
        }

        [AuthorizeRule(Rule = "OptimizacionHistorial_CanDelete")]
        public IHttpActionResult Delete(int id)
        {
            OptimizacionHistorialBusiness bs = new OptimizacionHistorialBusiness();
            bs.Delete(id);
            return Ok();
        }
    }
}
