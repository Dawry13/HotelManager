using System;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Security;
using HotelManager.Business.Dtos;
using Roles = HotelManager.Business.Seguridad.Roles;

namespace HotelManager.Web.Infrastructure
{
    /// <summary>Identidad del usuario autenticado (RNF-001).</summary>
    public class UsuarioIdentidad : GenericIdentity
    {
        public int IdUsuario { get; private set; }
        public int? IdHuesped { get; private set; }
        public string Nombre { get; private set; }
        public string Rol { get; private set; }
        public bool DebeCambiarClave { get; private set; }

        public UsuarioIdentidad(string nombreUsuario, int idUsuario, int? idHuesped, string nombre, string rol, bool debeCambiarClave)
            : base(nombreUsuario, "HotelManager")
        {
            IdUsuario = idUsuario;
            IdHuesped = idHuesped;
            Nombre = nombre;
            Rol = rol;
            DebeCambiarClave = debeCambiarClave;
        }
    }

    /// <summary>
    /// Sesion basada en una cookie cifrada y firmada con MachineKey (no requiere configurar Web.config).
    /// La cookie es HttpOnly, dura 8 horas y se renueva mientras el usuario la usa.
    /// </summary>
    public static class Autenticacion
    {
        private const string NombreCookie = ".HotelManager.Sesion";
        private const string Proposito = "HotelManager.Autenticacion.v1";
        private static readonly TimeSpan Duracion = TimeSpan.FromHours(8);

        public static void IniciarSesion(HttpContextBase contexto, UsuarioDto usuario)
        {
            EscribirCookie(contexto.Response, contexto.Request, usuario.NombreUsuario, usuario.IdUsuario, usuario.IdHuesped,
                usuario.Nombre, Roles.Nombre(usuario.Rol), usuario.DebeCambiarClave);
        }

        /// <summary>Actualiza la cookie (por ejemplo, despues de cambiar la clave).</summary>
        public static void Renovar(HttpContextBase contexto, UsuarioIdentidad id, bool debeCambiarClave)
        {
            EscribirCookie(contexto.Response, contexto.Request, id.Name, id.IdUsuario, id.IdHuesped, id.Nombre, id.Rol, debeCambiarClave);
        }

        public static void CerrarSesion(HttpContextBase contexto)
        {
            contexto.Response.Cookies.Add(new HttpCookie(NombreCookie, string.Empty)
            {
                Expires = DateTime.UtcNow.AddDays(-1),
                HttpOnly = true,
                Path = "/"
            });
        }

        /// <summary>Se llama en cada peticion (Application_PostAuthenticateRequest) para cargar el usuario.</summary>
        public static void CargarUsuario(HttpContext contexto)
        {
            var cookie = contexto.Request.Cookies[NombreCookie];
            if (cookie == null || string.IsNullOrEmpty(cookie.Value)) return;

            try
            {
                var bytes = MachineKey.Unprotect(Convert.FromBase64String(cookie.Value), Proposito);
                if (bytes == null) return;

                // version|nombreUsuario|idUsuario|idHuesped|rol|debeCambiar|expiraUtcTicks|nombre
                var p = Encoding.UTF8.GetString(bytes).Split(new[] { '|' }, 8);
                if (p.Length != 8 || p[0] != "1") return;

                var expira = new DateTime(long.Parse(p[6]), DateTimeKind.Utc);
                if (expira < DateTime.UtcNow) return;

                var identidad = new UsuarioIdentidad(p[1], int.Parse(p[2]),
                    string.IsNullOrEmpty(p[3]) ? (int?)null : int.Parse(p[3]), p[7], p[4], p[5] == "1");
                contexto.User = new GenericPrincipal(identidad, new[] { identidad.Rol });

                // Expiracion deslizante: si paso mas de la mitad, se emite de nuevo.
                if (expira - DateTime.UtcNow < TimeSpan.FromTicks(Duracion.Ticks / 2))
                    EscribirCookie(new HttpResponseWrapper(contexto.Response), new HttpRequestWrapper(contexto.Request),
                        identidad.Name, identidad.IdUsuario, identidad.IdHuesped, identidad.Nombre, identidad.Rol, identidad.DebeCambiarClave);
            }
            catch (Exception)
            {
                // Cookie alterada, vieja o de otra maquina: se ignora y el usuario debe iniciar sesion.
            }
        }

        private static void EscribirCookie(HttpResponseBase response, HttpRequestBase request, string nombreUsuario, int idUsuario,
                                           int? idHuesped, string nombre, string rol, bool debeCambiarClave)
        {
            var datos = string.Join("|", "1", nombreUsuario, idUsuario.ToString(), idHuesped.HasValue ? idHuesped.Value.ToString() : "",
                rol, debeCambiarClave ? "1" : "0", DateTime.UtcNow.Add(Duracion).Ticks.ToString(), nombre ?? "");
            var protegido = MachineKey.Protect(Encoding.UTF8.GetBytes(datos), Proposito);

            response.Cookies.Add(new HttpCookie(NombreCookie, Convert.ToBase64String(protegido))
            {
                HttpOnly = true,
                Secure = request.IsSecureConnection,
                Path = "/"
            });
        }
    }
}
