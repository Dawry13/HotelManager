using System.ComponentModel.DataAnnotations;
using HotelReservas.Domain.Enums;

namespace HotelReservas.Domain.Entities;

public class Habitacion
{
    public int IdHabitacion { get; set; }

    [Required(ErrorMessage = "El número es obligatorio.")]
    [StringLength(10)]
    [Display(Name = "Número")]
    public string Numero { get; set; } = string.Empty;

    [Range(1, 20, ErrorMessage = "La capacidad debe estar entre 1 y 20.")]
    public int Capacidad { get; set; } = 1;

    [Range(0.01, 1_000_000, ErrorMessage = "La tarifa debe ser mayor que cero.")]
    [Display(Name = "Tarifa por noche")]
    [DataType(DataType.Currency)]
    public decimal Tarifa { get; set; }

    public EstadoHabitacion Estado { get; set; } = EstadoHabitacion.Disponible;

    [Display(Name = "Piso")]
    public int IdPiso { get; set; }
    public Piso? Piso { get; set; }

    [Display(Name = "Tipo de habitación")]
    public int IdTipoHabitacion { get; set; }
    public TipoHabitacion? TipoHabitacion { get; set; }

    /// <summary>RN-007: solo una habitación disponible puede reservarse.</summary>
    public bool EsReservable => Estado == EstadoHabitacion.Disponible;
}
