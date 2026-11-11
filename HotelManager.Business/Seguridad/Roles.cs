using HotelManager.Domain.Enums;

namespace HotelManager.Business.Seguridad
{
    /// <summary>Nombres de los roles tal como se usan en la autorizacion de la capa web.</summary>
    public static class Roles
    {
        public const string Administrador = "Administrador";
        public const string Recepcionista = "Recepcionista";
        public const string Huesped = "Huesped";
        public const string ServicioNotificacion = "ServicioNotificacion";

        public static string Nombre(RolUsuario rol)
        {
            switch (rol)
            {
                case RolUsuario.Administrador: return Administrador;
                case RolUsuario.Recepcionista: return Recepcionista;
                case RolUsuario.Huesped: return Huesped;
                default: return ServicioNotificacion;
            }
        }

        /// <summary>Texto para mostrar en pantalla.</summary>
        public static string Descripcion(RolUsuario rol)
        {
            switch (rol)
            {
                case RolUsuario.Administrador: return "Administrador";
                case RolUsuario.Recepcionista: return "Recepcionista";
                case RolUsuario.Huesped: return "Huésped";
                default: return "Servicio de notificaciones";
            }
        }
    }
}
