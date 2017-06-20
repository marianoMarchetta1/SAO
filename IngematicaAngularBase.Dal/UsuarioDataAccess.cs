using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IngematicaAngularBase.Model;
using IngematicaAngularBase.Model.Entities;
using IngematicaAngularBase.Model.ViewModels;
using IngematicaAngularBase.Dal.Specification;
using IngematicaAngularBase.Model.Common;

namespace IngematicaAngularBase.Dal
{
    public class UsuarioDataAccess
    {
        public UsuarioDataAccess(Entities context)
        {
            this.context = context;
        }

        private readonly Entities context;
         
        public UsuarioViewModel GetById(int id)
        {
            IQueryable<Usuario> tUsuario = context.Set<Usuario>().AsNoTracking();
            IQueryable<Rol> tRol = context.Set<Rol>().AsNoTracking();
            IQueryable<Usuario> tUsuarioAlta = context.Set<Usuario>().AsNoTracking();
            IQueryable<Usuario> tUsuarioModificacion = context.Set<Usuario>().AsNoTracking();
            var result = from usuario in tUsuario
                         join rol in tRol on usuario.IdRol equals rol.IdRol
                               into _rol
                         from rol in _rol.DefaultIfEmpty()
                         join usuarioAlta in tUsuarioAlta on usuario.IdUsuarioAlta equals usuarioAlta.IdUsuario
                           into _usuarioAlta
                         from usuarioAlta in _usuarioAlta.DefaultIfEmpty()
                         join usuarioModificacion in tUsuarioModificacion on usuario.IdUsuarioModificacion equals usuarioModificacion.IdUsuario
                            into _usuarioModificacion
                         from usuarioModificacion in _usuarioModificacion.DefaultIfEmpty()
                         where usuario.IdUsuario == id
                         select new UsuarioViewModel
                         {
                             IdUsuario = usuario.IdUsuario,
                             NombreUsuario = usuario.NombreUsuario,
                             Nombre = usuario.Nombre,
                             Apellido = usuario.Apellido,
                             IdRol = usuario.IdRol,
                             RolNombre = rol.Nombre,
                             Activo = usuario.Activo,
                             Interno = usuario.Interno,
                             IdUsuarioAlta = usuario.IdUsuarioAlta,
                             UsuarioAlta = (usuarioAlta.Apellido != null && usuarioAlta.Nombre != null ? usuarioAlta.Apellido + ", " + usuarioAlta.Nombre : string.Empty),
                             FechaAlta = usuario.FechaAlta,
                             IdUsuarioModificacion = usuario.IdUsuarioModificacion,
                             UsuarioModificacion = (usuarioModificacion.Apellido != null && usuarioModificacion.Nombre != null ? usuarioModificacion.Apellido + ", " + usuarioModificacion.Nombre : string.Empty),
                             FechaModificacion = usuario.FechaModificacion,
                             Email = usuario.Email,
                             Password = usuario.Password,
                             PasswordSalt = usuario.PasswordSalt
                         };
            return result.FirstOrDefault();
        }

        public QueryResult<UsuarioListViewModel> GetList(UsuarioQuery query)
        {
            IQueryable<Usuario> tUsuario = context.Set<Usuario>().AsNoTracking();
            IQueryable<Rol> tRol = context.Set<Rol>().AsNoTracking();

            tUsuario = tUsuario.WithNombre(query.Nombre)
                                .WithContainsApellido(query.Apellido)
                                .WithContainsNombreUsuario(query.NombreUsuario)
                                .WithActivo(query.Activo)
                                .WithIdRol(query.IdRol);

            var result = from usuario in tUsuario
                         join rol in tRol on usuario.IdRol equals rol.IdRol
                            into _rol
                         from rol in _rol.DefaultIfEmpty()
                         select new UsuarioListViewModel
                         {
                             IdUsuario = usuario.IdUsuario,
                             Nombre = usuario.Nombre,
                             Apellido = usuario.Apellido,
                             NombreUsuario = usuario.NombreUsuario,
                             Rol = rol.Nombre,
                             Activo = usuario.Activo,
                             Interno = usuario.Interno
                         };
            return result.ToQueryResult(query);
        }

        public List<SelectionListSimple> GetRolList(bool activo)
        {
            IQueryable<Rol> tRol = context.Set<Rol>().AsNoTracking();
            tRol = tRol.WithActivo(activo);

            var result = from rol in tRol
                         orderby rol.Nombre
                         select new SelectionListSimple()
                         {
                             Id = rol.IdRol,
                             Desc = rol.Nombre
                         };
            return result.ToList();
        }

        public ChangePasswordViewModel GetUserByGuid(string guid)
        {
            string mensaje = string.Empty;
            DateTime fechaLimite = DateTime.Now.AddHours(-3);
            Usuario usuario = context.Set<Usuario>().AsNoTracking().Where(p => p.ResetPasswordGuid == guid).FirstOrDefault();

            if (usuario == null)
                mensaje = "El link de cambio de usuario es invalido.";

            if (mensaje == string.Empty && usuario.ResetPasswordFecha < fechaLimite)
                mensaje = "El link de cambio de usuario caduco, genere uno nuevo.";

            ChangePasswordViewModel result = new ChangePasswordViewModel() { UserName = usuario != null ? usuario.NombreUsuario : string.Empty, Message = mensaje };

            return result;
        }
    }
}
