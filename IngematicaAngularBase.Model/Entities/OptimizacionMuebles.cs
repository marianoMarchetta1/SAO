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
    
    public partial class OptimizacionMuebles
    {
        public int IdOptimizacionMuebles { get; set; }
        public int IdOptimizacionHistorial { get; set; }
        public string Mueble { get; set; }
        public int Cantidad { get; set; }
    
        public virtual OptimizacionHistorial OptimizacionHistorial { get; set; }
    }
}
