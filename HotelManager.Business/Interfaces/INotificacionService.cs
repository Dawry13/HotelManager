using System.Collections.Generic;
using HotelManager.Business.Dtos;
using HotelManager.Domain.Enums;

namespace HotelManager.Business.Interfaces
{
    public interface INotificacionService
    {
        /// <summary>RF-014: genera una notificacion pendiente para la reserva.</summary>
        int Notificar(int idReserva, TipoNotificacion tipo);

        /// <summary>RF-015: registra el resultado del envio (enviada o fallida).</summary>
        void RegistrarResultado(int idNotificacion, bool enviada, string detalleError = null);

        /// <summary>Vuelve a dejar una notificacion fallida como pendiente para reintentar el envio.</summary>
        void Reintentar(int idNotificacion);

        int ContarPendientes();

        IEnumerable<NotificacionDto> Listar();

        IEnumerable<NotificacionDto> ListarPorReserva(int idReserva);
    }
}
