using System.Collections.Generic;
using HotelManager.Business.Dtos;

namespace HotelManager.Business.Interfaces
{
    public interface IDisponibilidadService
    {
        IEnumerable<HabitacionDisponibleDto> Consultar(DisponibilidadRequestDto request);
    }
}