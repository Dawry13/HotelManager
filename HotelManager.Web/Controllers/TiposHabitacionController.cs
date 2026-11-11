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
    /// <summary>Mantenimiento del catalogo de tipos de habitacion.</summary>
    [Autorizar(Roles.Administrador)]
    public class TiposHabitacionController : BaseController
    {
        private readonly ITipoHabitacionService _tipoService;
        private readonly IHabitacionService _habitacionService;

        public TiposHabitacionController(ITipoHabitacionService tipoService, IHabitacionService habitacionService)
        {
            _tipoService = tipoService;
            _habitacionService = habitacionService;
        }

        public ActionResult Index()
        {
            var habitaciones = _habitacionService.Listar().ToList();
            var tipos = _tipoService.Listar()
                .Select(t =>
                {
                    var vm = ToViewModel(t);
                    vm.TotalHabitaciones = habitaciones.Count(h => h.IdTipoHabitacion == t.IdTipoHabitacion);
                    return vm;
                })
                .ToList();
            return View(tipos);
        }

        public ActionResult Crear()
        {
            return View(new TipoHabitacionViewModel { CapacidadMaxima = 1, TarifaBase = 0 });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Crear(TipoHabitacionViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            if (!Ejecutar(() => _tipoService.Crear(ToDto(vm))))
                return View(vm);

            Exito("Tipo de habitación \"" + vm.Nombre + "\" registrado.");
            return RedirectToAction("Index");
        }

        public ActionResult Editar(int id)
        {
            try { return View(ToViewModel(_tipoService.ObtenerPorId(id))); }
            catch (BusinessException ex) { Error(ex.Message); return RedirectToAction("Index"); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Editar(TipoHabitacionViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            if (!Ejecutar(() => _tipoService.Actualizar(ToDto(vm))))
                return View(vm);

            Exito("Tipo de habitación actualizado.");
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CambiarEstado(int id)
        {
            EjecutarConMensaje(() =>
            {
                var tipo = _tipoService.ObtenerPorId(id);
                if (EsActivo(tipo.Estado)) _tipoService.Desactivar(id);
                else _tipoService.Activar(id);
            }, "Estado del tipo de habitación actualizado.");
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Eliminar(int id)
        {
            EjecutarConMensaje(() => _tipoService.Eliminar(id), "Tipo de habitación eliminado.");
            return RedirectToAction("Index");
        }

        private static TipoHabitacionDto ToDto(TipoHabitacionViewModel vm)
        {
            return new TipoHabitacionDto
            {
                IdTipoHabitacion = vm.IdTipoHabitacion,
                Nombre = vm.Nombre,
                Descripcion = vm.Descripcion,
                CapacidadMaxima = vm.CapacidadMaxima.Value,
                TarifaBase = vm.TarifaBase.Value
            };
        }

        private static TipoHabitacionViewModel ToViewModel(TipoHabitacionDto t)
        {
            return new TipoHabitacionViewModel
            {
                IdTipoHabitacion = t.IdTipoHabitacion,
                Nombre = t.Nombre,
                Descripcion = t.Descripcion,
                CapacidadMaxima = t.CapacidadMaxima,
                TarifaBase = t.TarifaBase,
                Estado = t.Estado
            };
        }
    }
}
