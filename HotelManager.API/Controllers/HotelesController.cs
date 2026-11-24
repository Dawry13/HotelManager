using HotelManager.Business.Dtos;
using HotelManager.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelManager.API.Controllers;

[ApiController]
[Route("api/hoteles")]
public class HotelesController : ControllerBase
{
    private readonly IHotelService _hoteles;
    private readonly IPisoService _pisos;
    private readonly IBloqueService _bloques;
    private readonly IHabitacionService _habitaciones;

    public HotelesController(IHotelService hoteles, IPisoService pisos, IBloqueService bloques, IHabitacionService habitaciones)
    {
        _hoteles = hoteles;
        _pisos = pisos;
        _bloques = bloques;
        _habitaciones = habitaciones;
    }

    [HttpGet]
    public ActionResult<IEnumerable<HotelDto>> Listar() => Ok(_hoteles.Listar());

    [HttpGet("{id:int}")]
    public ActionResult<HotelDto> Obtener(int id) => Ok(_hoteles.ObtenerPorId(id));

    [HttpPost]
    public ActionResult<HotelDto> Crear(HotelDto dto)
    {
        var creado = _hoteles.Crear(dto);
        return CreatedAtAction(nameof(Obtener), new { id = creado.IdHotel }, creado);
    }

    [HttpPut("{id:int}")]
    public ActionResult<HotelDto> Actualizar(int id, HotelDto dto)
    {
        dto.IdHotel = id;
        return Ok(_hoteles.Actualizar(dto));
    }

    [HttpPost("{id:int}/activar")]
    public IActionResult Activar(int id) { _hoteles.Activar(id); return NoContent(); }

    [HttpPost("{id:int}/desactivar")]
    public IActionResult Desactivar(int id) { _hoteles.Desactivar(id); return NoContent(); }

    [HttpGet("{id:int}/pisos")]
    public ActionResult<IEnumerable<PisoDto>> Pisos(int id) => Ok(_pisos.ListarPorHotel(id));

    [HttpGet("{id:int}/bloques")]
    public ActionResult<IEnumerable<BloqueDto>> Bloques(int id) => Ok(_bloques.ListarPorHotel(id));

    [HttpGet("{id:int}/habitaciones")]
    public ActionResult<IEnumerable<HabitacionDto>> Habitaciones(int id) => Ok(_habitaciones.ListarPorHotel(id));

    /// <summary>Aplica la misma tarifa base a todas las habitaciones del hotel.</summary>
    [HttpPut("{id:int}/tarifa")]
    public ActionResult<object> ActualizarTarifa(int id, DTOs.TarifaRequest request)
        => Ok(new { habitacionesActualizadas = _habitaciones.ActualizarTarifaHotel(id, request.Tarifa) });
}
