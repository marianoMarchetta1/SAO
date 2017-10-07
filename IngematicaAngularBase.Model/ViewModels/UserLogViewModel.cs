using IngematicaAngularBase.Model.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngematicaAngularBase.Model.ViewModels
{
    public class UserLogQuery : QueryObject
    {
        public string Usuario { get; set; }
    }

    public class UserLogListViewModel
    {
        public int IdUserLog { get; set; }
        public string Usuario { get; set; }
        public string Descripcion { get; set; }
        public DateTime Fecha { get; set; }
    }
}
