using HotelManager.Business.Dtos;
using HotelManager.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelManager.API.Controllers;

[ApiController]
[Route("api/huespedes")]
public class HuespedesController : ControllerBase
{
    private readonly IHuespedService _huespedes;
    private readonly IReservaService _reservas;

    public HuespedesController(IHuespedService huespedes, IReservaService reservas)
    {
        _huespedes = huespedes;
        _reservas = reservas;
    }

    [HttpGet]
    public ActionResult<IEnumerable<HuespedDto>> Listar() => Ok(_huespedes.Listar());

    [HttpGet("{id:int}")]
    public ActionResult<HuespedDto> Obtener(int id) => Ok(_huespedes.ObtenerPorId(id));

    [HttpGet("documento/{documento}")]
    public ActionResult<HuespedDto> BuscarPorDocumento(string documento)
    {
        var huesped = _huespedes.BuscarPorDocumento(documento);
        return huesped is null ? NotFound() : Ok(huesped);
    }

    [HttpPost]
    public ActionResult<HuespedDto> Registrar(HuespedDto dto)
    {
        var creado = _huespedes.Registrar(dto);
        return CreatedAtAction(nameof(Obtener), new { id = creado.IdHuesped }, creado);
    }

    [HttpPut("{id:int}")]
    public ActionResult<HuespedDto> Actualizar(int id, HuespedDto dto)
    {
        dto.IdHuesped = id;
        return Ok(_huespedes.Actualizar(dto));
    }

    [HttpGet("{id:int}/reservas")]
    public ActionResult<IEnumerable<ReservaDto>> Reservas(int id) => Ok(_reservas.ObtenerHistorialPorHuesped(id));
}
