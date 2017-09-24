﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IngematicaAngularBase.Dal
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    using IngematicaAngularBase.Model.Entities;

    public partial class Entities : DbContext
    {
        public Entities()
            : base("name=Entities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Cliente> Cliente { get; set; }
        public virtual DbSet<Localidad> Localidad { get; set; }
        public virtual DbSet<Log> Log { get; set; }
        public virtual DbSet<Modulo> Modulo { get; set; }
        public virtual DbSet<Mueble> Mueble { get; set; }
        public virtual DbSet<OptimizacionHistorial> OptimizacionHistorial { get; set; }
        public virtual DbSet<OptimizacionHistorialArea> OptimizacionHistorialArea { get; set; }
        public virtual DbSet<OptimizacionHistorialAreaMueble> OptimizacionHistorialAreaMueble { get; set; }
        public virtual DbSet<Pais> Pais { get; set; }
        public virtual DbSet<Provincia> Provincia { get; set; }
        public virtual DbSet<Regla> Regla { get; set; }
        public virtual DbSet<Rol> Rol { get; set; }
        public virtual DbSet<RolRegla> RolRegla { get; set; }
        public virtual DbSet<SecurityClient> SecurityClient { get; set; }
        public virtual DbSet<SecurityRefreshToken> SecurityRefreshToken { get; set; }
        public virtual DbSet<sysdiagrams> sysdiagrams { get; set; }
        public virtual DbSet<Usuario> Usuario { get; set; }
    
        public virtual int ChangeUsuarioPassword(string nombreUsuario, string password, string passwordSalt)
        {
            var nombreUsuarioParameter = nombreUsuario != null ?
                new ObjectParameter("NombreUsuario", nombreUsuario) :
                new ObjectParameter("NombreUsuario", typeof(string));
    
            var passwordParameter = password != null ?
                new ObjectParameter("Password", password) :
                new ObjectParameter("Password", typeof(string));
    
            var passwordSaltParameter = passwordSalt != null ?
                new ObjectParameter("PasswordSalt", passwordSalt) :
                new ObjectParameter("PasswordSalt", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("ChangeUsuarioPassword", nombreUsuarioParameter, passwordParameter, passwordSaltParameter);
        }
    
        public virtual ObjectResult<GetSecurityClient_Result> GetSecurityClient(string name)
        {
            var nameParameter = name != null ?
                new ObjectParameter("Name", name) :
                new ObjectParameter("Name", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetSecurityClient_Result>("GetSecurityClient", nameParameter);
        }
    
        public virtual ObjectResult<GetSecurityRefreshToken_Result> GetSecurityRefreshToken(string token)
        {
            var tokenParameter = token != null ?
                new ObjectParameter("Token", token) :
                new ObjectParameter("Token", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetSecurityRefreshToken_Result>("GetSecurityRefreshToken", tokenParameter);
        }
    
        public virtual ObjectResult<GetUsuarioGuid_Result> GetUsuarioGuid(string resetPasswordGuid)
        {
            var resetPasswordGuidParameter = resetPasswordGuid != null ?
                new ObjectParameter("ResetPasswordGuid", resetPasswordGuid) :
                new ObjectParameter("ResetPasswordGuid", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetUsuarioGuid_Result>("GetUsuarioGuid", resetPasswordGuidParameter);
        }
    
        public virtual ObjectResult<GetUsuarioLogin_Result> GetUsuarioLogin(string nombreUsuario)
        {
            var nombreUsuarioParameter = nombreUsuario != null ?
                new ObjectParameter("NombreUsuario", nombreUsuario) :
                new ObjectParameter("NombreUsuario", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetUsuarioLogin_Result>("GetUsuarioLogin", nombreUsuarioParameter);
        }
    
        public virtual int InsertInterfaceLog(Nullable<System.DateTime> fecha, string descripcion, string stackTrace, string exceptionMessage, Nullable<int> idInterface, Nullable<int> idInterfaceLogTipo, string otrosDatos, string archivoNombre)
        {
            var fechaParameter = fecha.HasValue ?
                new ObjectParameter("Fecha", fecha) :
                new ObjectParameter("Fecha", typeof(System.DateTime));
    
            var descripcionParameter = descripcion != null ?
                new ObjectParameter("Descripcion", descripcion) :
                new ObjectParameter("Descripcion", typeof(string));
    
            var stackTraceParameter = stackTrace != null ?
                new ObjectParameter("StackTrace", stackTrace) :
                new ObjectParameter("StackTrace", typeof(string));
    
            var exceptionMessageParameter = exceptionMessage != null ?
                new ObjectParameter("ExceptionMessage", exceptionMessage) :
                new ObjectParameter("ExceptionMessage", typeof(string));
    
            var idInterfaceParameter = idInterface.HasValue ?
                new ObjectParameter("IdInterface", idInterface) :
                new ObjectParameter("IdInterface", typeof(int));
    
            var idInterfaceLogTipoParameter = idInterfaceLogTipo.HasValue ?
                new ObjectParameter("IdInterfaceLogTipo", idInterfaceLogTipo) :
                new ObjectParameter("IdInterfaceLogTipo", typeof(int));
    
            var otrosDatosParameter = otrosDatos != null ?
                new ObjectParameter("OtrosDatos", otrosDatos) :
                new ObjectParameter("OtrosDatos", typeof(string));
    
            var archivoNombreParameter = archivoNombre != null ?
                new ObjectParameter("ArchivoNombre", archivoNombre) :
                new ObjectParameter("ArchivoNombre", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("InsertInterfaceLog", fechaParameter, descripcionParameter, stackTraceParameter, exceptionMessageParameter, idInterfaceParameter, idInterfaceLogTipoParameter, otrosDatosParameter, archivoNombreParameter);
        }
    
        public virtual ObjectResult<Nullable<int>> InsertLog(string descripcion, string descripcion2, string controller, string accion, string otrosDatos, string usuario)
        {
            var descripcionParameter = descripcion != null ?
                new ObjectParameter("Descripcion", descripcion) :
                new ObjectParameter("Descripcion", typeof(string));
    
            var descripcion2Parameter = descripcion2 != null ?
                new ObjectParameter("Descripcion2", descripcion2) :
                new ObjectParameter("Descripcion2", typeof(string));
    
            var controllerParameter = controller != null ?
                new ObjectParameter("Controller", controller) :
                new ObjectParameter("Controller", typeof(string));
    
            var accionParameter = accion != null ?
                new ObjectParameter("Accion", accion) :
                new ObjectParameter("Accion", typeof(string));
    
            var otrosDatosParameter = otrosDatos != null ?
                new ObjectParameter("OtrosDatos", otrosDatos) :
                new ObjectParameter("OtrosDatos", typeof(string));
    
            var usuarioParameter = usuario != null ?
                new ObjectParameter("Usuario", usuario) :
                new ObjectParameter("Usuario", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<int>>("InsertLog", descripcionParameter, descripcion2Parameter, controllerParameter, accionParameter, otrosDatosParameter, usuarioParameter);
        }
    
        public virtual int InsertSecurityRefreshToken(string token, string username, string clienteId, Nullable<System.DateTime> issuedUtc, Nullable<System.DateTime> expiresUtc, string protectedTicket)
        {
            var tokenParameter = token != null ?
                new ObjectParameter("Token", token) :
                new ObjectParameter("Token", typeof(string));
    
            var usernameParameter = username != null ?
                new ObjectParameter("Username", username) :
                new ObjectParameter("Username", typeof(string));
    
            var clienteIdParameter = clienteId != null ?
                new ObjectParameter("ClienteId", clienteId) :
                new ObjectParameter("ClienteId", typeof(string));
    
            var issuedUtcParameter = issuedUtc.HasValue ?
                new ObjectParameter("IssuedUtc", issuedUtc) :
                new ObjectParameter("IssuedUtc", typeof(System.DateTime));
    
            var expiresUtcParameter = expiresUtc.HasValue ?
                new ObjectParameter("ExpiresUtc", expiresUtc) :
                new ObjectParameter("ExpiresUtc", typeof(System.DateTime));
    
            var protectedTicketParameter = protectedTicket != null ?
                new ObjectParameter("ProtectedTicket", protectedTicket) :
                new ObjectParameter("ProtectedTicket", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("InsertSecurityRefreshToken", tokenParameter, usernameParameter, clienteIdParameter, issuedUtcParameter, expiresUtcParameter, protectedTicketParameter);
        }
    
        public virtual int InsertUserPasswordGuid(string nombreUsuario, string resetPasswordGuid, Nullable<System.DateTime> resetPasswordFecha)
        {
            var nombreUsuarioParameter = nombreUsuario != null ?
                new ObjectParameter("NombreUsuario", nombreUsuario) :
                new ObjectParameter("NombreUsuario", typeof(string));
    
            var resetPasswordGuidParameter = resetPasswordGuid != null ?
                new ObjectParameter("ResetPasswordGuid", resetPasswordGuid) :
                new ObjectParameter("ResetPasswordGuid", typeof(string));
    
            var resetPasswordFechaParameter = resetPasswordFecha.HasValue ?
                new ObjectParameter("ResetPasswordFecha", resetPasswordFecha) :
                new ObjectParameter("ResetPasswordFecha", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("InsertUserPasswordGuid", nombreUsuarioParameter, resetPasswordGuidParameter, resetPasswordFechaParameter);
        }
    
        public virtual int ResetUserPassword(string nombreUsuario, string password, string passwordSalt)
        {
            var nombreUsuarioParameter = nombreUsuario != null ?
                new ObjectParameter("NombreUsuario", nombreUsuario) :
                new ObjectParameter("NombreUsuario", typeof(string));
    
            var passwordParameter = password != null ?
                new ObjectParameter("Password", password) :
                new ObjectParameter("Password", typeof(string));
    
            var passwordSaltParameter = passwordSalt != null ?
                new ObjectParameter("PasswordSalt", passwordSalt) :
                new ObjectParameter("PasswordSalt", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("ResetUserPassword", nombreUsuarioParameter, passwordParameter, passwordSaltParameter);
        }
    
        public virtual int sp_alterdiagram(string diagramname, Nullable<int> owner_id, Nullable<int> version, byte[] definition)
        {
            var diagramnameParameter = diagramname != null ?
                new ObjectParameter("diagramname", diagramname) :
                new ObjectParameter("diagramname", typeof(string));
    
            var owner_idParameter = owner_id.HasValue ?
                new ObjectParameter("owner_id", owner_id) :
                new ObjectParameter("owner_id", typeof(int));
    
            var versionParameter = version.HasValue ?
                new ObjectParameter("version", version) :
                new ObjectParameter("version", typeof(int));
    
            var definitionParameter = definition != null ?
                new ObjectParameter("definition", definition) :
                new ObjectParameter("definition", typeof(byte[]));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_alterdiagram", diagramnameParameter, owner_idParameter, versionParameter, definitionParameter);
        }
    
        public virtual int sp_creatediagram(string diagramname, Nullable<int> owner_id, Nullable<int> version, byte[] definition)
        {
            var diagramnameParameter = diagramname != null ?
                new ObjectParameter("diagramname", diagramname) :
                new ObjectParameter("diagramname", typeof(string));
    
            var owner_idParameter = owner_id.HasValue ?
                new ObjectParameter("owner_id", owner_id) :
                new ObjectParameter("owner_id", typeof(int));
    
            var versionParameter = version.HasValue ?
                new ObjectParameter("version", version) :
                new ObjectParameter("version", typeof(int));
    
            var definitionParameter = definition != null ?
                new ObjectParameter("definition", definition) :
                new ObjectParameter("definition", typeof(byte[]));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_creatediagram", diagramnameParameter, owner_idParameter, versionParameter, definitionParameter);
        }
    
        public virtual int sp_dropdiagram(string diagramname, Nullable<int> owner_id)
        {
            var diagramnameParameter = diagramname != null ?
                new ObjectParameter("diagramname", diagramname) :
                new ObjectParameter("diagramname", typeof(string));
    
            var owner_idParameter = owner_id.HasValue ?
                new ObjectParameter("owner_id", owner_id) :
                new ObjectParameter("owner_id", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_dropdiagram", diagramnameParameter, owner_idParameter);
        }
    
        public virtual ObjectResult<sp_helpdiagramdefinition_Result> sp_helpdiagramdefinition(string diagramname, Nullable<int> owner_id)
        {
            var diagramnameParameter = diagramname != null ?
                new ObjectParameter("diagramname", diagramname) :
                new ObjectParameter("diagramname", typeof(string));
    
            var owner_idParameter = owner_id.HasValue ?
                new ObjectParameter("owner_id", owner_id) :
                new ObjectParameter("owner_id", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_helpdiagramdefinition_Result>("sp_helpdiagramdefinition", diagramnameParameter, owner_idParameter);
        }
    
        public virtual ObjectResult<sp_helpdiagrams_Result> sp_helpdiagrams(string diagramname, Nullable<int> owner_id)
        {
            var diagramnameParameter = diagramname != null ?
                new ObjectParameter("diagramname", diagramname) :
                new ObjectParameter("diagramname", typeof(string));
    
            var owner_idParameter = owner_id.HasValue ?
                new ObjectParameter("owner_id", owner_id) :
                new ObjectParameter("owner_id", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_helpdiagrams_Result>("sp_helpdiagrams", diagramnameParameter, owner_idParameter);
        }
    
        public virtual int sp_renamediagram(string diagramname, Nullable<int> owner_id, string new_diagramname)
        {
            var diagramnameParameter = diagramname != null ?
                new ObjectParameter("diagramname", diagramname) :
                new ObjectParameter("diagramname", typeof(string));
    
            var owner_idParameter = owner_id.HasValue ?
                new ObjectParameter("owner_id", owner_id) :
                new ObjectParameter("owner_id", typeof(int));
    
            var new_diagramnameParameter = new_diagramname != null ?
                new ObjectParameter("new_diagramname", new_diagramname) :
                new ObjectParameter("new_diagramname", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_renamediagram", diagramnameParameter, owner_idParameter, new_diagramnameParameter);
        }
    
        public virtual int sp_upgraddiagrams()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_upgraddiagrams");
        }
    }
}
