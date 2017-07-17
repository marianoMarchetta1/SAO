using IngematicaAngularBase.Dal;
using IngematicaAngularBase.Model.Common;
using IngematicaAngularBase.Model.Entities;
using IngematicaAngularBase.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngematicaAngularBase.Bll
{
    public class MuebleBusiness
    {
        public QueryResult<MuebleListViewModel> GetList(MuebleQuery query)
        {
            using (var context = new Entities())
            {
                MuebleDataAccess muebleDataAccess = new MuebleDataAccess(context);
                return muebleDataAccess.GetList(query);
            }
        }

        public void Delete(int id)
        {
            using (var context = new Entities())
            {
                Mueble entity = context.Set<Mueble>().FirstOrDefault(x => x.IdMueble == id);
                context.Entry(entity).State = EntityState.Deleted;
                context.SaveChanges();
            }
        }

        public MuebleViewModel GetById(int id)
        {
            using (var context = new Entities())
            {
                MuebleDataAccess muebleDataAccess = new MuebleDataAccess(context);
                return muebleDataAccess.GetById(id);
            }
        }

        public void Add(Mueble entity)
        {
            using (var context = new Entities())
            {
                var item = context.Set<Mueble>().AsNoTracking().FirstOrDefault(x => x.Nombre == entity.Nombre);
                if (item != null && item.Nombre.ToLower() == entity.Nombre.ToLower())
                    throw new CustomApplicationException("Ya existe un mueble con el mismo nombre.");

                entity.FechaAlta = DateTime.Now;
                context.Mueble.Add(entity);
                context.SaveChanges();
            }
        }

        public void Update(Mueble entity)
        {
            using (var context = new Entities())
            {
                var item = context.Set<Mueble>().AsNoTracking().FirstOrDefault(x => x.Nombre == entity.Nombre);
                if (item != null && item.IdMueble != entity.IdMueble)
                    throw new CustomApplicationException("Ya existe un mueble con el mismo nombre.");

                if (!entity.PoseeRadio)
                {
                    entity.RadioMayor = 0;
                    entity.RadioMenor = 0;
                }else
                {
                    entity.Largo = 0;
                    entity.Ancho = 0;
                }

                entity.FechaModificacion = DateTime.Now;
                context.Entry(entity).State = EntityState.Modified;
                context.SaveChanges();
            }
        }
    }
}
