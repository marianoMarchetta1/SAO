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

        [AuthorizeRule(Rule = "Mueble_CanAdd")]
        public IHttpActionResult Post(MuebleViewModel model)
        {
            if (!ModelState.IsValid)
                throw new CustomApplicationException("ModelState Invalid.");

            MuebleBusiness bs = new MuebleBusiness();

            Mapper.CreateMap<MuebleViewModel, Mueble>().IgnoreAllVirtual();
            Mueble mueble = Mapper.Map<MuebleViewModel, Mueble>(model);

            mueble.IdUsuarioAlta = SecurityManager.GetIdUsuario(User.Identity.Name);

            bs.Add(mueble);

            return Ok();
        }

        [AuthorizeRule(Rule = "Mueble_CanEdit")]
        public IHttpActionResult Put(MuebleViewModel model)
        {
            if (!ModelState.IsValid)
                throw new CustomApplicationException("ModelState Invalid.");

            MuebleBusiness bs = new MuebleBusiness();

            Mapper.CreateMap<MuebleViewModel, Mueble>().IgnoreAllVirtual();
            Mueble mueble = Mapper.Map<MuebleViewModel, Mueble>(model);

            mueble.IdUsuarioModificacion = SecurityManager.GetIdUsuario(User.Identity.Name);

            bs.Update(mueble);

            return Ok();
        }
    }
}
