using System;

namespace HotelManager.Domain.Entities
{
    public class Tarifa
    {
        public int IdTarifa { get; set; }
        public int IdHabitacion { get; set; }
        public decimal PrecioPorNoche { get; set; }   // valor_noche
        public DateTime FechaInicio { get; set; }     // vigencia_desde
        public DateTime? FechaFin { get; set; }       // vigencia_hasta (null = sin fin)

        public bool AplicaEn(DateTime fecha)
            => fecha.Date >= FechaInicio.Date
               && (FechaFin == null || fecha.Date <= FechaFin.Value.Date);
    }
}