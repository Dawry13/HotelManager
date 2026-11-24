using HotelManager.Business.Dtos;
using HotelManager.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelManager.API.Controllers;

[ApiController]
[Route("api/bloques")]
public class BloquesController : ControllerBase
{
    private readonly IBloqueService _servicio;

    public BloquesController(IBloqueService servicio) => _servicio = servicio;

    [HttpGet]
    public ActionResult<IEnumerable<BloqueDto>> Listar() => Ok(_servicio.Listar());

    [HttpGet("{id:int}")]
    public ActionResult<BloqueDto> Obtener(int id) => Ok(_servicio.ObtenerPorId(id));

    [HttpPost]
    public ActionResult<BloqueDto> Crear(BloqueDto dto)
    {
        var creado = _servicio.Crear(dto);
        return CreatedAtAction(nameof(Obtener), new { id = creado.IdBloque }, creado);
    }

    [HttpPut("{id:int}")]
    public ActionResult<BloqueDto> Actualizar(int id, BloqueDto dto)
    {
        dto.IdBloque = id;
        return Ok(_servicio.Actualizar(dto));
    }

    [HttpPost("{id:int}/activar")]
    public IActionResult Activar(int id) { _servicio.Activar(id); return NoContent(); }

    [HttpPost("{id:int}/desactivar")]
    public IActionResult Desactivar(int id) { _servicio.Desactivar(id); return NoContent(); }

    [HttpDelete("{id:int}")]
    public IActionResult Eliminar(int id) { _servicio.Eliminar(id); return NoContent(); }
}
