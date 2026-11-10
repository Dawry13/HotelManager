using System.ComponentModel.DataAnnotations;

namespace HotelManager.Web.Models
{
    public class TipoHabitacionViewModel
    {
        public int IdTipoHabitacion { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
        public string Nombre { get; set; }

        [StringLength(250, ErrorMessage = "Máximo 250 caracteres.")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "La capacidad máxima es obligatoria.")]
        [Range(1, 20, ErrorMessage = "La capacidad debe estar entre 1 y 20.")]
        [Display(Name = "Capacidad máxima")]
        public int? CapacidadMaxima { get; set; }

        [Required(ErrorMessage = "La tarifa base es obligatoria.")]
        [Range(typeof(decimal), "0", "1000000", ErrorMessage = "La tarifa base no puede ser negativa.")]
        [Display(Name = "Tarifa base")]
        public decimal? TarifaBase { get; set; }

        public string Estado { get; set; }
        public int TotalHabitaciones { get; set; }
    }
}
