using System.Collections.Generic;

namespace HotelManager.Business.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T GetById(int id);
        int Add(T entity);      // devuelve el Id generado
        bool Update(T entity);
        bool Delete(int id);
    }
}
