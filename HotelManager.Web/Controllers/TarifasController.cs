using System.Linq;
using System.Web.Mvc;
using HotelManager.Business.Dtos;
using HotelManager.Business.Exceptions;
using HotelManager.Business.Interfaces;
using HotelManager.Web.Helpers;
using HotelManager.Web.Models;

namespace HotelManager.Web.Controllers
{
    /// <summary>Tarifas por vigencia (temporadas) de cada habitacion. Apoyan el calculo del costo (RN-013).</summary>
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
        public ActionResult Index(int? idHotel, int? idHabitacion)
        {
            var ctx = Contexto();
            var lista = _tarifaService.Listar()
                .Where(t => !idHabitacion.HasValue || t.IdHabitacion == idHabitacion.Value)
                .Where(t => !idHotel.HasValue || ctx.IdHotelDeHabitacion(t.IdHabitacion) == idHotel.Value)
                .Select(t => ToViewModel(t, ctx))
                .OrderBy(t => t.Hotel).ThenBy(t => t.Habitacion).ThenByDescending(t => t.VigenciaDesde)
                .ToList();

            ViewBag.Hoteles = HotelesSelect(_hotelService.Listar(), idHotel, false);
            ViewBag.IdHotel = idHotel;
            ViewBag.IdHabitacion = idHabitacion;
            if (idHabitacion.HasValue)
                ViewBag.HabitacionFiltro = "Hab. " + ctx.NumeroHabitacion(idHabitacion.Value) + " · " + ctx.HotelDeHabitacion(idHabitacion.Value);
            return View(lista);
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
