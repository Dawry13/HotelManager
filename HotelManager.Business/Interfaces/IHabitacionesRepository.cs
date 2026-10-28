using System;
using System.Collections.Generic;
using HotelManager.Business.Interfaces;
using HotelManager.Domain.Entities;
using HotelManager.Domain.Enums;

namespace HotelManager.Domain.Interfaces
{
    public interface IHabitacionesRepository : IBaseRepository<Habitacion>
    {
        IEnumerable<Habitacion> GetByPiso(int idPiso);
        IEnumerable<Habitacion> GetByHotel(int idHotel);
        bool ExisteNumeroEnHotel(int idHotel, int numero, int? excluirIdHabitacion = null);
        IEnumerable<Habitacion> GetDisponibles(DateTime entrada, DateTime salida, int huespedes, int? idHotel = null);
        bool CambiarEstado(int idHabitacion, EstadoHabitacion nuevoEstado);
    }
}