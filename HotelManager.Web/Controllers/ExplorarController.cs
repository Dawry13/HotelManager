using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HotelManager.Business.Dtos;
using HotelManager.Business.Exceptions;
using HotelManager.Business.Interfaces;
using HotelManager.Business.Seguridad;
using HotelManager.Domain.Enums;
using HotelManager.Web.Helpers;
using HotelManager.Web.Models;

namespace HotelManager.Web.Controllers
{
    /// <summary>
    /// Catalogo publico: cualquier visitante ve hoteles, bloques, pisos y habitaciones
    /// y consulta disponibilidad sin registrarse. La cuenta se pide solo al reservar.
    /// </summary>
    [AllowAnonymous]
    public class ExplorarController : BaseController
    {
        private readonly IHotelService _hotelService;
        private readonly IBloqueService _bloqueService;
        private readonly IPisoService _pisoService;
        private readonly IHabitacionService _habitacionService;
        private readonly ITipoHabitacionService _tipoService;
        private readonly IDisponibilidadService _disponibilidadService;

        public ExplorarController(IHotelService hotelService, IBloqueService bloqueService, IPisoService pisoService,
                                  IHabitacionService habitacionService, ITipoHabitacionService tipoService,
                                  IDisponibilidadService disponibilidadService)
        {
            _hotelService = hotelService;
            _bloqueService = bloqueService;
            _pisoService = pisoService;
            _habitacionService = habitacionService;
            _tipoService = tipoService;
            _disponibilidadService = disponibilidadService;
        }

        // GET: /Explorar?buscar=&categoria=5
        public ActionResult Index(string buscar, int? categoria)
        {
            var activos = Tarjetas(_hotelService.Listar().Where(h => EsActivo(h.Estado)).ToList());

            var vm = new CatalogoIndexViewModel
            {
                Buscar = (buscar ?? "").Trim(),
                Categoria = categoria,
                Total = activos.Count,
                PorCategoria = Enumerable.Range(3, 3).ToDictionary(c => c, c => activos.Count(h => h.Categoria == c))
            };

            vm.Hoteles = activos
                .Where(h => vm.Buscar.Length == 0
                            || Contiene(h.Nombre, vm.Buscar) || Contiene(h.Direccion, vm.Buscar)
                            || h.Caracteristicas.Any(c => Contiene(c, vm.Buscar)))
                .Where(h => !categoria.HasValue || h.Categoria == categoria.Value)
                .OrderByDescending(h => h.Categoria).ThenBy(h => h.Nombre)
                .ToList();

            return View(vm);
        }

        // GET: /Explorar/Hotel/5?entrada=&salida=&huespedes=
        public ActionResult Hotel(int id, DateTime? entrada, DateTime? salida, int? huespedes)
        {
            HotelDto hotel;
            try { hotel = _hotelService.ObtenerPorId(id); }
            catch (BusinessException) { return HttpNotFound(); }
            if (!EsActivo(hotel.Estado))
            {
                Error("Ese hotel no está disponible por el momento.");
                return RedirectToAction("Index");
            }

            var vm = new CatalogoHotelViewModel
            {
                Hotel = Tarjetas(new List<HotelDto> { hotel }).First(),
                Entrada = (entrada ?? DateTime.Today.AddDays(1)).Date,
                Huespedes = Math.Max(1, huespedes ?? 1)
            };
            vm.Salida = (salida ?? vm.Entrada.AddDays(1)).Date;

            var tipos = _tipoService.Listar().ToDictionary(t => t.IdTipoHabitacion);
            var bloques = _bloqueService.ListarPorHotel(id).Where(b => EsActivo(b.Estado)).ToList();
            var pisos = _pisoService.ListarPorHotel(id).Where(p => EsActivo(p.Estado)).ToList();
            // Solo se publican habitaciones disponibles y con precio asignado por el administrador.
            var habitaciones = _habitacionService.ListarPorHotel(id)
                .Where(h => EstadoHelper.Parse(h.Estado) == EstadoHabitacion.Disponible && h.Tarifa > 0)
                .ToList();

            Dictionary<int, HabitacionDisponibleDto> disponibles = null;
            if (entrada.HasValue)
            {
                vm.Consultado = true;
                try
                {
                    disponibles = _disponibilidadService.Consultar(new DisponibilidadRequestDto
                    {
                        Entrada = vm.Entrada,
                        Salida = vm.Salida,
                        CantidadHuespedes = vm.Huespedes,
                        IdHotel = id
                    }).ToDictionary(d => d.IdHabitacion);
                    vm.Disponibles = disponibles.Count;
                }
                catch (BusinessException ex)
                {
                    vm.Error = ex.Message;
                }
            }

            Func<HabitacionDto, HabitacionCatalogoViewModel> habitacion = h =>
            {
                TipoHabitacionDto tipo = null;
                if (h.IdTipoHabitacion.HasValue) tipos.TryGetValue(h.IdTipoHabitacion.Value, out tipo);
                HabitacionDisponibleDto d = null;
                var item = new HabitacionCatalogoViewModel
                {
                    IdHabitacion = h.IdHabitacion,
                    Numero = h.Numero,
                    Tipo = tipo != null ? TextoHelper.Titulo(tipo.Nombre) : "Habitación",
                    DescripcionTipo = tipo != null ? tipo.Descripcion : null,
                    Capacidad = h.Capacidad,
                    Tarifa = h.Tarifa,
                    Caracteristicas = h.Caracteristicas
                };
                if (disponibles != null)
                {
                    item.DisponibleEnFechas = disponibles.TryGetValue(h.IdHabitacion, out d);
                    if (d != null) { item.CostoTotal = d.CostoEstimado; item.TarifaPromedio = d.TarifaPorNoche; }
                }
                return item;
            };

            Func<IEnumerable<PisoDto>, List<PisoCatalogoViewModel>> armarPisos = lista => lista
                .OrderBy(p => p.Numero)
                .Select(p => new PisoCatalogoViewModel
                {
                    IdPiso = p.IdPiso,
                    Numero = p.Numero,
                    Habitaciones = habitaciones.Where(h => h.IdPiso == p.IdPiso).OrderBy(h => h.Numero).Select(habitacion).ToList()
                })
                .Where(p => p.Habitaciones.Any())
                .ToList();

            foreach (var b in bloques.OrderBy(b => b.Codigo))
            {
                vm.Bloques.Add(new BloqueCatalogoViewModel
                {
                    IdBloque = b.IdBloque,
                    Codigo = b.Codigo,
                    Nombre = TextoHelper.Titulo(b.Nombre),
                    Descripcion = b.Descripcion,
                    Pisos = armarPisos(pisos.Where(p => p.IdBloque == b.IdBloque))
                });
            }

            var idsBloque = new HashSet<int>(bloques.Select(b => b.IdBloque));
            var sinBloque = armarPisos(pisos.Where(p => !p.IdBloque.HasValue || !idsBloque.Contains(p.IdBloque.Value)));
            if (sinBloque.Any())
                vm.Bloques.Add(new BloqueCatalogoViewModel { Nombre = vm.Bloques.Any() ? "Edificio principal" : "Habitaciones", Pisos = sinBloque });

            vm.Bloques = vm.Bloques.Where(b => b.Pisos.Any()).ToList();
            return View(vm);
        }

