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
    
    public partial class PedidoProductoCajon
    {
        public int IdPedidoProductoCajon { get; set; }
        public int IdPedidoProductoProceso { get; set; }
        public int Cajon { get; set; }
        public int Cantidad { get; set; }
        public Nullable<bool> Embalado { get; set; }
    
        public virtual PedidoProductoProceso PedidoProductoProceso { get; set; }
    }
}
