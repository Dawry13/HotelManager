using System.Collections.Generic;
using System.Linq;
using HotelManager.Business.Dtos;
using HotelManager.Business.Exceptions;
using HotelManager.Business.Interfaces;
using HotelManager.Data.Interfaces;
using HotelManager.Domain.Entities;

namespace HotelManager.Business.Services
{
    public class BloqueService : IBloqueService
    {
        private readonly IBloqueRepository _bloqueRepository;
        private readonly IHotelRepository _hotelRepository;

        public BloqueService(IBloqueRepository bloqueRepository, IHotelRepository hotelRepository)
        {
            _bloqueRepository = bloqueRepository;
            _hotelRepository = hotelRepository;
        }

        public BloqueDto Crear(BloqueDto dto)
        {
            Validar(dto);

            var hotel = _hotelRepository.GetById(dto.IdHotel);
            if (hotel == null) throw new BusinessException("El hotel indicado no existe.");
            if (!hotel.Estado) throw new BusinessException("No se pueden agregar bloques a un hotel inactivo.");

            var codigo = dto.Codigo.Trim().ToUpper();
            if (_bloqueRepository.ExisteCodigo(dto.IdHotel, codigo, null))
                throw new BusinessException("Ya existe un bloque con ese código en el hotel.");

            var bloque = new Bloque
            {
                IdHotel = dto.IdHotel,
                Codigo = codigo,
                Nombre = dto.Nombre.Trim(),
                Descripcion = Limpiar(dto.Descripcion),
                Estado = true
            };

            bloque.IdBloque = _bloqueRepository.Add(bloque);
            return ToDto(bloque);
        }

        public BloqueDto Actualizar(BloqueDto dto)
        {
            Validar(dto);
            var bloque = ObtenerEntidad(dto.IdBloque);

            var codigo = dto.Codigo.Trim().ToUpper();
            if (_bloqueRepository.ExisteCodigo(bloque.IdHotel, codigo, bloque.IdBloque))
                throw new BusinessException("Ya existe un bloque con ese código en el hotel.");

            // El hotel de un bloque no se cambia: sus pisos pertenecen a ese hotel.
            bloque.Codigo = codigo;
            bloque.Nombre = dto.Nombre.Trim();
            bloque.Descripcion = Limpiar(dto.Descripcion);

            _bloqueRepository.Update(bloque);
            return ToDto(bloque);
        }

        public void Activar(int idBloque)
        {
            var bloque = ObtenerEntidad(idBloque);
            if (bloque.Estado) return;

            var hotel = _hotelRepository.GetById(bloque.IdHotel);
            if (hotel != null && !hotel.Estado)
                throw new BusinessException("No se puede activar un bloque de un hotel inactivo.");

            bloque.Estado = true;
            _bloqueRepository.Update(bloque);
        }

        public void Desactivar(int idBloque)
        {
            var bloque = ObtenerEntidad(idBloque);
            if (!bloque.Estado) return;
            bloque.Estado = false;
            _bloqueRepository.Update(bloque);
        }

        public void Eliminar(int idBloque)
        {
            ObtenerEntidad(idBloque);
            if (_bloqueRepository.ContarPisos(idBloque) > 0)
                throw new BusinessException("No se puede eliminar: el bloque tiene pisos asociados. Desactívelo en su lugar.");
            _bloqueRepository.Delete(idBloque);
        }

        public BloqueDto ObtenerPorId(int idBloque) => ToDto(ObtenerEntidad(idBloque));

        public IEnumerable<BloqueDto> Listar()
            => _bloqueRepository.GetAll().Select(ToDto).ToList();

        public IEnumerable<BloqueDto> ListarPorHotel(int idHotel)
            => _bloqueRepository.GetByHotel(idHotel).Select(ToDto).ToList();

        private Bloque ObtenerEntidad(int idBloque)
        {
            var bloque = _bloqueRepository.GetById(idBloque);
            if (bloque == null) throw new BusinessException("El bloque no existe.");
            return bloque;
        }

        private static void Validar(BloqueDto dto)
        {
            if (dto == null) throw new BusinessException("Los datos del bloque son obligatorios.");
            if (string.IsNullOrWhiteSpace(dto.Codigo)) throw new BusinessException("El código del bloque es obligatorio.");
            if (dto.Codigo.Trim().Length > 10) throw new BusinessException("El código no puede superar 10 caracteres.");
            if (string.IsNullOrWhiteSpace(dto.Nombre)) throw new BusinessException("El nombre del bloque es obligatorio.");
        }

        private static string Limpiar(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        private static BloqueDto ToDto(Bloque b) => new BloqueDto
        {
            IdBloque = b.IdBloque,
            IdHotel = b.IdHotel,
            Codigo = b.Codigo,
            Nombre = b.Nombre,
            Descripcion = b.Descripcion,
            Estado = b.Estado ? "Activo" : "Inactivo"
        };
    }
}
