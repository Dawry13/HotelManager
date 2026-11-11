using System;
using HotelManager.Domain.Enums;

namespace HotelManager.Domain.Entities
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }

        /// <summary>Hash PBKDF2 de la clave (formato: algoritmo$iteraciones$sal$hash). Nunca la clave en texto.</summary>
        public string ClaveHash { get; set; }

        public RolUsuario Rol { get; set; }
        public bool Estado { get; set; }
        public bool DebeCambiarClave { get; set; }

        /// <summary>Solo para el rol Huesped: registro de huesped al que pertenece la cuenta.</summary>
        public int? IdHuesped { get; set; }

        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimoAcceso { get; set; }
    }
}
