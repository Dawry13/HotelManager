using System.Collections.Generic;
using HotelManager.Business.Dtos;

namespace HotelManager.Business.Interfaces
{
    public interface IHotelService
    {
        HotelDto Crear(HotelDto dto);
        HotelDto Actualizar(HotelDto dto);
        HotelDto ObtenerPorId(int idHotel);
        IEnumerable<HotelDto> Listar();
    }
}