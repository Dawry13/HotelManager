using System;
using System.Collections.Generic;
using HotelManager.Domain.Entities;

namespace HotelManager.Data.Interfaces
{
    public interface ITarifaRepository : IBaseRepository<Tarifa>
    {
        /// <summary>Tarifa aplicable a una habitacion en una fecha (RN-013).</summary>
        Tarifa GetVigente(int idHabitacion, DateTime fecha);

        IEnumerable<Tarifa> GetByHabitacion(int idHabitacion);
    }
}
