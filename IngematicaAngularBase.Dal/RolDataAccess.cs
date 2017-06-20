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
    public class RolDataAccess
    {

        public RolDataAccess(Entities context)
        {
            this.context = context;                
        }

        private readonly Entities context;


        public RolViewModel GetById(int id)
        {
            RolViewModel rolViewModel;

            IQueryable<Rol> tRol = context.Set<Rol>().AsNoTracking();
            IQueryable<Usuario> tUsuarioAlta = context.Set<Usuario>().AsNoTracking();
            IQueryable<Usuario> tUsuarioModificacion = context.Set<Usuario>().AsNoTracking();

            var result = from rol in tRol
                         join usuarioAlta in tUsuarioAlta on rol.IdUsuarioAlta equals usuarioAlta.IdUsuario
                            into _usuarioAlta
                         from usuarioAlta in _usuarioAlta.DefaultIfEmpty()
                         join usuarioModificacion in tUsuarioModificacion on rol.IdUsuarioModificacion equals usuarioModificacion.IdUsuario
                            into _usuarioModificacion
                         from usuarioModificacion in _usuarioModificacion.DefaultIfEmpty()
                         where rol.IdRol == id
                         select new RolViewModel
                         {
                             IdRol = rol.IdRol,
                             Nombre = rol.Nombre,
                             Activo = rol.Activo,
                             FechaAlta = rol.FechaAlta,
                             FechaModificacion = rol.FechaModificacion,
                             Interno = rol.Interno,
                             IdUsuarioAlta = rol.IdUsuarioAlta,
                             IdUsuarioModificacion = rol.IdUsuarioModificacion,
                             UsuarioAlta = (usuarioAlta.Apellido != null && usuarioAlta.Nombre != null ? usuarioAlta.Apellido + ", " + usuarioAlta.Nombre : string.Empty),
                             UsuarioModificacion = (usuarioModificacion.Apellido != null && usuarioModificacion.Nombre != null ? usuarioModificacion.Apellido + ", " + usuarioModificacion.Nombre : string.Empty)
                         };

            rolViewModel = result.FirstOrDefault();

            if (rolViewModel != null)
            {
                IQueryable<RolRegla> tRolRegla = context.Set<RolRegla>().AsNoTracking();
                IQueryable<Regla> tRegla = context.Set<Regla>().AsNoTracking();
                IQueryable<Modulo> moduloSet = context.Set<Modulo>().AsNoTracking();
                tRolRegla = tRolRegla.WithIdRol(rolViewModel.IdRol);

                var resultRolRegla = from regla in tRegla
                                     join rolRegla in tRolRegla on regla.IdRegla equals rolRegla.IdRegla
                                     into _rolRegla
                                     from rr in _rolRegla.DefaultIfEmpty()
                                     join modulo in moduloSet on regla.IdModulo equals modulo.IdModulo into _modulo from mm in _modulo.DefaultIfEmpty()
                                     orderby regla.Descripcion
                                     select new RolReglaViewModel()
                                     {
                                         IdRolRegla = rr.IdRolRegla == null ? 0 : rr.IdRolRegla,
                                         IdRegla = regla.IdRegla,
                                         IdRol = rr.IdRol == null ? 0 : rr.IdRol,
                                         IdModulo = regla.IdModulo,
                                         ModuloNombre = mm.Nombre,
                                         ReglaNombre = regla.Descripcion,
                                         Checked = (rr.IdRolRegla != null)
                                     };

                rolViewModel.RolRegla = resultRolRegla.ToList();
            }

            return rolViewModel;
        }


        public QueryResult<RolListViewModel> GetList(RolQuery query)
        {
            IQueryable<Rol> tRol = context.Set<Rol>().AsNoTracking();
            tRol = tRol
                       .WithActivo(query.Activo)
                       .WithContainsNombre(query.Nombre);

            var result = from rol in tRol
                         select new RolListViewModel
                         {
                             IdRol = rol.IdRol,
                             Nombre = rol.Nombre,
                             Activo = rol.Activo,
                             FechaAlta = rol.FechaAlta,
                             Interno = rol.Interno
                         };

            return result.ToQueryResult(query);
        }


        public RolViewModel CreateRolConReglas()
        {
            RolViewModel rolViewModel = new RolViewModel();

            IQueryable<RolRegla> rolReglaSet = context.Set<RolRegla>().AsNoTracking();
            IQueryable<Regla> reglaSet = context.Set<Regla>().AsNoTracking();
            IQueryable<Modulo> moduloSet = context.Set<Modulo>().AsNoTracking();
            rolReglaSet = rolReglaSet.WithIdRol(rolViewModel.IdRol);

            var resultRolRegla = from regla in reglaSet
                                 join modulo in moduloSet on regla.IdModulo equals modulo.IdModulo into _modulo from mm in _modulo.DefaultIfEmpty()
                                 orderby regla.Descripcion
                                 select new RolReglaViewModel()
                                 {
                                     IdRolRegla = 0,
                                     IdRegla = regla.IdRegla,
                                     IdRol = 0,
                                     IdModulo = regla.IdModulo,
                                     ModuloNombre = mm.Nombre,
                                     ReglaNombre = regla.Descripcion,
                                     Checked = false
                                 };

            rolViewModel.RolRegla = resultRolRegla.ToList();
            rolViewModel.Activo = true;
            return rolViewModel;
        }
    }
}
