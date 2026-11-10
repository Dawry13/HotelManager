using System;
using HotelManager.Business.Dtos;

namespace HotelManager.Business.Interfaces
{
    public interface IReporteService
    {
        /// <summary>
        /// Ocupacion, ingresos y reservas entre dos fechas (inclusive).
        /// Las reservas se toman por su fecha de entrada; la ocupacion por las noches dentro del rango.
        /// </summary>
        ReporteGeneralDto Generar(DateTime desde, DateTime hasta, int? idHotel = null);
    }
}
