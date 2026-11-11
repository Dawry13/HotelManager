using System.Web.Mvc;
using HotelManager.Business.Dtos;
using HotelManager.Business.Interfaces;
using HotelManager.Business.Seguridad;
using HotelManager.Web.Infrastructure;
using HotelManager.Web.Models;

namespace HotelManager.Web.Controllers
{
    /// <summary>RNF-001: inicio y cierre de sesion, registro de huespedes y cambio de contrasena.</summary>
    public class CuentaController : BaseController
    {
        private readonly IUsuarioService _usuarioService;

        public CuentaController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated) return RedirigirSegunRol(User.Identity as UsuarioIdentidad, returnUrl);
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            UsuarioDto usuario = null;
            if (!Ejecutar(() => usuario = _usuarioService.Autenticar(vm.Usuario, vm.Clave)))
            {
                vm.Clave = null;
                return View(vm);
            }

            Autenticacion.IniciarSesion(HttpContext, usuario);
            if (usuario.DebeCambiarClave)
                return RedirectToAction("CambiarClave");

            return RedirigirSegunRol(Roles.Nombre(usuario.Rol), vm.ReturnUrl);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Salir()
        {
            Autenticacion.CerrarSesion(HttpContext);
            Exito("Sesión cerrada.");
            return RedirectToAction("Login");
        }

        // ACT-003: el huesped crea su cuenta para solicitar reservas.
        [AllowAnonymous]
        public ActionResult Registro()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View(new RegistroHuespedViewModel());
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public ActionResult Registro(RegistroHuespedViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            UsuarioDto usuario = null;
            var huesped = new HuespedDto
            {
                Nombre = vm.Nombre,
                Documento = vm.Documento,
                Telefono = string.IsNullOrWhiteSpace(vm.Telefono) ? null : vm.Telefono,
                Correo = vm.Correo
            };
            if (!Ejecutar(() => usuario = _usuarioService.RegistrarHuesped(huesped, vm.Usuario, vm.Clave)))
            {
                vm.Clave = vm.ConfirmarClave = null;
                return View(vm);
            }

            Autenticacion.IniciarSesion(HttpContext, usuario);
            Exito("¡Bienvenido! Su cuenta fue creada. Ya puede solicitar reservas.");
            return RedirectToAction("Index", "Portal");
        }

        public ActionResult CambiarClave()
        {
            var id = (UsuarioIdentidad)User.Identity;
            return View(new CambiarClaveViewModel { Obligatorio = id.DebeCambiarClave });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CambiarClave(CambiarClaveViewModel vm)
        {
            var id = (UsuarioIdentidad)User.Identity;
            vm.Obligatorio = id.DebeCambiarClave;
            if (!ModelState.IsValid) return View(vm);

            if (!Ejecutar(() => _usuarioService.CambiarClave(id.IdUsuario, vm.ClaveActual, vm.ClaveNueva)))
                return View(new CambiarClaveViewModel { Obligatorio = vm.Obligatorio });

            Autenticacion.Renovar(HttpContext, id, false);
            Exito("Contraseña actualizada.");
            return RedirigirSegunRol(id, null);
        }

        [AllowAnonymous]
        public ActionResult AccesoDenegado()
        {
            Response.StatusCode = 403;
            Response.TrySkipIisCustomErrors = true;
            return View();
        }

        private ActionResult RedirigirSegunRol(UsuarioIdentidad id, string returnUrl)
        {
            return RedirigirSegunRol(id != null ? id.Rol : null, returnUrl);
        }

        private ActionResult RedirigirSegunRol(string rol, string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && !returnUrl.StartsWith("/Cuenta"))
                return Redirect(returnUrl);

            switch (rol)
            {
                case Roles.Huesped: return RedirectToAction("Index", "Portal");
                case Roles.ServicioNotificacion: return RedirectToAction("Index", "Notificaciones");
                default: return RedirectToAction("Index", "Home");
            }
        }
    }
}
