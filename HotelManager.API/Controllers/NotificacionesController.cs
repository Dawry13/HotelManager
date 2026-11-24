using HotelManager.API.DTOs;
using HotelManager.Business.Dtos;
using HotelManager.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelManager.API.Controllers;

[ApiController]
[Route("api/notificaciones")]
public class NotificacionesController : ControllerBase
{
    private readonly INotificacionService _notificaciones;

    public NotificacionesController(INotificacionService notificaciones) => _notificaciones = notificaciones;

    [HttpGet]
    public ActionResult<IEnumerable<NotificacionDto>> Listar() => Ok(_notificaciones.Listar());

    [HttpGet("pendientes/cantidad")]
    public ActionResult<object> ContarPendientes() => Ok(new { pendientes = _notificaciones.ContarPendientes() });

    /// <summary>Como esta configurado el envio de correos (SMTP o carpeta de prueba).</summary>
    [HttpGet("configuracion")]
    public ActionResult<object> Configuracion()
        => Ok(new { descripcion = _notificaciones.DescripcionEnvio, modoPrueba = _notificaciones.ModoPrueba });

    /// <summary>Genera la notificacion de una reserva y envia el correo al huesped.</summary>
    [HttpPost]
    public ActionResult<object> Notificar(NotificarRequest request)
        => Ok(new { idNotificacion = _notificaciones.Notificar(request.IdReserva, request.Tipo) });

    [HttpPost("{id:int}/enviar")]
    public ActionResult<object> Enviar(int id) => Ok(new { enviada = _notificaciones.Enviar(id) });

    [HttpPost("{id:int}/reintentar")]
    public IActionResult Reintentar(int id) { _notificaciones.Reintentar(id); return NoContent(); }

    [HttpPost("pendientes/enviar")]
    public ActionResult<object> EnviarPendientes() => Ok(new { enviadas = _notificaciones.EnviarPendientes() });

    [HttpPut("{id:int}/resultado")]
    public IActionResult RegistrarResultado(int id, RegistrarResultadoNotificacionRequest request)
    {
        _notificaciones.RegistrarResultado(id, request.Enviada, request.DetalleError);
        return NoContent();
    }
}
