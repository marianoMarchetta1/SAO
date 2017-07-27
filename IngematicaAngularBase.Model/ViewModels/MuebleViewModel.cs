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
    public class MuebleViewModel
    {
        public int IdMueble { get; set; }
        public decimal? Largo { get; set; }
        public decimal? Ancho { get; set; }
        public decimal? RadioMayor { get; set; }
        public decimal? RadioMenor { get; set; }
        public decimal? Precio { get; set; }
        public int? IdUsuarioAlta { get; set; }
        public string UsuarioAlta { get; set; }
        public int? IdUsuarioModificacion { get; set; }
        public string UsuarioModificacion { get; set; }
        public DateTime? FechaAlta { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string Imagen { get; set; }
        public string Codigo { get; set; }

        [Required]
        [StringLength(256)]
        public string Nombre { get; set; }

        [Required]
        public decimal DistanciaParedes { get; set; }

        [Required]
        public bool Activo { get; set; }

        [Required]
        public decimal DistanciaProximoMueble { get; set; }

        [Required]
        public bool PoseeRadio { get; set; }

        [Required]
        public int OrdenDePrioridad { get; set; }
    }

    public class MuebleQuery : QueryObject
    {
        public int? IdMueble { get; set; }
        public string Nombre { get; set; }
        public bool? Activo { get; set; }
        public string Codigo { get; set; }
    }

    public class MuebleListViewModel
    {
        public int IdMueble { get; set; }
        public string Nombre { get; set; }
        public bool Activo { get; set; }
        public string Codigo { get; set; }
        public string Imagen { get; set; }
    }
}
