using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HotelManager.Web.Helpers;

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

        [Required(ErrorMessage = "Seleccione la categoría.")]
        [Range(3, 5, ErrorMessage = "La categoría debe ser de 3, 4 o 5 estrellas.")]
        [Display(Name = "Categoría")]
        public int Categoria { get; set; } = 3;

        [StringLength(500, ErrorMessage = "Máximo 500 caracteres.")]
        [Display(Name = "Descripción para los huéspedes")]
        public string Descripcion { get; set; }

        /// <summary>Amenidades marcadas en el formulario.</summary>
        public List<string> Caracteristicas { get; set; } = new List<string>();

        /// <summary>Amenidades adicionales escritas a mano, separadas por coma.</summary>
        [StringLength(250, ErrorMessage = "Máximo 250 caracteres.")]
        [Display(Name = "Otras características")]
        public string OtrasCaracteristicas { get; set; }

        public string Estado { get; set; }

        // Solo para la vista de listado / detalle
        public int TotalBloques { get; set; }
        public int TotalPisos { get; set; }
        public int TotalHabitaciones { get; set; }

        public List<string> TodasLasCaracteristicas()
        {
            return CatalogoHelper.Combinar(Caracteristicas, OtrasCaracteristicas);
        }
    }
}
