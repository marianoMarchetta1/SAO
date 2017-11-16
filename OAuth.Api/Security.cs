using Ingematica.MailService.Contract;
using Ingematica.MailService.Service;
using Ingematica.MailService.Service.WCF;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OAuth.Api
{

    public class Helper
    {
        public static string GetHash(string input)
        {
            HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider();

            byte[] byteValue = System.Text.Encoding.UTF8.GetBytes(input);

            byte[] byteHash = hashAlgorithm.ComputeHash(byteValue);

            return Convert.ToBase64String(byteHash);
        }
    }

    public class SecurityDataAccess
    {
        public static string SecurityConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["SecurityDB"].ConnectionString;
            }
        }

        public static string NombreAplicacion
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["NombreAplicacion"];
            }
        }


        /* Verificar Usuario */
        public static bool VerificarUsuario(string nombreUsuario, string password)
        {

            SecurityUser usuarioLogin = GetUsuarioLogin(nombreUsuario);
            if (usuarioLogin == null) return false;
            string pass = EncodePassword(password, 1, usuarioLogin.PasswordSalt);
            return pass.Equals(usuarioLogin.Password);
        }

        /* Obtener Usuario */
        private static SecurityUser GetUsuarioLogin(string nombreUsuario)
        {
            SecurityUser usuarioLogin = null;
            using (SqlConnection conn = new SqlConnection(SecurityConnectionString))
            {
                conn.Open();
                string selectQuery = "GetUsuarioLogin";
                using (SqlCommand comm = conn.CreateCommand())
                {
                    comm.CommandText = selectQuery;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);
                    SqlDataReader reader = comm.ExecuteReader();

                    int FieldNombreUsuario = reader.GetOrdinal("NombreUsuario");
                    int FieldPassword = reader.GetOrdinal("Password");
                    int FieldPasswordSalt = reader.GetOrdinal("PasswordSalt");
                    int FieldEmail = reader.GetOrdinal("Email");


                    if (reader.Read())
                    {
                        usuarioLogin = new SecurityUser();
                        usuarioLogin.NombreUsuario = reader.GetString(FieldNombreUsuario);
                        usuarioLogin.Password = reader.GetString(FieldPassword);
                        usuarioLogin.PasswordSalt = reader.GetString(FieldPasswordSalt);
                        usuarioLogin.Email = reader.GetString(FieldEmail);
                    }
                    reader.Close();
                    
                }
                conn.Close();
            }
            return usuarioLogin;
        }

        /* Cambiar Password */
        public static bool CambiarPassword(string nombre, string apellido, string nombreUsuario, string oldPassword, string newPassword, bool oldPasswordValidate)
        {
            SecurityUser usuarioLogin = GetUsuarioLogin(nombreUsuario);
            if (oldPasswordValidate)
            {
                string pass = EncodePassword(oldPassword, 1, usuarioLogin.PasswordSalt);
                if (!pass.Equals(usuarioLogin.Password))
                    return false;
            }
            string newPasswordSalt = GenerateSalt();
            string newPasswordHashed = EncodePassword(newPassword, 1, newPasswordSalt);
            if (!ChangeUsuarioPassword(nombreUsuario, newPasswordHashed, newPasswordSalt))
                return false;
            EnviarEmailNewPassword(nombre, apellido, nombreUsuario, newPassword, "Seguridad - Cambio de Contraseña", usuarioLogin.Email);
            return true;
        }

        private static bool ChangeUsuarioPassword(string nombreUsuario, string newPassword, string newPasswordSalt)
        {
            using (SqlConnection conn = new SqlConnection(SecurityConnectionString))
            {
                conn.Open();
                SqlTransaction sqlTransaction = conn.BeginTransaction(IsolationLevel.ReadCommitted);
                string selectQuery = "ChangeUsuarioPassword";
                using (SqlCommand comm = conn.CreateCommand())
                {
                    try
                    {
                        comm.Transaction = sqlTransaction;
                        comm.CommandText = selectQuery;
                        comm.CommandType = CommandType.StoredProcedure;
                        comm.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);
                        comm.Parameters.AddWithValue("@Password", newPassword);
                        comm.Parameters.AddWithValue("@PasswordSalt", newPasswordSalt);
                        comm.ExecuteNonQuery();
                        sqlTransaction.Commit();
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            return true;
        }

        /* Recuperar Password*/
        public static bool RecuperarPassword(string username, string host)
        {
            SecurityUser usuario = GetUsuarioLogin(username);
            if (usuario == null)
                return false;

            usuario.ResetPasswordGuid = Guid.NewGuid().ToString("n");
            usuario.ResetPasswordFecha = DateTime.Now;

            InsertUserPasswordGuid(usuario);

            string subject = "Recuperar Contraseña";
            string from = System.Configuration.ConfigurationManager.AppSettings["Mail.From"] != null ? System.Configuration.ConfigurationManager.AppSettings["Mail.From"].ToString() : "info@ingematica.net";
            string to = usuario.Email;
            string body = "En la aplicación " + NombreAplicacion +  " se ha pedido la recuperación de la contraseña del usuario " + usuario.NombreUsuario + ".<br/><br/>";
            body += "Por favor, ingrese en el siguiente link para generar la nueva contraseña http://" + host + "/#/auth/cambiarpasswordanonimo?guid=" + HttpUtility.UrlEncode(usuario.ResetPasswordGuid) + ".";
            SendMail(subject, from, to, body);
            return true;
        }

        public static void InsertUserPasswordGuid(SecurityUser usuario)
        {
            using (SqlConnection conn = new SqlConnection(SecurityConnectionString))
            {
                conn.Open();
                string selectQuery = "InsertUserPasswordGuid";
                using (SqlCommand comm = conn.CreateCommand())
                {
                    comm.CommandText = selectQuery;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@NombreUsuario", usuario.NombreUsuario);
                    comm.Parameters.AddWithValue("@ResetPasswordGuid", usuario.ResetPasswordGuid);
                    comm.Parameters.AddWithValue("@ResetPasswordFecha", usuario.ResetPasswordFecha);

                    comm.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        public static bool BlanquearPassword(string guid)
        {
            SecurityUser usuario = GetUsuarioGuid(guid);
            if (usuario == null)
                return false;

            TimeSpan? horasTranscurridas = DateTime.Now - (usuario.ResetPasswordFecha == null ? DateTime.Now : usuario.ResetPasswordFecha);
            if (horasTranscurridas.Value.TotalHours >= Int32.Parse(ConfigurationManager.AppSettings["ResetGuidHours"]))
                return false;

            string generatePassword = GeneratePassword();
            usuario.PasswordSalt = GenerateSalt();
            usuario.Password = EncodePassword(generatePassword, 1, usuario.PasswordSalt);

            ResetUserPassword(usuario);

            string subject = "Nueva Contraseña Generada";
            string from = System.Configuration.ConfigurationManager.AppSettings["Mail.From"] != null ? System.Configuration.ConfigurationManager.AppSettings["Mail.From"].ToString() : "info@ingematica.net";
            string to = usuario.Email;
            string body = "En la aplicación " + NombreAplicacion + " se ha generado una nueva contraseña para el usuario " + usuario.NombreUsuario + ".<br/><br/>";
            body += "La misma es: " + generatePassword + ".";
            SendMail(subject, from, to, body);
            return true;
        }

        public static void ResetUserPassword(SecurityUser usuario)
        {
            using (SqlConnection conn = new SqlConnection(SecurityConnectionString))
            {
                conn.Open();
                string selectQuery = "ResetUserPassword";
                using (SqlCommand comm = conn.CreateCommand())
                {
                    comm.CommandText = selectQuery;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@NombreUsuario", usuario.NombreUsuario);
                    comm.Parameters.AddWithValue("@Password", usuario.Password);
                    comm.Parameters.AddWithValue("@PasswordSalt", usuario.PasswordSalt);

                    comm.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        private static SecurityUser GetUsuarioGuid(string guid)
        {
            SecurityUser usuario = null;
            using (SqlConnection conn = new SqlConnection(SecurityConnectionString))
            {
                conn.Open();
                string selectQuery = "GetUsuarioGuid";
                using (SqlCommand comm = conn.CreateCommand())
                {
                    comm.CommandText = selectQuery;
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@ResetPasswordGuid", guid);
                    SqlDataReader reader = comm.ExecuteReader();

                    int FieldNombreUsuario = reader.GetOrdinal("NombreUsuario");
                    int FieldEmail = reader.GetOrdinal("Email");
                    int FieldResetPasswordFecha = reader.GetOrdinal("ResetPasswordFecha");

                    if (reader.Read())
                    {
                        usuario = new SecurityUser();
                        usuario.NombreUsuario = reader.GetString(FieldNombreUsuario);
                        usuario.Email = reader.GetString(FieldEmail);
                        usuario.ResetPasswordFecha = reader.GetDateTime(FieldResetPasswordFecha);
                    }
                    reader.Close();
                }
                conn.Close();
            }
            return usuario;
        }

        public static string GeneratePassword()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var result = new string(
                Enumerable.Repeat(chars, 8)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            return result;
        }

        /* Gestion de Hash */
        public static string GenerateSalt()
        {
            byte[] buf = new byte[16];
            (new RNGCryptoServiceProvider()).GetBytes(buf);
            return Convert.ToBase64String(buf);
        }

        public static string EncodePassword(string pass, int passwordFormat, string salt)
        {
            if (passwordFormat == 0) // MembershipPasswordFormat.Clear
                return pass;

            byte[] bIn = Encoding.Unicode.GetBytes(pass);
            byte[] bSalt = Convert.FromBase64String(salt);
            byte[] bAll = new byte[bSalt.Length + bIn.Length];
            byte[] bRet = null;

            Buffer.BlockCopy(bSalt, 0, bAll, 0, bSalt.Length);
            Buffer.BlockCopy(bIn, 0, bAll, bSalt.Length, bIn.Length);
            if (passwordFormat == 1)
            { // MembershipPasswordFormat.Hashed
                HashAlgorithm s = HashAlgorithm.Create("SHA1");
                // Hardcoded "SHA1" instead of Membership.HashAlgorithmType
                bRet = s.ComputeHash(bAll);
            }
            else
            {
                //bRet = EncryptPassword(bAll);
            }
            return Convert.ToBase64String(bRet);
        }

        private static string GetHash(string input)
        {
            HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider();

            byte[] byteValue = System.Text.Encoding.UTF8.GetBytes(input);

            byte[] byteHash = hashAlgorithm.ComputeHash(byteValue);

            return Convert.ToBase64String(byteHash);
        }

        /* Envio de Mails */
        private static void EnviarEmailNewPassword(string nombre, string apellido, string nombreUsuario, string newPassword, string subject, string email)
        {
            string mailFrom = System.Configuration.ConfigurationManager.AppSettings["Mail.From"] != null ? System.Configuration.ConfigurationManager.AppSettings["Mail.From"].ToString() : "info@ingematica.net";
            string body = "Se ha modificado la contraseña del usuario " + apellido + ", " + nombre + " en la aplicación de " + NombreAplicacion + ".<br/><br/>";
            //body += "Se indica a continuación, la nueva contraseña: <br/>";
            //body += " Contraseña: " + newPassword + "<br/>";

            SendMail(subject, mailFrom, email, body);
        }

        public static void EnviarEmailPassword(string nombre, string apellido, string usuario, string email, string generatePassword, string subject)
        {
            string mailFrom = System.Configuration.ConfigurationManager.AppSettings["Mail.From"] != null ? System.Configuration.ConfigurationManager.AppSettings["Mail.From"].ToString() : "info@ingematica.net";
            string body = "Se ha dado de alta el usuario " + apellido + ", " + nombre + " en la aplicación de " + NombreAplicacion + ".<br/><br/>";
            body += "Se indican a continuación, el nombre del usuario y la contraseña: <br/>";
            body += " Usuario: " + usuario + "<br/>";
            body += " Contraseña: " + generatePassword + "<br/>";
            SendMail(subject, mailFrom, email, body);
        }

        public static void SendMail(string subject, string From, string To, string body)
        {
            System.Net.Mail.MailAddress from = new System.Net.Mail.MailAddress(From);
            System.Net.Mail.MailAddress to = new System.Net.Mail.MailAddress(To);
            MailMessage message = new MailMessage(from, to);
            message.Body = body;
            message.BodyEncoding = UTF8Encoding.UTF8;
            message.Subject = subject;
            message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            SmtpClient client = new SmtpClient();
            client.Port = 587;
            client.Host = "smtp.gmail.com";
            client.Timeout = 10000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("sao.arquitectura2017@gmail.com", "sao.arquitectura");
            client.Send(message);
        }

        /* Tokens */
        public static SecurityRefreshToken InsertRefreshToken(SecurityRefreshToken token)
        {
            SecurityRefreshToken tokenResult = new SecurityRefreshToken();
            using (SqlConnection conn = new SqlConnection(SecurityConnectionString))
            {
                conn.Open();
                using (SqlCommand comm = conn.CreateCommand())
                {
                    comm.CommandText = "InsertSecurityRefreshToken";
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@Token", (object)token.Token ?? DBNull.Value);
                    comm.Parameters.AddWithValue("@Username", (object)token.Username ?? DBNull.Value);
                    comm.Parameters.AddWithValue("@ClienteId", (object)token.ClienteId ?? DBNull.Value);
                    comm.Parameters.AddWithValue("@IssuedUtc", (object)token.IssuedUtc ?? DBNull.Value);
                    comm.Parameters.AddWithValue("@ExpiresUtc", (object)token.ExpiresUtc ?? DBNull.Value);
                    comm.Parameters.AddWithValue("@ProtectedTicket", (object)token.ProtectedTicket ?? DBNull.Value);
                    comm.ExecuteNonQuery();
                }
            }
            return tokenResult;
        }

        public static SecurityRefreshToken GetRefreshToken(string token)
        {
            SecurityRefreshToken tokenResult = null;
            using (SqlConnection conn = new SqlConnection(SecurityConnectionString))
            {
                conn.Open();
                using (SqlCommand comm = conn.CreateCommand())
                {
                    comm.CommandText = "GetSecurityRefreshToken";
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@Token", token);

                    SqlDataReader reader = comm.ExecuteReader();

                    if (reader.Read())
                    {
                        tokenResult = new SecurityRefreshToken();
                        int FieldIdRefreshTokens = reader.GetOrdinal("IdSecurityRefreshToken");
                        int FieldToken = reader.GetOrdinal("Token");
                        int FieldUsername = reader.GetOrdinal("Username");
                        int FieldClientId = reader.GetOrdinal("ClientId");
                        int FieldIssuedUtc = reader.GetOrdinal("IssuedUtc");
                        int FieldExpiresUtc = reader.GetOrdinal("ExpiresUtc");
                        int FieldProtectedTicket = reader.GetOrdinal("ProtectedTicket");

                        tokenResult.IdSecurityRefreshToken = reader.GetInt32(FieldIdRefreshTokens);
                        tokenResult.Token = reader.GetString(FieldToken);
                        tokenResult.Username = reader.GetString(FieldUsername);
                        tokenResult.ClienteId = reader.GetString(FieldClientId);
                        tokenResult.IssuedUtc = reader.GetDateTime(FieldIssuedUtc);
                        tokenResult.ExpiresUtc = reader.GetDateTime(FieldExpiresUtc);
                        tokenResult.ProtectedTicket = reader.GetString(FieldProtectedTicket);
                    }
                    reader.Close();

                }
            }
            return tokenResult;
        }

        /* Funciones Varias */
        private void Dummy()
        {
            Ingematica.MailService.Service.WCF.BaseWCFServiceClient aa = new BaseWCFServiceClient();
        }

        public static SecurityClient GetClient(string name)
        {
            SecurityClient clientResult = null;
            using (SqlConnection conn = new SqlConnection(SecurityConnectionString))
            {
                conn.Open();
                using (SqlCommand comm = conn.CreateCommand())
                {
                    comm.CommandText = "GetSecurityClient";
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.AddWithValue("@Name", name);

                    SqlDataReader reader = comm.ExecuteReader();

                    if(reader.Read())
                    {
                        clientResult = new SecurityClient();
                        int FieldIdSecurityClient = reader.GetOrdinal("IdSecurityClient");
                        int FieldName = reader.GetOrdinal("Name");
                        int FieldSecret = reader.GetOrdinal("Secret");
                        int FieldApplicationType = reader.GetOrdinal("ApplicationType");
                        int FieldActive = reader.GetOrdinal("Active");
                        int FieldRefreshTokenLifeTime = reader.GetOrdinal("RefreshTokenLifeTime");
                        int FieldAllowedOrigin = reader.GetOrdinal("AllowedOrigin");

                        clientResult.IdSecurityClient = reader.GetInt32(FieldIdSecurityClient);
                        clientResult.Name = reader.GetString(FieldName);
                        clientResult.Secret = reader.GetString(FieldSecret);
                        clientResult.ApplicationType = reader.GetInt32(FieldApplicationType);
                        clientResult.Active = reader.GetBoolean(FieldActive);
                        clientResult.RefreshTokenLifeTime = reader.GetInt32(FieldRefreshTokenLifeTime);
                        clientResult.AllowedOrigin = reader.GetString(FieldAllowedOrigin);

                    }


                    reader.Close();

                }
            }
            return clientResult;
        }

    }

}
