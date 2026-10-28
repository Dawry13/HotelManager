using System.Collections.Generic;
using HotelManager.Domain.Entities;

namespace HotelManager.Data.Interfaces
{
    public interface INotificacionRepository
    {
        IEnumerable<Notificacion> GetByReserva(int idReserva);
        IEnumerable<Notificacion> GetPorLeida(bool leida);
    }
}