using System;

namespace HotelManager.Business.Dtos
{
    public class TarifaDto
    {
        public int IdTarifa { get; set; }
        public int IdHabitacion { get; set; }
        public decimal ValorNoche { get; set; }
        public DateTime VigenciaDesde { get; set; }
        public DateTime? VigenciaHasta { get; set; }

        public bool EstaVigente(DateTime fecha)
        {
            return fecha.Date >= VigenciaDesde.Date && (!VigenciaHasta.HasValue || fecha.Date <= VigenciaHasta.Value.Date);
        }
    }
}