        /// <summary>
        /// Boton "Reservar": si no hay sesion pide iniciar sesion o registrarse y luego vuelve aqui.
        /// Huesped -> confirmar solicitud. Administrador/Recepcion -> formulario de reserva con los datos.
        /// </summary>
        // GET: /Explorar/Reservar?idHabitacion=&entrada=&salida=&huespedes=
        public ActionResult Reservar(int idHabitacion, DateTime entrada, DateTime salida, int huespedes)
        {
            var parametros = new
            {
                idHabitacion,
                entrada = entrada.ToString("yyyy-MM-dd"),
                salida = salida.ToString("yyyy-MM-dd"),
                huespedes
            };

            if (!User.Identity.IsAuthenticated)
            {
                TempData["Info"] = "Para completar su reserva, inicie sesión o cree su cuenta de huésped. Solo le tomará un minuto.";
                return RedirectToAction("Login", "Cuenta", new { returnUrl = Url.Action("Reservar", parametros) });
            }

            if (User.IsInRole(Roles.Huesped))
                return RedirectToAction("Confirmar", "Portal", parametros);

            if (User.IsInRole(Roles.Administrador) || User.IsInRole(Roles.Recepcionista))
                return RedirectToAction("Crear", "Reservas", parametros);

            return RedirectToAction("AccesoDenegado", "Cuenta");
        }

        private List<HotelTarjetaViewModel> Tarjetas(List<HotelDto> hoteles)
        {
            var bloques = _bloqueService.Listar().Where(b => EsActivo(b.Estado)).ToList();
            var pisos = _pisoService.Listar().Where(p => EsActivo(p.Estado)).ToList();
            var pisoHotel = pisos.ToDictionary(p => p.IdPiso, p => p.IdHotel);
            var habitaciones = _habitacionService.Listar()
                .Where(h => EstadoHelper.Parse(h.Estado) == EstadoHabitacion.Disponible && h.Tarifa > 0 && pisoHotel.ContainsKey(h.IdPiso))
                .ToList();

            return hoteles.Select(h =>
            {
                var delHotel = habitaciones.Where(x => pisoHotel[x.IdPiso] == h.IdHotel).ToList();
                return new HotelTarjetaViewModel
                {
                    IdHotel = h.IdHotel,
                    Nombre = TextoHelper.Titulo(h.Nombre),
                    Direccion = TextoHelper.Titulo(h.Direccion),
                    Descripcion = h.Descripcion,
                    Categoria = h.Categoria,
                    Caracteristicas = h.Caracteristicas,
                    TotalBloques = bloques.Count(b => b.IdHotel == h.IdHotel),
                    TotalPisos = pisos.Count(p => p.IdHotel == h.IdHotel),
                    TotalHabitaciones = delHotel.Count,
                    CapacidadMaxima = delHotel.Any() ? delHotel.Max(x => x.Capacidad) : 0,
                    PrecioDesde = delHotel.Any() ? delHotel.Min(x => x.Tarifa) : (decimal?)null
                };
            }).ToList();
        }

        private static bool Contiene(string texto, string buscado)
        {
            return !string.IsNullOrEmpty(texto) && texto.IndexOf(buscado, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
