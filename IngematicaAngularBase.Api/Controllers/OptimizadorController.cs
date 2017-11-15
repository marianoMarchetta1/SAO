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
using System.IO.Compression;


namespace IngematicaAngularBase.Api.Controllers
{
    [Authorize]
    [HandleApiException]
    public class OptimizadorController : ApiController
    {

        [AuthorizeRule(Rule = "Optimizador_CanAdd")]
        [Route("api/optimizador/getParamsList")]
        [HandleApiException]
        public IHttpActionResult GetParamsList()
        {
            OptimizadorBusiness bs = new OptimizadorBusiness();
            Dictionary<string, object> result = new Dictionary<string, object>();
            result.Add("muebleList", bs.GetMuebleList(true));
            return Ok(result);
        }

        public class PathFromCliente {
            public string Path { get; set; }
        }

        [Route("api/optimizador/postFileToBlob")]
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

        public class Fuck
        {
            public string Base64 { get; set; }
        }

        [Route("api/optimizador/postFileToBlobImage")]
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

        [HttpPost]
        [Route("api/optimizador/uploadFile")]
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

        [Route("api/optimizador/generate")]
        public IHttpActionResult Generate(OptimizadorOptimizacionViewModel file)
        {
            OptimizadorBusiness optimizadorBusiness = new OptimizadorBusiness();
            RespuestaServidor rta = new RespuestaServidor();
            rta = optimizadorBusiness.Generate(file, SecurityManager.GetIdUsuario(User.Identity.Name));

            return Ok(rta);
        }
    }
}
