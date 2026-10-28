using System;

namespace HotelManager.Domain.Entities
{
    public class Notificacion
    {
        public int IdNotificacion { get; set; }
        public int IdReserva { get; set; }
        public string Mensaje { get; set; }
        public DateTime Fecha { get; set; }
        public bool Leida { get; set; }
    }
}
