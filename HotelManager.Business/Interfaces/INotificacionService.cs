using System.Collections.Generic;
using HotelManager.Business.Dtos;
using HotelManager.Domain.Enums;

namespace HotelManager.Business.Interfaces
{
    public interface INotificacionService
    {
        /// <summary>RF-014: genera la notificacion de la reserva y envia el correo al huesped.</summary>
        int Notificar(int idReserva, TipoNotificacion tipo);

        /// <summary>Envia (o reenvia) el correo de una notificacion. Devuelve false si fallo (RF-015).</summary>
        bool Enviar(int idNotificacion);

        /// <summary>Envia todas las notificaciones pendientes. Devuelve cuantas se enviaron.</summary>
        int EnviarPendientes();

        /// <summary>RF-015: registra manualmente el resultado del envio (enviada o fallida).</summary>
        void RegistrarResultado(int idNotificacion, bool enviada, string detalleError = null);

        /// <summary>Envia o reenvia el correo de una notificacion (lanza BusinessException si falla).</summary>
        void Reintentar(int idNotificacion);

        int ContarPendientes();

        IEnumerable<NotificacionDto> Listar();

        IEnumerable<NotificacionDto> ListarPorReserva(int idReserva);

        /// <summary>Como esta configurado el envio (SMTP o carpeta de prueba).</summary>
        string DescripcionEnvio { get; }

        /// <summary>true si los correos se guardan como archivo (no se envian a internet).</summary>
        bool ModoPrueba { get; }
    }
}
