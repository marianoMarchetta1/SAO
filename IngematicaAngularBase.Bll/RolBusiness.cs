using IngematicaAngularBase.Dal;
using IngematicaAngularBase.Model;
using IngematicaAngularBase.Model.Common;
using IngematicaAngularBase.Model.Entities;
using IngematicaAngularBase.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace IngematicaAngularBase.Bll
{
    public class RolBusiness
    {

        public QueryResult<RolListViewModel> GetList(RolQuery query)
        {

            using (var context = new Entities())
            {

                RolDataAccess rolDataAccess = new RolDataAccess(context);
                return rolDataAccess.GetList(query);

            }

        }


        public RolViewModel GetById(int id)
        {

            using (var context = new Entities())
            {

                RolDataAccess rolDataAccess = new RolDataAccess(context);
                return rolDataAccess.GetById(id);

            }
           
        }


        public void Add(Rol entity)
        {
            using (var context = new Entities())
            {
                var item = context.Set<Rol>().AsNoTracking().FirstOrDefault(x => x.Nombre == entity.Nombre);

                if (item != null && item.Nombre.ToLower() == entity.Nombre.ToLower())
                    throw new CustomApplicationException("Ya existe un rol con el mismo nombre.");

                entity.FechaAlta = DateTime.Now;
                //entity.IdTenant = 1; //LoginConfig.IdTenant;

                context.Rol.Add(entity);
                context.SaveChanges();
            }

        }


        public void Update(Rol entity)
        {
            using (var context = new Entities())
            {
                var item = context.Set<Rol>().AsNoTracking().FirstOrDefault(x => x.Nombre == entity.Nombre);
                if (item != null && item.IdRol != entity.IdRol && item.Nombre.ToLower() == entity.Nombre.ToLower())
                    throw new CustomApplicationException("Ya existe un rol con el mismo nombre.");

                Rol entityBD = context.Set<Rol>().AsNoTracking().FirstOrDefault(x => x.IdRol == entity.IdRol);
                if (entityBD != null && entityBD.Interno)
                {
                    throw new CustomApplicationException("No se pueden editar las propiedades del rol.");
                }

                int[] actuales = entity.RolRegla.Where(p => p.IdRolRegla > 0).Select(s => s.IdRolRegla).ToArray();

                var rolReglaList = from rolRegla in context.Set<RolRegla>()
                                   where rolRegla.IdRol == entity.IdRol && !actuales.Contains(rolRegla.IdRolRegla)
                                   select rolRegla;

                rolReglaList.ToList().ForEach(x => context.Entry(x).State = EntityState.Deleted);
                entity.RolRegla.Where(x => x.IdRolRegla == 0).ToList().ForEach(y => context.Entry(y).State = EntityState.Added);

                //EN OTROS CASOS VAMOS A UTILIZAR ESTA LAMBDA QUE CONTEMPLA TAMBIEN LAS MODIFICACIONES 
                //entity.RolRegla.ToList().ForEach(y => context.Entry(y).State = (y.IdRolRegla == 0 ? EntityState.Added : EntityState.Modified));


                rolReglaList.ToList().ForEach(x => context.Entry(x).State = EntityState.Deleted);
                entity.RolRegla.Where(x => x.IdRolRegla == 0).ToList().ForEach(y => context.Entry(y).State = EntityState.Added);

                entity.FechaModificacion = DateTime.Now;
                //entity.IdTenant = 1; //LoginConfig.IdTenant;

                context.Entry(entity).State = EntityState.Modified;
                context.SaveChanges();
            }
        }


        public void Delete(int id)
        {
            using (var context = new Entities())
            {
                Rol entity = context.Set<Rol>().FirstOrDefault(x => x.IdRol == id);
                if(entity != null && entity.Interno == true)
                    throw new CustomApplicationException("El rol no se puede eliminar.");

                var items = from rolRegla in context.Set<RolRegla>()
                            where rolRegla.IdRol == id
                            select rolRegla;

                foreach (var rolRegla in items)
                {
                    context.Entry(rolRegla).State = EntityState.Deleted;
                }

                context.Entry(entity).State = EntityState.Deleted; 
                context.SaveChanges();
            }
        }


        public RolViewModel CreateRolConReglas()
        {
            using (var context = new Entities())
            {
                RolDataAccess rolDataAccess = new RolDataAccess(context);
                return rolDataAccess.CreateRolConReglas();
            }
        }

    }
}
