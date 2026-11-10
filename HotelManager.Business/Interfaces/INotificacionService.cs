using HotelManager.Domain.Enums;

namespace HotelManager.Business.Interfaces
{
    public interface INotificacionService
    {
        /// <summary>RF-014: genera una notificacion pendiente para la reserva.</summary>
        int Notificar(int idReserva, TipoNotificacion tipo);

        /// <summary>RF-015: registra el resultado del envio (enviada o fallida).</summary>
        void RegistrarResultado(int idNotificacion, bool enviada, string detalleError = null);

        int ContarPendientes();
    }
}
