using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IngematicaAngularBase.Model;
using System.ComponentModel.DataAnnotations;
using IngematicaAngularBase.Model.Common;

namespace IngematicaAngularBase.Model.ViewModels
{

    public class RolQuery: QueryObject
    {
        [Range(1, int.MaxValue)]
        public int ?IdRol { get; set; }
        public string Nombre { get; set; }
        public bool ?Activo { get; set; }
    }

    public class RolListViewModel
    {
        public int IdRol { get; set; }
        public string Nombre { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
        public bool Interno { get; set; }
    }

}

