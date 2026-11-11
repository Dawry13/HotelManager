using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace HotelManager.Web.Infrastructure
{
    /// <summary>
    /// RNF-001: solo usuarios autenticados y con el rol indicado pueden ejecutar la accion.
    ///  - Registrado como filtro global: exige iniciar sesion en toda la aplicacion (salvo [AllowAnonymous]).
    ///  - En controladores/acciones: [Autorizar(Roles.Administrador, Roles.Recepcionista)].
    /// Sin sesion -> pantalla de inicio de sesion. Con sesion pero sin permiso -> "Acceso denegado".
    /// Si el usuario debe cambiar su clave, se le lleva a esa pantalla antes de continuar.
    /// </summary>
    public class AutorizarAttribute : AuthorizeAttribute
    {
        public AutorizarAttribute(params string[] roles)
        {
            Roles = string.Join(",", roles);
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            if (filterContext.Result != null) return;

            // Obligar a cambiar la clave temporal antes de usar el sistema.
            var identidad = filterContext.HttpContext.User.Identity as UsuarioIdentidad;
            if (identidad != null && identidad.DebeCambiarClave)
            {
                var controlador = (string)filterContext.RouteData.Values["controller"];
                var accion = (string)filterContext.RouteData.Values["action"];
                var permitido = controlador == "Cuenta" && (accion == "CambiarClave" || accion == "Salir");
                if (!permitido)
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Cuenta", action = "CambiarClave" }));
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var contexto = filterContext.HttpContext;

            if (contexto.Request.IsAjaxRequest() || EsJson(contexto.Request))
            {
                filterContext.Result = new HttpStatusCodeResult(contexto.User.Identity.IsAuthenticated ? 403 : 401);
                return;
            }

            if (!contexto.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    controller = "Cuenta",
                    action = "Login",
                    returnUrl = contexto.Request.RawUrl
                }));
            }
            else
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Cuenta", action = "AccesoDenegado" }));
            }
        }

        private static bool EsJson(HttpRequestBase request)
        {
            return request.AcceptTypes != null && System.Array.IndexOf(request.AcceptTypes, "application/json") >= 0;
        }
    }
}
