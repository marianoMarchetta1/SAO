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
    
    public partial class OptimizacionHistorialArea
    {
        public OptimizacionHistorialArea()
        {
            this.OptimizacionHistorialAreaMueble = new HashSet<OptimizacionHistorialAreaMueble>();
        }
    
        public int IdOptimizacionHistorialArea { get; set; }
        public int IdOptimizacionHistorial { get; set; }
        public float VerticeIzquierdaArribaX { get; set; }
        public float VerticeIzquierdaArribaY { get; set; }
        public float VerticeDerechaArribaX { get; set; }
        public float VerticeDerechaArribaY { get; set; }
        public float VerticeIzquierdaAbajoX { get; set; }
        public float VerticeIzquierdaAbajoY { get; set; }
        public float VerticeDerechaAbajoX { get; set; }
        public float VerticeDerechaAbajoY { get; set; }
    
        public virtual OptimizacionHistorial OptimizacionHistorial { get; set; }
        public virtual ICollection<OptimizacionHistorialAreaMueble> OptimizacionHistorialAreaMueble { get; set; }
    }
}