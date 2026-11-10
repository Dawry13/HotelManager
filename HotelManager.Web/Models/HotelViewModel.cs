using System.ComponentModel.DataAnnotations;

namespace HotelManager.Web.Models
{
    public class HotelViewModel
    {
        public int IdHotel { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        [StringLength(250, ErrorMessage = "Máximo 250 caracteres.")]
        [Display(Name = "Dirección")]
        public string Direccion { get; set; }

        public string Estado { get; set; }

        // Solo para la vista de listado / detalle
        public int TotalBloques { get; set; }
        public int TotalPisos { get; set; }
        public int TotalHabitaciones { get; set; }
    }
}
