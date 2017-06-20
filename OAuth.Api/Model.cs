using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;

namespace OAuth.Api
{
    public class SecurityUser
    {
        public string NombreUsuario { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public string Email { get; set; }
        public string ResetPasswordGuid { get; set; }
        public DateTime? ResetPasswordFecha { get; set; }
    }

    public class SecurityRefreshToken
    {
        public int IdSecurityRefreshToken { get; set; }
        public string Token { get; set; }
        public string Username { get; set; }
        public string ClienteId { get; set; }
        public DateTime IssuedUtc { get; set; }
        public DateTime ExpiresUtc { get; set; }
        public string ProtectedTicket { get; set; }
    }

    public class SecurityClient
    {
        public int IdSecurityClient { get; set; }
        public string Name { get; set; }
        public string Secret { get; set; }
        public string Description { get; set; }
        public int ApplicationType { get; set; }
        public bool Active { get; set; }
        public int RefreshTokenLifeTime { get; set; }
        public string AllowedOrigin { get; set; }
    }

}
