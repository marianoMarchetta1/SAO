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
    public class UserLogController : ApiController
    {
        [Route("api/userLog/list")]
        [AuthorizeRule(Rule = "UserLog_CanList")]
        public IHttpActionResult Post(UserLogQuery query)
        {
            if (!ModelState.IsValid)
                throw new CustomApplicationException("ModelState Invalid.");
            UserLogBusiness bs = new UserLogBusiness();
            return Ok(bs.GetList(query));
        }

        [AuthorizeRule(Rule = "UserLog_CanDelete")]
        public IHttpActionResult Delete(int id)
        {
            UserLogBusiness bs = new UserLogBusiness();
            bs.Delete(id);
            return Ok();
        }
    }
}
