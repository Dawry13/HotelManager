using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using HotelManager.Business.Dtos;
using HotelManager.Business.Exceptions;
using HotelManager.Business.Interfaces;
using HotelManager.Web.Helpers;

namespace HotelManager.Web.Controllers
{
    /// <summary>Reportes de ocupacion, ingresos y reservas por periodo.</summary>
    public class ReportesController : BaseController
    {
        private readonly IReporteService _reporteService;
        private readonly IReservaService _reservaService;
        private readonly IHotelService _hotelService;
        private readonly IBloqueService _bloqueService;
        private readonly IPisoService _pisoService;
        private readonly IHabitacionService _habitacionService;
        private readonly ITipoHabitacionService _tipoService;
        private readonly IHuespedService _huespedService;

        public ReportesController(IReporteService reporteService, IReservaService reservaService, IHotelService hotelService,
                                  IBloqueService bloqueService, IPisoService pisoService, IHabitacionService habitacionService,
                                  ITipoHabitacionService tipoService, IHuespedService huespedService)
        {
            _reporteService = reporteService;
            _reservaService = reservaService;
            _hotelService = hotelService;
            _bloqueService = bloqueService;
            _pisoService = pisoService;
            _habitacionService = habitacionService;
            _tipoService = tipoService;
            _huespedService = huespedService;
        }

        private static void RangoPorDefecto(ref DateTime? desde, ref DateTime? hasta)
        {
            var hoy = DateTime.Today;
            if (!desde.HasValue) desde = new DateTime(hoy.Year, hoy.Month, 1);
            if (!hasta.HasValue) hasta = desde.Value.AddMonths(1).AddDays(-1);
        }

        // GET: /Reportes?desde=&hasta=&idHotel=
        public ActionResult Index(DateTime? desde, DateTime? hasta, int? idHotel)
        {
            RangoPorDefecto(ref desde, ref hasta);
            ReporteGeneralDto reporte;
            try
            {
                reporte = _reporteService.Generar(desde.Value, hasta.Value, idHotel);
            }
            catch (BusinessException ex)
            {
                Error(ex.Message);
                return RedirectToAction("Index", new { idHotel });
            }

            ViewBag.Hoteles = HotelesSelect(_hotelService.Listar(), idHotel, false);
            ViewBag.IdHotel = idHotel;
            return View(reporte);
        }

        // GET: /Reportes/ExportarCsv  -> reservas del periodo en CSV (se abre en Excel)
        public ActionResult ExportarCsv(DateTime? desde, DateTime? hasta, int? idHotel)
        {
            RangoPorDefecto(ref desde, ref hasta);
            var ctx = ContextoHotel.Cargar(_hotelService, _bloqueService, _pisoService, _habitacionService, _tipoService, _huespedService);

            var reservas = _reservaService.Listar()
                .Where(r => r.FechaEntrada.Date >= desde.Value.Date && r.FechaEntrada.Date <= hasta.Value.Date)
                .Where(r => !idHotel.HasValue || ctx.IdHotelDeHabitacion(r.HabitacionId) == idHotel.Value)
                .OrderBy(r => r.FechaEntrada)
                .ToList();

            Func<string, string> campo = s => "\"" + (s ?? string.Empty).Replace("\"", "\"\"") + "\"";
            var csv = new StringBuilder();
            csv.AppendLine("Reserva;Huesped;Documento;Hotel;Habitacion;Tipo;Entrada;Salida;Noches;Huespedes;Costo;Estado");
            foreach (var r in reservas)
            {
                HuespedDto h;
                ctx.Huespedes.TryGetValue(r.HuespedId, out h);
                csv.AppendLine(string.Join(";", new[]
                {
                    r.Id.ToString(),
                    campo(ctx.Huesped(r.HuespedId)),
                    campo(h != null ? h.Documento : ""),
                    campo(ctx.HotelDeHabitacion(r.HabitacionId)),
                    campo(ctx.NumeroHabitacion(r.HabitacionId)),
                    campo(ctx.TipoDeHabitacion(r.HabitacionId)),
                    r.FechaEntrada.ToString("yyyy-MM-dd"),
                    r.FechaSalida.ToString("yyyy-MM-dd"),
                    (r.FechaSalida.Date - r.FechaEntrada.Date).Days.ToString(),
                    r.CantidadHuespedes.ToString(),
                    r.CostoTotal.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                    r.Estado
                }));
            }

            // BOM UTF-8 para que Excel muestre bien los acentos
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
            var nombre = "reservas_" + desde.Value.ToString("yyyyMMdd") + "_" + hasta.Value.ToString("yyyyMMdd") + ".csv";
            return File(bytes, "text/csv", nombre);
        }
    }
}
