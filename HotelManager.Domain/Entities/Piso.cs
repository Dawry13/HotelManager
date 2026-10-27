using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManager.HotelManager.Domain.Entities
{
    public class Piso
    {
        public int IdPiso { get; set; }
        public int Numero { get; set; }
        public bool Estado { get; set; }
        public int IdHotel { get; set; }
    }
}
