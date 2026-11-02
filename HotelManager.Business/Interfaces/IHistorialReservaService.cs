using System;
using System.Collections.Generic;
using HotelManager.Business.Dtos;

namespace HotelManager.Business.Interfaces
{
    public interface IHistorialReservaService
    {
        IEnumerable<HistorialReservaDto> ConsultarPorReserva(int idReserva);
        IEnumerable<HistorialReservaDto> Consultar(DateTime? desde, DateTime? hasta);
    }
}