using System.Collections.Generic;
using HotelManager.Domain.Entities;

namespace HotelManager.Data.Interfaces
{
    public interface IPisoRepository
    {
        IEnumerable<Piso> GetByHotel(int idHotel);
        bool ExisteNumeroActivo(int idHotel, int numero, int? excluirIdPiso = null);
    }
}