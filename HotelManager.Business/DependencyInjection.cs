using System;
using Microsoft.Extensions.DependencyInjection;

// Interfaces y repositorios (capa Data)
using HotelManager.Data.Base;
using HotelManager.Data.Interfaces;
using HotelManager.Data.Repositories;

// Interfaces y servicios (capa Business)
using HotelManager.Business.Interfaces;
using HotelManager.Business.Services;

namespace HotelManager.Business
{
    public static class DependencyInjection
    {
        /// <summary>
        /// Registra repositorios y servicios del sistema.
        /// </summary>
        public static IServiceCollection AddHotelManager(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddRepositories();
            services.AddBusinessServices();

            return services;
        }

        // ---------- Capa Data ----------
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

            return services;
        }

        // ---------- Capa Business ----------
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

            return services;
        }
    }
}