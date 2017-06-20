using IngematicaAngularBase.Api.Common;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;

namespace IngematicaAngularBase.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
           config.Filters.Add(new NoCacheHeaderFilter());

           var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
           jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
    }
}