using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HotelManager.Business.Dtos;
using HotelManager.Business.Exceptions;
using HotelManager.Business.Interfaces;
using HotelManager.Web.Models;

namespace HotelManager.Web.Controllers
{
    /// <summary>CU-002 Gestionar pisos (RF-002) - RN-001, RN-002.</summary>
    public class PisosController : BaseController
    {
        private readonly IPisoService _pisoService;
        private readonly IHotelService _hotelService;
        private readonly IBloqueService _bloqueService;
        private readonly IHabitacionService _habitacionService;

        public PisosController(IPisoService pisoService, IHotelService hotelService,
                               IBloqueService bloqueService, IHabitacionService habitacionService)
        {
            _pisoService = pisoService;
            _hotelService = hotelService;
            _bloqueService = bloqueService;
            _habitacionService = habitacionService;
        }

        // GET: /Pisos?idHotel=1&idBloque=2
        public ActionResult Index(int? idHotel, int? idBloque)
        {
            var hoteles = _hotelService.Listar().ToList();
            var nombresHotel = hoteles.ToDictionary(h => h.IdHotel, h => h.Nombre);
            var bloques = _bloqueService.Listar().ToDictionary(b => b.IdBloque);
            var habitaciones = _habitacionService.Listar().ToList();

            var pisos = (idHotel.HasValue ? _pisoService.ListarPorHotel(idHotel.Value) : _pisoService.Listar())
                .Where(p => !idBloque.HasValue || p.IdBloque == idBloque)
                .Select(p =>
                {
                    var vm = ToViewModel(p);
                    vm.NombreHotel = nombresHotel.ContainsKey(p.IdHotel) ? nombresHotel[p.IdHotel] : "";
                    vm.NombreBloque = p.IdBloque.HasValue && bloques.ContainsKey(p.IdBloque.Value)
                        ? bloques[p.IdBloque.Value].Codigo + " - " + bloques[p.IdBloque.Value].Nombre : "—";
                    vm.TotalHabitaciones = habitaciones.Count(h => h.IdPiso == p.IdPiso);
                    return vm;
                })
                .OrderBy(p => p.NombreHotel).ThenBy(p => p.NombreBloque).ThenBy(p => p.Numero)
                .ToList();

            ViewBag.Hoteles = HotelesSelect(hoteles, idHotel, false);
            ViewBag.Bloques = new SelectList(
                bloques.Values.Where(b => !idHotel.HasValue || b.IdHotel == idHotel)
                       .OrderBy(b => b.Codigo)
                       .Select(b => new { b.IdBloque, Texto = b.Codigo + " - " + Helpers.TextoHelper.Titulo(b.Nombre) }),
                "IdBloque", "Texto", idBloque);
            ViewBag.IdHotel = idHotel;
            ViewBag.IdBloque = idBloque;
            return View(pisos);
        }

        // GET: /Pisos/Crear?idHotel=1&idBloque=2
        public ActionResult Crear(int? idHotel, int? idBloque)
        {
            var vm = new PisoViewModel { IdHotel = idHotel, IdBloque = idBloque };
            CargarListas(vm, true);
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Crear(PisoViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var dto = new PisoDto
                {
                    IdHotel = vm.IdHotel.Value,
                    IdBloque = vm.IdBloque,
                    Numero = vm.Numero.Value
                };
                if (Ejecutar(() => _pisoService.Crear(dto)))
                {
                    Exito("Piso registrado correctamente.");
                    return RedirectToAction("Index", new { idHotel = vm.IdHotel });
                }
            }
            CargarListas(vm, true);
            return View(vm);
        }

        // GET: /Pisos/Editar/5
        public ActionResult Editar(int id)
        {
            try
            {
                var vm = ToViewModel(_pisoService.ObtenerPorId(id));
                vm.NombreHotel = _hotelService.ObtenerPorId(vm.IdHotel.Value).Nombre;
                CargarListas(vm, false);
                return View(vm);
            }
            catch (BusinessException ex) { Error(ex.Message); return RedirectToAction("Index"); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Editar(PisoViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var dto = new PisoDto
                {
                    IdPiso = vm.IdPiso,
                    IdHotel = vm.IdHotel.Value,
                    IdBloque = vm.IdBloque,
                    Numero = vm.Numero.Value
                };
                if (Ejecutar(() => _pisoService.Actualizar(dto)))
                {
                    Exito("Piso actualizado correctamente.");
                    return RedirectToAction("Index", new { idHotel = vm.IdHotel });
                }
            }
            CargarListas(vm, false);
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CambiarEstado(int id, int? idHotel)
        {
            EjecutarConMensaje(() =>
            {
                var piso = _pisoService.ObtenerPorId(id);
                if (EsActivo(piso.Estado)) _pisoService.Desactivar(id);
                else _pisoService.Activar(id);
            }, "Estado del piso actualizado.");
            return RedirectToAction("Index", new { idHotel });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Eliminar(int id, int? idHotel)
        {
            EjecutarConMensaje(() => _pisoService.Eliminar(id), "Piso eliminado.");
            return RedirectToAction("Index", new { idHotel });
        }

        /// <summary>JSON para listas en cascada: pisos activos de un hotel (RN-006).</summary>
        [HttpGet]
        public JsonResult PorHotel(int idHotel)
        {
            var bloques = _bloqueService.ListarPorHotel(idHotel).ToDictionary(b => b.IdBloque);
            var data = _pisoService.ListarPorHotel(idHotel)
                .Where(p => EsActivo(p.Estado))
                .Select(p => new { id = p.IdPiso, texto = EtiquetaPiso(p, bloques) })
                .OrderBy(p => p.texto);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        private void CargarListas(PisoViewModel vm, bool soloHotelesActivos)
        {
            ViewBag.Hoteles = HotelesSelect(_hotelService.Listar(), vm.IdHotel, soloHotelesActivos);

            IEnumerable<BloqueDto> bloques = vm.IdHotel.HasValue
                ? _bloqueService.ListarPorHotel(vm.IdHotel.Value)
                : Enumerable.Empty<BloqueDto>();
            ViewBag.Bloques = new SelectList(
                bloques.Where(b => EsActivo(b.Estado) || b.IdBloque == vm.IdBloque)
                       .Select(b => new { b.IdBloque, Texto = b.Codigo + " - " + Helpers.TextoHelper.Titulo(b.Nombre) }),
                "IdBloque", "Texto", vm.IdBloque);
        }

        private static PisoViewModel ToViewModel(PisoDto p)
        {
            return new PisoViewModel
            {
                IdPiso = p.IdPiso,
                IdHotel = p.IdHotel,
                IdBloque = p.IdBloque,
                Numero = p.Numero,
                Estado = p.Estado
            };
        }
    }
}
