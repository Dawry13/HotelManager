using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using HotelManager.Business.Interfaces;

namespace HotelManager.Web.Infrastructure
{
    /// <summary>
    /// Carga los datos comunes del layout (usuario y notificaciones pendientes)
    /// antes de mostrar cualquier vista.
    /// </summary>
    public class LayoutDataFilter : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (!(filterContext.Result is ViewResult)) return;

            var viewBag = filterContext.Controller.ViewBag;
            var nombre = ConfigurationManager.AppSettings["UsuarioNombre"] ?? "Administrador";
            viewBag.UsuarioNombre = nombre;
            viewBag.UsuarioRol = ConfigurationManager.AppSettings["UsuarioRol"] ?? "Administrador";
            viewBag.UsuarioIniciales = new string(nombre.Split(' ')
                .Where(p => p.Length > 0).Take(2).Select(p => char.ToUpper(p[0])).ToArray());

            try
            {
                var notificaciones = DependencyResolver.Current.GetService<INotificacionService>();
                viewBag.NotificacionesPendientes = notificaciones != null ? notificaciones.ContarPendientes() : 0;
            }
            catch
            {
                // Si la base de datos no responde, el layout se muestra sin el contador.
                viewBag.NotificacionesPendientes = 0;
            }
        }
    }
}
