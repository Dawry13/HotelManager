using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using HotelManager.Business;
using HotelManager.Web.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace HotelManager.Web
{
    public class MvcApplication : HttpApplication
    {
        /// <summary>Mensaje del ultimo error al preparar la base de datos (null si todo fue bien).</summary>
        public static string ErrorBaseDatos { get; private set; }

        protected void Application_Start()
        {
            // Crea o adjunta HotelManagerDB si hace falta (capa Data).
            try
            {
                foreach (var accion in DependencyInjection.InicializarBaseDatos())
                    System.Diagnostics.Trace.TraceInformation("HotelManagerDB: " + accion);
            }
            catch (Exception ex)
            {
                ErrorBaseDatos = ex.Message;
                System.Diagnostics.Trace.TraceError("No se pudo preparar HotelManagerDB: " + ex);
            }

            AreaRegistration.RegisterAllAreas();
            GlobalFilters.Filters.Add(new HandleErrorAttribute());
            GlobalFilters.Filters.Add(new LayoutDataFilter());
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Inyeccion de dependencias: repositorios + servicios de negocio + controladores.
            var services = new ServiceCollection();
            services.AddHotelManager();

            var controllers = typeof(MvcApplication).Assembly.GetTypes()
                .Where(t => typeof(IController).IsAssignableFrom(t) && !t.IsAbstract);
            foreach (var controller in controllers)
                services.AddTransient(controller);

            DependencyResolver.SetResolver(
                new ServiceProviderDependencyResolver(services.BuildServiceProvider()));
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            ServiceProviderDependencyResolver.DisposeRequestScope(HttpContext.Current);
        }
    }
}
