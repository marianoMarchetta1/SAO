using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngematicaAngularBase.Model.Common
{
    public class SelectionListSimple
    {
        public int Id;
        public string Desc;
        public object Data;
        public decimal DecimalData { get; set; }
    }

    public class SelectionListInfo
    {
        public int Id;
        public string Desc;
        public string Info;
    }
}
