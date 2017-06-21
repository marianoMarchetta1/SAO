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
                             Activo = mueble.Activo
                         };
            return result.ToQueryResult(query);
        }
    }
}
