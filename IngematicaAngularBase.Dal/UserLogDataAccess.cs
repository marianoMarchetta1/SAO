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
    public class UserLogDataAccess
    {
        public UserLogDataAccess(Entities context)
        {
            this.context = context;
        }

        private readonly Entities context;

        public QueryResult<UserLogListViewModel> GetList(UserLogQuery query)
        {
            IQueryable<UserLog> tUserLog = context.Set<UserLog>().AsNoTracking();
            tUserLog = tUserLog.WithContainsUsuario(query.Usuario);

            var result = from userLog in tUserLog
                         select new UserLogListViewModel
                         {
                             Descripcion = userLog.Descripcion,
                             Fecha = userLog.Fecha,
                             IdUserLog = userLog.IdUserLog,
                             Usuario = userLog.Usuario
                         };
            return result.ToQueryResult(query);
        }
    }
}
