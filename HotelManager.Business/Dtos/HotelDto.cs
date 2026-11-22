using System.Collections.Generic;

namespace HotelManager.Business.Dtos
{
    public class HotelDto
    {
        public int IdHotel { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string Estado { get; set; }
        /// <summary>3, 4 o 5 estrellas.</summary>
        public int Categoria { get; set; } = 3;
        public string Descripcion { get; set; }
        public List<string> Caracteristicas { get; set; } = new List<string>();
    }
}
