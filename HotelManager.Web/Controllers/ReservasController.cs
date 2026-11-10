using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HotelManager.Business.Dtos;
using HotelManager.Business.Exceptions;
using HotelManager.Business.Interfaces;
using HotelManager.Domain.Enums;
using HotelManager.Web.Helpers;
using HotelManager.Web.Models;

namespace HotelManager.Web.Controllers
{
    /// <summary>
    /// CU-006 Consultar disponibilidad, CU-007 Crear reserva, CU-008 Modificar reserva,
    /// CU-009 Cancelar reserva y CU-010 Consultar historial.
    /// </summary>
    public class ReservasController : BaseController
    {
        private readonly IReservaService _reservaService;
        private readonly IDisponibilidadService _disponibilidadService;
        private readonly IHistorialReservaService _historialService;
        private readonly INotificacionService _notificacionService;
        private readonly ITarifaService _tarifaService;
        private readonly IHotelService _hotelService;
        private readonly IBloqueService _bloqueService;
        private readonly IPisoService _pisoService;
        private readonly IHabitacionService _habitacionService;
        private readonly ITipoHabitacionService _tipoService;
        private readonly IHuespedService _huespedService;

        public ReservasController(IReservaService reservaService, IDisponibilidadService disponibilidadService,
                                  IHistorialReservaService historialService, INotificacionService notificacionService,
                                  ITarifaService tarifaService, IHotelService hotelService, IBloqueService bloqueService,
                                  IPisoService pisoService, IHabitacionService habitacionService,
                                  ITipoHabitacionService tipoService, IHuespedService huespedService)
        {
            _reservaService = reservaService;
            _disponibilidadService = disponibilidadService;
            _historialService = historialService;
            _notificacionService = notificacionService;
            _tarifaService = tarifaService;
            _hotelService = hotelService;
            _bloqueService = bloqueService;
            _pisoService = pisoService;
            _habitacionService = habitacionService;
            _tipoService = tipoService;
            _huespedService = huespedService;
        }

        private ContextoHotel Contexto()
        {
            return ContextoHotel.Cargar(_hotelService, _bloqueService, _pisoService, _habitacionService, _tipoService, _huespedService);
        }

        private static ReservaItemViewModel ToItem(ReservaDto r, ContextoHotel ctx)
        {
            var idHotel = ctx.IdHotelDeHabitacion(r.HabitacionId);
            return new ReservaItemViewModel
            {
                Id = r.Id,
                IdHuesped = r.HuespedId,
                Huesped = ctx.Huesped(r.HuespedId),
                IdHabitacion = r.HabitacionId,
                Habitacion = ctx.NumeroHabitacion(r.HabitacionId),
                Ubicacion = ctx.UbicacionHabitacion(r.HabitacionId),
                Tipo = ctx.TipoDeHabitacion(r.HabitacionId),
                IdHotel = idHotel,
                Hotel = ctx.Hotel(idHotel),
                Entrada = r.FechaEntrada,
                Salida = r.FechaSalida,
                Huespedes = r.CantidadHuespedes,
                Costo = r.CostoTotal,
                Estado = r.Estado
            };
        }

        // GET: /Reservas?estado=&idHotel=&buscar=&desde=&hasta=
        public ActionResult Index(string estado, int? idHotel, string buscar, DateTime? desde, DateTime? hasta)
        {
            var ctx = Contexto();
            var todas = _reservaService.Listar().Select(r => ToItem(r, ctx)).ToList();

            var lista = todas
                .Where(r => string.IsNullOrEmpty(estado) || r.Estado == estado)
                .Where(r => !idHotel.HasValue || r.IdHotel == idHotel.Value)
                .Where(r => !desde.HasValue || r.Salida.Date > desde.Value.Date)
                .Where(r => !hasta.HasValue || r.Entrada.Date <= hasta.Value.Date)
                .Where(r => string.IsNullOrWhiteSpace(buscar)
                            || r.Huesped.IndexOf(buscar.Trim(), StringComparison.OrdinalIgnoreCase) >= 0
                            || r.Habitacion.IndexOf(buscar.Trim(), StringComparison.OrdinalIgnoreCase) >= 0
                            || ("#" + r.Id) == buscar.Trim() || r.Id.ToString() == buscar.Trim())
                .ToList();

            ViewBag.Conteos = todas.GroupBy(r => r.Estado).ToDictionary(g => g.Key, g => g.Count());
            ViewBag.Hoteles = HotelesSelect(_hotelService.Listar(), idHotel, false);
            ViewBag.Estado = estado;
            ViewBag.IdHotel = idHotel;
            ViewBag.Buscar = buscar;
            ViewBag.Desde = desde;
            ViewBag.Hasta = hasta;
            return View(lista);
        }

