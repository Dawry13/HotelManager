using System.Collections.Generic;
using HotelManager.Business.Dtos;

namespace HotelManager.Business.Interfaces
{
    /// <summary>RNF-001: autenticacion de usuarios y administracion de cuentas y roles.</summary>
    public interface IUsuarioService
    {
        /// <summary>Valida usuario y clave. Lanza BusinessException si no son validos o la cuenta esta inactiva.</summary>
        UsuarioDto Autenticar(string nombreUsuario, string clave);

        IEnumerable<UsuarioDto> Listar();
        UsuarioDto ObtenerPorId(int idUsuario);

        /// <summary>El administrador crea una cuenta. La clave inicial obliga a cambiarla en el primer ingreso.</summary>
        UsuarioDto Crear(UsuarioDto dto, string claveInicial);
        UsuarioDto Actualizar(UsuarioDto dto, int idUsuarioActual);
        void Activar(int idUsuario);
        void Desactivar(int idUsuario, int idUsuarioActual);
        void RestablecerClave(int idUsuario, string claveTemporal);

        /// <summary>El propio usuario cambia su clave.</summary>
        void CambiarClave(int idUsuario, string claveActual, string claveNueva);

        /// <summary>ACT-003: un huesped crea su cuenta (y su registro de huesped si no existe).</summary>
        UsuarioDto RegistrarHuesped(HuespedDto huesped, string nombreUsuario, string clave);

        /// <summary>Crea las cuentas iniciales si la tabla de usuarios esta vacia. Devuelve "usuario / clave" creadas.</summary>
        IList<string> CrearUsuariosIniciales();
    }
}
