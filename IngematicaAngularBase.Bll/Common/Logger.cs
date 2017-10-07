using IngematicaAngularBase.Dal;
using IngematicaAngularBase.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngematicaAngularBase.Bll.Common
{
    public static class Logger
    {
        public static void Log(int idUsuario, string mensaje)
        {
            using (var context = new Entities())
            {
                Usuario user = context.Set<Usuario>().FirstOrDefault(x => x.IdUsuario == idUsuario);

                UserLog entity = new UserLog();
                entity.Descripcion = mensaje;
                entity.Usuario = user.Nombre;
                entity.Fecha = DateTime.Now;

                context.UserLog.Add(entity);
                context.SaveChanges();
            }
        }
    }
}
