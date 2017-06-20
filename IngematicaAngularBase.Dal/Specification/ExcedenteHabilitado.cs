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
	public static partial class ExcedenteHabilitadoSpecification
	{
		public static IQueryable<IngematicaAngularBase.Model.Entities.ExcedenteHabilitado> WithIdExcedenteHabilitado(this IQueryable<IngematicaAngularBase.Model.Entities.ExcedenteHabilitado> source, int? value)
		{
			if (value != null)
				return source.Where(p => p.IdExcedenteHabilitado == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.ExcedenteHabilitado> WithFecha(this IQueryable<IngematicaAngularBase.Model.Entities.ExcedenteHabilitado> source, System.DateTime? value)
		{
			if (value != null)
				return source.Where(p => p.Fecha == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.ExcedenteHabilitado> WithFechaBetweenWithTime(this IQueryable<IngematicaAngularBase.Model.Entities.ExcedenteHabilitado> source, System.DateTime? lower, System.DateTime? upper)
		{
            var range = source;
           
            if (lower != null)
                range = range.Where(p => p.Fecha >= lower);

            if (upper != null)
                range = range.Where(p => p.Fecha <= upper);

            return range;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.ExcedenteHabilitado> WithFechaBetween(this IQueryable<IngematicaAngularBase.Model.Entities.ExcedenteHabilitado> source, System.DateTime? lower, System.DateTime? upper)
		{
            var range = source;
           
            if (lower != null)
                range = range.Where(p => p.Fecha >= lower);

            if (upper != null)
			{
				upper = upper.Value.Date.AddDays(1);
                range = range.Where(p => p.Fecha < upper);
			}

            return range;
		}


		public static IQueryable<IngematicaAngularBase.Model.Entities.ExcedenteHabilitado> WithIdProgramacionPedidoProducto(this IQueryable<IngematicaAngularBase.Model.Entities.ExcedenteHabilitado> source, int? value)
		{
			if (value != null)
				return source.Where(p => p.IdProgramacionPedidoProducto == value);
			
			return source;
		}
	}
}
