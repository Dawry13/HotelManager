using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HotelManager.Web.Models
{
    /// <summary>Fila de los listados de reservas.</summary>
    public class ReservaItemViewModel
    {
        public int Id { get; set; }
        public int IdHuesped { get; set; }
        public string Huesped { get; set; }
        public int IdHabitacion { get; set; }
        public string Habitacion { get; set; }
        public string Ubicacion { get; set; }
        public string Tipo { get; set; }
        public int IdHotel { get; set; }
        public string Hotel { get; set; }
        public DateTime Entrada { get; set; }
        public DateTime Salida { get; set; }
        public int Noches { get { return (Salida.Date - Entrada.Date).Days; } }
        public int Huespedes { get; set; }
        public decimal Costo { get; set; }
        public string Estado { get; set; }

        public bool EsActiva { get { return Estado == "Pendiente" || Estado == "Confirmada" || Estado == "Modificada"; } }
    }

    /// <summary>Formulario para crear (CU-007) o modificar (CU-008) una reserva.</summary>
    public class ReservaFormViewModel : IValidatableObject
    {
        public int IdReserva { get; set; }

        [Required(ErrorMessage = "Seleccione el huésped.")]
        [Display(Name = "Huésped")]
        public int? IdHuesped { get; set; }

        [Required(ErrorMessage = "Seleccione la habitación.")]
        [Display(Name = "Habitación")]
        public int? IdHabitacion { get; set; }

        [Required(ErrorMessage = "Indique la fecha de entrada.")]
        [DataType(DataType.Date)]
        [Display(Name = "Entrada")]
        public DateTime? Entrada { get; set; }

        [Required(ErrorMessage = "Indique la fecha de salida.")]
        [DataType(DataType.Date)]
        [Display(Name = "Salida")]
        public DateTime? Salida { get; set; }

        [Required(ErrorMessage = "Indique la cantidad de huéspedes.")]
        [Range(1, 20, ErrorMessage = "La cantidad debe estar entre 1 y 20.")]
        [Display(Name = "Cantidad de huéspedes")]
        public int? CantidadHuespedes { get; set; }

        // Solo lectura (modificar)
        public string NombreHuesped { get; set; }
        public string Estado { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // RN-008: la salida debe ser posterior a la entrada
            if (Entrada.HasValue && Salida.HasValue && Salida.Value.Date <= Entrada.Value.Date)
                yield return new ValidationResult("La fecha de salida debe ser posterior a la de entrada.", new[] { "Salida" });
            // RN-009: no se permiten fechas anteriores a hoy
            if (Entrada.HasValue && Entrada.Value.Date < DateTime.Today)
                yield return new ValidationResult("No se permiten reservas con entrada anterior a hoy.", new[] { "Entrada" });
        }
    }

    /// <summary>CU-006 Consultar disponibilidad.</summary>
    public class DisponibilidadViewModel
    {
        public DateTime Entrada { get; set; }
        public DateTime Salida { get; set; }
        public int Huespedes { get; set; }
        public int? IdHotel { get; set; }
        public int? IdHuesped { get; set; }
        public bool Consultado { get; set; }
        public string Error { get; set; }
        public int Noches { get { return Math.Max(0, (Salida.Date - Entrada.Date).Days); } }
        public List<HabitacionDisponibleViewModel> Resultados { get; set; } = new List<HabitacionDisponibleViewModel>();
    }

    public class HabitacionDisponibleViewModel
    {
        public int IdHabitacion { get; set; }
        public string Numero { get; set; }
        public string Hotel { get; set; }
        public string Ubicacion { get; set; }
        public string Tipo { get; set; }
        public int Capacidad { get; set; }
        public decimal TarifaPromedio { get; set; }
        public decimal CostoTotal { get; set; }
    }

    /// <summary>Fila del historial global de reservas (CU-010).</summary>
    public class HistorialItemViewModel
    {
        public int IdReserva { get; set; }
        public DateTime Fecha { get; set; }
        public string Accion { get; set; }
        public string EstadoAnterior { get; set; }
        public string EstadoNuevo { get; set; }
        public string Huesped { get; set; }
        public string Habitacion { get; set; }
        public string Hotel { get; set; }
    }
}
