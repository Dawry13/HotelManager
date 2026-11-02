using System;
using System.Collections.Generic;
using System.Linq;
using HotelManager.Business.Dtos;
using HotelManager.Business.Interfaces;
using HotelManager.Business.Exceptions;
using HotelManager.Data.Interfaces;
using HotelManager.Domain.Entities;

namespace HotelManager.Business.Services
{
    public class HistorialReservaService : IHistorialReservaService
    {
        private readonly IHistorialReservaRepository _historialRepository;

        public HistorialReservaService(IHistorialReservaRepository historialRepository)
        {
            _historialRepository = historialRepository;
        }

        // CU-010 / RF-013
        public IEnumerable<HistorialReservaDto> ConsultarPorReserva(int idReserva)
        {
            if (idReserva <= 0)
                throw new BusinessException("La reserva indicada no es válida.");

            return _historialRepository.GetByReserva(idReserva)
                .OrderBy(h => h.FechaCambio)
                .Select(ToDto)
                .ToList();
        }

        public IEnumerable<HistorialReservaDto> Consultar(DateTime? desde, DateTime? hasta)
        {
            if (desde.HasValue && hasta.HasValue && hasta.Value < desde.Value)
                throw new BusinessException("La fecha final no puede ser anterior a la inicial.");

            var query = _historialRepository.GetAll().AsEnumerable();

            if (desde.HasValue) query = query.Where(h => h.FechaCambio >= desde.Value.Date);
            if (hasta.HasValue) query = query.Where(h => h.FechaCambio < hasta.Value.Date.AddDays(1));

            return query.OrderByDescending(h => h.FechaCambio).Select(ToDto).ToList();
        }

        private static HistorialReservaDto ToDto(HistorialReserva h)
        {
            return new HistorialReservaDto
            {
                IdHistorial = h.IdHistorial,
                IdReserva = h.IdReserva,
                Fecha = h.FechaCambio,
                Accion = h.Accion,
                EstadoAnterior = h.EstadoAnterior.ToString(),
                EstadoNuevo = h.EstadoNuevo.ToString()
            };
        }
    }
}