using System.Collections.Generic;
using System.Linq;
using HotelManager.Business.Dtos;
using HotelManager.Business.Exceptions;
using HotelManager.Business.Interfaces;
using HotelManager.Data.Interfaces;
using HotelManager.Domain.Entities;

namespace HotelManager.Business.Services
{
    public class PisoService : IPisoService
    {
        private readonly IPisoRepository _pisoRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly IBloqueRepository _bloqueRepository;

        public PisoService(IPisoRepository pisoRepository, IHotelRepository hotelRepository, IBloqueRepository bloqueRepository)
        {
            _pisoRepository = pisoRepository;
            _hotelRepository = hotelRepository;
            _bloqueRepository = bloqueRepository;
        }

        // CU-002: RN-001, RN-002
        public PisoDto Crear(PisoDto dto)
        {
            if (dto == null) throw new BusinessException("Los datos del piso son obligatorios.");
            if (dto.Numero < 0) throw new BusinessException("El número de piso no es válido.");

            var hotel = _hotelRepository.GetById(dto.IdHotel);
            if (hotel == null) throw new BusinessException("El hotel indicado no existe.");
            if (!hotel.Estado) throw new BusinessException("No se pueden agregar pisos a un hotel inactivo.");

            ValidarBloque(dto.IdBloque, dto.IdHotel);

            if (_pisoRepository.ExisteNumeroActivo(dto.IdHotel, dto.Numero, null))
                throw new BusinessException("Ya existe un piso activo con ese número en el hotel.");

            var piso = new Piso
            {
                Numero = dto.Numero,
                IdHotel = dto.IdHotel,
                IdBloque = dto.IdBloque,
                Estado = true
            };

            piso.IdPiso = _pisoRepository.Add(piso);
            return ToDto(piso);
        }

        public PisoDto Actualizar(PisoDto dto)
        {
            if (dto == null) throw new BusinessException("Los datos del piso son obligatorios.");
            if (dto.Numero < 0) throw new BusinessException("El número de piso no es válido.");

            var piso = ObtenerEntidad(dto.IdPiso);

            // RN-001: el piso no cambia de hotel; solo puede moverse entre bloques del mismo hotel.
            ValidarBloque(dto.IdBloque, piso.IdHotel);

            if (piso.Estado &&
                _pisoRepository.ExisteNumeroActivo(piso.IdHotel, dto.Numero, piso.IdPiso))
                throw new BusinessException("Ya existe un piso activo con ese número en el hotel.");

            piso.Numero = dto.Numero;
            piso.IdBloque = dto.IdBloque;
            _pisoRepository.Update(piso);
            return ToDto(piso);
        }

        public void Activar(int idPiso)
        {
            var piso = ObtenerEntidad(idPiso);
            if (piso.Estado) return;

            if (_pisoRepository.ExisteNumeroActivo(piso.IdHotel, piso.Numero, piso.IdPiso))
                throw new BusinessException("No se puede activar: ya existe un piso activo con ese número.");

            piso.Estado = true;
            _pisoRepository.Update(piso);
        }

        public void Desactivar(int idPiso)
        {
            var piso = ObtenerEntidad(idPiso);
            if (!piso.Estado) return;

            piso.Estado = false;
            _pisoRepository.Update(piso);
        }

        public void Eliminar(int idPiso)
        {
            ObtenerEntidad(idPiso);
            if (_pisoRepository.ContarHabitaciones(idPiso) > 0)
                throw new BusinessException("No se puede eliminar: el piso tiene habitaciones. Desactívelo en su lugar.");
            _pisoRepository.Delete(idPiso);
        }

        public PisoDto ObtenerPorId(int idPiso)
        {
            return ToDto(ObtenerEntidad(idPiso));
        }

        public IEnumerable<PisoDto> Listar()
        {
            return _pisoRepository.GetAll().Select(ToDto).ToList();
        }

        public IEnumerable<PisoDto> ListarPorHotel(int idHotel)
        {
            return _pisoRepository.GetByHotel(idHotel).Select(ToDto).ToList();
        }

        private void ValidarBloque(int? idBloque, int idHotel)
        {
            if (!idBloque.HasValue) return;

            var bloque = _bloqueRepository.GetById(idBloque.Value);
            if (bloque == null) throw new BusinessException("El bloque indicado no existe.");
            if (bloque.IdHotel != idHotel) throw new BusinessException("El bloque no pertenece al hotel seleccionado.");
            if (!bloque.Estado) throw new BusinessException("No se pueden asignar pisos a un bloque inactivo.");
        }

        private Piso ObtenerEntidad(int idPiso)
        {
            var piso = _pisoRepository.GetById(idPiso);
            if (piso == null) throw new BusinessException("El piso no existe.");
            return piso;
        }

        private static PisoDto ToDto(Piso p)
        {
            return new PisoDto
            {
                IdPiso = p.IdPiso,
                Numero = p.Numero,
                Estado = p.Estado ? "Activo" : "Inactivo",
                IdHotel = p.IdHotel,
                IdBloque = p.IdBloque
            };
        }
    }
}
