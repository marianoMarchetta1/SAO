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
    
    public partial class MateriaPrimaMarca
    {
        public MateriaPrimaMarca()
        {
            this.MateriaPrima = new HashSet<MateriaPrima>();
        }
    
        public int IdMateriaPrimaMarca { get; set; }
        public string Nombre { get; set; }
        public bool Activo { get; set; }
        public int IdUsuarioAlta { get; set; }
        public System.DateTime FechaAlta { get; set; }
        public Nullable<int> IdUsuarioModificacion { get; set; }
        public Nullable<System.DateTime> FechaModificacion { get; set; }
    
        public virtual ICollection<MateriaPrima> MateriaPrima { get; set; }
        public virtual Usuario Usuario { get; set; }
        public virtual Usuario Usuario1 { get; set; }
    }
}
