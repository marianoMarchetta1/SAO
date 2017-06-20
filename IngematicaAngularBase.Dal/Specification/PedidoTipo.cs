//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// <responsibility>
//     Especification Pattern used in the Data Layer to filter the sets of
//     data entities.
// </responsibility>
//------------------------------------------------------------------------------

using System;
using System.Data.Entity;
using System.Linq;

namespace IngematicaAngularBase.Dal.Specification
{
	public static partial class PedidoTipoSpecification
	{
		public static IQueryable<IngematicaAngularBase.Model.Entities.PedidoTipo> WithIdPedidoTipo(this IQueryable<IngematicaAngularBase.Model.Entities.PedidoTipo> source, int? value)
		{
			if (value != null)
				return source.Where(p => p.IdPedidoTipo == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.PedidoTipo> WithNombre(this IQueryable<IngematicaAngularBase.Model.Entities.PedidoTipo> source, string value)
		{
			if (value != null)
				return source.Where(p => p.Nombre == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.PedidoTipo> WithContainsNombre(this IQueryable<IngematicaAngularBase.Model.Entities.PedidoTipo> source, string value)
		{
			if (value != null)
				return source.Where(p => p.Nombre.Contains(value));
			
			return source;
		}

	}
}
