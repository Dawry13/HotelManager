using System;
using System.Collections.Generic;
using System.Linq;
using HotelManager.Business.Dtos;
using HotelManager.Business.Exceptions;
using HotelManager.Business.Interfaces;
using HotelManager.Data.Interfaces;
using HotelManager.Domain.Enums;

namespace HotelManager.Business.Services
{
    public class DisponibilidadService : IDisponibilidadService
    {
        private readonly IHabitacionRepository _habitacionRepository;
        private readonly IPisoRepository _pisoRepository;
        private readonly IReservaRepository _reservaRepository;
        private readonly ITarifaService _tarifaService;
        private readonly IHotelRepository _hotelRepository;

        public DisponibilidadService(
            IHabitacionRepository habitacionRepository,
            IPisoRepository pisoRepository,
            IReservaRepository reservaRepository,
            ITarifaService tarifaService,
            IHotelRepository hotelRepository)
        {
            _habitacionRepository = habitacionRepository;
            _pisoRepository = pisoRepository;
            _reservaRepository = reservaRepository;
            _tarifaService = tarifaService;
            _hotelRepository = hotelRepository;
        }

       
        public IEnumerable<HabitacionDisponibleDto> Consultar(DisponibilidadRequestDto request)
        {
            if (request == null)
                throw new BusinessException("La consulta es obligatoria.");

            var entrada = request.Entrada.Date;
            var salida = request.Salida.Date;

            if (salida <= entrada)
                throw new BusinessException("La fecha de salida debe ser posterior a la fecha de entrada.");      
            if (entrada < DateTime.Today)
                throw new BusinessException("No se permiten consultas para fechas anteriores a hoy.");
            if (request.CantidadHuespedes <= 0)
                throw new BusinessException("La cantidad de huéspedes debe ser mayor que cero.");

            var habitaciones = request.IdHotel.HasValue
                ? _habitacionRepository.GetByHotel(request.IdHotel.Value)
                : _habitacionRepository.GetAll();

            var pisos = _pisoRepository.GetAll().ToDictionary(p => p.IdPiso);
            var hotelesActivos = new HashSet<int>(_hotelRepository.GetAll().Where(x => x.Estado).Select(x => x.IdHotel));
            var noches = (salida - entrada).Days;
            var resultado = new List<HabitacionDisponibleDto>();

            foreach (var h in habitaciones)
            {
                if (h.Estado != EstadoHabitacion.Disponible) continue;                      
                if (h.Capacidad < request.CantidadHuespedes) continue;                      
                if (!pisos.ContainsKey(h.IdPiso) || !pisos[h.IdPiso].Estado) continue;
                if (!hotelesActivos.Contains(pisos[h.IdPiso].IdHotel)) continue;          // hotel inactivo
                if (_reservaRepository.ExisteSolapamiento(h.IdHabitacion, entrada, salida, null)) continue; 

                var costo = _tarifaService.CalcularCostoTotal(h.IdHabitacion, entrada, salida);

                resultado.Add(new HabitacionDisponibleDto
                {
                    IdHabitacion = h.IdHabitacion,
                    IdPiso = h.IdPiso,
                    IdHotel = pisos[h.IdPiso].IdHotel,
                    Numero = h.Numero,
                    Capacidad = h.Capacidad,
                    Noches = noches,
                    CostoEstimado = costo,
                    TarifaPorNoche = Math.Round(costo / noches, 2)
                });
            }

            
            return resultado.OrderBy(r => r.IdHotel).ThenBy(r => r.Numero).ToList();
        }
    }
}