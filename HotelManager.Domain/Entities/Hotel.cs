using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManager.HotelManager.Domain.Entities
{
    public class Hotel
    {
        public int IdHotel { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public bool Estado { get; set; }
    }
}
