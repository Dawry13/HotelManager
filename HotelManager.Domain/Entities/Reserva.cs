using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelManager.HotelManager.Domain.Enums;


namespace HotelManager.HotelManager.Domain.Entities
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
