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
    public class HuespedService : IHuespedService
    {
        private readonly IHuespedRepository _huespedRepository;

        public HuespedService(IHuespedRepository huespedRepository)
        {
            _huespedRepository = huespedRepository;
        }

       
        public HuespedDto Registrar(HuespedDto dto)
        {
            Validar(dto);

            if (BuscarExacto(dto.Documento.Trim()) != null)
                throw new BusinessException("Ya existe un huésped registrado con ese documento.");

            var huesped = new Huesped
            {
                Nombre = dto.Nombre.Trim(),
                Documento = dto.Documento.Trim(),
                Telefono = dto.Telefono?.Trim(),
                Correo = dto.Correo?.Trim()
            };

            huesped.IdHuesped = _huespedRepository.Add(huesped);
            return ToDto(huesped);
        }

        public HuespedDto Actualizar(HuespedDto dto)
        {
            Validar(dto);

            var huesped = _huespedRepository.GetById(dto.IdHuesped);
            if (huesped == null)
                throw new BusinessException("El huésped no existe.");

            var otro = BuscarExacto(dto.Documento.Trim());
            if (otro != null && otro.IdHuesped != huesped.IdHuesped)
                throw new BusinessException("Ya existe otro huésped con ese documento.");

            huesped.Nombre = dto.Nombre.Trim();
            huesped.Documento = dto.Documento.Trim();
            huesped.Telefono = dto.Telefono?.Trim();
            huesped.Correo = dto.Correo?.Trim();

            _huespedRepository.Update(huesped);
            return ToDto(huesped);
        }

        public HuespedDto ObtenerPorId(int idHuesped)
        {
            var h = _huespedRepository.GetById(idHuesped);
            if (h == null) throw new BusinessException("El huésped no existe.");
            return ToDto(h);
        }

        public HuespedDto BuscarPorDocumento(string documento)
        {
            if (string.IsNullOrWhiteSpace(documento))
                throw new BusinessException("Debe indicar un documento.");

            var h = BuscarExacto(documento.Trim());
            return h == null ? null : ToDto(h);
        }

        public IEnumerable<HuespedDto> Listar()
        {
            return _huespedRepository.GetAll().Select(ToDto).ToList();
        }

        
        private Huesped BuscarExacto(string documento)
        {
            return _huespedRepository.Buscar(documento)
                .FirstOrDefault(h => string.Equals(h.Documento, documento, StringComparison.OrdinalIgnoreCase));
        }

        private static void Validar(HuespedDto dto)
        {
            if (dto == null) throw new BusinessException("Los datos del huésped son obligatorios.");
            if (string.IsNullOrWhiteSpace(dto.Nombre)) throw new BusinessException("El nombre es obligatorio.");
            if (string.IsNullOrWhiteSpace(dto.Documento)) throw new BusinessException("El documento es obligatorio.");

            
            if (string.IsNullOrWhiteSpace(dto.Correo))
                throw new BusinessException("El correo es obligatorio para enviar notificaciones.");
            if (!dto.Correo.Contains("@") || dto.Correo.StartsWith("@") || dto.Correo.EndsWith("@"))
                throw new BusinessException("El correo no tiene un formato válido.");
        }

        private static HuespedDto ToDto(Huesped h)
        {
            return new HuespedDto
            {
                IdHuesped = h.IdHuesped,
                Nombre = h.Nombre,
                Documento = h.Documento,
                Telefono = h.Telefono,
                Correo = h.Correo
            };
        }
    }
}