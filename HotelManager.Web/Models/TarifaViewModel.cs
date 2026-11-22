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

    /// <summary>Tarifas agrupadas por hotel: tarifa base de cada habitacion y temporadas.</summary>
    public class TarifasHotelViewModel
    {
        public int IdHotel { get; set; }
        public string Hotel { get; set; }
        public int Categoria { get; set; }
        public List<TarifaHabitacionViewModel> Habitaciones { get; set; } = new List<TarifaHabitacionViewModel>();
        public List<TarifaViewModel> Temporadas { get; set; } = new List<TarifaViewModel>();
    }

    public class TarifaHabitacionViewModel
    {
        public int IdHabitacion { get; set; }
        public string Numero { get; set; }
        public string Tipo { get; set; }
        public string Ubicacion { get; set; }
        /// <summary>Precio por noche asignado a la habitacion (0 = sin precio).</summary>
        public decimal TarifaBase { get; set; }
        /// <summary>Tarifa de temporada que aplica hoy, si hay una.</summary>
        public decimal? TarifaHoy { get; set; }
        public int TotalTemporadas { get; set; }
    }
}
