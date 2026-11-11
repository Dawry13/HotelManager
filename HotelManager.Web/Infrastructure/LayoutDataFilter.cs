using System.Linq;
using System.Web.Mvc;
using HotelManager.Business.Interfaces;
using Roles = HotelManager.Business.Seguridad.Roles;

namespace HotelManager.Web.Infrastructure
{
    /// <summary>
    /// Carga los datos comunes del layout (usuario conectado y notificaciones pendientes)
    /// antes de mostrar cualquier vista.
    /// </summary>
    public class LayoutDataFilter : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (!(filterContext.Result is ViewResult)) return;

            var viewBag = filterContext.Controller.ViewBag;
            var usuario = filterContext.HttpContext.User;
            var identidad = usuario != null ? usuario.Identity as UsuarioIdentidad : null;

            var nombre = identidad != null ? Helpers.TextoHelper.Titulo(identidad.Nombre) : "Invitado";
            viewBag.UsuarioNombre = nombre;
            viewBag.UsuarioRol = identidad != null ? DescripcionRol(identidad.Rol) : "";
            viewBag.UsuarioIniciales = new string(nombre.Split(' ')
                .Where(p => p.Length > 0).Take(2).Select(p => char.ToUpper(p[0])).ToArray());

            viewBag.NotificacionesPendientes = 0;
            var veNotificaciones = usuario != null && (usuario.IsInRole(Roles.Administrador)
                || usuario.IsInRole(Roles.Recepcionista) || usuario.IsInRole(Roles.ServicioNotificacion));
            if (!veNotificaciones) return;

            try
            {
                var notificaciones = DependencyResolver.Current.GetService<INotificacionService>();
                viewBag.NotificacionesPendientes = notificaciones != null ? notificaciones.ContarPendientes() : 0;
            }
            catch
            {
                // Si la base de datos no responde, el layout se muestra sin el contador.
            }
        }

        private static string DescripcionRol(string rol)
        {
            switch (rol)
            {
                case Roles.Administrador: return "Administrador";
                case Roles.Recepcionista: return "Recepcionista";
                case Roles.Huesped: return "Huésped";
                case Roles.ServicioNotificacion: return "Servicio de notificaciones";
                default: return rol;
            }
        }
    }
}
