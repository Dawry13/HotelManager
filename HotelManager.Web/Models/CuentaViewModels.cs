using System.ComponentModel.DataAnnotations;
using HotelManager.Domain.Enums;

namespace HotelManager.Web.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Ingrese su usuario.")]
        [Display(Name = "Usuario")]
        public string Usuario { get; set; }

        [Required(ErrorMessage = "Ingrese su contraseña.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Clave { get; set; }

        public string ReturnUrl { get; set; }
    }

    public class CambiarClaveViewModel
    {
        [Required(ErrorMessage = "Ingrese su contraseña actual.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
        public string ClaveActual { get; set; }

        [Required(ErrorMessage = "Ingrese la nueva contraseña.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mínimo 8 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string ClaveNueva { get; set; }

        [Required(ErrorMessage = "Confirme la nueva contraseña.")]
        [Compare("ClaveNueva", ErrorMessage = "Las contraseñas no coinciden.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar nueva contraseña")]
        public string ConfirmarClave { get; set; }

        public bool Obligatorio { get; set; }
    }

    /// <summary>ACT-003: el huesped crea su propia cuenta.</summary>
    public class RegistroHuespedViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150)]
        [Display(Name = "Nombre completo")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El documento es obligatorio.")]
        [StringLength(30)]
        [Display(Name = "Documento (cédula o pasaporte)")]
        public string Documento { get; set; }

        [StringLength(20)]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(150)]
        [Display(Name = "Correo electrónico")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "Elija un nombre de usuario.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Entre 3 y 50 caracteres.")]
        [Display(Name = "Usuario")]
        public string Usuario { get; set; }

        [Required(ErrorMessage = "Elija una contraseña.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mínimo 8 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Clave { get; set; }

        [Required(ErrorMessage = "Confirme la contraseña.")]
        [Compare("Clave", ErrorMessage = "Las contraseñas no coinciden.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        public string ConfirmarClave { get; set; }
    }

    /// <summary>Administracion de cuentas (solo Administrador).</summary>
    public class UsuarioViewModel
    {
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El usuario es obligatorio.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Entre 3 y 50 caracteres.")]
        [Display(Name = "Usuario")]
        public string NombreUsuario { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(150)]
        [Display(Name = "Correo")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "Seleccione el rol.")]
        [Display(Name = "Rol")]
        public RolUsuario? Rol { get; set; }

        [Display(Name = "Huésped vinculado")]
        public int? IdHuesped { get; set; }

        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mínimo 8 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña temporal")]
        public string ClaveTemporal { get; set; }

        public bool Activo { get; set; }
    }
}
