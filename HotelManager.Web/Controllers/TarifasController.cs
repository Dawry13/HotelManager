using System.Linq;
using System.Web.Mvc;
using HotelManager.Web.Infrastructure;
using HotelManager.Business.Seguridad;
using HotelManager.Business.Dtos;
using HotelManager.Business.Exceptions;
using HotelManager.Business.Interfaces;
using HotelManager.Web.Helpers;
using HotelManager.Web.Models;

namespace HotelManager.Web.Controllers
{
    /// <summary>Tarifas por vigencia (temporadas) de cada habitacion. Apoyan el calculo del costo (RN-013).</summary>
    [Autorizar(Roles.Administrador)]
    public class TarifasController : BaseController
    {
        private readonly ITarifaService _tarifaService;
        private readonly IHotelService _hotelService;
        private readonly IBloqueService _bloqueService;
        private readonly IPisoService _pisoService;
        private readonly IHabitacionService _habitacionService;
        private readonly ITipoHabitacionService _tipoService;

        public TarifasController(ITarifaService tarifaService, IHotelService hotelService, IBloqueService bloqueService,
                                 IPisoService pisoService, IHabitacionService habitacionService, ITipoHabitacionService tipoService)
        {
            _tarifaService = tarifaService;
            _hotelService = hotelService;
            _bloqueService = bloqueService;
            _pisoService = pisoService;
            _habitacionService = habitacionService;
            _tipoService = tipoService;
        }

        private ContextoHotel Contexto()
        {
            return ContextoHotel.Cargar(_hotelService, _bloqueService, _pisoService, _habitacionService, _tipoService, null);
        }

        // GET: /Tarifas?idHotel=&idHabitacion=
        // Cada hotel con la tarifa base de sus habitaciones (editable) y sus tarifas por temporada.
        public ActionResult Index(int? idHotel, int? idHabitacion)
        {
            var ctx = Contexto();
            var hoy = System.DateTime.Today;
            var temporadas = _tarifaService.Listar()
                .Select(t => ToViewModel(t, ctx))
                .ToList();

            var hoteles = ctx.Hoteles.Values
                .Where(h => !idHotel.HasValue || h.IdHotel == idHotel.Value)
                .OrderByDescending(h => h.Categoria).ThenBy(h => h.Nombre)
                .Select(h =>
                {
                    var habitaciones = ctx.Habitaciones.Values
                        .Where(x => ctx.IdHotelDeHabitacion(x.IdHabitacion) == h.IdHotel)
                        .Where(x => !idHabitacion.HasValue || x.IdHabitacion == idHabitacion.Value)
                        .OrderBy(x => x.Numero)
                        .ToList();
                    var ids = new System.Collections.Generic.HashSet<int>(habitaciones.Select(x => x.IdHabitacion));
                    var delHotel = temporadas.Where(t => ids.Contains(t.IdHabitacion.Value)).ToList();

                    return new TarifasHotelViewModel
                    {
                        IdHotel = h.IdHotel,
                        Hotel = TextoHelper.Titulo(h.Nombre),
                        Categoria = h.Categoria,
                        Habitaciones = habitaciones.Select(x =>
                        {
                            var vigente = delHotel
                                .Where(t => t.IdHabitacion == x.IdHabitacion && t.EstadoVigencia == "Vigente")
                                .OrderByDescending(t => t.VigenciaDesde)
                                .FirstOrDefault();
                            return new TarifaHabitacionViewModel
                            {
                                IdHabitacion = x.IdHabitacion,
                                Numero = x.Numero,
                                Tipo = ctx.TipoDeHabitacion(x.IdHabitacion),
                                Ubicacion = ctx.UbicacionHabitacion(x.IdHabitacion),
                                TarifaBase = x.Tarifa,
                                TarifaHoy = vigente != null ? vigente.ValorNoche : null,
                                TotalTemporadas = delHotel.Count(t => t.IdHabitacion == x.IdHabitacion)
                            };
                        }).ToList(),
                        Temporadas = delHotel
                            .OrderBy(t => t.Habitacion).ThenByDescending(t => t.VigenciaDesde)
                            .ToList()
                    };
                })
                .Where(h => !idHabitacion.HasValue || h.Habitaciones.Any())
                .ToList();

            ViewBag.Temporadas = temporadas;
            ViewBag.Hoteles = HotelesSelect(_hotelService.Listar(), idHotel, false);
            ViewBag.IdHotel = idHotel;
            ViewBag.IdHabitacion = idHabitacion;
            if (idHabitacion.HasValue)
                ViewBag.HabitacionFiltro = "Hab. " + ctx.NumeroHabitacion(idHabitacion.Value) + " · " + ctx.HotelDeHabitacion(idHabitacion.Value);
            return View(hoteles);
        }

        // POST: /Tarifas/ActualizarBase  -> precio por noche de una habitacion
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ActualizarBase(int idHabitacion, decimal? tarifa, string returnUrl)
        {
            if (!tarifa.HasValue) Error("Indique la tarifa base.");
            else EjecutarConMensaje(() => _habitacionService.ActualizarTarifa(idHabitacion, tarifa.Value),
                "Tarifa base de la habitación actualizada a RD$ " + tarifa.Value.ToString("N2") + ".");
            return VolverAlListado(returnUrl);
        }

