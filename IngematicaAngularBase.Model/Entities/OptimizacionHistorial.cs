//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IngematicaAngularBase.Model.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class OptimizacionHistorial
    {
        public OptimizacionHistorial()
        {
            this.OptimizacionHistorialArea = new HashSet<OptimizacionHistorialArea>();
            this.OptimizacionMuebles = new HashSet<OptimizacionMuebles>();
        }
    
        public int IdOptimizacionHistorial { get; set; }
        public string Nombre { get; set; }
        public Nullable<int> CantidadPersonas { get; set; }
        public string Escala { get; set; }
        public Nullable<bool> OptimizarCosto { get; set; }
        public Nullable<float> CostoMaximo { get; set; }
    
        public virtual ICollection<OptimizacionHistorialArea> OptimizacionHistorialArea { get; set; }
        public virtual ICollection<OptimizacionMuebles> OptimizacionMuebles { get; set; }
    }
}
