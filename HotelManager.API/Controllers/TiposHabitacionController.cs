using HotelManager.Business.Dtos;
using HotelManager.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelManager.API.Controllers;

[ApiController]
[Route("api/tipos-habitacion")]
public class TiposHabitacionController : ControllerBase
{
    private readonly ITipoHabitacionService _servicio;

    public TiposHabitacionController(ITipoHabitacionService servicio) => _servicio = servicio;

    [HttpGet]
    public ActionResult<IEnumerable<TipoHabitacionDto>> Listar() => Ok(_servicio.Listar());

    [HttpGet("{id:int}")]
    public ActionResult<TipoHabitacionDto> Obtener(int id) => Ok(_servicio.ObtenerPorId(id));

    [HttpPost]
    public ActionResult<TipoHabitacionDto> Crear(TipoHabitacionDto dto)
    {
        var creado = _servicio.Crear(dto);
        return CreatedAtAction(nameof(Obtener), new { id = creado.IdTipoHabitacion }, creado);
    }

    [HttpPut("{id:int}")]
    public ActionResult<TipoHabitacionDto> Actualizar(int id, TipoHabitacionDto dto)
    {
        dto.IdTipoHabitacion = id;
        return Ok(_servicio.Actualizar(dto));
    }

    [HttpPost("{id:int}/activar")]
    public IActionResult Activar(int id) { _servicio.Activar(id); return NoContent(); }

    [HttpPost("{id:int}/desactivar")]
    public IActionResult Desactivar(int id) { _servicio.Desactivar(id); return NoContent(); }

    [HttpDelete("{id:int}")]
    public IActionResult Eliminar(int id) { _servicio.Eliminar(id); return NoContent(); }
}
