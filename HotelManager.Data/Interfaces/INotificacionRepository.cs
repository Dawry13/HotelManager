using System.Collections.Generic;
using HotelManager.Domain.Entities;

namespace HotelManager.Data.Interfaces
{
    public interface INotificacionRepository : IBaseRepository<Notificacion>
    {
        IEnumerable<Notificacion> GetByReserva(int idReserva);
        IEnumerable<Notificacion> GetPorLeida(bool leida);
    }
}