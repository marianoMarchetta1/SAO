//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IngematicaAngularBase.Model.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class MateriaPrima
    {
        public MateriaPrima()
        {
            this.MaterialMateriaPrima = new HashSet<MaterialMateriaPrima>();
        }
    
        public int IdMateriaPrima { get; set; }
        public int IdMateriaPrimaClase { get; set; }
        public string Nombre { get; set; }
        public int IdMateriaPrimaMarca { get; set; }
        public string Comentario { get; set; }
        public bool Activo { get; set; }
        public int IdUsuarioAlta { get; set; }
        public System.DateTime FechaAlta { get; set; }
        public Nullable<int> IdUsuarioModificacion { get; set; }
        public Nullable<System.DateTime> FechaModificacion { get; set; }
    
        public virtual ICollection<MaterialMateriaPrima> MaterialMateriaPrima { get; set; }
        public virtual MateriaPrimaClase MateriaPrimaClase { get; set; }
        public virtual MateriaPrimaMarca MateriaPrimaMarca { get; set; }
        public virtual Usuario Usuario { get; set; }
        public virtual Usuario Usuario1 { get; set; }
    }
}
