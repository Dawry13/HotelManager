using System.Collections.Generic;
using HotelManager.Domain.Entities;

namespace HotelManager.Data.Interfaces
{
    public interface IHuespedRepository : IBaseRepository<Huesped>
    {
        IEnumerable<Huesped> Buscar(string texto);
    }
}