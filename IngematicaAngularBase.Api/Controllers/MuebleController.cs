using IngematicaAngularBase.Api.Common;
using IngematicaAngularBase.Bll;
using IngematicaAngularBase.Model.Common;
using IngematicaAngularBase.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace IngematicaAngularBase.Api.Controllers
{
    [Authorize]
    [HandleApiException]
    public class MuebleController : ApiController
    {
        [Route("api/mueble/list")]
        [AuthorizeRule(Rule = "Mueble_CanList")]
        public IHttpActionResult Post(MuebleQuery query)
        {
            if (!ModelState.IsValid)
                throw new CustomApplicationException("ModelState Invalid.");
            MuebleBusiness bs = new MuebleBusiness();
            return Ok(bs.GetList(query));
        }

        [AuthorizeRule(Rule = "Mueble_CanDelete")]
        public IHttpActionResult Delete(int id)
        {
            MuebleBusiness bs = new MuebleBusiness();
            bs.Delete(id);
            return Ok();
        }

        [AuthorizeRule(Rule = "Mueble_CanList")]
        public IHttpActionResult Get(int id)
        {
            MuebleBusiness bs = new MuebleBusiness();
            return Ok(bs.GetById(id));
        }
    }
}
