using System;
using HotelManager.Domain.Enums;

namespace HotelManager.Domain.Entities
{
    /// <summary>
    /// Aviso asociado a una reserva (tabla notificaciones). RN-014, RN-015.
    /// </summary>
    public class Notificacion
    {
        public int IdNotificacion { get; set; }
        public int IdReserva { get; set; }
        public TipoNotificacion Tipo { get; set; }
        public DateTime Fecha { get; set; }
        public EstadoNotificacion Estado { get; set; }
        public string DetalleError { get; set; }
    }
}
