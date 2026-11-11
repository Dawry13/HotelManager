using HotelManager.Domain.Entities;

namespace HotelManager.Data.Interfaces
{
    public interface IUsuarioRepository : IBaseRepository<Usuario>
    {
        Usuario GetByNombreUsuario(string nombreUsuario);
        Usuario GetByHuesped(int idHuesped);
        bool ExisteNombreUsuario(string nombreUsuario, int? excluirIdUsuario = null);
        int ContarAdministradoresActivos(int? excluirIdUsuario = null);
        void RegistrarAcceso(int idUsuario);
        void CambiarClave(int idUsuario, string claveHash, bool debeCambiarClave);
    }
}
