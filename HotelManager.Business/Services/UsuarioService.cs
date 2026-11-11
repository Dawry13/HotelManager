using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HotelManager.Business.Dtos;
using HotelManager.Business.Exceptions;
using HotelManager.Business.Interfaces;
using HotelManager.Business.Seguridad;
using HotelManager.Data.Interfaces;
using HotelManager.Domain.Entities;
using HotelManager.Domain.Enums;

namespace HotelManager.Business.Services
{
    /// <summary>RNF-001: autenticacion, cuentas y roles de los actores del sistema.</summary>
    public class UsuarioService : IUsuarioService
    {
        private static readonly Regex FormatoUsuario = new Regex(@"^[a-z0-9._-]{3,50}$");

        private readonly IUsuarioRepository _usuarios;
        private readonly IHuespedRepository _huespedes;
        private readonly IHuespedService _huespedService;

        public UsuarioService(IUsuarioRepository usuarios, IHuespedRepository huespedes, IHuespedService huespedService)
        {
            _usuarios = usuarios;
            _huespedes = huespedes;
            _huespedService = huespedService;
        }

        public UsuarioDto Autenticar(string nombreUsuario, string clave)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrEmpty(clave))
                throw new BusinessException("Ingrese su usuario y contraseña.");

            var usuario = _usuarios.GetByNombreUsuario(Normalizar(nombreUsuario));

            // Mismo mensaje si el usuario no existe o la clave es incorrecta (no revela cuentas validas).
            if (usuario == null || !ClaveSegura.Verificar(clave, usuario.ClaveHash))
                throw new BusinessException("Usuario o contraseña incorrectos.");
            if (!usuario.Estado)
                throw new BusinessException("Su cuenta está desactivada. Comuníquese con el administrador.");

