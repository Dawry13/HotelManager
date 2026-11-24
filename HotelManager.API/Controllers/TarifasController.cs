using HotelManager.Business.Dtos;
using HotelManager.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelManager.API.Controllers;

[ApiController]
[Route("api/tarifas")]
public class TarifasController : ControllerBase
{
    private readonly ITarifaService _tarifas;

    public TarifasController(ITarifaService tarifas) => _tarifas = tarifas;

    [HttpGet]
    public ActionResult<IEnumerable<TarifaDto>> Listar() => Ok(_tarifas.Listar());

    [HttpGet("{id:int}")]
    public ActionResult<TarifaDto> Obtener(int id) => Ok(_tarifas.ObtenerPorId(id));

    [HttpPost]
    public ActionResult<TarifaDto> Crear(TarifaDto dto)
    {
        var creada = _tarifas.Crear(dto);
        return CreatedAtAction(nameof(Obtener), new { id = creada.IdTarifa }, creada);
    }

    [HttpPut("{id:int}")]
    public ActionResult<TarifaDto> Actualizar(int id, TarifaDto dto)
    {
        dto.IdTarifa = id;
        return Ok(_tarifas.Actualizar(dto));
    }

    [HttpDelete("{id:int}")]
    public IActionResult Eliminar(int id) { _tarifas.Eliminar(id); return NoContent(); }
}
