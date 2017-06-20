using Microsoft.Owin;
using IngematicaAngularBase.Web;

[assembly: OwinStartup(typeof(Startup))]
namespace IngematicaAngularBase.Web
{
    using System.Web.Http;
    using Microsoft.Owin;
    using Microsoft.Owin.Extensions;
    using Microsoft.Owin.FileSystems;
    using Microsoft.Owin.StaticFiles;
    using Owin;
    using Microsoft.Owin.Security.OAuth;
    using System;
    using OAuth.Api;
    using Microsoft.Owin.Security.Cookies;
    using Microsoft.Owin.Security;
    using System.Threading.Tasks;
    using IngematicaAngularBase.Web;


    public class Startup
    {
        public static OAuthBearerAuthenticationOptions OAuthBearerOptions { get; private set; }

        public void Configuration(IAppBuilder app)
        {

            
            var httpConfiguration = new HttpConfiguration();

            
            // Configure Web API Routes:
            // - Enable Attribute Mapping
            // - Enable Default routes at /api.
            httpConfiguration.MapHttpAttributeRoutes();
            WebApiConfig.Register(httpConfiguration);

            httpConfiguration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            ConfigureOAuth(app);
            
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            app.UseWebApi(httpConfiguration);

            // Make ./public the default root of the static files in our Web Application.
            app.UseFileServer(new FileServerOptions
            {
                RequestPath = new PathString(string.Empty),
                FileSystem = new PhysicalFileSystem("./app"),
                EnableDirectoryBrowsing = false,
            });


            app.UseStageMarker(PipelineStage.MapHandler);
        }


        public void ConfigureOAuth(IAppBuilder app)
        {
            //use a cookie to temporarily store information about a user logging in with a third party login provider
            app.UseExternalSignInCookie(Microsoft.AspNet.Identity.DefaultAuthenticationTypes.ExternalCookie);
            OAuthBearerOptions = new OAuthBearerAuthenticationOptions();

            OAuthAuthorizationServerOptions OAuthServerOptions = new OAuthAuthorizationServerOptions()
            {

                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                    #if DEBUG
                        AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(2),
                    #else
                        AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(5),
                    #endif
                Provider = new SimpleAuthorizationServerProvider(),
                RefreshTokenProvider = new SimpleRefreshTokenProvider(),
                ApplicationCanDisplayErrors = false,
                
            };

            // Token Generation
            app.UseOAuthAuthorizationServer(OAuthServerOptions);
            app.UseOAuthBearerAuthentication(OAuthBearerOptions);
           // app.Use(typeof(LoggingMiddleware));
        }
    }
    public class LoggingMiddleware : OwinMiddleware
    {
        public LoggingMiddleware(OwinMiddleware next)
            : base(next)
        { }

        public async override Task Invoke(IOwinContext context)
        {
            //Console.WriteLine("Begin Request");

            var response = context.Response;
            var request = context.Request;

            response.OnSendingHeaders(state =>
            {
                var resp = (OwinResponse)state;
                if (!resp.Headers.Keys.Contains("Pragma"))
                resp.Headers.Add("Pragma", new string[] { "no-cache" });
                if (!resp.Headers.Keys.Contains("Cache-Control"))
                resp.Headers.Add("Cache-Control", new string[] { "no-cache" });
            }, response);

            await Next.Invoke(context);
            //Console.WriteLine("End Request");
        }
    }
}
