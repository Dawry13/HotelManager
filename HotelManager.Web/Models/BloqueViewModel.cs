using System.ComponentModel.DataAnnotations;

namespace HotelManager.Web.Models
{
    public class BloqueViewModel
    {
        public int IdBloque { get; set; }

        [Required(ErrorMessage = "Seleccione un hotel.")]
        [Display(Name = "Hotel")]
        public int? IdHotel { get; set; }

        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(10, ErrorMessage = "Máximo 10 caracteres.")]
        [Display(Name = "Código")]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        public string Nombre { get; set; }

        [StringLength(250, ErrorMessage = "Máximo 250 caracteres.")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        public string Estado { get; set; }

        public string NombreHotel { get; set; }
        public int TotalPisos { get; set; }
    }
}
