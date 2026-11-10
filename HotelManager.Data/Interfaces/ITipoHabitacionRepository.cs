using HotelManager.Domain.Entities;

namespace HotelManager.Data.Interfaces
{
    public interface ITipoHabitacionRepository : IBaseRepository<TipoHabitacion>
    {
        bool ExisteNombre(string nombre, int? excluirIdTipo = null);
        int ContarHabitaciones(int idTipoHabitacion);
    }
}
