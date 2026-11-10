using System.Collections.Generic;
using HotelManager.Business.Dtos;

namespace HotelManager.Business.Interfaces
{
    public interface IHotelService
    {
        HotelDto Crear(HotelDto dto);
        HotelDto Actualizar(HotelDto dto);
        void Activar(int idHotel);
        void Desactivar(int idHotel);
        HotelDto ObtenerPorId(int idHotel);
        IEnumerable<HotelDto> Listar();
    }
}
