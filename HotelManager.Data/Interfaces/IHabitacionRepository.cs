using System;
using System.Collections.Generic;
using HotelManager.Domain.Entities;

namespace HotelManager.Data.Interfaces
{
    public interface IHabitacionRepository : IBaseRepository<Habitacion>
    {
        IEnumerable<Habitacion> GetByPiso(int idPiso);
        IEnumerable<Habitacion> GetByHotel(int idHotel);

        /// <summary>RN-005: el numero de habitacion debe ser unico dentro del hotel.</summary>
        bool ExisteNumeroEnHotel(int idHotel, int numero, int? excluirIdHabitacion = null);

        /// <summary>CU-006: habitaciones reservables, con capacidad suficiente y sin reservas activas solapadas.</summary>
        IEnumerable<Habitacion> GetDisponibles(DateTime entrada, DateTime salida, int huespedes, int? idHotel = null);
    }
}
