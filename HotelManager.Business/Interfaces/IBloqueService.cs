using System.Collections.Generic;
using HotelManager.Business.Dtos;

namespace HotelManager.Business.Interfaces
{
    public interface IBloqueService
    {
        BloqueDto Crear(BloqueDto dto);
        BloqueDto Actualizar(BloqueDto dto);
        void Activar(int idBloque);
        void Desactivar(int idBloque);
        void Eliminar(int idBloque);
        BloqueDto ObtenerPorId(int idBloque);
        IEnumerable<BloqueDto> Listar();
        IEnumerable<BloqueDto> ListarPorHotel(int idHotel);
    }
}
