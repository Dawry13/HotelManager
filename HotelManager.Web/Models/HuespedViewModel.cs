using System.ComponentModel.DataAnnotations;

namespace HotelManager.Web.Models
{
    public class HuespedViewModel
    {
        public int IdHuesped { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
        [Display(Name = "Nombre completo")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El documento es obligatorio.")]
        [StringLength(30, ErrorMessage = "Máximo 30 caracteres.")]
        [Display(Name = "Documento (cédula o pasaporte)")]
        public string Documento { get; set; }

        [StringLength(20, ErrorMessage = "Máximo 20 caracteres.")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio para enviar notificaciones.")]
        [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [Display(Name = "Correo electrónico")]
        public string Correo { get; set; }

        // Para listados
        public int TotalReservas { get; set; }
        public int ReservasActivas { get; set; }
        public decimal TotalGastado { get; set; }

        /// <summary>Si viene de "Nueva reserva", a donde volver al guardar.</summary>
        public string ReturnUrl { get; set; }
    }
}
