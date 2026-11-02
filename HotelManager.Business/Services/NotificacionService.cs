using System;
using HotelManager.Business.Interfaces;
using HotelManager.Data.Interfaces;
using HotelManager.Domain.Entities;
using HotelManager.Domain.Enums;

namespace HotelManager.Business.Services
{
    /// <summary>
    /// Registra la notificación asociada a una reserva (RF-014).
    /// El envío real al servicio externo se conectaría aquí.
    /// </summary>
    public class NotificacionService : INotificacionService
    {
        private readonly INotificacionRepository _notificaciones;

        public NotificacionService(INotificacionRepository notificaciones)
        {
            _notificaciones = notificaciones;
        }

        public void Notificar(int idReserva, TipoNotificacion tipo)
        {
            var notificacion = new Notificacion
            {
                IdReserva = idReserva,
                Mensaje = ConstruirMensaje(idReserva, tipo),
                Fecha = DateTime.Now,
                Leida = false
            };
            _notificaciones.Add(notificacion);
        }

        private static string ConstruirMensaje(int idReserva, TipoNotificacion tipo)
        {
            switch (tipo)
            {
                case TipoNotificacion.Confirmacion: return "Su reserva #" + idReserva + " fue confirmada.";
                case TipoNotificacion.Modificacion: return "Su reserva #" + idReserva + " fue modificada.";
                case TipoNotificacion.Cancelacion: return "Su reserva #" + idReserva + " fue cancelada.";
                default: return "Actualización de su reserva #" + idReserva + ".";
            }
        }
    }
}