using System;
using System.Web;
using HotelManager.Web.Infrastructure;

namespace HotelManager.Web
{
    /// <summary>
    /// IIS llama a Application_Start al arrancar; toda la configuracion esta en Program.cs.
    /// </summary>
    public class MvcApplication : HttpApplication
    {
        /// <summary>Mensaje del ultimo error al preparar la base de datos (null si todo fue bien).</summary>
        public static string ErrorBaseDatos { get { return Program.ErrorBaseDatos; } }

        protected void Application_Start()
        {
            Program.Iniciar();
        }

        // Carga el usuario de la cookie de sesion en cada peticion.
        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            Autenticacion.CargarUsuario(HttpContext.Current);
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            ServiceProviderDependencyResolver.DisposeRequestScope(HttpContext.Current);
        }
    }
}
