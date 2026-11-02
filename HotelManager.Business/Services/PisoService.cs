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

        public PisoService(IPisoRepository pisoRepository, IHotelRepository hotelRepository)
        {
            _pisoRepository = pisoRepository;
            _hotelRepository = hotelRepository;
        }

        // RN-001, RN-002, RN-003
        public PisoDto Crear(PisoDto dto)
        {
            if (dto == null) throw new BusinessException("Los datos del piso son obligatorios.");
            if (dto.Numero < 0) throw new BusinessException("El número de piso no es válido.");

            if (_hotelRepository.GetById(dto.IdHotel) == null)
                throw new BusinessException("El hotel indicado no existe.");

            if (_pisoRepository.ExisteNumeroActivo(dto.IdHotel, dto.Numero, null))
                throw new BusinessException("Ya existe un piso activo con ese número en el hotel.");

            var piso = new Piso
            {
                Numero = dto.Numero,
                IdHotel = dto.IdHotel,
                Estado = true
            };

            piso.IdPiso = _pisoRepository.Add(piso);
            return ToDto(piso);
        }

        public PisoDto Actualizar(PisoDto dto)
        {
            if (dto == null) throw new BusinessException("Los datos del piso son obligatorios.");

            var piso = ObtenerEntidad(dto.IdPiso);

            if (piso.Estado &&
                _pisoRepository.ExisteNumeroActivo(piso.IdHotel, dto.Numero, piso.IdPiso))
                throw new BusinessException("Ya existe un piso activo con ese número en el hotel.");

            piso.Numero = dto.Numero;
            _pisoRepository.Update(piso);
            return ToDto(piso);
        }

        public void Activar(int idPiso)
        {
            var piso = ObtenerEntidad(idPiso);
            if (piso.Estado) return;

            // RN-003: al reactivar no puede chocar con otro piso activo del mismo número
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

        public PisoDto ObtenerPorId(int idPiso)
        {
            return ToDto(ObtenerEntidad(idPiso));
        }

        public IEnumerable<PisoDto> ListarPorHotel(int idHotel)
        {
            return _pisoRepository.GetByHotel(idHotel).Select(ToDto).ToList();
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
                IdHotel = p.IdHotel
            };
        }
    }
}