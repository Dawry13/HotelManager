using System.ComponentModel.DataAnnotations;

namespace HotelReservas.Domain.Enums;

/// <summary>Estado operativo de una habitación (RF-004, RN-007).</summary>
public enum EstadoHabitacion
{
    Disponible = 1,
    Ocupada = 2,
    [Display(Name = "En mantenimiento")]
    Mantenimiento = 3,
    [Display(Name = "Fuera de servicio")]
    FueraDeServicio = 4,
    Inactiva = 5
}
