using HotelManager.HotelManager.Domain.Entities;

namespace HotelManager.Business.Interfaces
{
    public interface IHuespedResository : IBaseRepository<Huesped>
    {
        Huesped GetByDocumento(string documento);
        bool ExisteDocumento(string documento, int? excluirIdHuesped = null);
    }
}
