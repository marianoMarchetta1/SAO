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
                        .WithActivo(query.Activo)
                        .WithCodigo(query.Codigo);

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
            IQueryable<Mueble> tMueble = context.Set<Mueble>().AsNoTracking();
            IQueryable<Usuario> tUsuario = context.Set<Usuario>().AsNoTracking();
            IQueryable<Usuario> tUsuarioModif = context.Set<Usuario>().AsNoTracking();

            var result = from mueble in tMueble
                         join usuarioalta in tUsuario on mueble.IdUsuarioAlta equals usuarioalta.IdUsuario
                         join usuariomod in tUsuarioModif on mueble.IdUsuarioModificacion equals usuariomod.IdUsuario
                                into _usuariomod
                         from usuariomod in _usuariomod.DefaultIfEmpty()
                         where mueble.IdMueble == id
                         select new MuebleViewModel
                         {
                             IdMueble = mueble.IdMueble,
                             Largo = mueble.Largo,
                             Ancho = mueble.Ancho,
                             RadioMayor = mueble.RadioMayor,
                             RadioMenor = mueble.RadioMenor,
                             Precio = mueble.Precio,
                             IdUsuarioAlta = mueble.IdUsuarioAlta,
                             UsuarioAlta = (usuarioalta.Nombre != null && usuarioalta.Apellido != null ? usuarioalta.Apellido + ", " + usuarioalta.Nombre : string.Empty),
                             Imagen = mueble.Imagen,
                             Codigo = mueble.Codigo,
                             Nombre = mueble.Nombre,
                             Activo = mueble.Activo,
                             DistanciaProximoMueble = mueble.DistanciaProximoMueble,
                             PoseeRadio = mueble.PoseeRadio,
                             FechaAlta = mueble.FechaAlta,
                             FechaModificacion = mueble.FechaModificacion,
                             IdUsuarioModificacion = mueble.IdUsuarioModificacion,
                             UsuarioModificacion = (usuariomod.Nombre != null && usuariomod.Apellido != null ? usuariomod.Apellido + ", " + usuariomod.Nombre : string.Empty),
                             OrdenDePrioridad = (int)mueble.OrdenDePrioridad
                         };

            return result.FirstOrDefault();
        }

        public List<Mueble> GetMuebleList(List<int> idsMueble)
        {
            IQueryable<Mueble> tMueble = context.Set<Mueble>().AsNoTracking();

            var result = from mueble in tMueble
                         where idsMueble.Contains(mueble.IdMueble)
                         select mueble;

            return result.ToList();
        }
    }
}
