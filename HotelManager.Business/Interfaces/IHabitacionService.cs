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
        HabitacionDto ObtenerPorId(int idHabitacion);
        IEnumerable<HabitacionDto> Listar();
        IEnumerable<HabitacionDto> ListarPorHotel(int idHotel);
    }
}
