using System;
using System.Collections.Generic;
using HotelManager.Business.Dtos;

namespace HotelManager.Business.Interfaces
{
    public interface ITarifaService
    {
        decimal ObtenerTarifaAplicable(int idHabitacion, DateTime fecha);

        decimal CalcularCostoTotal(int idHabitacion, DateTime entrada, DateTime salida);

        // Mantenimiento de tarifas por vigencia
        IEnumerable<TarifaDto> Listar();
        IEnumerable<TarifaDto> ListarPorHabitacion(int idHabitacion);
        TarifaDto ObtenerPorId(int idTarifa);
        TarifaDto Crear(TarifaDto dto);
        TarifaDto Actualizar(TarifaDto dto);
        void Eliminar(int idTarifa);
    }
}
