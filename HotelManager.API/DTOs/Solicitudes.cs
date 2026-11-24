using HotelManager.Business.Dtos;
using HotelManager.Domain.Enums;

namespace HotelManager.API.DTOs;

public record CambiarEstadoHabitacionRequest(EstadoHabitacion Estado);

public record TarifaRequest(decimal Tarifa);

public record RegistrarResultadoNotificacionRequest(bool Enviada, string? DetalleError);

public record NotificarRequest(int IdReserva, TipoNotificacion Tipo);

public record LoginRequest(string NombreUsuario, string Clave);

public record CrearUsuarioRequest(UsuarioDto Usuario, string ClaveInicial);

public record ActualizarUsuarioRequest(UsuarioDto Usuario, int IdUsuarioActual);

public record ClaveTemporalRequest(string ClaveTemporal);

public record CambiarClaveRequest(string ClaveActual, string ClaveNueva);

public record RegistrarHuespedRequest(HuespedDto Huesped, string NombreUsuario, string Clave);
