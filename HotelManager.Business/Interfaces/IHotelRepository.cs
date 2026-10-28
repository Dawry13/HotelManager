using System.Collections.Generic;
using HotelManager.Domain.Entities;

namespace HotelManager.Business.Interfaces
{
    public interface IHotelRepository : IBaseRepository<Hotel>
    {
        IEnumerable<Hotel> GetActivos();
        IEnumerable<Piso> GetPisos(int idHotel);                      
        bool ExistePisoActivo(int idHotel, int numero, int? excluirIdPiso = null); 
    }
}
