using System.ComponentModel.DataAnnotations;
using HotelManager.Domain.Enums;

namespace HotelManager.Web.Models
{
    public class HabitacionViewModel
    {
        public int IdHabitacion { get; set; }

        [Required(ErrorMessage = "Seleccione un hotel.")]
        [Display(Name = "Hotel")]
        public int? IdHotel { get; set; }

        [Required(ErrorMessage = "Seleccione un piso.")]
        [Display(Name = "Piso")]
        public int? IdPiso { get; set; }

        [Required(ErrorMessage = "Seleccione un tipo de habitación.")]
        [Display(Name = "Tipo de habitación")]
        public int? IdTipoHabitacion { get; set; }

        [Required(ErrorMessage = "El número es obligatorio.")]
        [StringLength(10, ErrorMessage = "Máximo 10 caracteres.")]
        [Display(Name = "Número")]
        public string Numero { get; set; }

        [Required(ErrorMessage = "La capacidad es obligatoria.")]
        [Range(1, 20, ErrorMessage = "La capacidad debe estar entre 1 y 20.")]
        public int? Capacidad { get; set; }

        [Required(ErrorMessage = "La tarifa es obligatoria.")]
        [Range(typeof(decimal), "0.01", "1000000", ErrorMessage = "La tarifa debe ser mayor que cero.")]
        [Display(Name = "Tarifa por noche")]
        public decimal? Tarifa { get; set; }

        public EstadoHabitacion Estado { get; set; }

        public string NombreHotel { get; set; }
        public string DescripcionPiso { get; set; }
        public string NombreTipo { get; set; }
    }
}
