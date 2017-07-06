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

        [HttpPost]
        [Route("api/mueble/uploadFile")]
        public async Task<HttpResponseMessage> Upload()
        {
            if (!Request.Content.IsMimeMultipartContent())
                this.Request.CreateResponse(HttpStatusCode.UnsupportedMediaType);

            string path = System.Configuration.ConfigurationManager.AppSettings["TmpFiles"];

            var provider = new MultipartFormDataStreamProvider(path);
            var result = await Request.Content.ReadAsMultipartAsync(provider);

            if (string.IsNullOrEmpty(result.FileData[0].Headers.ContentDisposition.FileName))
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotAcceptable, "This request is not properly formatted"));
            }
            string fileName = result.FileData[0].Headers.ContentDisposition.FileName;
            if (fileName.StartsWith("\"") && fileName.EndsWith("\""))
            {
                fileName = fileName.Trim('"');
            }
            if (fileName.Contains(@"/") || fileName.Contains(@"\"))
            {
                fileName = Path.GetFileName(fileName);
            }

            var returnData = result.FileData[0].LocalFileName;

            return this.Request.CreateResponse(HttpStatusCode.OK, new { returnData });
        }
    }
}
