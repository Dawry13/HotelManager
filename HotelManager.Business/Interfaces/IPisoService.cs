using System.Collections.Generic;
using HotelManager.Business.Dtos;

namespace HotelManager.Business.Interfaces
{
    public interface IPisoService
    {
        PisoDto Crear(PisoDto dto);
        PisoDto Actualizar(PisoDto dto);
        void Activar(int idPiso);
        void Desactivar(int idPiso);
        void Eliminar(int idPiso);
        PisoDto ObtenerPorId(int idPiso);
        IEnumerable<PisoDto> Listar();
        IEnumerable<PisoDto> ListarPorHotel(int idHotel);
    }
}
