// HotelManager.Data/Interfaces/IReservaRepository.cs
using System;
using System.Collections.Generic;
using HotelManager.Domain.Entities;

namespace HotelManager.Data.Interfaces
{
    public interface IReservaRepository : IBaseRepository<Reserva>
    {
        bool ExisteSolapamiento(int idHabitacion, DateTime entrada, DateTime salida, int? excluirIdReserva = null);
        IEnumerable<Reserva> GetByHuesped(int idHuesped);
    }
}