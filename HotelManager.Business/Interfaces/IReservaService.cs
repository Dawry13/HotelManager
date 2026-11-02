using System.Collections.Generic;
using HotelManager.Business.Dtos;

namespace HotelManager.Business.Interfaces
{
    public interface IReservaService
    {
        ReservaDto Crear(CrearReservaDto dto);
        ReservaDto Modificar(ModificarReservaDto dto);
        void Cancelar(int idReserva);
        ReservaDto ObtenerPorId(int idReserva);
        IEnumerable<ReservaDto> ObtenerHistorialPorHuesped(int idHuesped);
    }
}