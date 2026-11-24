using HotelManager.Business.Dtos;
using HotelManager.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelManager.API.Controllers;

[ApiController]
[Route("api/reservas")]
public class ReservasController : ControllerBase
{
    private readonly IReservaService _reservas;
    private readonly IHistorialReservaService _historial;
    private readonly INotificacionService _notificaciones;

    public ReservasController(IReservaService reservas, IHistorialReservaService historial, INotificacionService notificaciones)
    {
        _reservas = reservas;
        _historial = historial;
        _notificaciones = notificaciones;
    }

    [HttpGet]
    public ActionResult<IEnumerable<ReservaDto>> Listar() => Ok(_reservas.Listar());

    [HttpGet("activas")]
    public ActionResult<IEnumerable<ReservaDto>> Activas() => Ok(_reservas.ListarActivas());

    [HttpGet("{id:int}")]
    public ActionResult<ReservaDto> Obtener(int id) => Ok(_reservas.ObtenerPorId(id));

    /// <summary>Reserva creada por recepcion.</summary>
    [HttpPost]
    public ActionResult<ReservaDto> Crear(CrearReservaDto dto)
    {
        var creada = _reservas.Crear(dto);
        return CreatedAtAction(nameof(Obtener), new { id = creada.Id }, creada);
    }

    /// <summary>Solicitud del huesped: queda Pendiente hasta que recepcion la confirma.</summary>
    [HttpPost("solicitudes")]
    public ActionResult<ReservaDto> Solicitar(CrearReservaDto dto)
    {
        var creada = _reservas.Solicitar(dto);
        return CreatedAtAction(nameof(Obtener), new { id = creada.Id }, creada);
    }

    [HttpPut("{id:int}")]
    public ActionResult<ReservaDto> Modificar(int id, ModificarReservaDto dto)
    {
        dto.ReservaId = id;
        return Ok(_reservas.Modificar(dto));
    }

    [HttpPost("{id:int}/confirmar")]
    public IActionResult Confirmar(int id) { _reservas.Confirmar(id); return NoContent(); }

    [HttpPost("{id:int}/cancelar")]
    public IActionResult Cancelar(int id) { _reservas.Cancelar(id); return NoContent(); }

    /// <summary>Check-out.</summary>
    [HttpPost("{id:int}/finalizar")]
    public IActionResult Finalizar(int id) { _reservas.Finalizar(id); return NoContent(); }

    [HttpGet("{id:int}/historial")]
    public ActionResult<IEnumerable<HistorialReservaDto>> Historial(int id) => Ok(_historial.ConsultarPorReserva(id));

    [HttpGet("{id:int}/notificaciones")]
    public ActionResult<IEnumerable<NotificacionDto>> Notificaciones(int id) => Ok(_notificaciones.ListarPorReserva(id));
}
