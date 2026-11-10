using System.ComponentModel.DataAnnotations;
using HotelReservas.Domain.Enums;

namespace HotelReservas.Domain.Entities;

public class Hotel
{
    public int IdHotel { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [StringLength(200)]
    [Display(Name = "Dirección")]
    public string Direccion { get; set; } = string.Empty;

    [StringLength(20)]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [StringLength(100)]
    [EmailAddress(ErrorMessage = "Correo no válido.")]
    public string? Correo { get; set; }

    public EstadoRegistro Estado { get; set; } = EstadoRegistro.Activo;

    [Display(Name = "Fecha de registro")]
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public ICollection<Bloque> Bloques { get; set; } = new List<Bloque>();
    public ICollection<Piso> Pisos { get; set; } = new List<Piso>();
}
