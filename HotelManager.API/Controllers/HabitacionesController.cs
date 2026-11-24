using HotelManager.API.DTOs;
using HotelManager.Business.Dtos;
using HotelManager.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelManager.API.Controllers;

[ApiController]
[Route("api/habitaciones")]
public class HabitacionesController : ControllerBase
{
    private readonly IHabitacionService _habitaciones;
    private readonly ITarifaService _tarifas;

    public HabitacionesController(IHabitacionService habitaciones, ITarifaService tarifas)
    {
        _habitaciones = habitaciones;
        _tarifas = tarifas;
    }

    [HttpGet]
    public ActionResult<IEnumerable<HabitacionDto>> Listar() => Ok(_habitaciones.Listar());

    [HttpGet("{id:int}")]
    public ActionResult<HabitacionDto> Obtener(int id) => Ok(_habitaciones.ObtenerPorId(id));

    [HttpPost]
    public ActionResult<HabitacionDto> Crear(HabitacionDto dto)
    {
        var creada = _habitaciones.Crear(dto);
        return CreatedAtAction(nameof(Obtener), new { id = creada.IdHabitacion }, creada);
    }

    [HttpPut("{id:int}")]
    public ActionResult<HabitacionDto> Actualizar(int id, HabitacionDto dto)
    {
        dto.IdHabitacion = id;
        return Ok(_habitaciones.Actualizar(dto));
    }

    [HttpPut("{id:int}/estado")]
    public IActionResult CambiarEstado(int id, CambiarEstadoHabitacionRequest request)
    {
        _habitaciones.CambiarEstado(id, request.Estado);
        return NoContent();
    }

    [HttpPut("{id:int}/tarifa")]
    public IActionResult ActualizarTarifa(int id, TarifaRequest request)
    {
        _habitaciones.ActualizarTarifa(id, request.Tarifa);
        return NoContent();
    }

    [HttpGet("{id:int}/tarifas")]
    public ActionResult<IEnumerable<TarifaDto>> Tarifas(int id) => Ok(_tarifas.ListarPorHabitacion(id));

    /// <summary>Tarifa por noche aplicable en una fecha.</summary>
    [HttpGet("{id:int}/tarifa-aplicable")]
    public ActionResult<object> TarifaAplicable(int id, [FromQuery] DateTime fecha)
        => Ok(new { idHabitacion = id, fecha = fecha.Date, tarifa = _tarifas.ObtenerTarifaAplicable(id, fecha) });

    /// <summary>Costo total de una estadia en la habitacion.</summary>
    [HttpGet("{id:int}/costo")]
    public ActionResult<object> Costo(int id, [FromQuery] DateTime entrada, [FromQuery] DateTime salida)
        => Ok(new { idHabitacion = id, entrada = entrada.Date, salida = salida.Date, costoTotal = _tarifas.CalcularCostoTotal(id, entrada, salida) });
}