        // GET: /Reservas/Disponibilidad  (CU-006)
        public ActionResult Disponibilidad(DateTime? entrada, DateTime? salida, int? huespedes, int? idHotel, int? idHuesped)
        {
            var vm = new DisponibilidadViewModel
            {
                Entrada = (entrada ?? DateTime.Today).Date,
                Salida = (salida ?? (entrada ?? DateTime.Today).AddDays(1)).Date,
                Huespedes = huespedes ?? 1,
                IdHotel = idHotel,
                IdHuesped = idHuesped
            };

            if (entrada.HasValue)
            {
                vm.Consultado = true;
                try
                {
                    var ctx = Contexto();
                    vm.Resultados = _disponibilidadService.Consultar(new DisponibilidadRequestDto
                    {
                        Entrada = vm.Entrada,
                        Salida = vm.Salida,
                        CantidadHuespedes = vm.Huespedes,
                        IdHotel = idHotel
                    })
                    .Select(d => new HabitacionDisponibleViewModel
                    {
                        IdHabitacion = d.IdHabitacion,
                        Numero = d.Numero,
                        Hotel = ctx.Hotel(d.IdHotel),
                        Ubicacion = ctx.UbicacionHabitacion(d.IdHabitacion),
                        Tipo = ctx.TipoDeHabitacion(d.IdHabitacion),
                        Capacidad = d.Capacidad,
                        TarifaPromedio = d.TarifaPorNoche,
                        CostoTotal = d.CostoEstimado
                    })
                    .OrderBy(d => d.CostoTotal)
                    .ToList();
                }
                catch (BusinessException ex)
                {
                    vm.Error = ex.Message;
                }
            }

            ViewBag.Hoteles = HotelesSelect(_hotelService.Listar(), idHotel, true);
            if (idHuesped.HasValue)
            {
                try { ViewBag.NombreHuesped = TextoHelper.Titulo(_huespedService.ObtenerPorId(idHuesped.Value).Nombre); }
                catch (BusinessException) { vm.IdHuesped = null; }
            }
            return View(vm);
        }

