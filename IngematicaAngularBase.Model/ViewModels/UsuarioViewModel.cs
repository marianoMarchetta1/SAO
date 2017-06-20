using IngematicaAngularBase.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace IngematicaAngularBase.Model.ViewModels
{

    public class UsuarioViewModel
    {
        public int IdUsuario { get; set; }

        [Required]
        [StringLength(50)]
        public string NombreUsuario { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }

        [StringLength(50)]
        public string Apellido { get; set; }

        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public Nullable<int> IdRol { get; set; }

        public string RolNombre { get; set; }

        public bool Activo { get; set; }

        public int? IdUsuarioAlta { get; set; }
        public string UsuarioAlta { get; set; }

        public DateTime FechaAlta { get; set; }

        public int? IdUsuarioModificacion { get; set; }
        public string UsuarioModificacion { get; set; }

        public DateTime? FechaModificacion { get; set; }

        public string Password { get; set; }

        public string PasswordSalt { get; set; }

        public bool Interno { get; set; }
    }

    public class ChangePasswordViewModel
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string NewPassword { get; set; }

        public string NewPasswordRepeat { get; set; }

        public string Message { get; set; }
    }

}

