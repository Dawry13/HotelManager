using System;
using System.Collections.Generic;
using HotelManager.HotelManager.Domain.Entities;
using HotelManager.HotelManager.Domain.Enums;

namespace HotelManager.Business.Interfaces
{
    public interface IReservaResository : IBaseRepository<Reserva>
    {
        bool HaySolapamiento(int idHabitacion, DateTime entrada, DateTime salida, int? excluirIdReserva = null); 
        IEnumerable<Reserva> GetByHuesped(int idHuesped);
        IEnumerable<Reserva> GetByHabitacion(int idHabitacion);
        bool CambiarEstado(int idReserva, EstadoReserva nuevoEstado);                                           
    }
}
