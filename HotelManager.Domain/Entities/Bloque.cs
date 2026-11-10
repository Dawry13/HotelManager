namespace HotelManager.Domain.Entities
{
    /// <summary>
    /// Bloque, torre o edificio dentro de un hotel. Agrupa pisos.
    /// </summary>
    public class Bloque
    {
        public int IdBloque { get; set; }
        public int IdHotel { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool Estado { get; set; }
    }
}
