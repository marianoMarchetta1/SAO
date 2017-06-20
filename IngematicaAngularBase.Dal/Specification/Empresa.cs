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
	public static partial class EmpresaSpecification
	{
		public static IQueryable<IngematicaAngularBase.Model.Entities.Empresa> WithIdEmpresa(this IQueryable<IngematicaAngularBase.Model.Entities.Empresa> source, int? value)
		{
			if (value != null)
				return source.Where(p => p.IdEmpresa == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.Empresa> WithRazonSocial(this IQueryable<IngematicaAngularBase.Model.Entities.Empresa> source, string value)
		{
			if (value != null)
				return source.Where(p => p.RazonSocial == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.Empresa> WithContainsRazonSocial(this IQueryable<IngematicaAngularBase.Model.Entities.Empresa> source, string value)
		{
			if (value != null)
				return source.Where(p => p.RazonSocial.Contains(value));
			
			return source;
		}


		public static IQueryable<IngematicaAngularBase.Model.Entities.Empresa> WithCUIT(this IQueryable<IngematicaAngularBase.Model.Entities.Empresa> source, string value)
		{
			if (value != null)
				return source.Where(p => p.CUIT == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.Empresa> WithContainsCUIT(this IQueryable<IngematicaAngularBase.Model.Entities.Empresa> source, string value)
		{
			if (value != null)
				return source.Where(p => p.CUIT.Contains(value));
			
			return source;
		}


		public static IQueryable<IngematicaAngularBase.Model.Entities.Empresa> WithTelefono(this IQueryable<IngematicaAngularBase.Model.Entities.Empresa> source, string value)
		{
			if (value != null)
				return source.Where(p => p.Telefono == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.Empresa> WithContainsTelefono(this IQueryable<IngematicaAngularBase.Model.Entities.Empresa> source, string value)
		{
			if (value != null)
				return source.Where(p => p.Telefono.Contains(value));
			
			return source;
		}


		public static IQueryable<IngematicaAngularBase.Model.Entities.Empresa> WithDireccion(this IQueryable<IngematicaAngularBase.Model.Entities.Empresa> source, string value)
		{
			if (value != null)
				return source.Where(p => p.Direccion == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.Empresa> WithContainsDireccion(this IQueryable<IngematicaAngularBase.Model.Entities.Empresa> source, string value)
		{
			if (value != null)
				return source.Where(p => p.Direccion.Contains(value));
			
			return source;
		}


		public static IQueryable<IngematicaAngularBase.Model.Entities.Empresa> WithIdProvincia(this IQueryable<IngematicaAngularBase.Model.Entities.Empresa> source, int? value)
		{
			if (value != null)
				return source.Where(p => p.IdProvincia == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.Empresa> WithIdLocalidad(this IQueryable<IngematicaAngularBase.Model.Entities.Empresa> source, int? value)
		{
			if (value != null)
				return source.Where(p => p.IdLocalidad == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.Empresa> WithCodigoPostal(this IQueryable<IngematicaAngularBase.Model.Entities.Empresa> source, string value)
		{
			if (value != null)
				return source.Where(p => p.CodigoPostal == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.Empresa> WithContainsCodigoPostal(this IQueryable<IngematicaAngularBase.Model.Entities.Empresa> source, string value)
		{
			if (value != null)
				return source.Where(p => p.CodigoPostal.Contains(value));
			
			return source;
		}


		public static IQueryable<IngematicaAngularBase.Model.Entities.Empresa> WithIngresosBrutos(this IQueryable<IngematicaAngularBase.Model.Entities.Empresa> source, string value)
		{
			if (value != null)
				return source.Where(p => p.IngresosBrutos == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.Empresa> WithContainsIngresosBrutos(this IQueryable<IngematicaAngularBase.Model.Entities.Empresa> source, string value)
		{
			if (value != null)
				return source.Where(p => p.IngresosBrutos.Contains(value));
			
			return source;
		}


		public static IQueryable<IngematicaAngularBase.Model.Entities.Empresa> WithInicioActividad(this IQueryable<IngematicaAngularBase.Model.Entities.Empresa> source, System.DateTime? value)
		{
			if (value != null)
				return source.Where(p => p.InicioActividad == value);
			
			return source;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.Empresa> WithInicioActividadBetweenWithTime(this IQueryable<IngematicaAngularBase.Model.Entities.Empresa> source, System.DateTime? lower, System.DateTime? upper)
		{
            var range = source;
           
            if (lower != null)
                range = range.Where(p => p.InicioActividad >= lower);

            if (upper != null)
                range = range.Where(p => p.InicioActividad <= upper);

            return range;
		}

		public static IQueryable<IngematicaAngularBase.Model.Entities.Empresa> WithInicioActividadBetween(this IQueryable<IngematicaAngularBase.Model.Entities.Empresa> source, System.DateTime? lower, System.DateTime? upper)
		{
            var range = source;
           
            if (lower != null)
                range = range.Where(p => p.InicioActividad >= lower);

            if (upper != null)
			{
				upper = upper.Value.Date.AddDays(1);
                range = range.Where(p => p.InicioActividad < upper);
			}

            return range;
		}

	}
}
