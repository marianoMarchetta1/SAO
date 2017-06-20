using IngematicaAngularBase.Dal;
using IngematicaAngularBase.Model;
using IngematicaAngularBase.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngematicaAngularBase.Bll
{
    public class LogBusiness
    {
        public int InsertLog(LogDTO log)
        {
            LogDataAccess logDataAccess = new LogDataAccess();
            return logDataAccess.InsertLog(log);
        }
    }
}
