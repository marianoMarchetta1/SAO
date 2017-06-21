using IngematicaAngularBase.Dal;
using IngematicaAngularBase.Model.Common;
using IngematicaAngularBase.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngematicaAngularBase.Bll
{
    public class MuebleBusiness
    {
        public QueryResult<MuebleListViewModel> GetList(MuebleQuery query)
        {
            using (var context = new Entities())
            {
                MuebleDataAccess muebleDataAccess = new MuebleDataAccess(context);
                return muebleDataAccess.GetList(query);
            }
        }
    }
}
