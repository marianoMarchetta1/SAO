using IngematicaAngularBase.Model.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngematicaAngularBase.Model.ViewModels
{
    public class MuebleViewModel
    {
        public int IdMueble { get; set; }

        [Required]
        [StringLength(256)]
        public string Nombre { get; set; }

        [Required]
        public bool Activo { get; set; }

        public int? IdUsuarioAlta { get; set; }
        public string UsuarioAlta { get; set; }

        public int? IdUsuarioModificacion { get; set; }
        public string UsuarioModificacion { get; set; }

        public DateTime? FechaAlta { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }

    public class MuebleQuery : QueryObject
    {
        public int? IdMueble { get; set; }
        public string Nombre { get; set; }
        public bool? Activo { get; set; }
    }

    public class MuebleListViewModel
    {
        public int IdMueble { get; set; }
        public string Nombre { get; set; }
        public bool Activo { get; set; }
    }
}
