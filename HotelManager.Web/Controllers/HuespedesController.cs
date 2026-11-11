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
    /// <summary>CU-005 Registrar huesped (RF-005).</summary>
    [Autorizar(Roles.Administrador, Roles.Recepcionista)]
    public class HuespedesController : BaseController
    {
        private static readonly string[] EstadosActivos = { "Pendiente", "Confirmada", "Modificada" };

        private readonly IHuespedService _huespedService;
        private readonly IReservaService _reservaService;

        public HuespedesController(IHuespedService huespedService, IReservaService reservaService)
        {
            _huespedService = huespedService;
            _reservaService = reservaService;
        }

        // GET: /Huespedes?buscar=
        public ActionResult Index(string buscar)
        {
            var reservas = _reservaService.Listar().ToList();
            var lista = _huespedService.Listar()
                .Where(h => string.IsNullOrWhiteSpace(buscar)
                            || Contiene(h.Nombre, buscar) || Contiene(h.Documento, buscar)
                            || Contiene(h.Correo, buscar) || Contiene(h.Telefono, buscar))
                .Select(h =>
                {
                    var vm = ToViewModel(h);
                    var suyas = reservas.Where(r => r.HuespedId == h.IdHuesped).ToList();
                    vm.TotalReservas = suyas.Count;
                    vm.ReservasActivas = suyas.Count(r => EstadosActivos.Contains(r.Estado));
                    vm.TotalGastado = suyas.Where(r => r.Estado != "Cancelada").Sum(r => r.CostoTotal);
                    return vm;
                })
                .OrderBy(h => h.Nombre)
                .ToList();

            ViewBag.Buscar = buscar;
            return View(lista);
        }

        // GET: /Huespedes/Detalle/5
        public ActionResult Detalle(int id)
        {
            HuespedDto huesped;
            try { huesped = _huespedService.ObtenerPorId(id); }
            catch (BusinessException ex) { Error(ex.Message); return RedirectToAction("Index"); }

            // CU-010 / RF-013: historial de reservas del huesped
            var reservas = _reservaService.ObtenerHistorialPorHuesped(id).ToList();
            var vm = ToViewModel(huesped);
            vm.TotalReservas = reservas.Count;
            vm.ReservasActivas = reservas.Count(r => EstadosActivos.Contains(r.Estado));
            vm.TotalGastado = reservas.Where(r => r.Estado != "Cancelada").Sum(r => r.CostoTotal);

            ViewBag.Reservas = reservas;
            ViewBag.Contexto = CargarContexto();
            return View(vm);
        }

        public ActionResult Crear(string returnUrl)
        {
            return View(new HuespedViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Crear(HuespedViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            HuespedDto creado = null;
            if (!Ejecutar(() => creado = _huespedService.Registrar(ToDto(vm))))
                return View(vm);

            Exito("Huésped " + Helpers.TextoHelper.Titulo(creado.Nombre) + " registrado correctamente.");
            if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            {
                var url = System.Text.RegularExpressions.Regex.Replace(vm.ReturnUrl, @"([?&])idHuesped=[^&]*&?", "$1").TrimEnd('?', '&');
                var separador = url.Contains("?") ? "&" : "?";
                return Redirect(url + separador + "idHuesped=" + creado.IdHuesped);
            }
            return RedirectToAction("Detalle", new { id = creado.IdHuesped });
        }

        public ActionResult Editar(int id)
        {
            try { return View(ToViewModel(_huespedService.ObtenerPorId(id))); }
            catch (BusinessException ex) { Error(ex.Message); return RedirectToAction("Index"); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Editar(HuespedViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            if (!Ejecutar(() => _huespedService.Actualizar(ToDto(vm))))
                return View(vm);

            Exito("Datos del huésped actualizados.");
            return RedirectToAction("Detalle", new { id = vm.IdHuesped });
        }

        /// <summary>JSON para buscar un huesped por documento desde el formulario de reserva.</summary>
        [HttpGet]
        public JsonResult PorDocumento(string documento)
        {
            try
            {
                var h = _huespedService.BuscarPorDocumento(documento);
                return Json(h == null ? null : new { id = h.IdHuesped, nombre = Helpers.TextoHelper.Titulo(h.Nombre) },
                    JsonRequestBehavior.AllowGet);
            }
            catch (BusinessException)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        private Helpers.ContextoHotel CargarContexto()
        {
            var r = DependencyResolver.Current;
            return Helpers.ContextoHotel.Cargar(r.GetService<IHotelService>(), r.GetService<IBloqueService>(),
                r.GetService<IPisoService>(), r.GetService<IHabitacionService>(), r.GetService<ITipoHabitacionService>(), null);
        }

        private static bool Contiene(string valor, string texto)
        {
            return !string.IsNullOrEmpty(valor) && valor.IndexOf(texto.Trim(), StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static HuespedDto ToDto(HuespedViewModel vm)
        {
            return new HuespedDto
            {
                IdHuesped = vm.IdHuesped,
                Nombre = vm.Nombre,
                Documento = vm.Documento,
                Telefono = string.IsNullOrWhiteSpace(vm.Telefono) ? null : vm.Telefono,
                Correo = vm.Correo
            };
        }

        private static HuespedViewModel ToViewModel(HuespedDto h)
        {
            return new HuespedViewModel
            {
                IdHuesped = h.IdHuesped,
                Nombre = h.Nombre,
                Documento = h.Documento,
                Telefono = h.Telefono,
                Correo = h.Correo
            };
        }
    }
}
