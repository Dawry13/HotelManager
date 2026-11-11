using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace HotelManager.Business.Correo
{
    /// <summary>Envia correos con System.Net.Mail (SMTP) o los guarda como .eml en una carpeta (modo prueba).</summary>
    public class EnviadorCorreoSmtp : IEnviadorCorreo
    {
        public ConfiguracionCorreo Configuracion { get; private set; }

        public EnviadorCorreoSmtp() : this(ConfiguracionCorreo.Cargar()) { }

        public EnviadorCorreoSmtp(ConfiguracionCorreo configuracion)
        {
            Configuracion = configuracion;
        }

        public void Enviar(MensajeCorreo mensaje)
        {
            if (mensaje == null) throw new ArgumentNullException(nameof(mensaje));
            if (!Configuracion.Habilitado)
                throw new InvalidOperationException("El envío de correos está deshabilitado (Correo:Habilitado = false).");

            using (var mail = new MailMessage())
            {
                mail.From = new MailAddress(Configuracion.Remitente, Configuracion.NombreRemitente, Encoding.UTF8);
                mail.To.Add(new MailAddress(mensaje.Para, mensaje.NombrePara ?? string.Empty, Encoding.UTF8));
                mail.Subject = mensaje.Asunto;
                mail.SubjectEncoding = Encoding.UTF8;
                mail.HeadersEncoding = Encoding.UTF8;
                mail.BodyEncoding = Encoding.UTF8;

                // Texto plano + HTML: cada cliente de correo muestra la version que soporte.
                mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
                    mensaje.CuerpoTexto ?? string.Empty, Encoding.UTF8, MediaTypeNames.Text.Plain));
                mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
                    mensaje.CuerpoHtml ?? string.Empty, Encoding.UTF8, MediaTypeNames.Text.Html));

                using (var smtp = CrearCliente())
                {
                    try
                    {
                        smtp.Send(mail);
                    }
                    catch (SmtpException ex)
                    {
                        throw new InvalidOperationException(ExplicarError(ex), ex);
                    }
                }
            }
        }

        /// <summary>
        /// Traduce los errores SMTP mas comunes a un mensaje que indique que hacer.
        /// Gmail, cuando rechaza el usuario/clave, a veces no responde 535 sino que cierra la conexion
        /// (System.Net.Mail lo reporta como "net_io_connectionclosed" o "conexion interrumpida por el host remoto").
        /// </summary>
        private string ExplicarError(SmtpException ex)
        {
            var detalle = ex.InnerException != null ? ex.Message + " " + ex.InnerException.Message : ex.Message;
            var esGmail = (Configuracion.Servidor ?? string.Empty).IndexOf("gmail", StringComparison.OrdinalIgnoreCase) >= 0;

            var conexionCerrada = ex.InnerException is IOException
                || detalle.IndexOf("connectionclosed", StringComparison.OrdinalIgnoreCase) >= 0
                || detalle.IndexOf("host remoto", StringComparison.OrdinalIgnoreCase) >= 0
                || detalle.IndexOf("remote host", StringComparison.OrdinalIgnoreCase) >= 0;
            var autenticacion = ex.StatusCode == SmtpStatusCode.ClientNotPermitted
                || detalle.IndexOf("5.7.", StringComparison.OrdinalIgnoreCase) >= 0
                || detalle.IndexOf("Authentication", StringComparison.OrdinalIgnoreCase) >= 0
                || detalle.IndexOf("Username and Password", StringComparison.OrdinalIgnoreCase) >= 0;

            if (esGmail && (conexionCerrada || autenticacion))
                return "Gmail rechazó el inicio de sesión de " + Configuracion.Usuario + ". En Correo:Clave debe ir una " +
                       "\"contraseña de aplicación\" de 16 letras (Cuenta de Google > Seguridad > Verificación en 2 pasos > " +
                       "Contraseñas de aplicaciones), no la contraseña normal de la cuenta. Detalle: " + detalle;
            if (autenticacion)
                return "El servidor SMTP rechazó el usuario o la clave (Correo:Usuario / Correo:Clave). Detalle: " + detalle;
            if (conexionCerrada)
                return "El servidor SMTP cerró la conexión. Revise Correo:Usuario, Correo:Clave, el puerto (587 con Ssl=true) " +
                       "y que ningún antivirus bloquee el envío. Detalle: " + detalle;
            return detalle;
        }

        private SmtpClient CrearCliente()
        {
            if (!Configuracion.UsarSmtp)
            {
                Directory.CreateDirectory(Configuracion.Carpeta);
                return new SmtpClient
                {
                    DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                    PickupDirectoryLocation = Configuracion.Carpeta
                };
            }

            if (string.IsNullOrWhiteSpace(Configuracion.Usuario) || string.IsNullOrWhiteSpace(Configuracion.Clave))
                throw new InvalidOperationException("Falta configurar Correo:Usuario y Correo:Clave para enviar por SMTP.");

            // Gmail, Outlook y la mayoria de proveedores exigen TLS 1.2
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            return new SmtpClient(Configuracion.Servidor, Configuracion.Puerto)
            {
                EnableSsl = Configuracion.Ssl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(Configuracion.Usuario, Configuracion.Clave),
                Timeout = 20000
            };
        }
    }
}
