using HotelManager.Domain.Enums;

namespace HotelManager.Business.Dtos
{
    /// <summary>Cuenta que se crea al arrancar la aplicacion si aun no existe (ver Program.cs de la capa web).</summary>
    public class CuentaInicialDto
    {
        public string Usuario { get; set; }
        public string Nombre { get; set; }
        public string Clave { get; set; }
        public RolUsuario Rol { get; set; }
        /// <summary>Si es true se pide cambiar la clave en el primer ingreso.</summary>
        public bool DebeCambiarClave { get; set; }
    }
}
