using IngematicaAngularBase.Dal.Specification;
using IngematicaAngularBase.Model.Common;
using IngematicaAngularBase.Model.Entities;
using IngematicaAngularBase.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngematicaAngularBase.Dal
{
    public class MuebleDataAccess
    {
        public MuebleDataAccess(Entities context)
        {
            this.context = context;
        }

        private readonly Entities context;

        public QueryResult<MuebleListViewModel> GetList(MuebleQuery query)
        {
            IQueryable<Mueble> tMueble = context.Set<Mueble>().AsNoTracking();
            tMueble = tMueble
                        .WithContainsNombre(query.Nombre)
                        .WithActivo(query.Activo);

            var result = from mueble in tMueble
                         select new MuebleListViewModel
                         {
                             IdMueble = mueble.IdMueble,
                             Nombre = mueble.Nombre,
                             Activo = mueble.Activo,
                             Codigo = mueble.Codigo,
                             Imagen = mueble.Imagen
                         };
            return result.ToQueryResult(query);
        }

        public MuebleViewModel GetById(int id)
        {
            //IQueryable<Pais> tPais = context.Set<Pais>().AsNoTracking();
            //IQueryable<Usuario> tUsuario = context.Set<Usuario>().AsNoTracking();
            //IQueryable<Usuario> tUsuarioModif = context.Set<Usuario>().AsNoTracking();

            //var result = from pais in tPais
            //             join usuarioalta in tUsuario on pais.IdUsuarioAlta equals usuarioalta.IdUsuario
            //             join usuariomod in tUsuarioModif on pais.IdUsuarioModificacion equals usuariomod.IdUsuario
            //                    into _usuariomod
            //             from usuariomod in _usuariomod.DefaultIfEmpty()
            //             where pais.IdPais == id
            //             select new MuebleViewModel
            //             {
            //                 IdPais = pais.IdPais,
            //                 Nombre = pais.Nombre,
            //                 Activo = pais.Activo,
            //                 IdUsuarioAlta = pais.IdUsuarioAlta,
            //                 UsuarioAlta = (usuarioalta.Nombre != null && usuarioalta.Apellido != null ? usuarioalta.Apellido + ", " + usuarioalta.Nombre : string.Empty),
            //                 FechaAlta = pais.FechaAlta,
            //                 FechaModificacion = pais.FechaModificacion,
            //                 IdUsuarioModificacion = pais.IdUsuarioModificacion,
            //                 UsuarioModificacion = (usuariomod.Nombre != null && usuariomod.Apellido != null ? usuariomod.Apellido + ", " + usuariomod.Nombre : string.Empty)
            //             };
            //return result.FirstOrDefault();

            return null;
        }
    }
}
