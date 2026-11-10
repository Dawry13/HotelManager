using System.ComponentModel.DataAnnotations;
using HotelReservas.Domain.Enums;

namespace HotelReservas.Domain.Entities;

/// <summary>Bloque, torre o edificio dentro de un hotel. Agrupa pisos.</summary>
public class Bloque
{
    public int IdBloque { get; set; }

    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(10)]
    [Display(Name = "Código")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(250)]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    public EstadoRegistro Estado { get; set; } = EstadoRegistro.Activo;

    [Display(Name = "Hotel")]
    public int IdHotel { get; set; }
    public Hotel? Hotel { get; set; }

    public ICollection<Piso> Pisos { get; set; } = new List<Piso>();
}
