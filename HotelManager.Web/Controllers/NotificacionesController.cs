using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HotelManager.Web.Infrastructure;
using HotelManager.Business.Seguridad;
using HotelManager.Business.Dtos;
using HotelManager.Business.Interfaces;
using HotelManager.Web.Helpers;

namespace HotelManager.Web.Controllers
{
    /// <summary>
    /// RF-014 / RF-015: seguimiento de las notificaciones de confirmacion, modificacion y cancelacion.
    /// El envio real (correo) es externo; aqui se registra su resultado (RN-014, RN-015).
    /// </summary>
    [Autorizar(Roles.Administrador, Roles.Recepcionista, Roles.ServicioNotificacion)]
    public class NotificacionesController : BaseController
    {
        private readonly INotificacionService _notificacionService;
        private readonly IReservaService _reservaService;
        private readonly IHuespedService _huespedService;
        private readonly IHotelService _hotelService;
        private readonly IBloqueService _bloqueService;
        private readonly IPisoService _pisoService;
        private readonly IHabitacionService _habitacionService;
        private readonly ITipoHabitacionService _tipoService;

        public NotificacionesController(INotificacionService notificacionService, IReservaService reservaService,
                                        IHuespedService huespedService, IHotelService hotelService, IBloqueService bloqueService,
                                        IPisoService pisoService, IHabitacionService habitacionService, ITipoHabitacionService tipoService)
        {
            _notificacionService = notificacionService;
            _reservaService = reservaService;
            _huespedService = huespedService;
            _hotelService = hotelService;
            _bloqueService = bloqueService;
            _pisoService = pisoService;
            _habitacionService = habitacionService;
            _tipoService = tipoService;
        }

        public class NotificacionItem
        {
            public NotificacionDto Notificacion { get; set; }
            public string Huesped { get; set; }
            public string Correo { get; set; }
            public string Habitacion { get; set; }
        }

        // GET: /Notificaciones?estado=&tipo=
        public ActionResult Index(string estado, string tipo)
        {
            var ctx = ContextoHotel.Cargar(_hotelService, _bloqueService, _pisoService, _habitacionService, _tipoService, _huespedService);
            var reservas = _reservaService.Listar().ToDictionary(r => r.Id);
            var todas = _notificacionService.Listar().ToList();

            var lista = todas
                .Where(n => string.IsNullOrEmpty(estado) || n.Estado == estado)
                .Where(n => string.IsNullOrEmpty(tipo) || n.Tipo == tipo)
                .Select(n =>
                {
                    ReservaDto r;
                    reservas.TryGetValue(n.IdReserva, out r);
                    HuespedDto h = null;
                    if (r != null) ctx.Huespedes.TryGetValue(r.HuespedId, out h);
                    return new NotificacionItem
                    {
                        Notificacion = n,
                        Huesped = h != null ? TextoHelper.Titulo(h.Nombre) : "",
                        Correo = h != null ? h.Correo : "",
                        Habitacion = r != null ? "Hab. " + ctx.NumeroHabitacion(r.HabitacionId) + " · " + ctx.HotelDeHabitacion(r.HabitacionId) : ""
                    };
                })
                .ToList();

            ViewBag.Conteos = todas.GroupBy(n => n.Estado).ToDictionary(g => g.Key, g => g.Count());
            ViewBag.Estado = estado;
            ViewBag.Tipo = tipo;
            ViewBag.DescripcionEnvio = _notificacionService.DescripcionEnvio;
            ViewBag.ModoPrueba = _notificacionService.ModoPrueba;
            return View(lista);
        }

        // POST: /Notificaciones/Enviar/5  -> envia o reenvia el correo al huesped
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Enviar(int id, string returnUrl)
        {
            EjecutarConMensaje(() => _notificacionService.Reintentar(id),
                _notificacionService.ModoPrueba
                    ? "Modo de prueba: el correo se guardó como archivo .eml (no se envió a internet)."
                    : "Correo enviado al huésped.");
            return Volver(returnUrl);
        }

        // POST: /Notificaciones/EnviarPendientes
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult EnviarPendientes()
        {
            var pendientes = _notificacionService.ContarPendientes();
            var enviadas = 0;
            EjecutarConMensaje(() => enviadas = _notificacionService.EnviarPendientes(), "Envío de pendientes terminado.");
            if (enviadas < pendientes)
                Error(enviadas + " de " + pendientes + " correo(s) enviado(s). Revise el motivo de los que fallaron.");
            else
                Exito(enviadas + " correo(s) enviado(s).");
            return RedirectToAction("Index");
        }

        // POST: /Notificaciones/MarcarFallida/5  (registro manual; RN-015: el fallo no afecta la reserva)
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult MarcarFallida(int id, string detalle, string returnUrl)
        {
            EjecutarConMensaje(() => _notificacionService.RegistrarResultado(id, false, detalle),
                "Fallo registrado. La reserva se conserva sin cambios (RN-015).");
            return Volver(returnUrl);
        }

        private ActionResult Volver(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction("Index");
        }
    }
}
