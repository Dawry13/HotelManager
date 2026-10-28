using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace HotelManager.Data.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        T GetById(int id);
        IEnumerable<T> GetAll();
        IEnumerable<T> Find(Expression<Func<T, bool>> predicado);
        void Add(T entidad);
        void Update(T entidad);
        void Remove(T entidad);
        int SaveChanges();
    }
}
