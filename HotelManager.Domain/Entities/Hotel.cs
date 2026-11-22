using System.Collections.Generic;

namespace HotelManager.Domain.Entities
{
    public class Hotel
    {
        public int IdHotel { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public bool Estado { get; set; }
        /// <summary>Categoria en estrellas: 3, 4 o 5.</summary>
        public int Categoria { get; set; } = 3;
        /// <summary>Amenidades separadas por ';' (Wi-Fi;Piscina;Spa).</summary>
        public string Caracteristicas { get; set; }
        public string Descripcion { get; set; }
        public List<Piso> Pisos { get; set; }
    }
}