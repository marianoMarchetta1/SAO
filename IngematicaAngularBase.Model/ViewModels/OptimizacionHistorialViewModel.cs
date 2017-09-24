using IngematicaAngularBase.Model;
using IngematicaAngularBase.Model.Common;
using IngematicaAngularBase.Model.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace IngematicaAngularBase.Model.ViewModels
{
    public class OptimizacionHistorialQuery : QueryObject
    {
        public string Nombre { get; set; }
    }

    public class OptimizacionHistorialListViewModel
    {
        public int IdOptimizacionHistorial { get; set; }
        public int? CantidadPersonas { get; set; }
        public string Nombre { get; set; }
        public string Escala { get; set; }
        public double? CostoMaximo { get; set; }
    }
}
