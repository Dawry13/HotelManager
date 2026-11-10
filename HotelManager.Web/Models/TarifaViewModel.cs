using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HotelManager.Web.Models
{
    public class TarifaViewModel : IValidatableObject
    {
        public int IdTarifa { get; set; }

        [Required(ErrorMessage = "Seleccione la habitación.")]
        [Display(Name = "Habitación")]
        public int? IdHabitacion { get; set; }

        [Required(ErrorMessage = "Indique el valor por noche.")]
        [Range(typeof(decimal), "0.01", "1000000", ErrorMessage = "El valor debe ser mayor que cero.")]
        [Display(Name = "Valor por noche")]
        public decimal? ValorNoche { get; set; }

        [Required(ErrorMessage = "Indique el inicio de la vigencia.")]
        [DataType(DataType.Date)]
        [Display(Name = "Vigente desde")]
        public DateTime? VigenciaDesde { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Vigente hasta")]
        public DateTime? VigenciaHasta { get; set; }

        // Para listados
        public string Habitacion { get; set; }
        public string Hotel { get; set; }
        public decimal TarifaBase { get; set; }

        public string EstadoVigencia
        {
            get
            {
                var hoy = DateTime.Today;
                if (VigenciaDesde.HasValue && VigenciaDesde.Value.Date > hoy) return "Futura";
                if (VigenciaHasta.HasValue && VigenciaHasta.Value.Date < hoy) return "Vencida";
                return "Vigente";
            }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (VigenciaDesde.HasValue && VigenciaHasta.HasValue && VigenciaHasta.Value.Date < VigenciaDesde.Value.Date)
                yield return new ValidationResult("La vigencia final no puede ser anterior a la inicial.", new[] { "VigenciaHasta" });
        }
    }
}
