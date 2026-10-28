using System.Collections.Generic;

namespace HotelManager.Domain.Entities
{
    public class Hotel
    {
        public int IdHotel { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public bool Estado { get; set; }
        public List<Piso> Pisos { get; set; }
    }
}