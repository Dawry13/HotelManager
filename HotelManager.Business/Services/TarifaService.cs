using System;
using System.Collections.Generic;
using System.Linq;
using HotelManager.Business.Dtos;
using HotelManager.Business.Exceptions;
using HotelManager.Business.Interfaces;
using HotelManager.Data.Interfaces;
using HotelManager.Domain.Entities;

namespace HotelManager.Business.Services
{
    public class TarifaService : ITarifaService
    {
        private readonly ITarifaRepository _tarifaRepository;
        private readonly IHabitacionRepository _habitacionRepository;

        public TarifaService(ITarifaRepository tarifaRepository, IHabitacionRepository habitacionRepository)
        {
            _tarifaRepository = tarifaRepository;
            _habitacionRepository = habitacionRepository;
        }

        public decimal ObtenerTarifaAplicable(int idHabitacion, DateTime fecha)
        {
            var habitacion = _habitacionRepository.GetById(idHabitacion);
            if (habitacion == null)
                throw new BusinessException("La habitación no existe.");

            var tarifa = _tarifaRepository.GetVigente(idHabitacion, fecha.Date);
            return tarifa != null ? tarifa.PrecioPorNoche : habitacion.Tarifa;
        }

        public decimal CalcularCostoTotal(int idHabitacion, DateTime entrada, DateTime salida)
        {
            if (salida.Date <= entrada.Date)
                throw new BusinessException("La fecha de salida debe ser posterior a la de entrada.");

            decimal total = 0m;
            for (var noche = entrada.Date; noche < salida.Date; noche = noche.AddDays(1))
                total += ObtenerTarifaAplicable(idHabitacion, noche);

            return total;
        }

        public IEnumerable<TarifaDto> Listar()
        {
            return _tarifaRepository.GetAll()
                .OrderBy(t => t.IdHabitacion).ThenByDescending(t => t.FechaInicio)
                .Select(ToDto).ToList();
        }

        public IEnumerable<TarifaDto> ListarPorHabitacion(int idHabitacion)
        {
            return _tarifaRepository.GetByHabitacion(idHabitacion).Select(ToDto).ToList();
        }

        public TarifaDto ObtenerPorId(int idTarifa)
        {
            return ToDto(ObtenerEntidad(idTarifa));
        }

        public TarifaDto Crear(TarifaDto dto)
        {
            Validar(dto, null);
            var tarifa = new Tarifa
            {
                IdHabitacion = dto.IdHabitacion,
                PrecioPorNoche = dto.ValorNoche,
                FechaInicio = dto.VigenciaDesde.Date,
                FechaFin = dto.VigenciaHasta.HasValue ? dto.VigenciaHasta.Value.Date : (DateTime?)null
            };
            tarifa.IdTarifa = _tarifaRepository.Add(tarifa);
            return ToDto(tarifa);
        }

        public TarifaDto Actualizar(TarifaDto dto)
        {
            var tarifa = ObtenerEntidad(dto.IdTarifa);
            dto.IdHabitacion = tarifa.IdHabitacion;   // la tarifa no cambia de habitacion
            Validar(dto, tarifa.IdTarifa);

            tarifa.PrecioPorNoche = dto.ValorNoche;
            tarifa.FechaInicio = dto.VigenciaDesde.Date;
            tarifa.FechaFin = dto.VigenciaHasta.HasValue ? dto.VigenciaHasta.Value.Date : (DateTime?)null;
            _tarifaRepository.Update(tarifa);
            return ToDto(tarifa);
        }

        public void Eliminar(int idTarifa)
        {
            ObtenerEntidad(idTarifa);
            _tarifaRepository.Delete(idTarifa);
        }

        private void Validar(TarifaDto dto, int? excluirIdTarifa)
        {
            if (dto == null) throw new BusinessException("Los datos de la tarifa son obligatorios.");
            if (_habitacionRepository.GetById(dto.IdHabitacion) == null)
                throw new BusinessException("La habitación indicada no existe.");
            if (dto.ValorNoche <= 0)
                throw new BusinessException("El valor por noche debe ser mayor que cero.");
            if (dto.VigenciaHasta.HasValue && dto.VigenciaHasta.Value.Date < dto.VigenciaDesde.Date)
                throw new BusinessException("La vigencia final no puede ser anterior a la inicial.");

            // Dos tarifas de la misma habitacion no pueden tener vigencias que se crucen.
            var desde = dto.VigenciaDesde.Date;
            var hasta = dto.VigenciaHasta.HasValue ? dto.VigenciaHasta.Value.Date : DateTime.MaxValue.Date;
            var cruce = _tarifaRepository.GetByHabitacion(dto.IdHabitacion)
                .Where(t => !excluirIdTarifa.HasValue || t.IdTarifa != excluirIdTarifa.Value)
                .FirstOrDefault(t => t.FechaInicio.Date <= hasta && (t.FechaFin ?? DateTime.MaxValue).Date >= desde);
            if (cruce != null)
                throw new BusinessException("La vigencia se cruza con otra tarifa de la habitación (desde " +
                    cruce.FechaInicio.ToString("dd/MM/yyyy") + ").");
        }

        private Tarifa ObtenerEntidad(int idTarifa)
        {
            var t = _tarifaRepository.GetById(idTarifa);
            if (t == null) throw new BusinessException("La tarifa no existe.");
            return t;
        }

        private static TarifaDto ToDto(Tarifa t)
        {
            return new TarifaDto
            {
                IdTarifa = t.IdTarifa,
                IdHabitacion = t.IdHabitacion,
                ValorNoche = t.PrecioPorNoche,
                VigenciaDesde = t.FechaInicio,
                VigenciaHasta = t.FechaFin
            };
        }
    }
}
