using System.Collections.Generic;

namespace HotelManager.Data.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        int Add(T entity);          // devuelve el nuevo Id
        bool Update(T entity);
        bool Delete(int id);
        T GetById(int id);
        IEnumerable<T> GetAll();
    }
}