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

        /// <summary>Todas las reservas, de la mas reciente a la mas antigua.</summary>
        IEnumerable<ReservaDto> Listar();

        void Confirmar(int idReserva);

        /// <summary>Check-out: marca la reserva como finalizada.</summary>
        void Finalizar(int idReserva);

        /// <summary>Reservas activas (pendiente, confirmada o modificada) que aun no terminan.</summary>
        IEnumerable<ReservaDto> ListarActivas();
    }
}