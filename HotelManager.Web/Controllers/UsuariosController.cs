using System;
using System.Linq;
using System.Web.Mvc;
using HotelManager.Business.Dtos;
using HotelManager.Business.Exceptions;
using HotelManager.Business.Interfaces;
using HotelManager.Business.Seguridad;
using HotelManager.Domain.Enums;
using HotelManager.Web.Helpers;
using HotelManager.Web.Infrastructure;
using HotelManager.Web.Models;

namespace HotelManager.Web.Controllers
{
    /// <summary>RNF-001: el Administrador gestiona las cuentas y los roles de los actores.</summary>
    [Autorizar(Roles.Administrador)]
    public class UsuariosController : BaseController
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IHuespedService _huespedService;

        public UsuariosController(IUsuarioService usuarioService, IHuespedService huespedService)
        {
            _usuarioService = usuarioService;
            _huespedService = huespedService;
        }

        private int IdUsuarioActual
        {
            get { return ((UsuarioIdentidad)User.Identity).IdUsuario; }
        }

        public ActionResult Index(RolUsuario? rol)
        {
            var todos = _usuarioService.Listar().ToList();
            ViewBag.Conteos = todos.GroupBy(u => u.Rol).ToDictionary(g => g.Key, g => g.Count());
            ViewBag.Rol = rol;
            ViewBag.IdUsuarioActual = IdUsuarioActual;
            ViewBag.Huespedes = _huespedService.Listar().ToDictionary(h => h.IdHuesped, h => TextoHelper.Titulo(h.Nombre));
            return View(todos.Where(u => !rol.HasValue || u.Rol == rol.Value).ToList());
        }

        public ActionResult Crear()
        {
            CargarListas(null);
            return View(new UsuarioViewModel { Rol = RolUsuario.Recepcionista });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Crear(UsuarioViewModel vm)
        {
            if (string.IsNullOrEmpty(vm.ClaveTemporal))
                ModelState.AddModelError("ClaveTemporal", "Indique una contraseña temporal.");

            if (ModelState.IsValid && Ejecutar(() => _usuarioService.Crear(ToDto(vm), vm.ClaveTemporal)))
            {
                Exito("Usuario " + vm.NombreUsuario.Trim().ToLower() + " creado. Deberá cambiar la contraseña en su primer ingreso.");
                return RedirectToAction("Index");
            }
            vm.ClaveTemporal = null;
            CargarListas(null);
            return View(vm);
        }

        public ActionResult Editar(int id)
        {
            try
            {
                var u = _usuarioService.ObtenerPorId(id);
                CargarListas(u.IdUsuario);
                ViewBag.EsUsuarioActual = u.IdUsuario == IdUsuarioActual;
                return View(new UsuarioViewModel
                {
                    IdUsuario = u.IdUsuario,
                    NombreUsuario = u.NombreUsuario,
                    Nombre = u.Nombre,
                    Correo = u.Correo,
                    Rol = u.Rol,
                    IdHuesped = u.IdHuesped,
                    Activo = u.Activo
                });
            }
            catch (BusinessException ex) { Error(ex.Message); return RedirectToAction("Index"); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Editar(UsuarioViewModel vm)
        {
            ModelState.Remove("ClaveTemporal");
            if (ModelState.IsValid && Ejecutar(() => _usuarioService.Actualizar(ToDto(vm), IdUsuarioActual)))
            {
                Exito("Usuario actualizado.");
                return RedirectToAction("Index");
            }
            CargarListas(vm.IdUsuario);
            ViewBag.EsUsuarioActual = vm.IdUsuario == IdUsuarioActual;
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CambiarEstado(int id)
        {
            EjecutarConMensaje(() =>
            {
                var u = _usuarioService.ObtenerPorId(id);
                if (u.Activo) _usuarioService.Desactivar(id, IdUsuarioActual);
                else _usuarioService.Activar(id);
            }, "Estado del usuario actualizado.");
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult RestablecerClave(int id, string claveTemporal)
        {
            EjecutarConMensaje(() => _usuarioService.RestablecerClave(id, claveTemporal),
                "Contraseña restablecida. El usuario deberá cambiarla al iniciar sesión.");
            return RedirectToAction("Editar", new { id });
        }

        private void CargarListas(int? idUsuario)
        {
            ViewBag.Roles = Enum.GetValues(typeof(RolUsuario)).Cast<RolUsuario>()
                .Select(r => new SelectListItem { Value = r.ToString(), Text = Roles.Descripcion(r) }).ToList();

            // Huespedes sin cuenta (o el ya vinculado a este usuario)
            var vinculados = _usuarioService.Listar()
                .Where(u => u.IdHuesped.HasValue && u.IdUsuario != idUsuario)
                .Select(u => u.IdHuesped.Value).ToList();
            ViewBag.Huespedes = _huespedService.Listar()
                .Where(h => !vinculados.Contains(h.IdHuesped))
                .OrderBy(h => h.Nombre)
                .Select(h => new SelectListItem { Value = h.IdHuesped.ToString(), Text = TextoHelper.Titulo(h.Nombre) + " — " + h.Documento })
                .ToList();
        }

        private static UsuarioDto ToDto(UsuarioViewModel vm)
        {
            return new UsuarioDto
            {
                IdUsuario = vm.IdUsuario,
                NombreUsuario = vm.NombreUsuario,
                Nombre = vm.Nombre,
                Correo = vm.Correo,
                Rol = vm.Rol ?? RolUsuario.Recepcionista,
                IdHuesped = vm.Rol == RolUsuario.Huesped ? vm.IdHuesped : null
            };
        }
    }
}
