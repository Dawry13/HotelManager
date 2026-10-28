using System;
using System.Collections.Generic;
using HotelManager.Domain.Entities;
using HotelManager.Domain.Enums;

namespace HotelManager.Data.Interfaces
{
    public interface IReservaRepository : IBaseRepository<Reserva>
    {
        bool HaySolapamiento(int idHabitacion, DateTime entrada, DateTime salida, int? excluirIdReserva = null);
        IEnumerable<Reserva> GetByHuesped(int idHuesped);
        IEnumerable<Reserva> GetByHabitacion(int idHabitacion);
        bool CambiarEstado(int idReserva, EstadoReserva nuevoEstado);
    }
}