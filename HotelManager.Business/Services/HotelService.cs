using System.Collections.Generic;
using System.Linq;
using HotelManager.Business.Catalogos;
using HotelManager.Business.Dtos;
using HotelManager.Business.Exceptions;
using HotelManager.Business.Interfaces;
using HotelManager.Data.Interfaces;
using HotelManager.Domain.Entities;

namespace HotelManager.Business.Services
{
    public class HotelService : IHotelService
    {
        private readonly IHotelRepository _hotelRepository;

        public HotelService(IHotelRepository hotelRepository)
        {
            _hotelRepository = hotelRepository;
        }

        public HotelDto Crear(HotelDto dto)
        {
            Validar(dto);

            if (_hotelRepository.ExisteNombre(dto.Nombre.Trim(), null))
                throw new BusinessException("Ya existe un hotel con ese nombre.");

            var hotel = new Hotel
            {
                Nombre = dto.Nombre.Trim(),
                Direccion = dto.Direccion.Trim(),
                Estado = true,
                Categoria = dto.Categoria,
                Descripcion = Limpiar(dto.Descripcion),
                Caracteristicas = Caracteristicas.Unir(dto.Caracteristicas)
            };

            hotel.IdHotel = _hotelRepository.Add(hotel);
            return ToDto(hotel);
        }

        public HotelDto Actualizar(HotelDto dto)
        {
            Validar(dto);

            var hotel = _hotelRepository.GetById(dto.IdHotel);
            if (hotel == null)
                throw new BusinessException("El hotel no existe.");

            if (_hotelRepository.ExisteNombre(dto.Nombre.Trim(), hotel.IdHotel))
                throw new BusinessException("Ya existe un hotel con ese nombre.");

            hotel.Nombre = dto.Nombre.Trim();
            hotel.Direccion = dto.Direccion.Trim();
            hotel.Categoria = dto.Categoria;
            hotel.Descripcion = Limpiar(dto.Descripcion);
            hotel.Caracteristicas = Caracteristicas.Unir(dto.Caracteristicas);

            _hotelRepository.Update(hotel);
            return ToDto(hotel);
        }

        public void Activar(int idHotel)
        {
            CambiarEstado(idHotel, true);
        }

        public void Desactivar(int idHotel)
        {
            CambiarEstado(idHotel, false);
        }

        private void CambiarEstado(int idHotel, bool activo)
        {
            var hotel = _hotelRepository.GetById(idHotel);
            if (hotel == null)
                throw new BusinessException("El hotel no existe.");
            if (hotel.Estado == activo) return;

            hotel.Estado = activo;
            _hotelRepository.Update(hotel);
        }

        public HotelDto ObtenerPorId(int idHotel)
        {
            var hotel = _hotelRepository.GetById(idHotel);
            if (hotel == null)
                throw new BusinessException("El hotel no existe.");
            return ToDto(hotel);
        }

        public IEnumerable<HotelDto> Listar()
        {
            return _hotelRepository.GetAll().Select(ToDto).ToList();
        }

        private static void Validar(HotelDto dto)
        {
            if (dto == null)
                throw new BusinessException("Los datos del hotel son obligatorios.");
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                throw new BusinessException("El nombre del hotel es obligatorio.");
            if (string.IsNullOrWhiteSpace(dto.Direccion))
                throw new BusinessException("La dirección del hotel es obligatoria.");
            if (dto.Categoria < Caracteristicas.CategoriaMinima || dto.Categoria > Caracteristicas.CategoriaMaxima)
                throw new BusinessException("La categoría del hotel debe ser de 3, 4 o 5 estrellas.");
            if (dto.Descripcion != null && dto.Descripcion.Trim().Length > 500)
                throw new BusinessException("La descripción no puede superar 500 caracteres.");
        }

        private static string Limpiar(string texto)
        {
            return string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
        }

        private static HotelDto ToDto(Hotel h)
        {
            return new HotelDto
            {
                IdHotel = h.IdHotel,
                Nombre = h.Nombre,
                Direccion = h.Direccion,
                Estado = h.Estado ? "Activo" : "Inactivo",
                Categoria = h.Categoria,
                Descripcion = h.Descripcion,
                Caracteristicas = Caracteristicas.Separar(h.Caracteristicas)
            };
        }
    }
}