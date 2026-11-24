using HotelManager.API.DTOs;
using HotelManager.Business.Dtos;
using HotelManager.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelManager.API.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarios;

    public UsuariosController(IUsuarioService usuarios) => _usuarios = usuarios;

    /// <summary>Valida usuario y clave. (Pendiente: emitir token JWT.)</summary>
    [HttpPost("autenticar")]
    public ActionResult<UsuarioDto> Autenticar(LoginRequest request)
        => Ok(_usuarios.Autenticar(request.NombreUsuario, request.Clave));

    /// <summary>Un huesped crea su propia cuenta.</summary>
    [HttpPost("registro-huesped")]
    public ActionResult<UsuarioDto> RegistrarHuesped(RegistrarHuespedRequest request)
    {
        var creado = _usuarios.RegistrarHuesped(request.Huesped, request.NombreUsuario, request.Clave);
        return CreatedAtAction(nameof(Obtener), new { id = creado.IdUsuario }, creado);
    }

    [HttpGet]
    public ActionResult<IEnumerable<UsuarioDto>> Listar() => Ok(_usuarios.Listar());

    [HttpGet("{id:int}")]
    public ActionResult<UsuarioDto> Obtener(int id) => Ok(_usuarios.ObtenerPorId(id));

    [HttpPost]
    public ActionResult<UsuarioDto> Crear(CrearUsuarioRequest request)
    {
        var creado = _usuarios.Crear(request.Usuario, request.ClaveInicial);
        return CreatedAtAction(nameof(Obtener), new { id = creado.IdUsuario }, creado);
    }

    [HttpPut("{id:int}")]
    public ActionResult<UsuarioDto> Actualizar(int id, ActualizarUsuarioRequest request)
    {
        request.Usuario.IdUsuario = id;
        return Ok(_usuarios.Actualizar(request.Usuario, request.IdUsuarioActual));
    }

    [HttpPost("{id:int}/activar")]
    public IActionResult Activar(int id) { _usuarios.Activar(id); return NoContent(); }

    [HttpPost("{id:int}/desactivar")]
    public IActionResult Desactivar(int id, [FromQuery] int idUsuarioActual)
    {
        _usuarios.Desactivar(id, idUsuarioActual);
        return NoContent();
    }

    [HttpPost("{id:int}/restablecer-clave")]
    public IActionResult RestablecerClave(int id, ClaveTemporalRequest request)
    {
        _usuarios.RestablecerClave(id, request.ClaveTemporal);
        return NoContent();
    }

    [HttpPost("{id:int}/cambiar-clave")]
    public IActionResult CambiarClave(int id, CambiarClaveRequest request)
    {
        _usuarios.CambiarClave(id, request.ClaveActual, request.ClaveNueva);
        return NoContent();
    }
}
