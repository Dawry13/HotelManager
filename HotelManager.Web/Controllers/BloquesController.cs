using System.Linq;
using System.Web.Mvc;
using HotelManager.Web.Infrastructure;
using HotelManager.Business.Seguridad;
using HotelManager.Business.Dtos;
using HotelManager.Business.Exceptions;
using HotelManager.Business.Interfaces;
using HotelManager.Web.Models;

namespace HotelManager.Web.Controllers
{
    /// <summary>Gestion de bloques (torres / edificios) de cada hotel.</summary>
    [Autorizar(Roles.Administrador)]
    public class BloquesController : BaseController
    {
        private readonly IBloqueService _bloqueService;
        private readonly IHotelService _hotelService;
        private readonly IPisoService _pisoService;

        public BloquesController(IBloqueService bloqueService, IHotelService hotelService, IPisoService pisoService)
        {
            _bloqueService = bloqueService;
            _hotelService = hotelService;
            _pisoService = pisoService;
        }

        // GET: /Bloques?idHotel=1
        public ActionResult Index(int? idHotel)
        {
            var hoteles = _hotelService.Listar().ToList();
            var nombres = hoteles.ToDictionary(h => h.IdHotel, h => h.Nombre);
            var pisos = _pisoService.Listar().ToList();

            var bloques = (idHotel.HasValue ? _bloqueService.ListarPorHotel(idHotel.Value) : _bloqueService.Listar())
                .Select(b =>
                {
                    var vm = ToViewModel(b);
                    vm.NombreHotel = nombres.ContainsKey(b.IdHotel) ? nombres[b.IdHotel] : "";
                    vm.TotalPisos = pisos.Count(p => p.IdBloque == b.IdBloque);
                    return vm;
                })
                .OrderBy(b => b.NombreHotel).ThenBy(b => b.Codigo)
                .ToList();

            ViewBag.Hoteles = HotelesSelect(hoteles, idHotel, false);
            ViewBag.IdHotel = idHotel;
            return View(bloques);
        }

        // GET: /Bloques/Crear?idHotel=1
        public ActionResult Crear(int? idHotel)
        {
            CargarHoteles(idHotel, true);
            return View(new BloqueViewModel { IdHotel = idHotel });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Crear(BloqueViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var dto = new BloqueDto
                {
                    IdHotel = vm.IdHotel.Value,
                    Codigo = vm.Codigo,
                    Nombre = vm.Nombre,
                    Descripcion = vm.Descripcion
                };
                if (Ejecutar(() => _bloqueService.Crear(dto)))
                {
                    Exito("Bloque registrado correctamente.");
                    return RedirectToAction("Index", new { idHotel = vm.IdHotel });
                }
            }
            CargarHoteles(vm.IdHotel, true);
            return View(vm);
        }

        // GET: /Bloques/Editar/5
        public ActionResult Editar(int id)
        {
            try
            {
                var vm = ToViewModel(_bloqueService.ObtenerPorId(id));
                vm.NombreHotel = _hotelService.ObtenerPorId(vm.IdHotel.Value).Nombre;
                return View(vm);
            }
            catch (BusinessException ex) { Error(ex.Message); return RedirectToAction("Index"); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Editar(BloqueViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var dto = new BloqueDto
                {
                    IdBloque = vm.IdBloque,
                    IdHotel = vm.IdHotel.Value,
                    Codigo = vm.Codigo,
                    Nombre = vm.Nombre,
                    Descripcion = vm.Descripcion
                };
                if (Ejecutar(() => _bloqueService.Actualizar(dto)))
                {
                    Exito("Bloque actualizado correctamente.");
                    return RedirectToAction("Index", new { idHotel = vm.IdHotel });
                }
            }
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CambiarEstado(int id, int? idHotel)
        {
            EjecutarConMensaje(() =>
            {
                var bloque = _bloqueService.ObtenerPorId(id);
                if (EsActivo(bloque.Estado)) _bloqueService.Desactivar(id);
                else _bloqueService.Activar(id);
            }, "Estado del bloque actualizado.");
            return RedirectToAction("Index", new { idHotel });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Eliminar(int id, int? idHotel)
        {
            EjecutarConMensaje(() => _bloqueService.Eliminar(id), "Bloque eliminado.");
            return RedirectToAction("Index", new { idHotel });
        }

        /// <summary>JSON para listas en cascada: bloques activos de un hotel.</summary>
        [HttpGet]
        public JsonResult PorHotel(int idHotel)
        {
            var data = _bloqueService.ListarPorHotel(idHotel)
                .Where(b => EsActivo(b.Estado))
                .Select(b => new { id = b.IdBloque, texto = b.Codigo + " - " + Helpers.TextoHelper.Titulo(b.Nombre) });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        private void CargarHoteles(int? seleccionado, bool soloActivos)
        {
            ViewBag.Hoteles = HotelesSelect(_hotelService.Listar(), seleccionado, soloActivos);
        }

        private static BloqueViewModel ToViewModel(BloqueDto b)
        {
            return new BloqueViewModel
            {
                IdBloque = b.IdBloque,
                IdHotel = b.IdHotel,
                Codigo = b.Codigo,
                Nombre = b.Nombre,
                Descripcion = b.Descripcion,
                Estado = b.Estado
            };
        }
    }
}
