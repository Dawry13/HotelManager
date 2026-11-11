namespace HotelManager.Domain.Enums
{
    /// <summary>Actores del sistema (documento de analisis: ACT-001 a ACT-004).</summary>
    public enum RolUsuario
    {
        /// <summary>ACT-001: gestiona la estructura hotelera y parametros operativos.</summary>
        Administrador = 1,

        /// <summary>ACT-002: consulta disponibilidad, registra huespedes y gestiona reservas.</summary>
        Recepcionista = 2,

        /// <summary>ACT-003: solicita reservas y recibe notificaciones.</summary>
        Huesped = 3,

        /// <summary>ACT-004: envia confirmaciones, modificaciones y cancelaciones.</summary>
        ServicioNotificacion = 4
    }
}
