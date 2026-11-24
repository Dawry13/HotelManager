using HotelManager.Business.Dtos;
using HotelManager.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelManager.API.Controllers;

[ApiController]
[Route("api/disponibilidad")]
public class DisponibilidadController : ControllerBase
{
    private readonly IDisponibilidadService _disponibilidad;

    public DisponibilidadController(IDisponibilidadService disponibilidad) => _disponibilidad = disponibilidad;

    /// <summary>Habitaciones libres para las fechas y cantidad de huespedes indicadas.</summary>
    [HttpGet]
    public ActionResult<IEnumerable<HabitacionDisponibleDto>> Consultar([FromQuery] DisponibilidadRequestDto request)
        => Ok(_disponibilidad.Consultar(request));
}

[ApiController]
[Route("api/historial-reservas")]
public class HistorialReservasController : ControllerBase
{
    private readonly IHistorialReservaService _historial;

    public HistorialReservasController(IHistorialReservaService historial) => _historial = historial;

    [HttpGet]
    public ActionResult<IEnumerable<HistorialReservaDto>> Consultar([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        => Ok(_historial.Consultar(desde, hasta));
}

[ApiController]
[Route("api/reportes")]
public class ReportesController : ControllerBase
{
    private readonly IReporteService _reportes;

    public ReportesController(IReporteService reportes) => _reportes = reportes;

    /// <summary>Ocupacion, ingresos y reservas entre dos fechas (inclusive).</summary>
    [HttpGet("general")]
    public ActionResult<ReporteGeneralDto> General([FromQuery] DateTime desde, [FromQuery] DateTime hasta, [FromQuery] int? idHotel)
        => Ok(_reportes.Generar(desde, hasta, idHotel));
}
