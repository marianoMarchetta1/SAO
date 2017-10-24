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

        public Mueble Clone(Mueble entity)
        {
            Mueble entityClone = new Mueble();
            entityClone.Activo                 = entity.Activo;
            entityClone.Ancho                  = entity.Ancho;
            entityClone.Codigo                 = entity.Codigo;
            entityClone.DistanciaParedes       = entity.DistanciaParedes;
            entityClone.DistanciaProximoMueble = entity.DistanciaProximoMueble;
            entityClone.FechaAlta              = entity.FechaAlta;
            entityClone.FechaModificacion      = entity.FechaModificacion;
            entityClone.IdMueble               = entity.IdMueble;
            entityClone.IdUsuarioAlta          = entity.IdUsuarioAlta;
            entityClone.IdUsuarioModificacion  = entity.IdUsuarioModificacion;
            entityClone.Imagen                 = entity.Imagen;
            entityClone.Largo                  = entity.Largo;
            entityClone.Nombre                 = entity.Nombre;
            entityClone.OrdenDePrioridad       = entity.OrdenDePrioridad;
            entityClone.PoseeRadio             = entity.PoseeRadio;
            entityClone.Precio                 = entity.Precio;
            entityClone.RadioMayor             = entity.RadioMayor;
            entityClone.RadioMenor             = entity.RadioMenor;
            entityClone.Usuario                = entity.Usuario;
            entityClone.Usuario1               = entity.Usuario1;
            entityClone.ImagenMueble           = entity.ImagenMueble;
            return entityClone;

        }

        public netDxf.Entities.LwPolylineVertex ConvertVertex(Vector2 vertice)
        {
            netDxf.Vector2 posicion = new netDxf.Vector2(vertice.X, vertice.Y);
            return new netDxf.Entities.LwPolylineVertex(posicion);
        }

        public MueblesOptmizacion AjustarTamanio(ref MueblesOptmizacion muebleOpt)
        {
            if (muebleOpt.Mueble.Activo)
            {
                double dif = muebleOpt.Ancho - System.Convert.ToDouble(muebleOpt.Mueble.Ancho);

                if ( dif > 0)
                {
                    muebleOpt.Ancho = System.Convert.ToDouble(muebleOpt.Mueble.Ancho);
                    muebleOpt.VerticeDerechaArriba.X -= dif;
                    muebleOpt.VerticeDerechaAbajo.X  -= dif;
                }

                dif = muebleOpt.Largo - System.Convert.ToDouble(muebleOpt.Mueble.Largo);

                if (dif > 0)
                {
                    muebleOpt.Largo = System.Convert.ToDouble(muebleOpt.Mueble.Largo);
                    muebleOpt.VerticeIzquierdaAbajo.Y += dif;
                    muebleOpt.VerticeDerechaAbajo.Y   += dif;
                }
            }
            return muebleOpt;
        }

        public MueblesOptmizacion DesplazarArriba(ref MueblesOptmizacion muebleOpt, double nuevaCoordY)
        {
            double largo = muebleOpt.VerticeDerechaArriba.Y - muebleOpt.VerticeDerechaAbajo.Y;
            muebleOpt.VerticeDerechaArriba.Y   = nuevaCoordY;
            muebleOpt.VerticeIzquierdaArriba.Y = nuevaCoordY;
            muebleOpt.VerticeIzquierdaAbajo.Y  = nuevaCoordY - largo;
            muebleOpt.VerticeDerechaAbajo.Y    = nuevaCoordY - largo;
            return muebleOpt;
        }

        public MueblesOptmizacion DesplazarIzquierda(ref MueblesOptmizacion muebleOpt, double nuevaCoordX)
        {
            muebleOpt.VerticeIzquierdaAbajo.X = nuevaCoordX;
            muebleOpt.VerticeIzquierdaArriba.X = nuevaCoordX;
            muebleOpt.VerticeDerechaAbajo.X    = nuevaCoordX  + muebleOpt.Ancho;
            muebleOpt.VerticeDerechaArriba.X   = nuevaCoordX  + muebleOpt.Ancho;
            return muebleOpt;
        }

    }
}
