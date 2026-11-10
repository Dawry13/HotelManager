using System.Collections.Generic;
using HotelManager.Domain.Entities;

namespace HotelManager.Data.Interfaces
{
    public interface IPisoRepository : IBaseRepository<Piso>
    {
        IEnumerable<Piso> GetByHotel(int idHotel);
        IEnumerable<Piso> GetByBloque(int idBloque);

        /// <summary>RN-002: numero de piso activo unico dentro del hotel.</summary>
        bool ExisteNumeroActivo(int idHotel, int numero, int? excluirIdPiso = null);

        int ContarHabitaciones(int idPiso);
    }
}
