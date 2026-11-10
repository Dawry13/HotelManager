using System.ComponentModel.DataAnnotations;
using HotelReservas.Domain.Enums;

namespace HotelReservas.Domain.Entities;

public class Piso
{
    public int IdPiso { get; set; }

    [Range(-5, 200, ErrorMessage = "El número de piso debe estar entre -5 y 200.")]
    [Display(Name = "Número")]
    public int Numero { get; set; }

    [StringLength(100)]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    public EstadoRegistro Estado { get; set; } = EstadoRegistro.Activo;

    [Display(Name = "Hotel")]
    public int IdHotel { get; set; }
    public Hotel? Hotel { get; set; }

    [Display(Name = "Bloque")]
    public int? IdBloque { get; set; }
    public Bloque? Bloque { get; set; }

    public ICollection<Habitacion> Habitaciones { get; set; } = new List<Habitacion>();
}
