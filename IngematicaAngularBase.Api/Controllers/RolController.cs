using IngematicaAngularBase.Model.ViewModels;
using System.Web.Http;
using IngematicaAngularBase.Bll;
using System.Collections.Generic;
using IngematicaAngularBase.Model;
using IngematicaAngularBase.Model.Entities;
using System;
using System.Threading;
using AutoMapper;
using IngematicaAngularBase.Api.Common;
using IngematicaAngularBase.Model.Common;
using IngematicaAngularBase.Api.Common;
    //aaaa
namespace IngematicaAngularBase.Api.Controllers
{
    [Authorize]
    [HandleApiException]
    public class RolController : ApiController 
    {
        [Route("api/rol/list")]
        [AuthorizeRule(Rule = "Rol_CanList")]
        public IHttpActionResult Post(RolQuery query)
        {
            if (!ModelState.IsValid)
                throw new CustomApplicationException("ModelState Invalid.");
            RolBusiness bs = new RolBusiness();
            return Ok(bs.GetList(query));
        }

        [AuthorizeRule(Rule = "Rol_CanAdd")]
        public IHttpActionResult Post(RolViewModel model)
        {
            RolBusiness bs = new RolBusiness();

            Mapper.CreateMap<RolViewModel, Rol>().ForMember(dto => dto.RolRegla, opt => opt.Ignore());
            Rol rol = Mapper.Map<RolViewModel, Rol>(model);
            foreach (RolReglaViewModel item in model.RolRegla.FindAll(p => p.Checked))
                rol.RolRegla.Add(new RolRegla { IdRolRegla = 0, IdRegla = item.IdRegla, IdRol = 0 });

            rol.IdUsuarioAlta = SecurityManager.GetIdUsuario(User.Identity.Name);

            bs.Add(rol);
            return Ok();
        }

        [AuthorizeRule(Rule = "Rol_CanEdit")]
        public IHttpActionResult Put(int id, RolViewModel model)
        {
            RolBusiness bs = new RolBusiness();

            Mapper.CreateMap<RolViewModel, Rol>().ForMember(dto => dto.RolRegla, opt => opt.Ignore());
            Rol rol = Mapper.Map<RolViewModel, Rol>(model);
            foreach (RolReglaViewModel item in model.RolRegla.FindAll(p => p.Checked))
                rol.RolRegla.Add(new RolRegla { IdRolRegla = item.IdRolRegla, IdRegla = item.IdRegla, IdRol = model.IdRol });

            rol.IdUsuarioModificacion = SecurityManager.GetIdUsuario(User.Identity.Name);

            bs.Update(rol);

            SecurityManager.InvalidateRol(id);
            return Ok();
        }

        [AuthorizeRule(Rule = "Rol_CanDelete")]
        public IHttpActionResult Delete(int id)
        {
            RolBusiness bs = new RolBusiness();

            bs.Delete(id);
            return Ok();
        }

        [AuthorizeRule(Rule = "Rol_CanList")]
        public IHttpActionResult Get(int id)
        {
            RolBusiness bs = new RolBusiness();
            return Ok(bs.GetById(id));
        }

        [Route("api/rol/getnew")]
        [AuthorizeRule(Rule = "Rol_CanAdd")]
        public IHttpActionResult GetNewRol()
        {
            RolBusiness bs = new RolBusiness();
            return Ok(bs.CreateRolConReglas());
        }

        [AuthorizeRule(Rule = "Rol_CanList")]
        [Route("api/rol/getParamsAddUpdate")]
        public IHttpActionResult getParamsAddUpdate()
        {
            ModuloBusiness bs = new ModuloBusiness();
            Dictionary<string, object> result = new Dictionary<string, object>();
            result.Add("moduloList", bs.GetModuloList());
            return Ok(result);
        }
    }
}

