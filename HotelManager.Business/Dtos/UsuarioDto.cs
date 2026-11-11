using System;
using HotelManager.Domain.Enums;

namespace HotelManager.Business.Dtos
{
    public class UsuarioDto
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public RolUsuario Rol { get; set; }
        public string RolDescripcion { get; set; }
        public bool Activo { get; set; }
        public bool DebeCambiarClave { get; set; }
        public int? IdHuesped { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimoAcceso { get; set; }
    }
}
