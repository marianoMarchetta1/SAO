using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IngematicaAngularBase.Model;
using System.ComponentModel.DataAnnotations;

namespace IngematicaAngularBase.Model.ViewModels
{

    public class RolViewModel
    {
        public int IdRol { get; set; }

        [Required]
        [StringLength(256)]
        public string Nombre { get; set; }
        public bool Activo { get; set; }

        public List<RolReglaViewModel> RolRegla { get; set; }

        public bool Interno { get; set; }

        //AUDITORIA
        public System.DateTime FechaAlta { get; set; }
        public Nullable<System.DateTime> FechaModificacion { get; set; }
        public Nullable<int> IdUsuarioAlta { get; set; }
        public Nullable<int> IdUsuarioModificacion { get; set; }
        public string UsuarioAlta { get; set; }
        public string UsuarioModificacion { get; set; }
    }

    public class RolReglaViewModel
    {
        public int IdRolRegla { get; set; }
        public int IdRegla { get; set; }
        public int IdRol { get; set; }
        public int? IdModulo { get; set; }
        public string ReglaNombre { get; set; }
        public string ModuloNombre { get; set; }
        public bool Checked { get; set; }
    }
}

