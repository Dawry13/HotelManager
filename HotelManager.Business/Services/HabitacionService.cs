using System.Collections.Generic;
using System.Linq;
using HotelManager.Business.Dtos;
using HotelManager.Business.Exceptions;
using HotelManager.Business.Interfaces;
using HotelManager.Data.Interfaces;
using HotelManager.Domain.Entities;
using HotelManager.Domain.Enums;

namespace HotelManager.Business.Services
{
    public class HabitacionService : IHabitacionService
    {
        private readonly IHabitacionRepository _habitacionRepository;
        private readonly IPisoRepository _pisoRepository;

        public HabitacionService(IHabitacionRepository habitacionRepository, IPisoRepository pisoRepository)
        {
            _habitacionRepository = habitacionRepository;
            _pisoRepository = pisoRepository;
        }

        // CU-003: RN-004, RN-005, RN-006
        public HabitacionDto Crear(HabitacionDto dto)
        {
            ValidarDatos(dto);

            var piso = _pisoRepository.GetById(dto.IdPiso);
            if (piso == null)
                throw new BusinessException("El piso indicado no existe.");

            if (!piso.Estado)
                throw new BusinessException("Un piso inactivo no puede recibir nuevas habitaciones.");

            if (_habitacionRepository.ExisteNumeroEnHotel(piso.IdHotel, dto.Numero, null))
                throw new BusinessException("Ya existe una habitación con ese número en el hotel.");

            var habitacion = new Habitacion
            {
                Numero = dto.Numero,
                Capacidad = dto.Capacidad,
                Tarifa = dto.Tarifa,
                Estado = EstadoHabitacion.Disponible,
                IdPiso = dto.IdPiso
            };

            habitacion.IdHabitacion = _habitacionRepository.Add(habitacion);
            return ToDto(habitacion);
        }

        public HabitacionDto Actualizar(HabitacionDto dto)
        {
            ValidarDatos(dto);

            var habitacion = ObtenerEntidad(dto.IdHabitacion);
            var piso = _pisoRepository.GetById(habitacion.IdPiso);

            if (_habitacionRepository.ExisteNumeroEnHotel(piso.IdHotel, dto.Numero, habitacion.IdHabitacion))
                throw new BusinessException("Ya existe una habitación con ese número en el hotel.");

            habitacion.Numero = dto.Numero;
            habitacion.Capacidad = dto.Capacidad;
            habitacion.Tarifa = dto.Tarifa;

            _habitacionRepository.Update(habitacion);
            return ToDto(habitacion);
        }

        // CU-004 / RF-004 / RN-007
        public void CambiarEstado(int idHabitacion, EstadoHabitacion nuevoEstado)
        {
            var habitacion = ObtenerEntidad(idHabitacion);
            if (habitacion.Estado == nuevoEstado) return;

            habitacion.Estado = nuevoEstado;
            _habitacionRepository.Update(habitacion);
        }

        public HabitacionDto ObtenerPorId(int idHabitacion)
        {
            return ToDto(ObtenerEntidad(idHabitacion));
        }

        public IEnumerable<HabitacionDto> ListarPorHotel(int idHotel)
        {
            return _habitacionRepository.GetByHotel(idHotel).Select(ToDto).ToList();
        }

        private Habitacion ObtenerEntidad(int idHabitacion)
        {
            var h = _habitacionRepository.GetById(idHabitacion);
            if (h == null) throw new BusinessException("La habitación no existe.");
            return h;
        }

        private static void ValidarDatos(HabitacionDto dto)
        {
            if (dto == null) throw new BusinessException("Los datos de la habitación son obligatorios.");
            if (dto.Numero <= 0) throw new BusinessException("El número de habitación no es válido.");
            if (dto.Capacidad <= 0) throw new BusinessException("La capacidad debe ser mayor que cero.");
            if (dto.Tarifa <= 0) throw new BusinessException("La tarifa debe ser mayor que cero.");
        }

        private static HabitacionDto ToDto(Habitacion h)
        {
            return new HabitacionDto
            {
                IdHabitacion = h.IdHabitacion,
                Numero = h.Numero,
                Capacidad = h.Capacidad,
                Tarifa = h.Tarifa,
                Estado = h.Estado.ToString(),
                IdPiso = h.IdPiso
            };
        }
    }
}