using HotelManager.Domain.Enums;

namespace HotelManager.Business.Interfaces
{
    public interface INotificacionService
    {
        void Notificar(int idReserva, TipoNotificacion tipo);
    }
}