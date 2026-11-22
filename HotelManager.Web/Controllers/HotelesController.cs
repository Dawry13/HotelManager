using System;
using System.Collections.Generic;
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
    /// <summary>
    /// CU-001 Gestionar hoteles (RF-001).
    /// El detalle del hotel es el centro de trabajo: desde ahi el administrador registra bloques,
    /// pisos y habitaciones, y administrador o recepcion hacen reservas sin salir del hotel.
    /// </summary>
    [Autorizar(Roles.Administrador, Roles.Recepcionista)]
    public class HotelesController : BaseController
    {
        private readonly IHotelService _hotelService;
        private readonly IBloqueService _bloqueService;
        private readonly IPisoService _pisoService;
        private readonly IHabitacionService _habitacionService;
        private readonly ITipoHabitacionService _tipoService;
        private readonly IHuespedService _huespedService;
        private readonly IReservaService _reservaService;
        private readonly IDisponibilidadService _disponibilidadService;

        public HotelesController(IHotelService hotelService, IBloqueService bloqueService,
                                 IPisoService pisoService, IHabitacionService habitacionService,
                                 ITipoHabitacionService tipoService, IHuespedService huespedService,
                                 IReservaService reservaService, IDisponibilidadService disponibilidadService)
        {
            _hotelService = hotelService;
            _bloqueService = bloqueService;
            _pisoService = pisoService;
            _habitacionService = habitacionService;
            _tipoService = tipoService;
            _huespedService = huespedService;
            _reservaService = reservaService;
            _disponibilidadService = disponibilidadService;
        }

        // GET: /Hoteles?buscar=&estado=&categoria=
        public ActionResult Index(string buscar, string estado, int? categoria)
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
                .Where(h => !categoria.HasValue || h.Categoria == categoria.Value)
                .OrderByDescending(h => h.Categoria).ThenBy(h => h.Nombre)
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
            ViewBag.Categoria = categoria;
            return View(hoteles);
        }

        // GET: /Hoteles/Detalle/5?tab=reservas&entrada=&salida=&huespedes=
        public ActionResult Detalle(int id, string tab, DateTime? entrada, DateTime? salida, int? huespedes)
        {
            HotelDto hotel;
            try { hotel = _hotelService.ObtenerPorId(id); }
            catch (BusinessException ex) { Error(ex.Message); return RedirectToAction("Index"); }

            var esAdmin = User.IsInRole(Roles.Administrador);
            var bloques = _bloqueService.ListarPorHotel(id).OrderBy(b => b.Codigo).ToList();
            var pisos = _pisoService.ListarPorHotel(id).ToList();
            var habitaciones = _habitacionService.ListarPorHotel(id).ToList();
            var tipos = _tipoService.Listar().ToList();
            var bloquesPorId = bloques.ToDictionary(b => b.IdBloque);
            var pisosPorId = pisos.ToDictionary(p => p.IdPiso);

            var vm = new HotelGestionViewModel
            {
                Hotel = ToViewModel(hotel),
                Bloques = bloques,
                Pisos = pisos.OrderBy(p => p.IdBloque.HasValue && bloquesPorId.ContainsKey(p.IdBloque.Value) ? bloquesPorId[p.IdBloque.Value].Codigo : "")
                             .ThenBy(p => p.Numero).ToList(),
                BloquesPorId = bloquesPorId,
                Tipos = tipos,
                HabitacionesPorPiso = habitaciones.GroupBy(h => h.IdPiso).ToDictionary(g => g.Key, g => g.OrderBy(h => h.Numero).ToList()),
                Tab = string.IsNullOrEmpty(tab) ? (esAdmin ? "estructura" : "reservas") : tab.ToLowerInvariant(),
                Entrada = (entrada ?? DateTime.Today).Date,
                Salida = (salida ?? (entrada ?? DateTime.Today).AddDays(1)).Date,
                Huespedes = Math.Max(1, huespedes ?? 1)
            };
            vm.Hotel.TotalBloques = bloques.Count;
            vm.Hotel.TotalPisos = pisos.Count;
            vm.Hotel.TotalHabitaciones = habitaciones.Count;
            if (!esAdmin && vm.Tab == "estructura") vm.Tab = "reservas";

            // Etiquetas "Torre A - Piso 2" para los formularios rapidos
            vm.PisosActivos = pisos.Where(p => EsActivo(p.Estado))
                .Select(p => new SelectListItem { Value = p.IdPiso.ToString(), Text = EtiquetaPiso(p, bloquesPorId) })
                .OrderBy(i => i.Text).ToList();
            vm.EtiquetaPiso = pisos.ToDictionary(p => p.IdPiso, p => EtiquetaPiso(p, bloquesPorId));

            // Reservas del hotel (proximas y en curso)
            var idsHabitacion = new HashSet<int>(habitaciones.Select(h => h.IdHabitacion));
            var nombresHuesped = _huespedService.Listar().ToDictionary(h => h.IdHuesped, h => h.Nombre);
            var numeros = habitaciones.ToDictionary(h => h.IdHabitacion);
            vm.Reservas = _reservaService.ListarActivas()
                .Where(r => idsHabitacion.Contains(r.HabitacionId) && r.FechaSalida.Date >= DateTime.Today)
                .OrderBy(r => r.FechaEntrada)
                .Take(25)
                .Select(r => new ReservaItemViewModel
                {
                    Id = r.Id,
                    IdHuesped = r.HuespedId,
                    Huesped = nombresHuesped.ContainsKey(r.HuespedId) ? TextoHelper.Titulo(nombresHuesped[r.HuespedId]) : "Huésped #" + r.HuespedId,
                    IdHabitacion = r.HabitacionId,
                    Habitacion = numeros[r.HabitacionId].Numero,
                    Ubicacion = pisosPorId.ContainsKey(numeros[r.HabitacionId].IdPiso) ? vm.EtiquetaPiso[numeros[r.HabitacionId].IdPiso] : "",
                    IdHotel = id,
                    Hotel = hotel.Nombre,
                    Entrada = r.FechaEntrada,
                    Salida = r.FechaSalida,
                    Huespedes = r.CantidadHuespedes,
                    Costo = r.CostoTotal,
                    Estado = r.Estado
                })
                .ToList();

            // Disponibilidad para reservar sin salir del hotel
            if (entrada.HasValue)
            {
                vm.Consultado = true;
                vm.Tab = "reservas";
                try
                {
                    vm.Disponibles = _disponibilidadService.Consultar(new DisponibilidadRequestDto
                    {
                        Entrada = vm.Entrada,
                        Salida = vm.Salida,
                        CantidadHuespedes = vm.Huespedes,
                        IdHotel = id
                    })
                    .Select(d => new HabitacionDisponibleViewModel
                    {
                        IdHabitacion = d.IdHabitacion,
                        Numero = d.Numero,
                        Hotel = hotel.Nombre,
                        Ubicacion = vm.EtiquetaPiso.ContainsKey(d.IdPiso) ? vm.EtiquetaPiso[d.IdPiso] : "",
                        Tipo = TipoDe(numeros.ContainsKey(d.IdHabitacion) ? numeros[d.IdHabitacion] : null, tipos),
                        Capacidad = d.Capacidad,
                        TarifaPromedio = d.TarifaPorNoche,
                        CostoTotal = d.CostoEstimado,
                        Caracteristicas = numeros.ContainsKey(d.IdHabitacion) ? numeros[d.IdHabitacion].Caracteristicas : new List<string>()
                    })
                    .OrderBy(d => d.CostoTotal)
                    .ToList();
                }
                catch (BusinessException ex)
                {
                    vm.ErrorDisponibilidad = ex.Message;
                }
            }

            vm.ListaHuespedes = _huespedService.Listar()
                .OrderBy(h => h.Nombre)
                .Select(h => new SelectListItem { Value = h.IdHuesped.ToString(), Text = TextoHelper.Titulo(h.Nombre) + " · " + h.Documento })
                .ToList();

            ViewBag.EsAdmin = esAdmin;
            return View(vm);
        }

        // GET: /Hoteles/Crear
        [Autorizar(Roles.Administrador)]
        public ActionResult Crear()
        {
            return View(new HotelViewModel());
        }

        // POST: /Hoteles/Crear -> lleva al centro del hotel para registrar su estructura
        [HttpPost, ValidateAntiForgeryToken, Autorizar(Roles.Administrador)]
        public ActionResult Crear(HotelViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            HotelDto creado = null;
            if (!Ejecutar(() => creado = _hotelService.Crear(ToDto(vm))))
                return View(vm);

            Exito("Hotel \"" + TextoHelper.Titulo(creado.Nombre) + "\" registrado. Ahora agregue sus bloques, pisos y habitaciones desde aquí.");
            return RedirectToAction("Detalle", new { id = creado.IdHotel, tab = "estructura" });
        }

        // GET: /Hoteles/Editar/5
        [Autorizar(Roles.Administrador)]
        public ActionResult Editar(int id)
        {
            try { return View(ToViewModel(_hotelService.ObtenerPorId(id))); }
            catch (BusinessException ex) { Error(ex.Message); return RedirectToAction("Index"); }
        }

        // POST: /Hoteles/Editar/5
        [HttpPost, ValidateAntiForgeryToken, Autorizar(Roles.Administrador)]
        public ActionResult Editar(HotelViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            if (!Ejecutar(() => _hotelService.Actualizar(ToDto(vm))))
                return View(vm);

            Exito("Hotel actualizado correctamente.");
            return RedirectToAction("Detalle", new { id = vm.IdHotel });
        }

        // POST: /Hoteles/CambiarEstado/5
        [HttpPost, ValidateAntiForgeryToken, Autorizar(Roles.Administrador)]
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

        private static string TipoDe(HabitacionDto habitacion, List<TipoHabitacionDto> tipos)
        {
            if (habitacion == null || !habitacion.IdTipoHabitacion.HasValue) return "Habitación";
            var tipo = tipos.FirstOrDefault(t => t.IdTipoHabitacion == habitacion.IdTipoHabitacion.Value);
            return tipo != null ? TextoHelper.Titulo(tipo.Nombre) : "Habitación";
        }

        private static HotelDto ToDto(HotelViewModel vm)
        {
            return new HotelDto
            {
                IdHotel = vm.IdHotel,
                Nombre = vm.Nombre,
                Direccion = vm.Direccion,
                Categoria = vm.Categoria,
                Descripcion = vm.Descripcion,
                Caracteristicas = vm.TodasLasCaracteristicas()
            };
        }

        private static HotelViewModel ToViewModel(HotelDto h)
        {
            return new HotelViewModel
            {
                IdHotel = h.IdHotel,
                Nombre = h.Nombre,
                Direccion = h.Direccion,
                Estado = h.Estado,
                Categoria = h.Categoria,
                Descripcion = h.Descripcion,
                Caracteristicas = h.Caracteristicas.Where(c => CatalogoHelper.CaracteristicasHotel.ContainsKey(c)).ToList(),
                OtrasCaracteristicas = CatalogoHelper.Otras(h.Caracteristicas, CatalogoHelper.CaracteristicasHotel)
            };
        }
    }
}
