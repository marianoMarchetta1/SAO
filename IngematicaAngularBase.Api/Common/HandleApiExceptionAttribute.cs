using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Net.Http;
using IngematicaAngularBase.Model;
using IngematicaAngularBase.Bll;
using IngematicaAngularBase.Model.Common;
using System.Data.Entity.Infrastructure;

namespace IngematicaAngularBase.Api.Common
{
    [AttributeUsage((AttributeTargets.Class | AttributeTargets.Method), AllowMultiple = false, Inherited = true)]
    public class HandleApiException : ExceptionFilterAttribute
    {

        public override void OnException(HttpActionExecutedContext context)
        {
            var request = context.ActionContext.Request;

            var controllerName = context.ActionContext.ControllerContext.ControllerDescriptor.ControllerName;
            var actionName = context.ActionContext.ActionDescriptor.ActionName;
            HttpError httpError = new HttpError();
            GenerateHttpError(context, httpError, actionName);
            context.Response = request.CreateResponse(HttpStatusCode.InternalServerError, httpError);
        }

        private void GenerateHttpError(HttpActionExecutedContext context, HttpError error, string actionName)
        {
            List<string> modelSateErrorsList = new List<string>();
            Exception ex = context.Exception;
            if (ex is CustomApplicationException)
            {
                error.Add("Message", ex.Message);               
                error.Add("ManagedException", true);
                error.Add("ModelSateErrors", false);
              
                error.Add("ApplicationErrors", false);

                if (!context.ActionContext.ModelState.IsValid)
                {
                    Dictionary<string, object> modelSateErrors = new Dictionary<string,object>();

                    foreach (var item1 in context.ActionContext.ModelState)
                    {

                        foreach (var item2 in item1.Value.Errors)
                            modelSateErrorsList.Add(item2.ErrorMessage);

                        modelSateErrors.Add(item1.Key, modelSateErrorsList);
                    }
                    error["ModelSateErrors"] = modelSateErrors;
                    error["ManagedException"] = false;
                }   

                CustomApplicationException customEx = ex as CustomApplicationException;
                if (customEx.Errors != null && customEx.Errors.Any())
                    error["ApplicationErrors"] = customEx.Errors;
                
                                                
            }
            else if (ex is System.Data.Entity.Infrastructure.DbUpdateConcurrencyException)
            {
                error.Add("Message", "Ocurrió un error al intentar guardar el Registro. Probablemente haya sido eliminado o modificado desde otro lugar.");
                error.Add("ErrorCode", 3);
                error.Add("ManagedException", true);

            }
            else if (ex is System.Data.Entity.Infrastructure.DbUpdateException && actionName == "Delete" && TryDecodeDbUpdateException(ex, 547))
            {         
                error.Add("Message", "No se puede eliminar el Registro, porque tiene datos relacionados.");
                error.Add("ErrorCode", 2);
                error.Add("ManagedException", true);
            }
            else if (ex is System.Data.Entity.Infrastructure.DbUpdateException && actionName == "Put" && TryDecodeDbUpdateException(ex, 547))
            {
                error.Add("Message", "No se pueden guardar los datos debido a que se han eliminado registros que tienen datos relacionados.");
                error.Add("ErrorCode", 2);
                error.Add("ManagedException", true);
            }
            else if (ex is System.Data.Entity.Infrastructure.DbUpdateException && (actionName == "Post" || actionName == "Put"))
            {
                error.Add("Message", "Ha ocurrido un error inesperado, al intentar guardar los datos.");
                error.Add("ErrorCode", 1);
                error.Add("ManagedException", false);
            }
            else
            {
                error.Add("Message", "Ha ocurrido un error inesperado.");
                error.Add("ErrorCode", 0);
                error.Add("ManagedException", false);
            }

            if (!Convert.ToBoolean(error["ManagedException"]))
            {              
                LogDTO log = new LogDTO();
                log.Accion = actionName;
                log.Usuario = "Admin";
                log.Descripcion = ex.Message;
                if (modelSateErrorsList.Any())
                    log.Descripcion2 = string.Join(",", modelSateErrorsList);
                else
                    log.Descripcion2 = ex.StackTrace;
                LogBusiness logBusiness = new LogBusiness();
                int idErrorLog = logBusiness.InsertLog(log);

                error["Message"] = error["Message"] + " (Codigo de error:  " + idErrorLog.ToString() + ")";
            }
        }

        private bool TryDecodeDbUpdateException(Exception ex, int errorNumSearch)
        {
            if (!(ex.InnerException is System.Data.Entity.Core.UpdateException) ||
                !(ex.InnerException.InnerException is System.Data.SqlClient.SqlException))
                return false;
            var sqlException =
                (System.Data.SqlClient.SqlException)ex.InnerException.InnerException;
            for (int i = 0; i < sqlException.Errors.Count; i++)
            {
                var errorNum = sqlException.Errors[i].Number;
                if (errorNum == errorNumSearch)
                    return true;
            }
            return false;
        }
    }

}