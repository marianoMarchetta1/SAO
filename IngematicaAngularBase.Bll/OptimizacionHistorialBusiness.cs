using IngematicaAngularBase.Dal;
using IngematicaAngularBase.Model.Common;
using IngematicaAngularBase.Model.Entities;
using IngematicaAngularBase.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace IngematicaAngularBase.Bll
{
    public class OptimizacionHistorialBusiness
    {
        public QueryResult<OptimizacionHistorialListViewModel> GetList(OptimizacionHistorialQuery query)
        {
            using (var context = new Entities())
            {
                OptimizacionHistorialDataAccess optimizacionHistorialDataAccess = new OptimizacionHistorialDataAccess(context);
                return optimizacionHistorialDataAccess.GetList(query);
            }
        }

        public void Delete(int id)
        {
            using (var context = new Entities())
            {
                var optimizacionHistorial = from optimizacionHistorialElement in context.Set<OptimizacionHistorial>()
                                            where optimizacionHistorialElement.IdOptimizacionHistorial == id
                                            select optimizacionHistorialElement;

                OptimizacionHistorial opHis = optimizacionHistorial.First();

                optimizacionHistorial = from optimizacionHistorialElement in context.Set<OptimizacionHistorial>()
                                        where optimizacionHistorialElement.Nombre == opHis.Nombre
                                        select optimizacionHistorialElement;

                List<int> idsOptimizacionHistorial = optimizacionHistorial.Select(x => x.IdOptimizacionHistorial).ToList();

                var optimizacionHistorialAreaList = from optimizacionHistorialAreaElement in context.Set<OptimizacionHistorialArea>()
                                                    where idsOptimizacionHistorial.Contains(optimizacionHistorialAreaElement.IdOptimizacionHistorial)
                                                    select optimizacionHistorialAreaElement;

                List<int> idsOptimizacionHistorialArea = optimizacionHistorialAreaList.Select(x => x.IdOptimizacionHistorialArea).ToList();

                var optimizacionHistorialAreaMuebleList = from optimizacionHistorialAreaMuebleElement in context.Set<OptimizacionHistorialAreaMueble>()
                                                          where idsOptimizacionHistorialArea.Contains(optimizacionHistorialAreaMuebleElement.IdOptimizacionHistorialArea)
                                                          select optimizacionHistorialAreaMuebleElement;

                optimizacionHistorialAreaMuebleList.ToList().ForEach(x => context.Entry(x).State = EntityState.Deleted);
                optimizacionHistorialAreaList.ToList().ForEach(x => context.Entry(x).State = EntityState.Deleted);
                optimizacionHistorial.ToList().ForEach(x => context.Entry(x).State = EntityState.Deleted);
                              
                context.SaveChanges();
            }
        }
    }
}
