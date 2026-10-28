using HotelManager.Domain.Entities;

namespace HotelManager.Business.Interfaces
{
    public interface IHuespedRepository : IBaseRepository<Huesped>
    {
        Huesped GetByDocumento(string documento);
        bool ExisteDocumento(string documento, int? excluirIdHuesped = null);
    }
}
