using System;
using System.Collections.Generic;
using System.Linq;
using HotelManager.Business.Dtos;
using HotelManager.Business.Exceptions;
using HotelManager.Business.Interfaces;
using HotelManager.Data.Interfaces;
using HotelManager.Domain.Entities;
using HotelManager.Domain.Enums;

namespace HotelManager.Business.Services
{
    public class ReporteService : IReporteService
    {
        private readonly IReservaRepository _reservas;
        private readonly IHabitacionRepository _habitaciones;
        private readonly IPisoRepository _pisos;
        private readonly IHotelRepository _hoteles;
        private readonly ITipoHabitacionRepository _tipos;
        private readonly IHuespedRepository _huespedes;

        public ReporteService(IReservaRepository reservas, IHabitacionRepository habitaciones, IPisoRepository pisos,
                              IHotelRepository hoteles, ITipoHabitacionRepository tipos, IHuespedRepository huespedes)
        {
            _reservas = reservas;
            _habitaciones = habitaciones;
            _pisos = pisos;
            _hoteles = hoteles;
            _tipos = tipos;
            _huespedes = huespedes;
        }

        public ReporteGeneralDto Generar(DateTime desde, DateTime hasta, int? idHotel = null)
        {
            desde = desde.Date;
            hasta = hasta.Date;
            if (hasta < desde)
                throw new BusinessException("La fecha final no puede ser anterior a la inicial.");
            if ((hasta - desde).TotalDays > 731)
                throw new BusinessException("El rango del reporte no puede superar dos años.");

            var dias = (hasta - desde).Days + 1;
            var hotelDePiso = _pisos.GetAll().ToDictionary(p => p.IdPiso, p => p.IdHotel);
            var hoteles = _hoteles.GetAll().Where(h => !idHotel.HasValue || h.IdHotel == idHotel.Value).ToList();
            var idsHotel = new HashSet<int>(hoteles.Select(h => h.IdHotel));

            var habitaciones = _habitaciones.GetAll()
                .Where(h => hotelDePiso.ContainsKey(h.IdPiso) && idsHotel.Contains(hotelDePiso[h.IdPiso]))
                .ToDictionary(h => h.IdHabitacion);
            Func<int, int> hotelDeHabitacion = idHab => hotelDePiso[habitaciones[idHab].IdPiso];

            var tipos = _tipos.GetAll().ToDictionary(t => t.IdTipoHabitacion, t => t.Nombre);
            var huespedes = _huespedes.GetAll().ToDictionary(h => h.IdHuesped, h => h.Nombre);

            var todas = _reservas.GetAll().Where(r => habitaciones.ContainsKey(r.IdHabitacion)).ToList();

            // Reservas del periodo (por fecha de entrada) y las que generan ingreso (no canceladas)
            var delPeriodo = todas.Where(r => r.Entrada.Date >= desde && r.Entrada.Date <= hasta).ToList();
            var validas = delPeriodo.Where(r => r.Estado != EstadoReserva.Cancelada).ToList();

            // Ocupacion: noches ocupadas dentro del rango por reservas no canceladas
            Func<Reserva, int> nochesEnRango = r =>
            {
                var ini = r.Entrada.Date > desde ? r.Entrada.Date : desde;
                var fin = r.Salida.Date.AddDays(-1) < hasta ? r.Salida.Date.AddDays(-1) : hasta;
                return fin >= ini ? (fin - ini).Days + 1 : 0;
            };
            var ocupantes = todas.Where(r => r.Estado != EstadoReserva.Cancelada).ToList();

            var reporte = new ReporteGeneralDto
            {
                Desde = desde,
                Hasta = hasta,
                TotalReservas = delPeriodo.Count,
                ReservasCanceladas = delPeriodo.Count(r => r.Estado == EstadoReserva.Cancelada),
                Ingresos = validas.Sum(r => r.CostoTotal),
                IngresoPromedioReserva = validas.Count == 0 ? 0 : Math.Round(validas.Average(r => r.CostoTotal), 2),
                NochesVendidas = ocupantes.Sum(nochesEnRango),
                HuespedesAtendidos = validas.Select(r => r.IdHuesped).Distinct().Count()
            };

            foreach (var hotel in hoteles.OrderBy(h => h.Nombre))
            {
                var habsHotel = habitaciones.Values.Where(h => hotelDePiso[h.IdPiso] == hotel.IdHotel).ToList();
                var operativas = habsHotel.Count(h => h.Estado != EstadoHabitacion.Inactiva);
                var ocupadas = ocupantes.Where(r => hotelDeHabitacion(r.IdHabitacion) == hotel.IdHotel).Sum(nochesEnRango);
                var disponibles = operativas * dias;
                var reservasHotel = validas.Where(r => hotelDeHabitacion(r.IdHabitacion) == hotel.IdHotel).ToList();

                reporte.PorHotel.Add(new OcupacionHotelDto
                {
                    IdHotel = hotel.IdHotel,
                    Hotel = hotel.Nombre,
                    Habitaciones = habsHotel.Count,
                    NochesDisponibles = disponibles,
                    NochesOcupadas = ocupadas,
                    Ocupacion = disponibles == 0 ? 0 : Math.Round(Math.Min(100m, ocupadas * 100m / disponibles), 1),
                    Reservas = reservasHotel.Count,
                    Ingresos = reservasHotel.Sum(r => r.CostoTotal)
                });
            }

            var totalDisponibles = reporte.PorHotel.Sum(h => h.NochesDisponibles);
            reporte.OcupacionPromedio = totalDisponibles == 0
                ? 0 : Math.Round(Math.Min(100m, reporte.NochesVendidas * 100m / totalDisponibles), 1);

            reporte.PorEstado = Enum.GetValues(typeof(EstadoReserva)).Cast<EstadoReserva>()
                .Select(e => new ConteoDto
                {
                    Nombre = e.ToString(),
                    Cantidad = delPeriodo.Count(r => r.Estado == e),
                    Monto = delPeriodo.Where(r => r.Estado == e).Sum(r => r.CostoTotal)
                })
                .ToList();

            reporte.PorTipoHabitacion = validas
                .GroupBy(r =>
                {
                    var idTipo = habitaciones[r.IdHabitacion].IdTipoHabitacion;
                    return idTipo.HasValue && tipos.ContainsKey(idTipo.Value) ? tipos[idTipo.Value] : "sin tipo";
                })
                .Select(g => new ConteoDto { Nombre = g.Key, Cantidad = g.Count(), Monto = g.Sum(r => r.CostoTotal) })
                .OrderByDescending(c => c.Monto)
                .ToList();

            reporte.PorMes = validas
                .GroupBy(r => new { r.Entrada.Year, r.Entrada.Month })
                .Select(g => new IngresoMesDto { Anio = g.Key.Year, Mes = g.Key.Month, Reservas = g.Count(), Ingresos = g.Sum(r => r.CostoTotal) })
                .OrderBy(m => m.Anio).ThenBy(m => m.Mes)
                .ToList();

            reporte.HuespedesFrecuentes = validas
                .GroupBy(r => r.IdHuesped)
                .Select(g => new HuespedFrecuenteDto
                {
                    IdHuesped = g.Key,
                    Nombre = huespedes.ContainsKey(g.Key) ? huespedes[g.Key] : "Huésped #" + g.Key,
                    Reservas = g.Count(),
                    Noches = g.Sum(r => (r.Salida.Date - r.Entrada.Date).Days),
                    Total = g.Sum(r => r.CostoTotal)
                })
                .OrderByDescending(h => h.Reservas).ThenByDescending(h => h.Total)
                .Take(5)
                .ToList();

            return reporte;
        }
    }
}
