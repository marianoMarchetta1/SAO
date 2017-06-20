using IngematicaAngularBase.Dal;
using IngematicaAngularBase.Model;
using IngematicaAngularBase.Model.Common;
using IngematicaAngularBase.Model.Entities;
using IngematicaAngularBase.Model.ViewModels;
using Ingematica.MailService.Common;
using Ingematica.MailService.Contract;
using Ingematica.MailService.Service;
using Ingematica.MailService.Service.WCF;
using OAuth.Api;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace IngematicaAngularBase.Bll
{
    public class UsuarioBusiness
    {

        public QueryResult<UsuarioListViewModel> GetList(UsuarioQuery query)
        {
            using (var context = new Entities())
            {
                UsuarioDataAccess usuarioDataAccess = new UsuarioDataAccess(context);
                return usuarioDataAccess.GetList(query);
            }
        }

        public UsuarioViewModel GetById(int id)
        {
            using (var context = new Entities())
            {
                UsuarioDataAccess usuarioDataAccess = new UsuarioDataAccess(context);
                return usuarioDataAccess.GetById(id);
            }
        }

        public void Add(Usuario entity)
        {
            using (var context = new Entities())
            {
                var item = context.Set<Usuario>().AsNoTracking().FirstOrDefault(x => (x.NombreUsuario == entity.NombreUsuario || x.Email == entity.Email));
                if (item != null)
                {
                    if (item.NombreUsuario.ToLower() == entity.NombreUsuario.ToLower())
                        throw new CustomApplicationException("Ya existe un usuario con el mismo nombre usuario.");
                    else
                        throw new CustomApplicationException("Ya existe un usuario con el mismo email.");

                }
#if DEBUG
                string generatePassword = "123456";
#else
                string generatePassword = SecurityDataAccess.GeneratePassword();
#endif

                entity.PasswordSalt = SecurityDataAccess.GenerateSalt();
                entity.Password = SecurityDataAccess.EncodePassword(generatePassword, 1, entity.PasswordSalt);
                entity.FechaModificacion = DateTime.Now;
                entity.FechaAlta = DateTime.Now;
                entity.Interno = false;

                context.Usuario.Add(entity);
                context.SaveChanges();

                SecurityDataAccess.EnviarEmailPassword(entity.Nombre, entity.Apellido, entity.NombreUsuario, entity.Email, generatePassword, "Seguridad - Usuario generado");
            }
        }

        public void Update(Usuario entity)
        {
            using (var context = new Entities())
            {
                var item = context.Set<Usuario>().AsNoTracking().FirstOrDefault(x => (x.NombreUsuario == entity.NombreUsuario || x.Email == entity.Email));
                if (item != null && item.IdUsuario != entity.IdUsuario)
                {
                    if (item.NombreUsuario == entity.NombreUsuario)
                        throw new CustomApplicationException("Ya existe un usuario con el mismo nombre usuario.");
                    else
                        throw new CustomApplicationException("Ya existe un usuario con el mismo email.");

                }

                Usuario entityBD = context.Set<Usuario>().AsNoTracking().FirstOrDefault(x => x.IdUsuario == entity.IdUsuario);
                if (entityBD != null && entityBD.Interno)
                {
                    entity.NombreUsuario = entityBD.NombreUsuario;
                    entity.IdRol = entityBD.IdRol;
                    entity.Activo = entityBD.Activo;
                }

                context.Entry(entity).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            using (var context = new Entities())
            {
                Usuario entity = context.Set<Usuario>().FirstOrDefault(x => x.IdUsuario == id);
                if (entity != null && entity.Interno == true)
                    throw new CustomApplicationException("El usuario no se puede eliminar.");
                context.Entry(entity).State = EntityState.Deleted;
                context.SaveChanges();
            }
        }

        public void CambiarPassword(ChangePasswordViewModel model, bool oldPasswordValidate)
        {
            if (model.NewPassword != model.NewPasswordRepeat)
                throw new CustomApplicationException("Las nuevas contraseñas no coinciden.");
            if (model.Password == model.NewPassword)
                throw new CustomApplicationException("La nueva contraseña es idéntica a la actual.");

            using (var context = new Entities())
            {
                Usuario entity = context.Set<Usuario>().FirstOrDefault(x => x.NombreUsuario == model.UserName);
                if (!SecurityDataAccess.CambiarPassword(entity.Nombre, entity.Apellido, model.UserName, model.Password, model.NewPassword, oldPasswordValidate))
                    throw new CustomApplicationException("La contraseña actual no es correcta.");
            }
        }

        public void RecuperarPassword(string username, string host)
        {
            if (!SecurityDataAccess.RecuperarPassword(username, host))
                throw new CustomApplicationException("El nombre de usuario es incorrecto.");
        }

        public void BlanquearPassword(string guid)
        {
            if (!SecurityDataAccess.BlanquearPassword(guid))
                throw new CustomApplicationException("El guid no es válido.");
        }

        public List<SelectionListSimple> GetRolList(bool activo)
        {
            using (var context = new Entities())
            {
                UsuarioDataAccess usuarioDataAccess = new UsuarioDataAccess(context);
                return usuarioDataAccess.GetRolList(activo);
            }
        }

        public ChangePasswordViewModel GetUserByGuid(string guid)
        {
            using (var context = new Entities())
            {
                UsuarioDataAccess usuarioDataAccess = new UsuarioDataAccess(context);
                return usuarioDataAccess.GetUserByGuid(guid);
            }
        }
    }
}
