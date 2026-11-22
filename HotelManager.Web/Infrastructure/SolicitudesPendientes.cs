using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HotelManager.Business.Interfaces;
using HotelManager.Web.Helpers;

namespace HotelManager.Web.Infrastructure
{
    /// <summary>Solicitud de reserva hecha por un huesped que espera aprobacion del personal.</summary>
    public class SolicitudPendiente
    {
        public int IdReserva { get; set; }
        public string Huesped { get; set; }
        public string Hotel { get; set; }
        public string Habitacion { get; set; }
        public DateTime Entrada { get; set; }
        public DateTime Salida { get; set; }
        public int Personas { get; set; }
        public decimal Costo { get; set; }
    }

    /// <summary>
    /// Avisos para Administrador y Recepcionista: las reservas en estado Pendiente son
    /// solicitudes de huespedes que deben aprobarse. Se muestran en la campana de la barra superior.
    /// </summary>
    public static class SolicitudesPendientes
    {
        /// <summary>Total de solicitudes y las mas recientes (hasta <paramref name="maximo"/>).</summary>
        public static Tuple<int, List<SolicitudPendiente>> Cargar(int maximo)
        {
            var reservas = DependencyResolver.Current.GetService<IReservaService>();
            var pendientes = reservas.Listar()
                .Where(r => string.Equals(r.Estado, "Pendiente", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(r => r.Id)
                .ToList();
            if (pendientes.Count == 0) return Tuple.Create(0, new List<SolicitudPendiente>());

            var ctx = ContextoHotel.Cargar(
                DependencyResolver.Current.GetService<IHotelService>(),
                DependencyResolver.Current.GetService<IBloqueService>(),
                DependencyResolver.Current.GetService<IPisoService>(),
                DependencyResolver.Current.GetService<IHabitacionService>(),
                DependencyResolver.Current.GetService<ITipoHabitacionService>(),
                DependencyResolver.Current.GetService<IHuespedService>());

            var lista = pendientes.Take(maximo).Select(r => new SolicitudPendiente
            {
                IdReserva = r.Id,
                Huesped = ctx.Huesped(r.HuespedId),
                Hotel = ctx.HotelDeHabitacion(r.HabitacionId),
                Habitacion = ctx.NumeroHabitacion(r.HabitacionId),
                Entrada = r.FechaEntrada,
                Salida = r.FechaSalida,
                Personas = r.CantidadHuespedes,
                Costo = r.CostoTotal
            }).ToList();

            return Tuple.Create(pendientes.Count, lista);
        }

        /// <summary>Solo el total y el id mas reciente (para la consulta periodica de la campana).</summary>
        public static Tuple<int, int> Resumen()
        {
            var reservas = DependencyResolver.Current.GetService<IReservaService>();
            var ids = reservas.Listar()
                .Where(r => string.Equals(r.Estado, "Pendiente", StringComparison.OrdinalIgnoreCase))
                .Select(r => r.Id)
                .ToList();
            return Tuple.Create(ids.Count, ids.Count == 0 ? 0 : ids.Max());
        }
    }
}
