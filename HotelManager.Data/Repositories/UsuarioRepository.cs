using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using HotelManager.Data.Base;
using HotelManager.Data.Interfaces;
using HotelManager.Domain.Entities;
using HotelManager.Domain.Enums;

namespace HotelManager.Data.Repositories
{
    public class UsuarioRepository : BaseRepository<Usuario>, IUsuarioRepository
    {
        protected override string TableName => "usuarios";
        protected override string KeyColumn => "id_usuario";

        // ck_usuarios_rol: 'administrador','recepcionista','huesped','servicio_notificacion'
        public static string RolTexto(RolUsuario rol)
        {
            switch (rol)
            {
                case RolUsuario.Administrador: return "administrador";
                case RolUsuario.Recepcionista: return "recepcionista";
                case RolUsuario.Huesped: return "huesped";
                default: return "servicio_notificacion";
            }
        }

        public static RolUsuario RolDesdeTexto(string texto)
        {
            switch ((texto ?? string.Empty).Trim().ToLower())
            {
                case "administrador": return RolUsuario.Administrador;
                case "recepcionista": return RolUsuario.Recepcionista;
                case "huesped": return RolUsuario.Huesped;
                default: return RolUsuario.ServicioNotificacion;
            }
        }

        protected override Usuario Map(IDataRecord r) => new Usuario
        {
            IdUsuario = (int)r["id_usuario"],
            NombreUsuario = (string)r["nombre_usuario"],
            Nombre = (string)r["nombre"],
            Correo = Texto(r, "correo"),
            ClaveHash = (string)r["clave_hash"],
            Rol = RolDesdeTexto((string)r["rol"]),
            Estado = EsActivo(r["estado"]),
            DebeCambiarClave = (bool)r["debe_cambiar_clave"],
            IdHuesped = EnteroNulo(r, "id_huesped"),
            FechaCreacion = (DateTime)r["fecha_creacion"],
            UltimoAcceso = r["ultimo_acceso"] == DBNull.Value ? (DateTime?)null : (DateTime)r["ultimo_acceso"]
        };

        public override IEnumerable<Usuario> GetAll()
            => Query("select * from usuarios order by rol, nombre_usuario");

        public Usuario GetByNombreUsuario(string nombreUsuario)
            => Query("select * from usuarios where nombre_usuario = @u",
                new SqlParameter("@u", (nombreUsuario ?? string.Empty).Trim())).FirstOrDefault();

        public Usuario GetByHuesped(int idHuesped)
            => Query("select * from usuarios where id_huesped = @h", new SqlParameter("@h", idHuesped)).FirstOrDefault();

        public bool ExisteNombreUsuario(string nombreUsuario, int? excluirIdUsuario = null)
            => Convert.ToInt32(Scalar(@"select count(1) from usuarios
                                       where nombre_usuario = @u and (@x is null or id_usuario <> @x)",
                new SqlParameter("@u", nombreUsuario),
                new SqlParameter("@x", SqlDbType.Int) { Value = DbVal(excluirIdUsuario) })) > 0;

        public int ContarAdministradoresActivos(int? excluirIdUsuario = null)
            => Convert.ToInt32(Scalar(@"select count(1) from usuarios
                                       where rol = 'administrador' and estado = 'activo'
                                         and (@x is null or id_usuario <> @x)",
                new SqlParameter("@x", SqlDbType.Int) { Value = DbVal(excluirIdUsuario) }));

        public void RegistrarAcceso(int idUsuario)
            => Execute("update usuarios set ultimo_acceso = sysdatetime() where id_usuario = @id",
                new SqlParameter("@id", idUsuario));

        public void CambiarClave(int idUsuario, string claveHash, bool debeCambiarClave)
            => Execute("update usuarios set clave_hash = @c, debe_cambiar_clave = @d where id_usuario = @id",
                new SqlParameter("@c", claveHash),
                new SqlParameter("@d", debeCambiarClave),
                new SqlParameter("@id", idUsuario));

        public override int Add(Usuario e)
            => Convert.ToInt32(Scalar(@"insert into usuarios (nombre_usuario, nombre, correo, clave_hash, rol, estado, debe_cambiar_clave, id_huesped)
                                       output inserted.id_usuario
                                       values (@u, @n, @c, @h, @r, @e, @d, @hu)",
                new SqlParameter("@u", e.NombreUsuario),
                new SqlParameter("@n", e.Nombre),
                new SqlParameter("@c", SqlDbType.VarChar, 150) { Value = DbVal(e.Correo) },
                new SqlParameter("@h", e.ClaveHash),
                new SqlParameter("@r", RolTexto(e.Rol)),
                new SqlParameter("@e", EstadoTexto(e.Estado)),
                new SqlParameter("@d", e.DebeCambiarClave),
                new SqlParameter("@hu", SqlDbType.Int) { Value = DbVal(e.IdHuesped) }));

        // No modifica la clave: para eso esta CambiarClave.
        public override bool Update(Usuario e)
            => Execute(@"update usuarios
                         set nombre_usuario = @u, nombre = @n, correo = @c, rol = @r, estado = @e, id_huesped = @hu
                         where id_usuario = @id",
                new SqlParameter("@u", e.NombreUsuario),
                new SqlParameter("@n", e.Nombre),
                new SqlParameter("@c", SqlDbType.VarChar, 150) { Value = DbVal(e.Correo) },
                new SqlParameter("@r", RolTexto(e.Rol)),
                new SqlParameter("@e", EstadoTexto(e.Estado)),
                new SqlParameter("@hu", SqlDbType.Int) { Value = DbVal(e.IdHuesped) },
                new SqlParameter("@id", e.IdUsuario)) > 0;
    }
}
