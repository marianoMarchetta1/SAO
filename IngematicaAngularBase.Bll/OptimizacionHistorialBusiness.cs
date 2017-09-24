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
    }
}
