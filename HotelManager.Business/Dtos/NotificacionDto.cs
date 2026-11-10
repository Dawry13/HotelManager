using System;

namespace HotelManager.Business.Dtos
{
    public class NotificacionDto
    {
        public int IdNotificacion { get; set; }
        public int IdReserva { get; set; }
        public string Tipo { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; }
        public string DetalleError { get; set; }
    }
}
