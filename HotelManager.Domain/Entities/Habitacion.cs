using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelManager.HotelManager.Domain.Enums;

namespace HotelManager.HotelManager.Domain.Entities
{
    public class Habitacion
    {
        public int IdHabitacion { get; set; }
        public int Numero { get; set; }
        public int Capacidad { get; set; }
        public decimal Tarifa { get; set; }
        public EstadoHabitacion Estado { get; set; }
        public int IdPiso { get; set; }
    }
}
