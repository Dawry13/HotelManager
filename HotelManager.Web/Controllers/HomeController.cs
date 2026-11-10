using System;
using System.Linq;
using System.Web.Mvc;
using HotelManager.Business.Interfaces;
using HotelManager.Domain.Enums;
using HotelManager.Web.Helpers;
using HotelManager.Web.Models;

namespace HotelManager.Web.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IHotelService _hotelService;
        private readonly IBloqueService _bloqueService;
        private readonly IPisoService _pisoService;
        private readonly IHabitacionService _habitacionService;
        private readonly ITipoHabitacionService _tipoService;
        private readonly IReservaService _reservaService;
        private readonly IHuespedService _huespedService;

        public HomeController(IHotelService hotelService, IBloqueService bloqueService, IPisoService pisoService,
                              IHabitacionService habitacionService, ITipoHabitacionService tipoService,
                              IReservaService reservaService, IHuespedService huespedService)
        {
            _hotelService = hotelService;
            _bloqueService = bloqueService;
            _pisoService = pisoService;
            _habitacionService = habitacionService;
            _tipoService = tipoService;
            _reservaService = reservaService;
            _huespedService = huespedService;
        }

        public ActionResult Index()
        {
            var hoy = DateTime.Today;
            var hoteles = _hotelService.Listar().ToList();
            var habitaciones = _habitacionService.Listar().ToList();
            var huespedes = _huespedService.Listar().ToDictionary(h => h.IdHuesped, h => h.Nombre);
            var reservas = _reservaService.ListarActivas().ToList();
            var pisos = _pisoService.Listar().ToDictionary(p => p.IdPiso, p => p.IdHotel);
            var nombresHotel = hoteles.ToDictionary(h => h.IdHotel, h => h.Nombre);

            // Habitaciones con una reserva activa que cubre el dia de hoy
            var ocupadasHoy = reservas
                .Where(r => r.FechaEntrada.Date <= hoy && r.FechaSalida.Date > hoy)
                .Select(r => r.HabitacionId)
                .Distinct()
                .ToList();

            var estados = habitaciones.Select(h => new { h.IdHabitacion, h.IdPiso, Estado = EstadoHelper.Parse(h.Estado) }).ToList();
            var disponiblesSet = estados.Where(h => h.Estado == EstadoHabitacion.Disponible).Select(h => h.IdHabitacion).ToList();

            var modelo = new DashboardViewModel
            {
                Hoteles = hoteles.Count,
                HotelesActivos = hoteles.Count(h => EsActivo(h.Estado)),
                Bloques = _bloqueService.Listar().Count(),
                Pisos = pisos.Count,
                TiposHabitacion = _tipoService.Listar().Count(),
                Habitaciones = habitaciones.Count,
                ReservasActivas = reservas.Count,
                ReservasHoy = reservas.Count(r => r.FechaEntrada.Date == hoy),
                Huespedes = huespedes.Count,
                Ocupadas = ocupadasHoy.Count(id => disponiblesSet.Contains(id)),
                Disponibles = disponiblesSet.Count(id => !ocupadasHoy.Contains(id)),
                Mantenimiento = estados.Count(h => h.Estado == EstadoHabitacion.Mantenimiento),
                FueraDeServicio = estados.Count(h => h.Estado == EstadoHabitacion.FueraDeServicio),
                Inactivas = estados.Count(h => h.Estado == EstadoHabitacion.Inactiva)
            };

            var habitacionesPorId = habitaciones.ToDictionary(h => h.IdHabitacion);
            modelo.ProximasReservas = reservas
                .Where(r => r.FechaSalida.Date > hoy)
                .OrderBy(r => r.FechaEntrada)
                .Take(5)
                .Select(r =>
                {
                    var hab = habitacionesPorId.ContainsKey(r.HabitacionId) ? habitacionesPorId[r.HabitacionId] : null;
                    var idHotel = hab != null && pisos.ContainsKey(hab.IdPiso) ? pisos[hab.IdPiso] : 0;
                    return new ReservaProximaViewModel
                    {
                        Id = r.Id,
                        Huesped = huespedes.ContainsKey(r.HuespedId) ? huespedes[r.HuespedId] : "Huésped #" + r.HuespedId,
                        Habitacion = hab != null ? hab.Numero : r.HabitacionId.ToString(),
                        Hotel = nombresHotel.ContainsKey(idHotel) ? nombresHotel[idHotel] : "",
                        Entrada = r.FechaEntrada,
                        Salida = r.FechaSalida,
                        Estado = r.Estado
                    };
                })
                .ToList();

            modelo.PorHotel = hoteles
                .OrderBy(h => h.Nombre)
                .Select(h =>
                {
                    var delHotel = estados.Where(x => pisos.ContainsKey(x.IdPiso) && pisos[x.IdPiso] == h.IdHotel).ToList();
                    return new HabitacionesPorHotelViewModel
                    {
                        IdHotel = h.IdHotel,
                        Hotel = h.Nombre,
                        Total = delHotel.Count,
                        Disponibles = delHotel.Count(x => x.Estado == EstadoHabitacion.Disponible && !ocupadasHoy.Contains(x.IdHabitacion))
                    };
                })
                .Take(7)
                .ToList();

            return View(modelo);
        }

        // GET: /Home/Buscar?q=...
        public ActionResult Buscar(string q)
        {
            var modelo = new BusquedaViewModel { Texto = (q ?? string.Empty).Trim() };
            if (modelo.Texto.Length == 0) return View(modelo);

            Func<string, bool> coincide = s => !string.IsNullOrEmpty(s)
                && s.IndexOf(modelo.Texto, StringComparison.OrdinalIgnoreCase) >= 0;

            var hoteles = _hotelService.Listar().ToList();
            var nombresHotel = hoteles.ToDictionary(h => h.IdHotel, h => h.Nombre);

            modelo.Resultados.AddRange(hoteles
                .Where(h => coincide(h.Nombre) || coincide(h.Direccion))
                .Select(h => new ResultadoBusqueda
                {
                    Modulo = "Hotel", Icono = "bi-buildings", Titulo = h.Nombre, Detalle = h.Direccion,
                    Url = Url.Action("Detalle", "Hoteles", new { id = h.IdHotel })
                }));

            modelo.Resultados.AddRange(_bloqueService.Listar()
                .Where(b => coincide(b.Nombre) || coincide(b.Codigo) || coincide(b.Descripcion))
                .Select(b => new ResultadoBusqueda
                {
                    Modulo = "Bloque", Icono = "bi-grid-1x2", Titulo = b.Codigo + " - " + b.Nombre,
                    Detalle = nombresHotel.ContainsKey(b.IdHotel) ? nombresHotel[b.IdHotel] : "",
                    Url = Url.Action("Editar", "Bloques", new { id = b.IdBloque })
                }));

            var pisos = _pisoService.Listar().ToDictionary(p => p.IdPiso, p => p.IdHotel);
            modelo.Resultados.AddRange(_habitacionService.Listar()
                .Where(h => coincide(h.Numero))
                .Select(h => new ResultadoBusqueda
                {
                    Modulo = "Habitación", Icono = "bi-door-closed", Titulo = "Habitación " + h.Numero,
                    Detalle = pisos.ContainsKey(h.IdPiso) && nombresHotel.ContainsKey(pisos[h.IdPiso]) ? nombresHotel[pisos[h.IdPiso]] : "",
                    Url = Url.Action("Editar", "Habitaciones", new { id = h.IdHabitacion })
                }));

            var huespedes = _huespedService.Listar().ToList();
            modelo.Resultados.AddRange(huespedes
                .Where(h => coincide(h.Nombre) || coincide(h.Documento) || coincide(h.Correo))
                .Select(h => new ResultadoBusqueda
                {
                    Modulo = "Huésped", Icono = "bi-person", Titulo = h.Nombre, Detalle = h.Documento + " · " + h.Correo,
                    Url = Url.Action("Detalle", "Huespedes", new { id = h.IdHuesped })
                }));

            var nombresHuesped = huespedes.ToDictionary(h => h.IdHuesped, h => h.Nombre);
            var textoId = modelo.Texto.TrimStart('#');
            modelo.Resultados.AddRange(_reservaService.Listar()
                .Where(r => r.Id.ToString() == textoId
                            || (nombresHuesped.ContainsKey(r.HuespedId) && coincide(nombresHuesped[r.HuespedId])))
                .Take(20)
                .Select(r => new ResultadoBusqueda
                {
                    Modulo = "Reserva", Icono = "bi-calendar-week",
                    Titulo = "Reserva #" + r.Id + " · " + (nombresHuesped.ContainsKey(r.HuespedId) ? nombresHuesped[r.HuespedId] : ""),
                    Detalle = r.FechaEntrada.ToString("dd/MM/yyyy") + " → " + r.FechaSalida.ToString("dd/MM/yyyy") + " · " + r.Estado,
                    Url = Url.Action("Detalle", "Reservas", new { id = r.Id })
                }));

            modelo.Resultados.AddRange(_tipoService.Listar()
                .Where(t => coincide(t.Nombre) || coincide(t.Descripcion))
                .Select(t => new ResultadoBusqueda
                {
                    Modulo = "Tipo de habitación", Icono = "bi-tags", Titulo = t.Nombre, Detalle = t.Descripcion,
                    Url = Url.Action("Editar", "TiposHabitacion", new { id = t.IdTipoHabitacion })
                }));

            return View(modelo);
        }
    }
}
