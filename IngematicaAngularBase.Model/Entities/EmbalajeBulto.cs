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
    
    public partial class EmbalajeBulto
    {
        public int IdEmbalajeBulto { get; set; }
        public int IdEmbalaje { get; set; }
        public int Cantidad { get; set; }
        public int IdPedidoProductoMedida { get; set; }
        public int Scrap { get; set; }
    
        public virtual Embalaje Embalaje { get; set; }
        public virtual PedidoProductoMedida PedidoProductoMedida { get; set; }
    }
}
