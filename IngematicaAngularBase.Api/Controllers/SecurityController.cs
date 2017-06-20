using System.Linq;
using System.Web.Http;
using System.Security.Claims;
using IngematicaAngularBase.Model.ViewModels;
using IngematicaAngularBase.Api.Common;

namespace IngematicaAngularBase.Api.Controllers
{

    [Authorize]
    public class SecurityController : ApiController
    {
           
        [Route("api/security/rol")]
        public SecurityUsuarioViewModel PostRol()
        {
            ClaimsIdentity identity = User.Identity as ClaimsIdentity;

            int b = SecurityManager.GetIdUsuario(User.Identity.Name);

            var a = identity.Claims.Select(c => new
            {
                Type = c.Type,
                Value = c.Value
            });

            return SecurityManager.GetSecurityForUser(identity.Name);        
        }
    }

}

