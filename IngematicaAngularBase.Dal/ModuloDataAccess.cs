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
    public class ModuloDataAccess
    {
        public ModuloDataAccess(Entities context)
        {
            this.context = context;
        }

        private readonly Entities context;

        public List<SelectionListSimple> GetModuloList()
        {
            IQueryable<Modulo> moduloSet = context.Set<Modulo>().AsNoTracking();

            var result = from modulo in moduloSet
                         orderby modulo.Nombre
                         select new SelectionListSimple()
                         {
                             Id = modulo.IdModulo,
                             Desc = modulo.Nombre
                         };

            return result.ToList();
        }
    }
}
