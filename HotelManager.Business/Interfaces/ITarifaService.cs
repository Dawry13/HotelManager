using System;

namespace HotelManager.Business.Interfaces
{
    public interface ITarifaService
    {
        /// <summary>Tarifa por noche aplicable a la habitación en una fecha (RN-013).</summary>
        decimal ObtenerTarifaAplicable(int idHabitacion, DateTime fecha);

        /// <summary>Costo total = suma de la tarifa de cada noche entre entrada (incl.) y salida (excl.).</summary>
        decimal CalcularCostoTotal(int idHabitacion, DateTime entrada, DateTime salida);
    }
}