        // POST: /Tarifas/ActualizarBaseHotel  -> misma tarifa para todas las habitaciones del hotel
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ActualizarBaseHotel(int idHotel, decimal? tarifa, string returnUrl)
        {
            if (!tarifa.HasValue) Error("Indique la tarifa base.");
            else
            {
                var cambiadas = 0;
                EjecutarConMensaje(() => cambiadas = _habitacionService.ActualizarTarifaHotel(idHotel, tarifa.Value), null);
                if (TempData["Error"] == null)
                    Exito(cambiadas == 0
                        ? "Todas las habitaciones del hotel ya tenían esa tarifa."
                        : "Tarifa base de RD$ " + tarifa.Value.ToString("N2") + " aplicada a " + cambiadas + " habitación(es) del hotel.");
            }
            return VolverAlListado(returnUrl);
        }

        private ActionResult VolverAlListado(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction("Index");
        }

        public ActionResult Crear(int? idHabitacion)
        {
            var vm = new TarifaViewModel { IdHabitacion = idHabitacion, VigenciaDesde = System.DateTime.Today };
            CargarHabitaciones(vm);
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Crear(TarifaViewModel vm)
        {
            if (ModelState.IsValid && Ejecutar(() => _tarifaService.Crear(ToDto(vm))))
            {
                Exito("Tarifa registrada correctamente.");
                return RedirectToAction("Index", new { idHabitacion = vm.IdHabitacion });
            }
            CargarHabitaciones(vm);
            return View(vm);
        }

        public ActionResult Editar(int id)
        {
            try
            {
                var vm = ToViewModel(_tarifaService.ObtenerPorId(id), Contexto());
                CargarHabitaciones(vm);
                return View(vm);
            }
            catch (BusinessException ex) { Error(ex.Message); return RedirectToAction("Index"); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Editar(TarifaViewModel vm)
        {
            if (ModelState.IsValid && Ejecutar(() => _tarifaService.Actualizar(ToDto(vm))))
            {
                Exito("Tarifa actualizada.");
                return RedirectToAction("Index", new { idHabitacion = vm.IdHabitacion });
            }
            CargarHabitaciones(vm);
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Eliminar(int id, int? idHabitacion)
        {
            EjecutarConMensaje(() => _tarifaService.Eliminar(id), "Tarifa eliminada. Se aplicará la tarifa base de la habitación en esas fechas.");
            return RedirectToAction("Index", new { idHabitacion });
        }

        private void CargarHabitaciones(TarifaViewModel vm)
        {
            var ctx = Contexto();
            var grupos = new System.Collections.Generic.Dictionary<int, SelectListGroup>();
            ViewBag.Habitaciones = ctx.Habitaciones.Values
                .OrderBy(h => ctx.HotelDeHabitacion(h.IdHabitacion)).ThenBy(h => h.Numero)
                .Select(h =>
                {
                    var idHotel = ctx.IdHotelDeHabitacion(h.IdHabitacion);
                    SelectListGroup g;
                    if (!grupos.TryGetValue(idHotel, out g)) grupos[idHotel] = g = new SelectListGroup { Name = ctx.Hotel(idHotel) };
                    return new SelectListItem
                    {
                        Value = h.IdHabitacion.ToString(),
                        Text = "Hab. " + h.Numero + " · " + ctx.TipoDeHabitacion(h.IdHabitacion) + " · base RD$ " + h.Tarifa.ToString("N0"),
                        Group = g,
                        Selected = h.IdHabitacion == vm.IdHabitacion
                    };
                })
                .ToList();
            if (vm.IdHabitacion.HasValue && ctx.Habitaciones.ContainsKey(vm.IdHabitacion.Value))
            {
                vm.Habitacion = ctx.NumeroHabitacion(vm.IdHabitacion.Value);
                vm.Hotel = ctx.HotelDeHabitacion(vm.IdHabitacion.Value);
                vm.TarifaBase = ctx.Habitaciones[vm.IdHabitacion.Value].Tarifa;
            }
        }

        private static TarifaDto ToDto(TarifaViewModel vm)
        {
            return new TarifaDto
            {
                IdTarifa = vm.IdTarifa,
                IdHabitacion = vm.IdHabitacion.Value,
                ValorNoche = vm.ValorNoche.Value,
                VigenciaDesde = vm.VigenciaDesde.Value.Date,
                VigenciaHasta = vm.VigenciaHasta.HasValue ? vm.VigenciaHasta.Value.Date : (System.DateTime?)null
            };
        }

        private static TarifaViewModel ToViewModel(TarifaDto t, ContextoHotel ctx)
        {
            return new TarifaViewModel
            {
                IdTarifa = t.IdTarifa,
                IdHabitacion = t.IdHabitacion,
                ValorNoche = t.ValorNoche,
                VigenciaDesde = t.VigenciaDesde,
                VigenciaHasta = t.VigenciaHasta,
                Habitacion = ctx.NumeroHabitacion(t.IdHabitacion),
                Hotel = ctx.HotelDeHabitacion(t.IdHabitacion),
                TarifaBase = ctx.Habitaciones.ContainsKey(t.IdHabitacion) ? ctx.Habitaciones[t.IdHabitacion].Tarifa : 0
            };
        }
    }
}
