using HotelManager.Business.Dtos;
using HotelManager.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelManager.API.Controllers;

[ApiController]
[Route("api/pisos")]
public class PisosController : ControllerBase
{
    private readonly IPisoService _servicio;

    public PisosController(IPisoService servicio) => _servicio = servicio;

    [HttpGet]
    public ActionResult<IEnumerable<PisoDto>> Listar() => Ok(_servicio.Listar());

    [HttpGet("{id:int}")]
    public ActionResult<PisoDto> Obtener(int id) => Ok(_servicio.ObtenerPorId(id));

    [HttpPost]
    public ActionResult<PisoDto> Crear(PisoDto dto)
    {
        var creado = _servicio.Crear(dto);
        return CreatedAtAction(nameof(Obtener), new { id = creado.IdPiso }, creado);
    }

    [HttpPut("{id:int}")]
    public ActionResult<PisoDto> Actualizar(int id, PisoDto dto)
    {
        dto.IdPiso = id;
        return Ok(_servicio.Actualizar(dto));
    }

    [HttpPost("{id:int}/activar")]
    public IActionResult Activar(int id) { _servicio.Activar(id); return NoContent(); }

    [HttpPost("{id:int}/desactivar")]
    public IActionResult Desactivar(int id) { _servicio.Desactivar(id); return NoContent(); }

    [HttpDelete("{id:int}")]
    public IActionResult Eliminar(int id) { _servicio.Eliminar(id); return NoContent(); }
}
