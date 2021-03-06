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
	public static partial class OptimizacionHistorialAreaMuebleSpecification
	{
		public static IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialAreaMueble> WithIdOptimizacionHistorialAreaMueble(this IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialAreaMueble> source, int? value)
		{
			if (value != null)
				return source.Where(p => p.IdOptimizacionHistorialAreaMueble == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialAreaMueble> WithIdOptimizacionHistorialArea(this IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialAreaMueble> source, int? value)
		{
			if (value != null)
				return source.Where(p => p.IdOptimizacionHistorialArea == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialAreaMueble> WithVerticeIzquierdaArribaX(this IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialAreaMueble> source, float? value)
		{
			if (value != null)
				return source.Where(p => p.VerticeIzquierdaArribaX == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialAreaMueble> WithVerticeIzquierdaArribaY(this IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialAreaMueble> source, float? value)
		{
			if (value != null)
				return source.Where(p => p.VerticeIzquierdaArribaY == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialAreaMueble> WithVerticeDerechaArribaX(this IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialAreaMueble> source, float? value)
		{
			if (value != null)
				return source.Where(p => p.VerticeDerechaArribaX == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialAreaMueble> WithVerticeDerechaArribaY(this IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialAreaMueble> source, float? value)
		{
			if (value != null)
				return source.Where(p => p.VerticeDerechaArribaY == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialAreaMueble> WithVerticeIzquierdaAbajoX(this IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialAreaMueble> source, float? value)
		{
			if (value != null)
				return source.Where(p => p.VerticeIzquierdaAbajoX == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialAreaMueble> WithVerticeIzquierdaAbajoY(this IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialAreaMueble> source, float? value)
		{
			if (value != null)
				return source.Where(p => p.VerticeIzquierdaAbajoY == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialAreaMueble> WithVerticeDerechaAbajoX(this IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialAreaMueble> source, float? value)
		{
			if (value != null)
				return source.Where(p => p.VerticeDerechaAbajoX == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialAreaMueble> WithVerticeDerechaAbajoY(this IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialAreaMueble> source, float? value)
		{
			if (value != null)
				return source.Where(p => p.VerticeDerechaAbajoY == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialAreaMueble> WithIdMueble(this IQueryable<IngematicaAngularBase.Model.Entities.OptimizacionHistorialAreaMueble> source, int? value)
		{
			if (value != null)
				return source.Where(p => p.IdMueble == value);
			
			return source;
		}
	}
}
