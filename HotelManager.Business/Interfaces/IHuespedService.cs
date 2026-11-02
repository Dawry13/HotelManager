using System.Collections.Generic;
using HotelManager.Business.Dtos;

namespace HotelManager.Business.Interfaces
{
    public interface IHuespedService
    {
        HuespedDto Registrar(HuespedDto dto);
        HuespedDto Actualizar(HuespedDto dto);
        HuespedDto ObtenerPorId(int idHuesped);
        HuespedDto BuscarPorDocumento(string documento);
        IEnumerable<HuespedDto> Listar();
    }
}