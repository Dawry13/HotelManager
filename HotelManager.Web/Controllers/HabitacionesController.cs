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
    /// CU-003 Gestionar habitaciones y CU-004 Actualizar estado de habitacion
    /// (RF-003, RF-004) - RN-004, RN-005, RN-006, RN-007.
    /// </summary>
    public class HabitacionesController : BaseController
    {
        private readonly IHabitacionService _habitacionService;
        private readonly IHotelService _hotelService;
        private readonly IPisoService _pisoService;
        private readonly IBloqueService _bloqueService;
        private readonly ITipoHabitacionService _tipoService;

        public HabitacionesController(IHabitacionService habitacionService, IHotelService hotelService,
                                      IPisoService pisoService, IBloqueService bloqueService,
                                      ITipoHabitacionService tipoService)
        {
            _habitacionService = habitacionService;
            _hotelService = hotelService;
            _pisoService = pisoService;
            _bloqueService = bloqueService;
            _tipoService = tipoService;
        }

        // GET: /Habitaciones?idHotel=1&idTipo=2&estado=Disponible
        public ActionResult Index(int? idHotel, int? idTipo, EstadoHabitacion? estado)
        {
            var hoteles = _hotelService.Listar().ToList();
            var tipos = _tipoService.Listar().ToList();

            var habitaciones = (idHotel.HasValue ? _habitacionService.ListarPorHotel(idHotel.Value) : _habitacionService.Listar())
                .Where(h => !idTipo.HasValue || h.IdTipoHabitacion == idTipo)
                .Where(h => !estado.HasValue || EstadoHelper.Parse(h.Estado) == estado.Value);

            var lista = ConstruirListado(habitaciones, hoteles, tipos);

            ViewBag.Hoteles = HotelesSelect(hoteles, idHotel, false);
            ViewBag.Tipos = new SelectList(
                tipos.OrderBy(t => t.Nombre).Select(t => new { t.IdTipoHabitacion, Nombre = TextoHelper.Titulo(t.Nombre) }),
                "IdTipoHabitacion", "Nombre", idTipo);
            ViewBag.Estados = EstadoHelper.OpcionesHabitacion(estado);
            ViewBag.IdHotel = idHotel;
            return View(lista);
        }

        // GET: /Habitaciones/Crear?idHotel=1&idPiso=3
        public ActionResult Crear(int? idHotel, int? idPiso)
        {
            if (!idHotel.HasValue && idPiso.HasValue)
            {
                try { idHotel = _pisoService.ObtenerPorId(idPiso.Value).IdHotel; }
                catch (BusinessException) { idPiso = null; }
            }

            var vm = new HabitacionViewModel { IdHotel = idHotel, IdPiso = idPiso, Capacidad = 1 };
            CargarListas(vm);
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Crear(HabitacionViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var dto = new HabitacionDto
                {
                    IdPiso = vm.IdPiso.Value,
                    IdTipoHabitacion = vm.IdTipoHabitacion,
                    Numero = vm.Numero,
                    Capacidad = vm.Capacidad.Value,
                    Tarifa = vm.Tarifa.Value
                };
                if (Ejecutar(() => _habitacionService.Crear(dto)))
                {
                    Exito("Habitación " + dto.Numero + " registrada correctamente.");
                    return RedirectToAction("Index", new { idHotel = vm.IdHotel });
                }
            }
            CargarListas(vm);
            return View(vm);
        }

        // GET: /Habitaciones/Editar/5
        public ActionResult Editar(int id)
        {
            try
            {
                var vm = ToViewModel(_habitacionService.ObtenerPorId(id));
                CompletarUbicacion(vm);
                CargarListas(vm);
                return View(vm);
            }
            catch (BusinessException ex) { Error(ex.Message); return RedirectToAction("Index"); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Editar(HabitacionViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var dto = new HabitacionDto
                {
                    IdHabitacion = vm.IdHabitacion,
                    IdPiso = vm.IdPiso.Value,
                    IdTipoHabitacion = vm.IdTipoHabitacion,
                    Numero = vm.Numero,
                    Capacidad = vm.Capacidad.Value,
                    Tarifa = vm.Tarifa.Value
                };
                if (Ejecutar(() => _habitacionService.Actualizar(dto)))
                {
                    Exito("Habitación actualizada correctamente.");
                    return RedirectToAction("Index", new { idHotel = vm.IdHotel });
                }
            }
            CompletarUbicacion(vm);
            CargarListas(vm);
            return View(vm);
        }

        // POST: /Habitaciones/CambiarEstado  (CU-004)
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CambiarEstado(int id, EstadoHabitacion nuevoEstado, string returnUrl)
        {
            EjecutarConMensaje(() => _habitacionService.CambiarEstado(id, nuevoEstado),
                "Estado cambiado a \"" + EstadoHelper.Texto(nuevoEstado) + "\".");

            if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction("Index");
        }

        private List<HabitacionViewModel> ConstruirListado(IEnumerable<HabitacionDto> habitaciones,
                                                           List<HotelDto> hoteles, List<TipoHabitacionDto> tipos)
        {
            var nombresHotel = hoteles.ToDictionary(h => h.IdHotel, h => h.Nombre);
            var nombresTipo = tipos.ToDictionary(t => t.IdTipoHabitacion, t => t.Nombre);
            var pisos = _pisoService.Listar().ToDictionary(p => p.IdPiso);
            var bloques = _bloqueService.Listar().ToDictionary(b => b.IdBloque);

            return habitaciones.Select(h =>
            {
                var vm = ToViewModel(h);
                PisoDto piso;
                if (pisos.TryGetValue(h.IdPiso, out piso))
                {
                    vm.IdHotel = piso.IdHotel;
                    vm.DescripcionPiso = EtiquetaPiso(piso, bloques);
                    vm.NombreHotel = nombresHotel.ContainsKey(piso.IdHotel) ? nombresHotel[piso.IdHotel] : "";
                }
                vm.NombreTipo = h.IdTipoHabitacion.HasValue && nombresTipo.ContainsKey(h.IdTipoHabitacion.Value)
                    ? nombresTipo[h.IdTipoHabitacion.Value] : "—";
                return vm;
            })
            .OrderBy(h => h.NombreHotel).ThenBy(h => h.Numero)
            .ToList();
        }

        private void CompletarUbicacion(HabitacionViewModel vm)
        {
            var piso = _pisoService.ObtenerPorId(vm.IdPiso.Value);
            var bloques = _bloqueService.ListarPorHotel(piso.IdHotel).ToDictionary(b => b.IdBloque);
            vm.IdHotel = piso.IdHotel;
            vm.DescripcionPiso = EtiquetaPiso(piso, bloques);
            vm.NombreHotel = _hotelService.ObtenerPorId(piso.IdHotel).Nombre;
        }

        private void CargarListas(HabitacionViewModel vm)
        {
            ViewBag.Hoteles = HotelesSelect(_hotelService.Listar(), vm.IdHotel, true);

            var pisos = new List<SelectListItem>();
            if (vm.IdHotel.HasValue)
            {
                var bloques = _bloqueService.ListarPorHotel(vm.IdHotel.Value).ToDictionary(b => b.IdBloque);
                pisos = _pisoService.ListarPorHotel(vm.IdHotel.Value)
                    .Where(p => EsActivo(p.Estado) || p.IdPiso == vm.IdPiso)
                    .Select(p => new SelectListItem
                    {
                        Value = p.IdPiso.ToString(),
                        Text = EtiquetaPiso(p, bloques),
                        Selected = p.IdPiso == vm.IdPiso
                    })
                    .OrderBy(i => i.Text)
                    .ToList();
            }
            ViewBag.Pisos = pisos;

            // Los tipos llevan capacidad y tarifa base para autocompletar el formulario.
            ViewBag.Tipos = _tipoService.Listar()
                .Where(t => EsActivo(t.Estado) || t.IdTipoHabitacion == vm.IdTipoHabitacion)
                .OrderBy(t => t.Nombre)
                .ToList();
        }

        private static HabitacionViewModel ToViewModel(HabitacionDto h)
        {
            return new HabitacionViewModel
            {
                IdHabitacion = h.IdHabitacion,
                IdPiso = h.IdPiso,
                IdTipoHabitacion = h.IdTipoHabitacion,
                Numero = h.Numero,
                Capacidad = h.Capacidad,
                Tarifa = h.Tarifa,
                Estado = EstadoHelper.Parse(h.Estado)
            };
        }
    }
}
