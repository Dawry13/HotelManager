using System;
using System.Collections.Generic;
using HotelManager.HotelManager.Domain.Entities;
using HotelManager.HotelManager.Domain.Enums;

namespace HotelManager.Business.Interfaces
{
    public interface IHabitacionesResository : IBaseRepository<Habitacion>
    {
        IEnumerable<Habitacion> GetByPiso(int idPiso);
        IEnumerable<Habitacion> GetByHotel(int idHotel);
        bool ExisteNumeroEnHotel(int idHotel, int numero, int? excluirIdHabitacion = null);
        IEnumerable<Habitacion> GetDisponibles(DateTime entrada, DateTime salida, int huespedes, int? idHotel = null); 
        bool CambiarEstado(int idHabitacion, EstadoHabitacion nuevoEstado);                  
    }
}
