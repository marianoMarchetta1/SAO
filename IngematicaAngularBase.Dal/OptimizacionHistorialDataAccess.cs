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
    public class OptimizacionHistorialDataAccess
    {
        public OptimizacionHistorialDataAccess(Entities context)
        {
            this.context = context;
        }

        private readonly Entities context;

        public QueryResult<OptimizacionHistorialListViewModel> GetList(OptimizacionHistorialQuery query)
        {
            IQueryable<OptimizacionHistorial> tOptimizacionHistorial = context.Set<OptimizacionHistorial>().AsNoTracking();
            tOptimizacionHistorial = tOptimizacionHistorial.WithContainsNombre(query.Nombre);

            var result = from optimizacionHistorial in tOptimizacionHistorial
                         select new OptimizacionHistorialListViewModel
                         {
                             CantidadPersonas = optimizacionHistorial.CantidadPersonas,
                             Nombre = optimizacionHistorial.Nombre,
                             CostoMaximo = optimizacionHistorial.CostoMaximo,
                             Escala = optimizacionHistorial.Escala,
                             IdOptimizacionHistorial = optimizacionHistorial.IdOptimizacionHistorial
                         };
            QueryResult<OptimizacionHistorialListViewModel> rta = result.ToQueryResult(query);
            rta.Data = rta.Data.GroupBy(x => x.Nombre).Select(x=> x.First()).ToList();
            return rta;
        }
    }
}
