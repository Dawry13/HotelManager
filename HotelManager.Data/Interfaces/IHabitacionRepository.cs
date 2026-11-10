using System;
using System.Collections.Generic;
using HotelManager.Domain.Entities;

namespace HotelManager.Data.Interfaces
{
    public interface IHabitacionRepository : IBaseRepository<Habitacion>
    {
        IEnumerable<Habitacion> GetByPiso(int idPiso);
        IEnumerable<Habitacion> GetByHotel(int idHotel);

        bool ExisteNumeroEnHotel(int idHotel, string numero, int? excluirIdHabitacion = null);

        IEnumerable<Habitacion> GetDisponibles(DateTime entrada, DateTime salida, int huespedes, int? idHotel = null);
    }
}
