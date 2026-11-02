using System;

namespace HotelManager.Business.Dtos
{
    public class HistorialReservaDto
    {
        public int IdHistorial { get; set; }
        public int IdReserva { get; set; }
        public DateTime Fecha { get; set; }
        public string Accion { get; set; }
        public string EstadoAnterior { get; set; }
        public string EstadoNuevo { get; set; }
    }
}