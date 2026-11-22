using System.Collections.Generic;
using HotelManager.Business.Dtos;
using HotelManager.Domain.Enums;

namespace HotelManager.Business.Interfaces
{
    public interface IHabitacionService
    {
        HabitacionDto Crear(HabitacionDto dto);
        HabitacionDto Actualizar(HabitacionDto dto);
        void CambiarEstado(int idHabitacion, EstadoHabitacion nuevoEstado);

        /// <summary>Cambia solo la tarifa base (precio por noche) de una habitacion.</summary>
        void ActualizarTarifa(int idHabitacion, decimal tarifa);

        /// <summary>Aplica la misma tarifa base a todas las habitaciones de un hotel. Devuelve cuantas cambiaron.</summary>
        int ActualizarTarifaHotel(int idHotel, decimal tarifa);
        HabitacionDto ObtenerPorId(int idHabitacion);
        IEnumerable<HabitacionDto> Listar();
        IEnumerable<HabitacionDto> ListarPorHotel(int idHotel);
    }
}
