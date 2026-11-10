using System.Collections.Generic;
using HotelManager.Domain.Entities;
using HotelManager.Domain.Enums;

namespace HotelManager.Data.Interfaces
{
    public interface INotificacionRepository : IBaseRepository<Notificacion>
    {
        IEnumerable<Notificacion> GetByReserva(int idReserva);
        IEnumerable<Notificacion> GetPorEstado(EstadoNotificacion estado);

        /// <summary>RF-015: registra si el envio fue exitoso o fallido.</summary>
        bool RegistrarResultado(int idNotificacion, EstadoNotificacion estado, string detalleError);
    }
}
