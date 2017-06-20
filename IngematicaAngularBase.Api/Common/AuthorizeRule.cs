using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Net.Http;
using System.Security.Claims;
using System.Net.Http.Headers;

namespace IngematicaAngularBase.Api.Common
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AuthorizeRule : ActionFilterAttribute
    {
        public string Rule { get; set; }
        public string OrRule { get; set; }
        public string AndRule { get; set; }

        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
   
            ClaimsIdentity identity = actionContext.RequestContext.Principal.Identity as ClaimsIdentity;

            var obj = SecurityManager.GetSecurityForUser(identity.Name);

     
            bool authorized = false;
            if (obj.Reglas.Contains(Rule) 
                || (!String.IsNullOrEmpty(OrRule) && obj.Reglas.Contains(OrRule)))
                authorized = true;
            else if (obj.Reglas.Contains("*"))
                authorized = true;

            if (!authorized)
            {
                actionContext.Response = actionContext
                          .Request
                          .CreateErrorResponse(HttpStatusCode.Forbidden, "Access Denied");
            }
        }
    }

    public class NoCacheHeaderFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Response != null) // can be null when exception happens
            {
                actionExecutedContext.Response.Headers.CacheControl =
                    new CacheControlHeaderValue { NoCache = true, NoStore = true, MustRevalidate = true };
                actionExecutedContext.Response.Headers.Pragma.Add(new NameValueHeaderValue("no-cache"));

                if (actionExecutedContext.Response.Content != null) // can be null (for example HTTP 400)
                {
                    actionExecutedContext.Response.Content.Headers.Expires = DateTimeOffset.UtcNow;
                }
            }
        }
    }
}