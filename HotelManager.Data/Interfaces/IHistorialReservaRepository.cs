using System.Collections.Generic;
using HotelManager.Domain.Entities;

namespace HotelManager.Data.Interfaces
{
    public interface IHistorialReservaRepository : IBaseRepository<HistorialReserva>
    {
        IEnumerable<HistorialReserva> GetByReserva(int idReserva);
    }
}
