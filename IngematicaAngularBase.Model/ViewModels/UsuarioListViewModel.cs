using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IngematicaAngularBase.Model;
using System.ComponentModel.DataAnnotations;
using IngematicaAngularBase.Model.Common;

namespace IngematicaAngularBase.Model.ViewModels
{

    public class UsuarioQuery: QueryObject
    {
        [Range(1, int.MaxValue)]
        public int? IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string NombreUsuario { get; set; }
        public int? IdRol { get; set; }
        public bool? Activo { get; set; }
    }

    public class UsuarioListViewModel
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string NombreUsuario { get; set; }
        public string Rol { get; set; }
        public bool Activo { get; set; }
        public bool Interno { get; set; }
    }

}

