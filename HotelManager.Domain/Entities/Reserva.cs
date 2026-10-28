using System;
using HotelManager.Domain.Enums;

namespace HotelManager.Domain.Entities
{
    public class Reserva
    {
        public int IdReserva { get; set; }
        public int IdHabitacion { get; set; }
        public int IdHuesped { get; set; }
        public DateTime Entrada { get; set; }
        public DateTime Salida { get; set; }
        public int Huespedes { get; set; }
        public decimal CostoTotal { get; set; }
        public EstadoReserva Estado { get; set; }
    }
}