using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using HotelManager.Business;
using HotelManager.Business.Dtos;
using HotelManager.Business.Interfaces;
using HotelManager.Domain.Enums;
using HotelManager.Web.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace HotelManager.Web
{
    public static class Program
    {
        public const string ConexionPorDefecto =
            @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=HotelManagerDB;Integrated Security=True;Connect Timeout=60";

        public static readonly IReadOnlyList<CuentaInicialDto> CuentasIniciales = new List<CuentaInicialDto>
        {
            new CuentaInicialDto { Usuario = "admin",          Clave = "Admin2026",     Rol = RolUsuario.Administrador,        Nombre = "Administrador del sistema" },
            new CuentaInicialDto { Usuario = "recepcion",      Clave = "Recepcion2026", Rol = RolUsuario.Recepcionista,        Nombre = "Recepción" }
        };

        /// <summary>Mensaje del ultimo error al preparar la base de datos (null si todo fue bien).</summary>
        public static string ErrorBaseDatos { get; private set; }

        public static void Iniciar()
        {
            PrepararBaseDatos();

            AreaRegistration.RegisterAllAreas();
            GlobalFilters.Filters.Add(new HandleErrorAttribute());
            GlobalFilters.Filters.Add(new LayoutDataFilter());
            // Toda la gestion exige iniciar sesion; el catalogo publico usa [AllowAnonymous].
            GlobalFilters.Filters.Add(new AutorizarAttribute());
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            var proveedor = ConfigurarServicios();
            DependencyResolver.SetResolver(new ServiceProviderDependencyResolver(proveedor));

            if (ErrorBaseDatos == null)
                CrearCuentasIniciales(proveedor);
        }

        private static void PrepararBaseDatos()
        {
            try
            {
                foreach (var accion in DependencyInjection.InicializarBaseDatos())
                    Trace.TraceInformation("HotelManagerDB: " + accion);
            }
            catch (Exception ex)
            {
                ErrorBaseDatos = ex.Message;
                Trace.TraceError("No se pudo preparar HotelManagerDB: " + ex);
            }
        }

        /// <summary>Repositorios + servicios de negocio + controladores.</summary>
        private static IServiceProvider ConfigurarServicios()
        {
            var services = new ServiceCollection();
            services.AddHotelManager();

            var controladores = typeof(Program).Assembly.GetTypes()
                .Where(t => typeof(IController).IsAssignableFrom(t) && !t.IsAbstract);
            foreach (var controlador in controladores)
                services.AddTransient(controlador);

            return services.BuildServiceProvider();
        }

        private static void CrearCuentasIniciales(IServiceProvider proveedor)
        {
            try
            {
                using (var scope = proveedor.CreateScope())
                {
                    var usuarios = scope.ServiceProvider.GetRequiredService<IUsuarioService>();
                    foreach (var creado in usuarios.CrearUsuariosIniciales(CuentasIniciales))
                        Trace.TraceInformation("Usuario inicial creado: " + creado);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("No se pudieron crear los usuarios iniciales: " + ex);
            }
        }
    }
}
