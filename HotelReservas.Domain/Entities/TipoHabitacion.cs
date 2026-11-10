using System.ComponentModel.DataAnnotations;
using HotelReservas.Domain.Enums;

namespace HotelReservas.Domain.Entities;

/// <summary>Catálogo de tipos de habitación (Sencilla, Doble, Suite...).</summary>
public class TipoHabitacion
{
    public int IdTipoHabitacion { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(250)]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    [Range(1, 20, ErrorMessage = "La capacidad máxima debe estar entre 1 y 20.")]
    [Display(Name = "Capacidad máxima")]
    public int CapacidadMaxima { get; set; } = 1;

    [Range(0, 1_000_000, ErrorMessage = "La tarifa base no puede ser negativa.")]
    [Display(Name = "Tarifa base")]
    [DataType(DataType.Currency)]
    public decimal TarifaBase { get; set; }

    public EstadoRegistro Estado { get; set; } = EstadoRegistro.Activo;

    public ICollection<Habitacion> Habitaciones { get; set; } = new List<Habitacion>();
}
