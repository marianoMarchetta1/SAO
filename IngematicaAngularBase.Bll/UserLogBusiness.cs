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
    public class UserLogBusiness
    {
        public QueryResult<UserLogListViewModel> GetList(UserLogQuery query)
        {
            using (var context = new Entities())
            {
                UserLogDataAccess userLogDataAccess = new UserLogDataAccess(context);
                return userLogDataAccess.GetList(query);
            }
        }

        public void Delete(int id)
        {
            using (var context = new Entities())
            {
                UserLog entity = context.Set<UserLog>().FirstOrDefault(x => x.IdUserLog == id);
                context.Entry(entity).State = EntityState.Deleted;
                context.SaveChanges();
            }
        }
    }
}
