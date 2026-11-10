using System;
using System.Collections.Generic;
using System.Linq;
using HotelManager.Business.Dtos;
using HotelManager.Business.Exceptions;
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
            var notificacion = ObtenerEntidad(idNotificacion);
            if (notificacion.Estado == EstadoNotificacion.Enviada)
                throw new BusinessException("La notificación ya fue enviada.");

            var detalle = enviada ? null : (string.IsNullOrWhiteSpace(detalleError) ? "Error de envío no especificado." : detalleError.Trim());
            if (detalle != null && detalle.Length > 500) detalle = detalle.Substring(0, 500);

            _notificaciones.RegistrarResultado(
                idNotificacion,
                enviada ? EstadoNotificacion.Enviada : EstadoNotificacion.Fallida,
                detalle);
        }

        public void Reintentar(int idNotificacion)
        {
            var notificacion = ObtenerEntidad(idNotificacion);
            if (notificacion.Estado != EstadoNotificacion.Fallida)
                throw new BusinessException("Solo se pueden reintentar notificaciones fallidas.");

            _notificaciones.RegistrarResultado(idNotificacion, EstadoNotificacion.Pendiente, null);
        }

        public IEnumerable<NotificacionDto> Listar()
        {
            return _notificaciones.GetAll().Select(ToDto).ToList();
        }

        public IEnumerable<NotificacionDto> ListarPorReserva(int idReserva)
        {
            return _notificaciones.GetByReserva(idReserva).Select(ToDto).ToList();
        }

        private Notificacion ObtenerEntidad(int idNotificacion)
        {
            var n = _notificaciones.GetById(idNotificacion);
            if (n == null) throw new BusinessException("La notificación no existe.");
            return n;
        }

        private static NotificacionDto ToDto(Notificacion n)
        {
            return new NotificacionDto
            {
                IdNotificacion = n.IdNotificacion,
                IdReserva = n.IdReserva,
                Tipo = n.Tipo.ToString(),
                Fecha = n.Fecha,
                Estado = n.Estado.ToString(),
                DetalleError = n.DetalleError
            };
        }
    }
}
