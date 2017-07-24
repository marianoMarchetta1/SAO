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
    public class OptimizadorDataAccess
    {
        public OptimizadorDataAccess(Entities context)
        {
            this.context = context;
        }

        private readonly Entities context;

        public List<SelectionListSimple> GetMuebleList(bool activo)
        {
            IQueryable<Mueble> tMueble = context.Set<Mueble>().AsNoTracking();
            tMueble = tMueble.WithActivo(activo);

            var result = from mueble in tMueble
                         orderby mueble.Nombre
                         select new SelectionListSimple()
                         {
                             Id = mueble.IdMueble,
                             Desc = mueble.Nombre
                         };
            return result.ToList();
        }
    }
}
