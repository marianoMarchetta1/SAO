using AutoMapper;
using IngematicaAngularBase.Api.Common;
using IngematicaAngularBase.Bll;
using IngematicaAngularBase.Model;
using IngematicaAngularBase.Model.Common;
using IngematicaAngularBase.Model.Entities;
using IngematicaAngularBase.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;

namespace IngematicaAngularBase.Api.Controllers
{
    [Authorize]
    [HandleApiException]
    public class UsuarioController : ApiController
    {
        [AuthorizeRule(Rule = "Usuario_CanList")]
        [Route("api/usuario/list")]
        public IHttpActionResult Post(UsuarioQuery query)
        {
            if (!ModelState.IsValid)
                throw new CustomApplicationException("ModelState Invalid.");
            UsuarioBusiness bs = new UsuarioBusiness();
            return Ok(bs.GetList(query));
        }

        [AuthorizeRule(Rule = "Usuario_CanAdd")]
        public IHttpActionResult Post(UsuarioViewModel model)
        {
            if (!ModelState.IsValid)
                throw new CustomApplicationException("ModelState Invalid.");
            UsuarioBusiness bs = new UsuarioBusiness();
            Mapper.CreateMap<UsuarioViewModel, Usuario>();
            Usuario usuario = Mapper.Map<UsuarioViewModel, Usuario>(model);

            usuario.IdUsuarioAlta = SecurityManager.GetIdUsuario(User.Identity.Name);

            bs.Add(usuario);
            return Ok();
        }

        [AuthorizeRule(Rule = "Usuario_CanEdit")]
        public IHttpActionResult Put(int id, UsuarioViewModel model)
        {
            if (!ModelState.IsValid)
                throw new CustomApplicationException("ModelState Invalid.");
            UsuarioBusiness bs = new UsuarioBusiness();
            Mapper.CreateMap<UsuarioViewModel, Usuario>().IgnoreAllVirtual(); 
            Usuario usuario = Mapper.Map<UsuarioViewModel, Usuario>(model);

            
            usuario.IdUsuarioModificacion = SecurityManager.GetIdUsuario(User.Identity.Name);

            bs.Update(usuario);

            SecurityManager.InvalidateUser(usuario.NombreUsuario);

            return Ok();
        }

        [AuthorizeRule(Rule = "Usuario_CanDelete")]
        public IHttpActionResult Delete(int id)
        {
            UsuarioBusiness bs = new UsuarioBusiness();
            bs.Delete(id);
            return Ok();
        }

        [AuthorizeRule(Rule = "Usuario_CanList")]
        public IHttpActionResult Get(int id)
        {
            UsuarioBusiness bs = new UsuarioBusiness();
            return Ok(bs.GetById(id));
        }

        [AuthorizeRule(Rule = "Usuario_CanList")]
        [Route("api/usuario/getInitParamsList")]
        [HandleApiException]
        public IHttpActionResult GetInitParamsList()
        {
            UsuarioBusiness bs = new UsuarioBusiness();
            Dictionary<string, object> result = new Dictionary<string, object>();
            result.Add("rolList", bs.GetRolList(true));
            return Ok(result);
        }

        [AuthorizeRule(Rule = "Usuario_CanList")]
        [Route("api/usuario/getInitParamsAddOrUpdate")]
        public IHttpActionResult GetInitParamsAddOrUpdate()
        {
            UsuarioBusiness bs = new UsuarioBusiness();
            Dictionary<string, object> result = new Dictionary<string, object>();
            result.Add("rolList", bs.GetRolList(true));
            return Ok(result);
        }

        [Route("api/usuario/cambiarPassword")]
        public IHttpActionResult CambiarPassword(ChangePasswordViewModel model)
        {
            UsuarioBusiness bs = new UsuarioBusiness();
            model.UserName = User.Identity.Name;
            bs.CambiarPassword(model, true);
            return Ok();
        }
    }

    [HandleApiException]
    public class LoginController : ApiController
    {
        [Route("api/usuario/enviarPassword")]
        public IHttpActionResult PostEnviarPassword([FromBody] string username)
        {
            string mm = Request.RequestUri.Host;
            UsuarioBusiness bs = new UsuarioBusiness();
            bs.RecuperarPassword(username, Request.RequestUri.Authority);
            return Ok();
        }

        [Route("api/usuario/recuperarPassword")]
        public IHttpActionResult GetRecuperarPassword([FromUri] string guid)
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Moved);

            UsuarioBusiness bs = new UsuarioBusiness();
            try
            {
                bs.BlanquearPassword(guid);
                response.Headers.Location = new Uri(string.Format("http://{0}/#/auth/resetpassexitoso", Request.RequestUri.Authority));
                throw new HttpResponseException(response);
            }
            catch (CustomApplicationException)
            {
                response.Headers.Location = new Uri(string.Format("http://{0}/#/auth/resetpasserror", Request.RequestUri.Authority));
                throw new HttpResponseException(response);
            }
        }

        [Route("api/usuario/getUserByGuid")]
        public IHttpActionResult GetUserByGuid([FromUri] string guid)
        {
            UsuarioBusiness ub = new UsuarioBusiness();
            ChangePasswordViewModel user = ub.GetUserByGuid(guid);
            Dictionary<string, object> result = new Dictionary<string, object>();
            result.Add("user", user);
            return Ok(result);
        }

        [Route("api/usuario/cambiarPasswordAnonimo")]
        public IHttpActionResult CambiarPasswordAnonimo(ChangePasswordViewModel model)
        {
            if (model.NewPassword.Length < 1)
                throw new CustomApplicationException("Debe ingresar un valor como nueva contraseña.");

            UsuarioBusiness bs = new UsuarioBusiness();
            bs.CambiarPassword(model, false);
            return Ok();
        }
    }
}

