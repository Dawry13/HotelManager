using System;
using System.Configuration;
using System.IO;

namespace HotelManager.Business.Correo
{
    /// <summary>
    /// Configuracion del envio de correos, leida de appSettings (Web.config / Correo.config).
    ///
    ///   Correo:Habilitado      true | false
    ///   Correo:Modo            Smtp    -> envia por el servidor SMTP indicado
    ///                          Carpeta -> guarda cada correo como .eml en Correo:Carpeta (para pruebas)
    ///   Correo:Carpeta         ~/App_Data/Correos
    ///   Correo:Remitente       direccion "from"
    ///   Correo:NombreRemitente nombre visible del remitente
    ///   Correo:Servidor, Correo:Puerto, Correo:Ssl, Correo:Usuario, Correo:Clave
    /// </summary>
    public class ConfiguracionCorreo
    {
        public bool Habilitado { get; set; }
        public bool UsarSmtp { get; set; }
        public string Carpeta { get; set; }
        public string Remitente { get; set; }
        public string NombreRemitente { get; set; }
        public string Servidor { get; set; }
        public int Puerto { get; set; }
        public bool Ssl { get; set; }
        public string Usuario { get; set; }
        public string Clave { get; set; }

        public static ConfiguracionCorreo Cargar()
        {
            Func<string, string, string> leer = (clave, defecto) =>
            {
                var valor = ConfigurationManager.AppSettings["Correo:" + clave];
                return string.IsNullOrWhiteSpace(valor) ? defecto : valor.Trim();
            };

            int puerto;
            var config = new ConfiguracionCorreo
            {
                Habilitado = !string.Equals(leer("Habilitado", "true"), "false", StringComparison.OrdinalIgnoreCase),
                UsarSmtp = string.Equals(leer("Modo", "Carpeta"), "Smtp", StringComparison.OrdinalIgnoreCase),
                Carpeta = ResolverRuta(leer("Carpeta", "~/App_Data/Correos")),
                Remitente = leer("Remitente", "reservas@hotelmanager.local"),
                NombreRemitente = leer("NombreRemitente", "HotelManager Reservas"),
                Servidor = leer("Servidor", "smtp.gmail.com"),
                Puerto = int.TryParse(leer("Puerto", "587"), out puerto) ? puerto : 587,
                Ssl = !string.Equals(leer("Ssl", "true"), "false", StringComparison.OrdinalIgnoreCase),
                Usuario = leer("Usuario", null),
                Clave = leer("Clave", null)
            };
            return config;
        }

        /// <summary>Texto para mostrar en pantalla como esta configurado el envio.</summary>
        public string Descripcion
        {
            get
            {
                if (!Habilitado) return "Envío de correos deshabilitado (las notificaciones quedan pendientes).";
                return UsarSmtp
                    ? "SMTP " + Servidor + ":" + Puerto + (Ssl ? " (SSL)" : "") + " · remitente " + Remitente
                    : "Modo de prueba: los correos NO se envían a internet; se guardan como archivos .eml en " + Carpeta;
            }
        }

        // "~/App_Data/Correos" -> ruta fisica dentro de la aplicacion
        private static string ResolverRuta(string ruta)
        {
            if (ruta.StartsWith("~/") || ruta.StartsWith("~\\"))
                ruta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ruta.Substring(2).Replace('/', Path.DirectorySeparatorChar));
            return ruta;
        }
    }
}
