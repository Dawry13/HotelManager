namespace HotelManager.Business.Dtos
{
    public class HabitacionDto
    {
        public int IdHabitacion { get; set; }
        public string Numero { get; set; }
        public int Capacidad { get; set; }
        public decimal Tarifa { get; set; }
        public string Estado { get; set; }
        public int IdPiso { get; set; }
        public int? IdTipoHabitacion { get; set; }
    }
}
