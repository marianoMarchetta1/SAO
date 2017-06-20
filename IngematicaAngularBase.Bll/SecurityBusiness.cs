using System.Collections.Generic;
using System.Linq;
using IngematicaAngularBase.Model.ViewModels;
using IngematicaAngularBase.Dal;
using IngematicaAngularBase.Model.Entities;

namespace IngematicaAngularBase.Bll
{
    public class SecurityBusiness
    {

        public List<string> GetyRolReglas(int idRol)
        {
            if (idRol == 1) // Cmabiar por web config
            {
                var list = new List<string>();
                list.Add("*");
                return list;
            }
            else
            {
                using (var context = new Entities())
                {

                    IQueryable<Rol> tRol = context.Set<Rol>().AsNoTracking();
                    IQueryable<RolRegla> tRolRegla = context.Set<RolRegla>().AsNoTracking();
                    IQueryable<Regla> tRegla = context.Set<Regla>().AsNoTracking();

                    var result = from rolRegla in tRolRegla
                                 join regla in tRegla on rolRegla.IdRegla equals regla.IdRegla
                                 where rolRegla.IdRol == idRol
                                 select regla.Nombre;


                    return result.ToList();

                }          
            
            }


        }


        public SecurityUsuarioViewModel GetSecurityUser(string nombreUsuario)
        {

            using (var context = new Entities())
            {

                IQueryable<Usuario> tUsuario = context.Set<Usuario>().AsNoTracking();
                IQueryable<Rol> tRol = context.Set<Rol>().AsNoTracking();
                IQueryable<RolRegla> tRolRegla = context.Set<RolRegla>().AsNoTracking();
                IQueryable<Regla> tRegla = context.Set<Regla>().AsNoTracking();

                var model = new SecurityUsuarioViewModel();


                var result = from usuario in tUsuario
                              where usuario.NombreUsuario == nombreUsuario
                              select usuario;
                              

                Usuario usr = result.SingleOrDefault();

                model.IdUsuario = usr.IdUsuario;
                model.IdRol = (int)usr.IdRol;
                model.ApellidoNombre = usr.Apellido + ", " + usr.Nombre;
                return model;

            }
           
        }
       
    }
}
