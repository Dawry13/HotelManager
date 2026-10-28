using HotelManager.Domain.Entities;

namespace HotelManager.Data.Interfaces
{
    public interface IHotelRepository : IBaseRepository<Hotel>
    {
        Hotel GetConPisos(int idHotel);
        bool ExisteNombre(string nombre, int? excluirIdHotel = null);
    }
}
