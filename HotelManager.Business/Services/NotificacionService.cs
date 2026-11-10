using System;
using System.Linq;
using HotelManager.Business.Interfaces;
using HotelManager.Data.Interfaces;
using HotelManager.Domain.Entities;
using HotelManager.Domain.Enums;

namespace HotelManager.Business.Services
{
    public class NotificacionService : INotificacionService
    {
        private readonly INotificacionRepository _notificaciones;

        public NotificacionService(INotificacionRepository notificaciones)
        {
            _notificaciones = notificaciones;
        }

        public int Notificar(int idReserva, TipoNotificacion tipo)
        {
            var notificacion = new Notificacion
            {
                IdReserva = idReserva,
                Tipo = tipo,
                Fecha = DateTime.Now,
                Estado = EstadoNotificacion.Pendiente
            };
            return _notificaciones.Add(notificacion);
        }

        public int ContarPendientes()
        {
            return _notificaciones.GetPorEstado(EstadoNotificacion.Pendiente).Count();
        }

        // RN-015: un fallo de notificacion se registra pero no afecta la reserva.
        public void RegistrarResultado(int idNotificacion, bool enviada, string detalleError = null)
        {
            var detalle = enviada ? null : detalleError;
            if (detalle != null && detalle.Length > 500) detalle = detalle.Substring(0, 500);

            _notificaciones.RegistrarResultado(
                idNotificacion,
                enviada ? EstadoNotificacion.Enviada : EstadoNotificacion.Fallida,
                detalle);
        }
    }
}
