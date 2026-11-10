namespace HotelManager.Business.Dtos
{
    public class BloqueDto
    {
        public int IdBloque { get; set; }
        public int IdHotel { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; }
    }
}
