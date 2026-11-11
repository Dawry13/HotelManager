using System;
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
    /// <summary>CU-001 Gestionar hoteles (RF-001).</summary>
    [Autorizar(Roles.Administrador)]
    public class HotelesController : BaseController
    {
        private readonly IHotelService _hotelService;
        private readonly IBloqueService _bloqueService;
        private readonly IPisoService _pisoService;
        private readonly IHabitacionService _habitacionService;

        public HotelesController(IHotelService hotelService, IBloqueService bloqueService,
                                 IPisoService pisoService, IHabitacionService habitacionService)
        {
            _hotelService = hotelService;
            _bloqueService = bloqueService;
            _pisoService = pisoService;
            _habitacionService = habitacionService;
        }

        // GET: /Hoteles?buscar=&estado=
        public ActionResult Index(string buscar, string estado)
        {
            var bloques = _bloqueService.Listar().ToList();
            var pisos = _pisoService.Listar().ToList();
            var pisoHotel = pisos.ToDictionary(p => p.IdPiso, p => p.IdHotel);
            var habitaciones = _habitacionService.Listar().ToList();

            var hoteles = _hotelService.Listar()
                .Where(h => string.IsNullOrWhiteSpace(buscar)
                            || h.Nombre.IndexOf(buscar, StringComparison.OrdinalIgnoreCase) >= 0
                            || h.Direccion.IndexOf(buscar, StringComparison.OrdinalIgnoreCase) >= 0)
                .Where(h => string.IsNullOrEmpty(estado) || h.Estado == estado)
                .OrderBy(h => h.Nombre)
                .Select(h =>
                {
                    var vm = ToViewModel(h);
                    vm.TotalBloques = bloques.Count(b => b.IdHotel == h.IdHotel);
                    vm.TotalPisos = pisos.Count(p => p.IdHotel == h.IdHotel);
                    vm.TotalHabitaciones = habitaciones.Count(x => pisoHotel.ContainsKey(x.IdPiso) && pisoHotel[x.IdPiso] == h.IdHotel);
                    return vm;
                })
                .ToList();

            ViewBag.Buscar = buscar;
            ViewBag.Estado = estado;
            return View(hoteles);
        }

        // GET: /Hoteles/Detalle/5
        public ActionResult Detalle(int id)
        {
            HotelDto hotel;
            try { hotel = _hotelService.ObtenerPorId(id); }
            catch (BusinessException ex) { Error(ex.Message); return RedirectToAction("Index"); }

            var bloques = _bloqueService.ListarPorHotel(id).ToList();
            var pisos = _pisoService.ListarPorHotel(id).ToList();
            var habitaciones = _habitacionService.ListarPorHotel(id).ToList();

            var vm = ToViewModel(hotel);
            vm.TotalBloques = bloques.Count;
            vm.TotalPisos = pisos.Count;
            vm.TotalHabitaciones = habitaciones.Count;

            ViewBag.Bloques = bloques;
            ViewBag.Pisos = pisos;
            ViewBag.BloquesPorId = bloques.ToDictionary(b => b.IdBloque);
            ViewBag.HabitacionesPorPiso = habitaciones.GroupBy(h => h.IdPiso).ToDictionary(g => g.Key, g => g.Count());
            return View(vm);
        }

        // GET: /Hoteles/Crear
        public ActionResult Crear()
        {
            return View(new HotelViewModel());
        }

        // POST: /Hoteles/Crear
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Crear(HotelViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            HotelDto creado = null;
            if (!Ejecutar(() => creado = _hotelService.Crear(new HotelDto { Nombre = vm.Nombre, Direccion = vm.Direccion })))
                return View(vm);

            Exito("Hotel \"" + creado.Nombre + "\" registrado correctamente.");
            return RedirectToAction("Index");
        }

        // GET: /Hoteles/Editar/5
        public ActionResult Editar(int id)
        {
            try { return View(ToViewModel(_hotelService.ObtenerPorId(id))); }
            catch (BusinessException ex) { Error(ex.Message); return RedirectToAction("Index"); }
        }

        // POST: /Hoteles/Editar/5
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Editar(HotelViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var dto = new HotelDto { IdHotel = vm.IdHotel, Nombre = vm.Nombre, Direccion = vm.Direccion };
            if (!Ejecutar(() => _hotelService.Actualizar(dto)))
                return View(vm);

            Exito("Hotel actualizado correctamente.");
            return RedirectToAction("Index");
        }

        // POST: /Hoteles/CambiarEstado/5
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CambiarEstado(int id, string returnUrl)
        {
            EjecutarConMensaje(() =>
            {
                var hotel = _hotelService.ObtenerPorId(id);
                if (EsActivo(hotel.Estado)) _hotelService.Desactivar(id);
                else _hotelService.Activar(id);
            }, "Estado del hotel actualizado.");

            if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction("Index");
        }

        private static HotelViewModel ToViewModel(HotelDto h)
        {
            return new HotelViewModel
            {
                IdHotel = h.IdHotel,
                Nombre = h.Nombre,
                Direccion = h.Direccion,
                Estado = h.Estado
            };
        }
    }
}
