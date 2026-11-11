using System;
using System.Collections.Generic;
using System.Linq;
using HotelManager.Business.Correo;
using HotelManager.Business.Dtos;
using HotelManager.Business.Exceptions;
using HotelManager.Business.Interfaces;
using HotelManager.Data.Interfaces;
using HotelManager.Domain.Entities;
using HotelManager.Domain.Enums;

namespace HotelManager.Business.Services
{
    /// <summary>
    /// RF-014 / RF-015: genera la notificacion de la reserva, la envia por correo al huesped
    /// y registra el resultado. RN-015: un fallo de envio nunca afecta la reserva.
    /// </summary>
    public class NotificacionService : INotificacionService
    {
        private readonly INotificacionRepository _notificaciones;
        private readonly IReservaRepository _reservas;
        private readonly IHuespedRepository _huespedes;
        private readonly IHabitacionRepository _habitaciones;
        private readonly IPisoRepository _pisos;
        private readonly IBloqueRepository _bloques;
        private readonly IHotelRepository _hoteles;
        private readonly ITipoHabitacionRepository _tipos;
        private readonly IEnviadorCorreo _correo;

        public NotificacionService(INotificacionRepository notificaciones, IReservaRepository reservas,
                                   IHuespedRepository huespedes, IHabitacionRepository habitaciones,
                                   IPisoRepository pisos, IBloqueRepository bloques, IHotelRepository hoteles,
                                   ITipoHabitacionRepository tipos, IEnviadorCorreo correo)
        {
            _notificaciones = notificaciones;
            _reservas = reservas;
            _huespedes = huespedes;
            _habitaciones = habitaciones;
            _pisos = pisos;
            _bloques = bloques;
            _hoteles = hoteles;
            _tipos = tipos;
            _correo = correo;
        }

        public string DescripcionEnvio
        {
            get { return _correo.Configuracion.Descripcion; }
        }

        public bool ModoPrueba
        {
            get { return _correo.Configuracion.Habilitado && !_correo.Configuracion.UsarSmtp; }
        }

        /// <summary>Crea la notificacion (pendiente) y la envia por correo de inmediato.</summary>
        public int Notificar(int idReserva, TipoNotificacion tipo)
        {
            var notificacion = new Notificacion
            {
                IdReserva = idReserva,
                Tipo = tipo,
                Fecha = DateTime.Now,
                Estado = EstadoNotificacion.Pendiente
            };
            notificacion.IdNotificacion = _notificaciones.Add(notificacion);

            if (_correo.Configuracion.Habilitado)
                Enviar(notificacion.IdNotificacion);

            return notificacion.IdNotificacion;
        }

        /// <summary>
        /// Envia (o reenvia) el correo de una notificacion y registra si fue enviada o fallida.
        /// Nunca lanza excepcion por un fallo de envio: devuelve false y guarda el motivo.
        /// </summary>
        public bool Enviar(int idNotificacion)
        {
            var notificacion = ObtenerEntidad(idNotificacion);

            try
            {
                var mensaje = PlantillaCorreoReserva.Construir(ArmarDatos(notificacion));
                if (string.IsNullOrWhiteSpace(mensaje.Para))
                    throw new InvalidOperationException("El huésped no tiene un correo registrado.");

                _correo.Enviar(mensaje);
                _notificaciones.RegistrarResultado(idNotificacion, EstadoNotificacion.Enviada, null);
                return true;
            }
            catch (Exception ex)
            {
                var detalle = ex.InnerException != null ? ex.Message + " " + ex.InnerException.Message : ex.Message;
                if (detalle.Length > 500) detalle = detalle.Substring(0, 500);
                _notificaciones.RegistrarResultado(idNotificacion, EstadoNotificacion.Fallida, detalle);
                return false;
            }
        }

        public int EnviarPendientes()
        {
            var enviadas = 0;
            foreach (var n in _notificaciones.GetPorEstado(EstadoNotificacion.Pendiente).ToList())
                if (Enviar(n.IdNotificacion)) enviadas++;
            return enviadas;
        }

        public int ContarPendientes()
        {
            return _notificaciones.GetPorEstado(EstadoNotificacion.Pendiente).Count();
        }

        // Registro manual del resultado (por ejemplo, si el aviso se dio por otro medio).
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

        // Envia o reenvia el correo (tambien sirve para reenviar uno ya enviado, p. ej. si el huesped no lo encuentra).
        public void Reintentar(int idNotificacion)
        {
            if (!_correo.Configuracion.Habilitado)
                throw new BusinessException("El envío de correos está deshabilitado (Correo:Habilitado = false).");

            if (!Enviar(idNotificacion))
                throw new BusinessException("No se pudo enviar el correo: " + ObtenerEntidad(idNotificacion).DetalleError);
        }

        public IEnumerable<NotificacionDto> Listar()
        {
            return _notificaciones.GetAll().Select(ToDto).ToList();
        }

        public IEnumerable<NotificacionDto> ListarPorReserva(int idReserva)
        {
            return _notificaciones.GetByReserva(idReserva).Select(ToDto).ToList();
        }

        /// <summary>Reune los datos de la reserva, el huesped, la habitacion y el hotel para el correo.</summary>
        private DatosCorreoReserva ArmarDatos(Notificacion n)
        {
            var reserva = _reservas.GetById(n.IdReserva);
            if (reserva == null) throw new InvalidOperationException("La reserva #" + n.IdReserva + " no existe.");

            var huesped = _huespedes.GetById(reserva.IdHuesped);
            if (huesped == null) throw new InvalidOperationException("El huésped de la reserva no existe.");

            var habitacion = _habitaciones.GetById(reserva.IdHabitacion);
            var piso = habitacion != null ? _pisos.GetById(habitacion.IdPiso) : null;
            var hotel = piso != null ? _hoteles.GetById(piso.IdHotel) : null;
            var bloque = piso != null && piso.IdBloque.HasValue ? _bloques.GetById(piso.IdBloque.Value) : null;
            var tipo = habitacion != null && habitacion.IdTipoHabitacion.HasValue ? _tipos.GetById(habitacion.IdTipoHabitacion.Value) : null;

            string ubicacion = null;
            if (piso != null)
                ubicacion = (bloque != null ? System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(bloque.Nombre) + " - " : "") + "Piso " + piso.Numero;

            return new DatosCorreoReserva
            {
                Tipo = n.Tipo,
                IdReserva = reserva.IdReserva,
                Huesped = huesped.Nombre,
                Documento = huesped.Documento,
                Correo = huesped.Correo,
                Hotel = hotel != null ? hotel.Nombre : "",
                DireccionHotel = hotel != null ? hotel.Direccion : null,
                Habitacion = habitacion != null ? habitacion.Numero : "#" + reserva.IdHabitacion,
                TipoHabitacion = tipo != null ? tipo.Nombre : null,
                Ubicacion = ubicacion,
                Entrada = reserva.Entrada,
                Salida = reserva.Salida,
                Huespedes = reserva.Huespedes,
                CostoTotal = reserva.CostoTotal,
                Estado = reserva.Estado.ToString()
            };
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
