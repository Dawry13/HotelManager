using System.Collections.Generic;
using HotelManager.Business.Dtos;

namespace HotelManager.Business.Interfaces
{
    public interface ITipoHabitacionService
    {
        TipoHabitacionDto Crear(TipoHabitacionDto dto);
        TipoHabitacionDto Actualizar(TipoHabitacionDto dto);
        void Activar(int idTipoHabitacion);
        void Desactivar(int idTipoHabitacion);
        void Eliminar(int idTipoHabitacion);
        TipoHabitacionDto ObtenerPorId(int idTipoHabitacion);
        IEnumerable<TipoHabitacionDto> Listar();
    }
}
