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
	public static partial class InyectoraSpecification
	{
		public static IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> WithIdInyectora(this IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> source, int? value)
		{
			if (value != null)
				return source.Where(p => p.IdInyectora == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> WithNombre(this IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> source, string value)
		{
			if (value != null)
				return source.Where(p => p.Nombre == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> WithContainsNombre(this IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> source, string value)
		{
			if (value != null)
				return source.Where(p => p.Nombre.Contains(value));
			
			return source;
		}


		public static IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> WithCantidadMateriales(this IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> source, int? value)
		{
			if (value != null)
				return source.Where(p => p.CantidadMateriales == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> WithCantidadEstaciones(this IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> source, int? value)
		{
			if (value != null)
				return source.Where(p => p.CantidadEstaciones == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> WithCantidadSuelas1(this IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> source, decimal? value)
		{
			if (value != null)
				return source.Where(p => p.CantidadSuelas1 == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> WithCantidadSuelas2(this IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> source, decimal? value)
		{
			if (value != null)
				return source.Where(p => p.CantidadSuelas2 == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> WithCantidadSuelas3(this IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> source, decimal? value)
		{
			if (value != null)
				return source.Where(p => p.CantidadSuelas3 == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> WithCantidadSuelas4(this IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> source, decimal? value)
		{
			if (value != null)
				return source.Where(p => p.CantidadSuelas4 == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> WithCantidadSuelas5(this IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> source, decimal? value)
		{
			if (value != null)
				return source.Where(p => p.CantidadSuelas5 == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> WithCantidadSuelas6(this IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> source, decimal? value)
		{
			if (value != null)
				return source.Where(p => p.CantidadSuelas6 == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> WithActivo(this IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> source, bool? value)
		{
			if (value != null)
				return source.Where(p => p.Activo == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> WithIdUsuarioAlta(this IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> source, int? value)
		{
			if (value != null)
				return source.Where(p => p.IdUsuarioAlta == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> WithFechaAlta(this IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> source, System.DateTime? value)
		{
			if (value != null)
				return source.Where(p => p.FechaAlta == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> WithFechaAltaBetweenWithTime(this IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> source, System.DateTime? lower, System.DateTime? upper)
		{
            var range = source;
           
            if (lower != null)
                range = range.Where(p => p.FechaAlta >= lower);

            if (upper != null)
                range = range.Where(p => p.FechaAlta <= upper);

            return range;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> WithFechaAltaBetween(this IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> source, System.DateTime? lower, System.DateTime? upper)
		{
            var range = source;
           
            if (lower != null)
                range = range.Where(p => p.FechaAlta >= lower);

            if (upper != null)
			{
				upper = upper.Value.Date.AddDays(1);
                range = range.Where(p => p.FechaAlta < upper);
			}

            return range;
		}


		public static IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> WithIdUsuarioModificacion(this IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> source, int? value)
		{
			if (value != null)
				return source.Where(p => p.IdUsuarioModificacion == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> WithFechaModificacion(this IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> source, System.DateTime? value)
		{
			if (value != null)
				return source.Where(p => p.FechaModificacion == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> WithFechaModificacionBetweenWithTime(this IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> source, System.DateTime? lower, System.DateTime? upper)
		{
            var range = source;
           
            if (lower != null)
                range = range.Where(p => p.FechaModificacion >= lower);

            if (upper != null)
                range = range.Where(p => p.FechaModificacion <= upper);

            return range;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> WithFechaModificacionBetween(this IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> source, System.DateTime? lower, System.DateTime? upper)
		{
            var range = source;
           
            if (lower != null)
                range = range.Where(p => p.FechaModificacion >= lower);

            if (upper != null)
			{
				upper = upper.Value.Date.AddDays(1);
                range = range.Where(p => p.FechaModificacion < upper);
			}

            return range;
		}


		public static IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> WithMoldeFinDia(this IQueryable<IngematicaAngularBase.Model.Entities.Inyectora> source, bool? value)
		{
			if (value != null)
				return source.Where(p => p.MoldeFinDia == value);
			
			return source;
		}
	}
}
