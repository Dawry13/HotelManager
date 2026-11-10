using System.Collections.Generic;
using HotelManager.Domain.Entities;

namespace HotelManager.Data.Interfaces
{
    public interface IBloqueRepository : IBaseRepository<Bloque>
    {
        IEnumerable<Bloque> GetByHotel(int idHotel);
        bool ExisteCodigo(int idHotel, string codigo, int? excluirIdBloque = null);
        int ContarPisos(int idBloque);
    }
}
