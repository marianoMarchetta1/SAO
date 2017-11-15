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
using System.Net.Http.Headers;
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

        [AuthorizeRule(Rule = "OptimizacionHistorial_CanList")]
        public IHttpActionResult Get(int id)
        {
            OptimizacionHistorialBusiness bs = new OptimizacionHistorialBusiness();
            return Ok(bs.GetById(id));
        }

        [Route("api/optimizacionHistorial/postFileToBlob")]
        public HttpResponseMessage PostFileToBlob(PathFromCliente pathFromClient)
        {
            HttpResponseMessage result = null;
            var info = System.IO.File.GetAttributes(pathFromClient.Path);
            result = Request.CreateResponse(HttpStatusCode.OK);
            result.Content = new StreamContent(new FileStream(pathFromClient.Path, FileMode.Open, FileAccess.Read));
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            //result.Content.Headers.Add("x-filename", "");
            result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            result.Content.Headers.ContentDisposition.FileName = pathFromClient.Path.Substring(pathFromClient.Path.Length - 23, 23);
            return result;
        }

        public class PathFromCliente
        {
            public string Path { get; set; }
        }

        public class Fuck
        {
            public string Base64 { get; set; }
        }

        [Route("api/optimizacionHistorial/postFileToBlobImage")]
        public IHttpActionResult PostFileToBlobImage(PathFromCliente pathFromClient)
        {
            System.Drawing.Image image = System.Drawing.Image.FromFile(pathFromClient.Path);
            MemoryStream m = new MemoryStream();

            image.Save(m, image.RawFormat);
            byte[] imageBytes = m.ToArray();

            Fuck fuck = new Fuck();
            fuck.Base64 = Convert.ToBase64String(imageBytes);
            return Ok(fuck);


        }
    }
}
