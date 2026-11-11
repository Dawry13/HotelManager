using System;
using System.Linq;
using System.Web.Mvc;
using HotelManager.Business.Dtos;
using HotelManager.Business.Exceptions;
using HotelManager.Business.Interfaces;
using HotelManager.Business.Seguridad;
using HotelManager.Web.Helpers;
using HotelManager.Web.Infrastructure;
using HotelManager.Web.Models;

namespace HotelManager.Web.Controllers
{
    /// <summary>
    /// ACT-003 Huesped: "solicita una reserva y recibe informacion/notificaciones relacionadas con ella".
    /// Solo ve sus propias reservas; sus solicitudes quedan Pendientes hasta que recepcion las confirma.
    /// </summary>
    [Autorizar(Roles.Huesped)]
    public class PortalController : BaseController
    {
        private readonly IReservaService _reservaService;
        private readonly IDisponibilidadService _disponibilidadService;
        private readonly INotificacionService _notificacionService;
        private readonly ITarifaService _tarifaService;
        private readonly IHotelService _hotelService;
        private readonly IBloqueService _bloqueService;
        private readonly IPisoService _pisoService;
        private readonly IHabitacionService _habitacionService;
        private readonly ITipoHabitacionService _tipoService;
        private readonly IHuespedService _huespedService;

        public PortalController(IReservaService reservaService, IDisponibilidadService disponibilidadService,
                                INotificacionService notificacionService, ITarifaService tarifaService,
                                IHotelService hotelService, IBloqueService bloqueService, IPisoService pisoService,
                                IHabitacionService habitacionService, ITipoHabitacionService tipoService,
                                IHuespedService huespedService)
        {
            _reservaService = reservaService;
            _disponibilidadService = disponibilidadService;
            _notificacionService = notificacionService;
            _tarifaService = tarifaService;
            _hotelService = hotelService;
            _bloqueService = bloqueService;
            _pisoService = pisoService;
            _habitacionService = habitacionService;
            _tipoService = tipoService;
            _huespedService = huespedService;
        }

        private int IdHuesped
        {
            get { return ((UsuarioIdentidad)User.Identity).IdHuesped ?? 0; }
        }

        private ContextoHotel Contexto()
        {
            return ContextoHotel.Cargar(_hotelService, _bloqueService, _pisoService, _habitacionService, _tipoService, null);
        }

        // GET: /Portal  -> mis reservas y mis notificaciones
        public ActionResult Index()
        {
            var ctx = Contexto();
            var reservas = _reservaService.ObtenerHistorialPorHuesped(IdHuesped)
                .OrderByDescending(r => r.FechaEntrada)
                .Select(r => ToItem(r, ctx))
                .ToList();
            var ids = reservas.Select(r => r.Id).ToList();

            ViewBag.Notificaciones = _notificacionService.Listar()
                .Where(n => ids.Contains(n.IdReserva))
                .OrderByDescending(n => n.Fecha)
                .Take(10)
                .ToList();
            ViewBag.Huesped = _huespedService.ObtenerPorId(IdHuesped);
            return View(reservas);
        }

        // GET: /Portal/Detalle/5  -> solo si la reserva es del huesped
        public ActionResult Detalle(int id)
        {
            ReservaDto r;
            try { r = _reservaService.ObtenerPorId(id); }
            catch (BusinessException) { return HttpNotFound(); }
            if (r.HuespedId != IdHuesped) return RedirectToAction("AccesoDenegado", "Cuenta");

            ViewBag.Notificaciones = _notificacionService.ListarPorReserva(id).ToList();
            return View(ToItem(r, Contexto()));
        }

        // GET: /Portal/Solicitar  -> buscar habitaciones disponibles (CU-006) y solicitar (ACT-003)
        public ActionResult Solicitar(DateTime? entrada, DateTime? salida, int? huespedes, int? idHotel)
        {
            var vm = new DisponibilidadViewModel
            {
                Entrada = (entrada ?? DateTime.Today.AddDays(1)).Date,
                Salida = (salida ?? (entrada ?? DateTime.Today.AddDays(1)).AddDays(1)).Date,
                Huespedes = huespedes ?? 1,
                IdHotel = idHotel,
                IdHuesped = IdHuesped
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
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Solicitar(int idHabitacion, DateTime entrada, DateTime salida, int huespedes)
        {
            ReservaDto solicitada = null;
            var dto = new CrearReservaDto
            {
                HuespedId = IdHuesped,       // siempre el huesped de la sesion
                HabitacionId = idHabitacion,
                FechaEntrada = entrada.Date,
                FechaSalida = salida.Date,
                CantidadHuespedes = huespedes
            };

            if (Ejecutar(() => solicitada = _reservaService.Solicitar(dto)))
            {
                Exito("Solicitud #" + solicitada.Id + " enviada. Le avisaremos por correo cuando recepción la confirme.");
                return RedirectToAction("Detalle", new { id = solicitada.Id });
            }

            Error(string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return RedirectToAction("Solicitar", new
            {
                entrada = entrada.ToString("yyyy-MM-dd"),
                salida = salida.ToString("yyyy-MM-dd"),
                huespedes
            });
        }

        private static ReservaItemViewModel ToItem(ReservaDto r, ContextoHotel ctx)
        {
            var idHotel = ctx.IdHotelDeHabitacion(r.HabitacionId);
            return new ReservaItemViewModel
            {
                Id = r.Id,
                IdHuesped = r.HuespedId,
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
    }
}
