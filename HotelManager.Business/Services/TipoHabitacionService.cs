using System.Collections.Generic;
using System.Linq;
using HotelManager.Business.Dtos;
using HotelManager.Business.Exceptions;
using HotelManager.Business.Interfaces;
using HotelManager.Data.Interfaces;
using HotelManager.Domain.Entities;

namespace HotelManager.Business.Services
{
    public class TipoHabitacionService : ITipoHabitacionService
    {
        private readonly ITipoHabitacionRepository _tipoRepository;

        public TipoHabitacionService(ITipoHabitacionRepository tipoRepository)
        {
            _tipoRepository = tipoRepository;
        }

        public TipoHabitacionDto Crear(TipoHabitacionDto dto)
        {
            Validar(dto);
            var nombre = dto.Nombre.Trim();
            if (_tipoRepository.ExisteNombre(nombre, null))
                throw new BusinessException("Ya existe un tipo de habitación con ese nombre.");

            var tipo = new TipoHabitacion
            {
                Nombre = nombre,
                Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim(),
                CapacidadMaxima = dto.CapacidadMaxima,
                TarifaBase = dto.TarifaBase,
                Estado = true
            };

            tipo.IdTipoHabitacion = _tipoRepository.Add(tipo);
            return ToDto(tipo);
        }

        public TipoHabitacionDto Actualizar(TipoHabitacionDto dto)
        {
            Validar(dto);
            var tipo = ObtenerEntidad(dto.IdTipoHabitacion);

            var nombre = dto.Nombre.Trim();
            if (_tipoRepository.ExisteNombre(nombre, tipo.IdTipoHabitacion))
                throw new BusinessException("Ya existe un tipo de habitación con ese nombre.");

            tipo.Nombre = nombre;
            tipo.Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim();
            tipo.CapacidadMaxima = dto.CapacidadMaxima;
            tipo.TarifaBase = dto.TarifaBase;

            _tipoRepository.Update(tipo);
            return ToDto(tipo);
        }

        public void Activar(int idTipoHabitacion)
        {
            var tipo = ObtenerEntidad(idTipoHabitacion);
            if (tipo.Estado) return;
            tipo.Estado = true;
            _tipoRepository.Update(tipo);
        }

        public void Desactivar(int idTipoHabitacion)
        {
            var tipo = ObtenerEntidad(idTipoHabitacion);
            if (!tipo.Estado) return;
            tipo.Estado = false;
            _tipoRepository.Update(tipo);
        }

        public void Eliminar(int idTipoHabitacion)
        {
            ObtenerEntidad(idTipoHabitacion);
            if (_tipoRepository.ContarHabitaciones(idTipoHabitacion) > 0)
                throw new BusinessException("No se puede eliminar: hay habitaciones de este tipo. Desactívelo en su lugar.");
            _tipoRepository.Delete(idTipoHabitacion);
        }

        public TipoHabitacionDto ObtenerPorId(int idTipoHabitacion) => ToDto(ObtenerEntidad(idTipoHabitacion));

        public IEnumerable<TipoHabitacionDto> Listar()
            => _tipoRepository.GetAll().Select(ToDto).ToList();

        private TipoHabitacion ObtenerEntidad(int id)
        {
            var tipo = _tipoRepository.GetById(id);
            if (tipo == null) throw new BusinessException("El tipo de habitación no existe.");
            return tipo;
        }

        private static void Validar(TipoHabitacionDto dto)
        {
            if (dto == null) throw new BusinessException("Los datos del tipo de habitación son obligatorios.");
            if (string.IsNullOrWhiteSpace(dto.Nombre)) throw new BusinessException("El nombre es obligatorio.");
            if (dto.CapacidadMaxima <= 0) throw new BusinessException("La capacidad máxima debe ser mayor que cero.");
            if (dto.TarifaBase < 0) throw new BusinessException("La tarifa base no puede ser negativa.");
        }

        private static TipoHabitacionDto ToDto(TipoHabitacion t) => new TipoHabitacionDto
        {
            IdTipoHabitacion = t.IdTipoHabitacion,
            Nombre = t.Nombre,
            Descripcion = t.Descripcion,
            CapacidadMaxima = t.CapacidadMaxima,
            TarifaBase = t.TarifaBase,
            Estado = t.Estado ? "Activo" : "Inactivo"
        };
    }
}
