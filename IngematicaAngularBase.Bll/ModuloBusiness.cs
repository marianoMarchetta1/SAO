using IngematicaAngularBase.Dal;
using IngematicaAngularBase.Model;
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
    public class ModuloBusiness
    {
        public List<SelectionListSimple> GetModuloList()
        {
            using (var context = new Entities())
            {
                ModuloDataAccess da = new ModuloDataAccess(context);
                return da.GetModuloList();
            }
        }
    }
}
