using System.ComponentModel.DataAnnotations;

namespace HotelManager.Web.Models
{
    public class PisoViewModel
    {
        public int IdPiso { get; set; }

        [Required(ErrorMessage = "Seleccione un hotel.")]
        [Display(Name = "Hotel")]
        public int? IdHotel { get; set; }

        [Display(Name = "Bloque")]
        public int? IdBloque { get; set; }

        [Required(ErrorMessage = "El número es obligatorio.")]
        [Range(0, 200, ErrorMessage = "El número debe estar entre 0 y 200.")]
        [Display(Name = "Número de piso")]
        public int? Numero { get; set; }

        public string Estado { get; set; }

        public string NombreHotel { get; set; }
        public string NombreBloque { get; set; }
        public int TotalHabitaciones { get; set; }
    }
}
