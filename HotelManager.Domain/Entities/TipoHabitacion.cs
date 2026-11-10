namespace HotelManager.Domain.Entities
{
    /// <summary>
    /// Catalogo de tipos de habitacion (Sencilla, Doble, Suite...).
    /// </summary>
    public class TipoHabitacion
    {
        public int IdTipoHabitacion { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int CapacidadMaxima { get; set; }
        public decimal TarifaBase { get; set; }
        public bool Estado { get; set; }
    }
}
