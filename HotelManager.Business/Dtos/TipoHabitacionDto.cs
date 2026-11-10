namespace HotelManager.Business.Dtos
{
    public class TipoHabitacionDto
    {
        public int IdTipoHabitacion { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int CapacidadMaxima { get; set; }
        public decimal TarifaBase { get; set; }
        public string Estado { get; set; }
    }
}
