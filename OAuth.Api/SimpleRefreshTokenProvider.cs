
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Security.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Configuration;


namespace OAuth.Api
{



    public class SimpleRefreshTokenProvider : IAuthenticationTokenProvider
    {

        private static int CookieDays;
        private static string Domain;
        static SimpleRefreshTokenProvider()
        {
            CookieDays = Int32.Parse(ConfigurationManager.AppSettings["CookiesDays"]);

            string domain = ConfigurationManager.AppSettings["Domain"];
            if (string.IsNullOrEmpty(domain))
                Domain = null;

        }


        public async Task CreateAsync(AuthenticationTokenCreateContext context)
        {

            var clientid = context.Ticket.Properties.Dictionary["as:client_id"];

            if (string.IsNullOrEmpty(clientid))
                return;

            var refreshTokenLifeTime = context.OwinContext.Get<string>("as:clientRefreshTokenLifeTime");

            var securityRefreshToken = new SecurityRefreshToken()
            {
                Token = Helper.GetHash(Guid.NewGuid().ToString("n")),
                ClienteId = clientid,
                Username = context.Ticket.Identity.Name,
                IssuedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.AddMinutes(Convert.ToDouble(refreshTokenLifeTime))
            };
            context.Ticket.Properties.IssuedUtc = securityRefreshToken.IssuedUtc;
            context.Ticket.Properties.ExpiresUtc = securityRefreshToken.ExpiresUtc;


            securityRefreshToken.ProtectedTicket = context.SerializeTicket();



            SecurityDataAccess.InsertRefreshToken(securityRefreshToken);

            context.Response.Cookies.Delete("refresh_token");
            Microsoft.Owin.CookieOptions cookieOptions = new Microsoft.Owin.CookieOptions { Expires = DateTime.Now.AddDays(CookieDays), HttpOnly = true, Domain = Domain };
            context.Response.Cookies.Append("refresh_token", securityRefreshToken.Token, cookieOptions);
            // context.Response.Cookies.Append("refresh_token", securityRefreshToken.Token);
            // context.SetToken(securityRefreshToken.Token);



        }


        public async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            var allowedOrigin = context.OwinContext.Get<string>("as:clientAllowedOrigin");
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { allowedOrigin });

            string hashedTokenId = context.Request.Cookies["refresh_token"];
            //string hashedTokenId = Helper.GetHash(context.Token);

            //Todo solo para ese cliente
            SecurityRefreshToken securityRefreshToken;

            securityRefreshToken = SecurityDataAccess.GetRefreshToken(hashedTokenId);


            if (securityRefreshToken != null)
            {
                context.DeserializeTicket(securityRefreshToken.ProtectedTicket);
            }

        }

        public void Create(AuthenticationTokenCreateContext context)
        {
            throw new NotImplementedException();
        }

        public void Receive(AuthenticationTokenReceiveContext context)
        {
            throw new NotImplementedException();
        }
    }
}