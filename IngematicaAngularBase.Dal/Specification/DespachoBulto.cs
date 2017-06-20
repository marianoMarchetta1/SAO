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
	public static partial class DespachoBultoSpecification
	{
		public static IQueryable<IngematicaAngularBase.Model.Entities.DespachoBulto> WithIdDespachoBulto(this IQueryable<IngematicaAngularBase.Model.Entities.DespachoBulto> source, int? value)
		{
			if (value != null)
				return source.Where(p => p.IdDespachoBulto == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.DespachoBulto> WithIdDespacho(this IQueryable<IngematicaAngularBase.Model.Entities.DespachoBulto> source, int? value)
		{
			if (value != null)
				return source.Where(p => p.IdDespacho == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.DespachoBulto> WithIdEmbalaje(this IQueryable<IngematicaAngularBase.Model.Entities.DespachoBulto> source, int? value)
		{
			if (value != null)
				return source.Where(p => p.IdEmbalaje == value);
			
			return source;
		}
	}
}
