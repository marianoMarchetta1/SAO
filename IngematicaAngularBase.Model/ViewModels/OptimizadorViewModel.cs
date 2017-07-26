using IngematicaAngularBase.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace IngematicaAngularBase.Model.ViewModels
{
    public class OptimizadorOptimizacionViewModel
    {
        public string Nombre { get; set; }

        public bool RegistrarEnHistorial { get; set; }
        public bool OptimizarCosto { get; set; }

        public decimal CostoMaximo { get; set; }

        public List<OptimizacionMueble> MuebleList { get; set; }

        public PlanoData Archivo { get; set; } 

        public int? CantidadPersonas { get; set; }
    }

    public class OptimizacionMueble
    {
        //public int IdOptimizacionMueble { get; set; }
        public int IdMueble { get; set; }
        public int Cantidad { get; set; }
    }

    public class PlanoData
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public string Type { get; set; }
        public string Path { get; set; }
    }
}