        // GET: /Reservas/Crear  (CU-007)
        public ActionResult Crear(int? idHabitacion, DateTime? entrada, DateTime? salida, int? huespedes, int? idHuesped)
        {
            var vm = new ReservaFormViewModel
            {
                IdHabitacion = idHabitacion,
                IdHuesped = idHuesped,
                Entrada = (entrada ?? DateTime.Today).Date,
                Salida = (salida ?? (entrada ?? DateTime.Today).AddDays(1)).Date,
                CantidadHuespedes = huespedes ?? 1
            };
            CargarListas(vm);
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Crear(ReservaFormViewModel vm)
        {
            if (ModelState.IsValid)
            {
                ReservaDto creada = null;
                var dto = new CrearReservaDto
                {
                    HuespedId = vm.IdHuesped.Value,
                    HabitacionId = vm.IdHabitacion.Value,
                    FechaEntrada = vm.Entrada.Value.Date,
                    FechaSalida = vm.Salida.Value.Date,
                    CantidadHuespedes = vm.CantidadHuespedes.Value
                };
                if (Ejecutar(() => creada = _reservaService.Crear(dto)))
                {
                    Exito("Reserva #" + creada.Id + " registrada. Costo total: RD$ " + creada.CostoTotal.ToString("N2") + ".");
                    return RedirectToAction("Detalle", new { id = creada.Id });
                }
            }
            CargarListas(vm);
            return View(vm);
        }

        // GET: /Reservas/Detalle/5
        public ActionResult Detalle(int id)
        {
            ReservaDto reserva;
            try { reserva = _reservaService.ObtenerPorId(id); }
            catch (BusinessException ex) { Error(ex.Message); return RedirectToAction("Index"); }

            var ctx = Contexto();
            ViewBag.Huesped = ctx.Huespedes.ContainsKey(reserva.HuespedId) ? ctx.Huespedes[reserva.HuespedId] : null;
            ViewBag.Historial = _historialService.ConsultarPorReserva(id).ToList();
            ViewBag.Notificaciones = _notificacionService.ListarPorReserva(id).ToList();
            return View(ToItem(reserva, ctx));
        }

        // GET: /Reservas/Modificar/5  (CU-008)
        public ActionResult Modificar(int id)
        {
            ReservaDto r;
            try { r = _reservaService.ObtenerPorId(id); }
            catch (BusinessException ex) { Error(ex.Message); return RedirectToAction("Index"); }

            if (r.Estado == "Cancelada" || r.Estado == "Finalizada")
            {
                Error("No se puede modificar una reserva " + r.Estado.ToLower() + ".");
                return RedirectToAction("Detalle", new { id });
            }

            var vm = new ReservaFormViewModel
            {
                IdReserva = r.Id,
                IdHuesped = r.HuespedId,
                IdHabitacion = r.HabitacionId,
                Entrada = r.FechaEntrada,
                Salida = r.FechaSalida,
                CantidadHuespedes = r.CantidadHuespedes,
                Estado = r.Estado
            };
            CargarListas(vm);
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Modificar(ReservaFormViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var dto = new ModificarReservaDto
                {
                    ReservaId = vm.IdReserva,
                    HabitacionId = vm.IdHabitacion.Value,
                    FechaEntrada = vm.Entrada.Value.Date,
                    FechaSalida = vm.Salida.Value.Date,
                    CantidadHuespedes = vm.CantidadHuespedes.Value
                };
                ReservaDto modificada = null;
                if (Ejecutar(() => modificada = _reservaService.Modificar(dto)))
                {
                    Exito("Reserva modificada. Nuevo costo: RD$ " + modificada.CostoTotal.ToString("N2") + ".");
                    return RedirectToAction("Detalle", new { id = vm.IdReserva });
                }
            }
            CargarListas(vm);
            return View(vm);
        }

        // POST: /Reservas/Cancelar/5  (CU-009)
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Cancelar(int id)
        {
            EjecutarConMensaje(() => _reservaService.Cancelar(id), "Reserva #" + id + " cancelada. La habitación queda libre para esas fechas.");
            return RedirectToAction("Detalle", new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Confirmar(int id)
        {
            EjecutarConMensaje(() => _reservaService.Confirmar(id), "Reserva #" + id + " confirmada.");
            return RedirectToAction("Detalle", new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Finalizar(int id)
        {
            EjecutarConMensaje(() => _reservaService.Finalizar(id), "Check-out registrado: reserva #" + id + " finalizada.");
            return RedirectToAction("Detalle", new { id });
        }

        // GET: /Reservas/Historial  (CU-010)
        public ActionResult Historial(DateTime? desde, DateTime? hasta, string accion)
        {
            var ctx = Contexto();
            var reservas = _reservaService.Listar().ToDictionary(r => r.Id);
            List<HistorialItemViewModel> lista;
            try
            {
                lista = _historialService.Consultar(desde, hasta)
                    .Where(h => string.IsNullOrEmpty(accion) || h.Accion == accion)
                    .Select(h =>
                    {
                        ReservaDto r;
                        reservas.TryGetValue(h.IdReserva, out r);
                        return new HistorialItemViewModel
                        {
                            IdReserva = h.IdReserva,
                            Fecha = h.Fecha,
                            Accion = h.Accion,
                            EstadoAnterior = h.EstadoAnterior,
                            EstadoNuevo = h.EstadoNuevo,
                            Huesped = r != null ? ctx.Huesped(r.HuespedId) : "",
                            Habitacion = r != null ? ctx.NumeroHabitacion(r.HabitacionId) : "",
                            Hotel = r != null ? ctx.HotelDeHabitacion(r.HabitacionId) : ""
                        };
                    })
                    .ToList();
            }
            catch (BusinessException ex)
            {
                Error(ex.Message);
                lista = new List<HistorialItemViewModel>();
            }

            ViewBag.Desde = desde;
            ViewBag.Hasta = hasta;
            ViewBag.Accion = accion;
            return View(lista);
        }

        /// <summary>JSON: costo de una estadia segun las tarifas vigentes (RN-013).</summary>
        [HttpGet]
        public JsonResult Cotizar(int idHabitacion, DateTime entrada, DateTime salida)
        {
            try
            {
                var total = _tarifaService.CalcularCostoTotal(idHabitacion, entrada, salida);
                var noches = (salida.Date - entrada.Date).Days;
                return Json(new { ok = true, noches, total, texto = "RD$ " + total.ToString("N2") }, JsonRequestBehavior.AllowGet);
            }
            catch (BusinessException ex)
            {
                return Json(new { ok = false, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private void CargarListas(ReservaFormViewModel vm)
        {
            var ctx = Contexto();

            ViewBag.Huespedes = ctx.Huespedes.Values
                .OrderBy(h => h.Nombre)
                .Select(h => new SelectListItem
                {
                    Value = h.IdHuesped.ToString(),
                    Text = TextoHelper.Titulo(h.Nombre) + " — " + h.Documento,
                    Selected = h.IdHuesped == vm.IdHuesped
                })
                .ToList();

            // Habitaciones reservables (disponibles, en piso y hotel activos), agrupadas por hotel.
            var grupos = new Dictionary<int, SelectListGroup>();
            ViewBag.Habitaciones = ctx.Habitaciones.Values
                .Where(h => h.IdHabitacion == vm.IdHabitacion
                            || (EstadoHelper.Parse(h.Estado) == EstadoHabitacion.Disponible
                                && ctx.Pisos.ContainsKey(h.IdPiso) && EsActivo(ctx.Pisos[h.IdPiso].Estado)
                                && ctx.Hoteles.ContainsKey(ctx.Pisos[h.IdPiso].IdHotel)
                                && EsActivo(ctx.Hoteles[ctx.Pisos[h.IdPiso].IdHotel].Estado)))
                .OrderBy(h => ctx.HotelDeHabitacion(h.IdHabitacion)).ThenBy(h => h.Numero)
                .Select(h =>
                {
                    var idHotel = ctx.IdHotelDeHabitacion(h.IdHabitacion);
                    SelectListGroup grupo;
                    if (!grupos.TryGetValue(idHotel, out grupo))
                        grupos[idHotel] = grupo = new SelectListGroup { Name = ctx.Hotel(idHotel) };
                    return new SelectListItem
                    {
                        Value = h.IdHabitacion.ToString(),
                        Text = "Hab. " + h.Numero + " · " + ctx.TipoDeHabitacion(h.IdHabitacion) + " · máx. " + h.Capacidad
                               + " · RD$ " + h.Tarifa.ToString("N0") + "/noche",
                        Group = grupo,
                        Selected = h.IdHabitacion == vm.IdHabitacion
                    };
                })
                .ToList();

            if (vm.IdHuesped.HasValue && ctx.Huespedes.ContainsKey(vm.IdHuesped.Value))
                vm.NombreHuesped = TextoHelper.Titulo(ctx.Huespedes[vm.IdHuesped.Value].Nombre);
        }
    }
}