            _usuarios.RegistrarAcceso(usuario.IdUsuario);
            return ToDto(usuario);
        }

        public IEnumerable<UsuarioDto> Listar()
        {
            return _usuarios.GetAll().Select(ToDto).ToList();
        }

        public UsuarioDto ObtenerPorId(int idUsuario)
        {
            return ToDto(ObtenerEntidad(idUsuario));
        }

        public UsuarioDto Crear(UsuarioDto dto, string claveInicial)
        {
            Validar(dto, null);
            ValidarClave(claveInicial);

            var usuario = new Usuario
            {
                NombreUsuario = Normalizar(dto.NombreUsuario),
                Nombre = dto.Nombre.Trim(),
                Correo = Limpiar(dto.Correo),
                ClaveHash = ClaveSegura.Hash(claveInicial),
                Rol = dto.Rol,
                Estado = true,
                DebeCambiarClave = true,
                IdHuesped = dto.Rol == RolUsuario.Huesped ? dto.IdHuesped : null
            };
            usuario.IdUsuario = _usuarios.Add(usuario);
            return ToDto(_usuarios.GetById(usuario.IdUsuario));
        }

        public UsuarioDto Actualizar(UsuarioDto dto, int idUsuarioActual)
        {
            var usuario = ObtenerEntidad(dto.IdUsuario);
            Validar(dto, usuario.IdUsuario);

            // No dejar el sistema sin administradores activos.
            if (usuario.Rol == RolUsuario.Administrador && dto.Rol != RolUsuario.Administrador
                && _usuarios.ContarAdministradoresActivos(usuario.IdUsuario) == 0)
                throw new BusinessException("Debe existir al menos un administrador activo.");
            if (usuario.IdUsuario == idUsuarioActual && dto.Rol != usuario.Rol)
                throw new BusinessException("No puede cambiar su propio rol.");

            usuario.NombreUsuario = Normalizar(dto.NombreUsuario);
            usuario.Nombre = dto.Nombre.Trim();
            usuario.Correo = Limpiar(dto.Correo);
            usuario.Rol = dto.Rol;
            usuario.IdHuesped = dto.Rol == RolUsuario.Huesped ? dto.IdHuesped : null;
            _usuarios.Update(usuario);
            return ToDto(usuario);
        }

        public void Activar(int idUsuario)
        {
            var usuario = ObtenerEntidad(idUsuario);
            if (usuario.Estado) return;
            usuario.Estado = true;
            _usuarios.Update(usuario);
        }

        public void Desactivar(int idUsuario, int idUsuarioActual)
        {
            var usuario = ObtenerEntidad(idUsuario);
            if (!usuario.Estado) return;
            if (usuario.IdUsuario == idUsuarioActual)
                throw new BusinessException("No puede desactivar su propia cuenta.");
            if (usuario.Rol == RolUsuario.Administrador && _usuarios.ContarAdministradoresActivos(usuario.IdUsuario) == 0)
                throw new BusinessException("Debe existir al menos un administrador activo.");

            usuario.Estado = false;
            _usuarios.Update(usuario);
        }

        public void RestablecerClave(int idUsuario, string claveTemporal)
        {
            ObtenerEntidad(idUsuario);
            ValidarClave(claveTemporal);
            _usuarios.CambiarClave(idUsuario, ClaveSegura.Hash(claveTemporal), true);
        }

        public void CambiarClave(int idUsuario, string claveActual, string claveNueva)
        {
            var usuario = ObtenerEntidad(idUsuario);
            if (!ClaveSegura.Verificar(claveActual ?? string.Empty, usuario.ClaveHash))
                throw new BusinessException("La contraseña actual no es correcta.");
            ValidarClave(claveNueva);
            if (claveNueva == claveActual)
                throw new BusinessException("La nueva contraseña debe ser distinta de la actual.");

            _usuarios.CambiarClave(idUsuario, ClaveSegura.Hash(claveNueva), false);
        }

        public UsuarioDto RegistrarHuesped(HuespedDto huesped, string nombreUsuario, string clave)
        {
            if (huesped == null) throw new BusinessException("Los datos del huésped son obligatorios.");
            var usuarioNormalizado = Normalizar(nombreUsuario);
            ValidarNombreUsuario(usuarioNormalizado, null);
            ValidarClave(clave);

            // Si recepcion ya lo registro (mismo documento), se vincula la cuenta a ese huesped.
            var existente = string.IsNullOrWhiteSpace(huesped.Documento) ? null : _huespedService.BuscarPorDocumento(huesped.Documento);
            int idHuesped;
            if (existente != null)
            {
                if (_usuarios.GetByHuesped(existente.IdHuesped) != null)
                    throw new BusinessException("Ya existe una cuenta para ese documento. Inicie sesión o comuníquese con recepción.");
                if (!string.Equals((existente.Correo ?? "").Trim(), (huesped.Correo ?? "").Trim(), StringComparison.OrdinalIgnoreCase))
                    throw new BusinessException("Ese documento ya está registrado con otro correo. Comuníquese con recepción.");
                idHuesped = existente.IdHuesped;
            }
            else
            {
                idHuesped = _huespedService.Registrar(huesped).IdHuesped;
            }

            var datos = _huespedes.GetById(idHuesped);
            var usuario = new Usuario
            {
                NombreUsuario = usuarioNormalizado,
                Nombre = datos.Nombre,
                Correo = datos.Correo,
                ClaveHash = ClaveSegura.Hash(clave),
                Rol = RolUsuario.Huesped,
                Estado = true,
                DebeCambiarClave = false,
                IdHuesped = idHuesped
            };
            usuario.IdUsuario = _usuarios.Add(usuario);
            return ToDto(_usuarios.GetById(usuario.IdUsuario));
        }

        public IList<string> CrearUsuariosIniciales()
        {
            var creados = new List<string>();
            if (_usuarios.GetAll().Any()) return creados;

            Action<string, string, string, RolUsuario, int?> crear = (usuario, nombre, clave, rol, idHuesped) =>
            {
                _usuarios.Add(new Usuario
                {
                    NombreUsuario = usuario,
                    Nombre = nombre,
                    ClaveHash = ClaveSegura.Hash(clave),
                    Rol = rol,
                    Estado = true,
                    DebeCambiarClave = true,   // se pide cambiarla en el primer ingreso
                    IdHuesped = idHuesped
                });
                creados.Add(usuario + " / " + clave + " (" + Roles.Descripcion(rol) + ")");
            };

            crear("admin", "Administrador del sistema", "Admin2026", RolUsuario.Administrador, null);
            crear("recepcion", "Recepción", "Recepcion2026", RolUsuario.Recepcionista, null);
            crear("notificaciones", "Servicio de notificaciones", "Notifica2026", RolUsuario.ServicioNotificacion, null);

            // Huesped de ejemplo vinculado al primer huesped de los datos iniciales.
            var huesped = _huespedes.GetAll().OrderBy(h => h.IdHuesped).FirstOrDefault();
            if (huesped != null)
                crear("huesped", huesped.Nombre, "Huesped2026", RolUsuario.Huesped, huesped.IdHuesped);

            return creados;
        }

        private void Validar(UsuarioDto dto, int? excluirIdUsuario)
        {
            if (dto == null) throw new BusinessException("Los datos del usuario son obligatorios.");
            ValidarNombreUsuario(Normalizar(dto.NombreUsuario), excluirIdUsuario);
            if (string.IsNullOrWhiteSpace(dto.Nombre)) throw new BusinessException("El nombre es obligatorio.");
            if (!Enum.IsDefined(typeof(RolUsuario), dto.Rol)) throw new BusinessException("Seleccione un rol válido.");

            if (dto.Rol == RolUsuario.Huesped)
            {
                if (!dto.IdHuesped.HasValue || _huespedes.GetById(dto.IdHuesped.Value) == null)
                    throw new BusinessException("Una cuenta de huésped debe estar vinculada a un huésped registrado.");
                var otro = _usuarios.GetByHuesped(dto.IdHuesped.Value);
                if (otro != null && otro.IdUsuario != excluirIdUsuario)
                    throw new BusinessException("Ese huésped ya tiene una cuenta de usuario (" + otro.NombreUsuario + ").");
            }
        }

        private void ValidarNombreUsuario(string nombreUsuario, int? excluirIdUsuario)
        {
            if (!FormatoUsuario.IsMatch(nombreUsuario))
                throw new BusinessException("El usuario debe tener entre 3 y 50 caracteres: letras, números, punto, guion o guion bajo.");
            if (_usuarios.ExisteNombreUsuario(nombreUsuario, excluirIdUsuario))
                throw new BusinessException("Ya existe una cuenta con ese nombre de usuario.");
        }

        private static void ValidarClave(string clave)
        {
            var error = ClaveSegura.ValidarPolitica(clave);
            if (error != null) throw new BusinessException(error);
        }

        private Usuario ObtenerEntidad(int idUsuario)
        {
            var u = _usuarios.GetById(idUsuario);
            if (u == null) throw new BusinessException("El usuario no existe.");
            return u;
        }

        private static string Normalizar(string nombreUsuario)
        {
            return (nombreUsuario ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static string Limpiar(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        }

        private static UsuarioDto ToDto(Usuario u)
        {
            return new UsuarioDto
            {
                IdUsuario = u.IdUsuario,
                NombreUsuario = u.NombreUsuario,
                Nombre = u.Nombre,
                Correo = u.Correo,
                Rol = u.Rol,
                RolDescripcion = Roles.Descripcion(u.Rol),
                Activo = u.Estado,
                DebeCambiarClave = u.DebeCambiarClave,
                IdHuesped = u.IdHuesped,
                FechaCreacion = u.FechaCreacion,
                UltimoAcceso = u.UltimoAcceso
            };
        }
    }
}
