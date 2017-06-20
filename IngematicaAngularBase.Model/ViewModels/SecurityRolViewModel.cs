using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IngematicaAngularBase.Model.ViewModels
{

    public class SecurityUsuarioViewModel
    {
        public int IdUsuario { get; set; }
        public int IdRol { get; set; }
        public string ApellidoNombre { get; set; }
        public List<string> Reglas { get; set; }
    }

    public class SecurityReglaViewModel
    {
        
        public int Id { get; set; }
        public string Nombre { get; set; }

    }
}