using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HotelManager.Domain.Enums;
using HotelManager.Web.Helpers;

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

        /// <summary>Caracteristicas marcadas (aire acondicionado, TV, balcon...).</summary>
        public List<string> Caracteristicas { get; set; } = new List<string>();

        [StringLength(250, ErrorMessage = "Máximo 250 caracteres.")]
        [Display(Name = "Otras características")]
        public string OtrasCaracteristicas { get; set; }

        public List<string> TodasLasCaracteristicas()
        {
            return CatalogoHelper.Combinar(Caracteristicas, OtrasCaracteristicas);
        }

        public string NombreHotel { get; set; }
        public string DescripcionPiso { get; set; }
        public string NombreTipo { get; set; }
    }
}
