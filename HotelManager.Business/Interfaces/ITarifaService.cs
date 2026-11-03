using System;

namespace HotelManager.Business.Interfaces
{
    public interface ITarifaService
    {
        decimal ObtenerTarifaAplicable(int idHabitacion, DateTime fecha);

       
        decimal CalcularCostoTotal(int idHabitacion, DateTime entrada, DateTime salida);
    }
}