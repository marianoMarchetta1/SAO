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
	public static partial class OptimizacionHistorialAreaSpecification
	{
		public static IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialArea> WithIdOptimizacionHistorialArea(this IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialArea> source, int? value)
		{
			if (value != null)
				return source.Where(p => p.IdOptimizacionHistorialArea == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialArea> WithIdOptimizacionHistorial(this IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialArea> source, int? value)
		{
			if (value != null)
				return source.Where(p => p.IdOptimizacionHistorial == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialArea> WithVerticeIzquierdaArribaX(this IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialArea> source, float? value)
		{
			if (value != null)
				return source.Where(p => p.VerticeIzquierdaArribaX == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialArea> WithVerticeIzquierdaArribaY(this IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialArea> source, float? value)
		{
			if (value != null)
				return source.Where(p => p.VerticeIzquierdaArribaY == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialArea> WithVerticeDerechaArribaX(this IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialArea> source, float? value)
		{
			if (value != null)
				return source.Where(p => p.VerticeDerechaArribaX == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialArea> WithVerticeDerechaArribaY(this IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialArea> source, float? value)
		{
			if (value != null)
				return source.Where(p => p.VerticeDerechaArribaY == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialArea> WithVerticeIzquierdaAbajoX(this IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialArea> source, float? value)
		{
			if (value != null)
				return source.Where(p => p.VerticeIzquierdaAbajoX == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialArea> WithVerticeIzquierdaAbajoY(this IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialArea> source, float? value)
		{
			if (value != null)
				return source.Where(p => p.VerticeIzquierdaAbajoY == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialArea> WithVerticeDerechaAbajoX(this IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialArea> source, float? value)
		{
			if (value != null)
				return source.Where(p => p.VerticeDerechaAbajoX == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialArea> WithVerticeDerechaAbajoY(this IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialArea> source, float? value)
		{
			if (value != null)
				return source.Where(p => p.VerticeDerechaAbajoY == value);
			
			return source;
		}
	}
}