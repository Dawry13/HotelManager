using System;
using Microsoft.Extensions.DependencyInjection;


using HotelManager.Data.Base;
using HotelManager.Data.Interfaces;
using HotelManager.Data.Repositories;


using HotelManager.Business.Interfaces;
using HotelManager.Business.Services;

namespace HotelManager.Business
{
    public static class DependencyInjection
    {
        
        public static IServiceCollection AddHotelManager(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddRepositories();
            services.AddBusinessServices();

            return services;
        }

        /// <summary>
        /// Verifica la base de datos HotelManagerDB al iniciar: la adjunta si sus archivos existen
        /// pero no esta registrada, o la crea con los scripts de HotelManager.Data/Database.
        /// Devuelve las acciones realizadas (vacio si ya estaba lista).
        /// </summary>
        public static System.Collections.Generic.IList<string> InicializarBaseDatos()
        {
            return InicializadorBaseDatos.Inicializar();
        }

       
        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IHotelRepository, HotelRepository>();
            services.AddScoped<IPisoRepository, PisoRepository>();
            services.AddScoped<IHabitacionRepository, HabitacionRepository>();
            services.AddScoped<IHuespedRepository, HuespedRepository>();
            services.AddScoped<IReservaRepository, ReservaRepository>();
            services.AddScoped<INotificacionRepository, NotificacionRepository>();
            services.AddScoped<ITarifaRepository, TarifaRepository>();
            services.AddScoped<IHistorialReservaRepository, HistorialReservaRepository>();
            services.AddScoped<IBloqueRepository, BloqueRepository>();
            services.AddScoped<ITipoHabitacionRepository, TipoHabitacionRepository>();

            return services;
        }

        
        private static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            services.AddScoped<IHotelService, HotelService>();
            services.AddScoped<IPisoService, PisoService>();
            services.AddScoped<IHabitacionService, HabitacionService>();
            services.AddScoped<IHuespedService, HuespedService>();
            services.AddScoped<IReservaService, ReservaService>();
            services.AddScoped<IDisponibilidadService, DisponibilidadService>();
            services.AddScoped<IHistorialReservaService, HistorialReservaService>();
            services.AddScoped<INotificacionService, NotificacionService>();
            services.AddScoped<ITarifaService, TarifaService>();
            services.AddScoped<IBloqueService, BloqueService>();
            services.AddScoped<ITipoHabitacionService, TipoHabitacionService>();
            services.AddScoped<IReporteService, ReporteService>();

            return services;
        }
    }
}