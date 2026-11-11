using System;
using System.Globalization;
using System.Net;
using System.Text;
using HotelManager.Domain.Enums;

namespace HotelManager.Business.Correo
{
    /// <summary>Datos de la reserva que se incluyen en el correo.</summary>
    public class DatosCorreoReserva
    {
        public TipoNotificacion Tipo { get; set; }
        public int IdReserva { get; set; }
        public string Huesped { get; set; }
        public string Documento { get; set; }
        public string Correo { get; set; }
        public string Hotel { get; set; }
        public string DireccionHotel { get; set; }
        public string Habitacion { get; set; }
        public string TipoHabitacion { get; set; }
        public string Ubicacion { get; set; }
        public DateTime Entrada { get; set; }
        public DateTime Salida { get; set; }
        public int Huespedes { get; set; }
        public decimal CostoTotal { get; set; }
        public string Estado { get; set; }
        public int Noches { get { return (Salida.Date - Entrada.Date).Days; } }
    }

    /// <summary>
    /// Arma el correo de confirmacion, modificacion o cancelacion de una reserva.
    /// HTML con estilos en linea y tablas (compatible con Gmail, Outlook y moviles) + version en texto plano.
    /// </summary>
    public static class PlantillaCorreoReserva
    {
        private static readonly CultureInfo Cultura = new CultureInfo("es-DO");

        public static MensajeCorreo Construir(DatosCorreoReserva d)
        {
            string titulo, intro, color, etiqueta;
            switch (d.Tipo)
            {
                case TipoNotificacion.Modificacion:
                    titulo = "Su reserva fue modificada";
                    intro = "le confirmamos que su reserva fue actualizada. Estos son los nuevos detalles de su estadía:";
                    color = "#7c3aed"; etiqueta = "MODIFICADA";
                    break;
                case TipoNotificacion.Cancelacion:
                    titulo = "Su reserva fue cancelada";
                    intro = "le informamos que su reserva fue cancelada. Estos eran los detalles de la estadía:";
                    color = "#dc2626"; etiqueta = "CANCELADA";
                    break;
                default:
                    titulo = "¡Su reserva está confirmada!";
                    intro = "gracias por elegirnos. Su reserva fue registrada y confirmada con los siguientes detalles:";
                    color = "#059669"; etiqueta = "CONFIRMADA";
                    break;
            }

            var asunto = (d.Tipo == TipoNotificacion.Confirmacion ? "Confirmación" : d.Tipo == TipoNotificacion.Modificacion ? "Modificación" : "Cancelación")
                         + " de reserva #" + d.IdReserva + " · " + Titulo(d.Hotel);

            return new MensajeCorreo
            {
                Para = d.Correo,
                NombrePara = Titulo(d.Huesped),
                Asunto = asunto,
                CuerpoHtml = Html(d, titulo, intro, color, etiqueta),
                CuerpoTexto = Texto(d, titulo, intro)
            };
        }

        private static string Html(DatosCorreoReserva d, string titulo, string intro, string color, string etiqueta)
        {
            Func<string, string> e = s => WebUtility.HtmlEncode(s ?? string.Empty);
            Func<string, string, string> fila = (k, v) =>
                "<tr><td style=\"padding:10px 0;color:#6b7280;font-size:14px;border-bottom:1px solid #eef0f4;width:45%\">" + e(k) + "</td>" +
                "<td style=\"padding:10px 0;color:#111827;font-size:14px;font-weight:600;border-bottom:1px solid #eef0f4;text-align:right\">" + v + "</td></tr>";

            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE html><html lang=\"es\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\"><title>")
              .Append(e(titulo)).Append("</title></head>");
            sb.Append("<body style=\"margin:0;padding:0;background:#f3f4f8;font-family:'Segoe UI',Arial,Helvetica,sans-serif\">");
            sb.Append("<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"background:#f3f4f8;padding:24px 12px\"><tr><td align=\"center\">");
            sb.Append("<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"max-width:600px;background:#ffffff;border-radius:14px;overflow:hidden;box-shadow:0 4px 18px rgba(16,24,40,.08)\">");

            // Encabezado
            // Encabezado igual al logo de la aplicacion: fondo azul-morado oscuro (solido, compatible con Gmail),
            // "Hotel" en blanco, "Manager" en dorado y el lema en lila, alineados a la izquierda.
            sb.Append("<tr><td bgcolor=\"#16133a\" align=\"left\" style=\"background-color:#16133a;padding:26px 32px 24px;text-align:left\">")
              .Append("<div style=\"font-family:'Poppins','Segoe UI',Arial,Helvetica,sans-serif;font-size:34px;font-weight:700;letter-spacing:-0.5px;line-height:1.1\">")
              .Append("<font color=\"#ffffff\"><span style=\"color:#ffffff\">Hotel</span></font>")
              .Append("<font color=\"#ffb000\"><span style=\"color:#ffb000\">Manager</span></font></div>")
              .Append("<div style=\"font-family:'Poppins','Segoe UI',Arial,Helvetica,sans-serif;font-size:16px;color:#8f8cf5;margin-top:4px\">")
              .Append("<font color=\"#8f8cf5\">Hospitalidad, en tus manos</font></div></td></tr>");

            // Titulo
            sb.Append("<tr><td style=\"padding:28px 32px 8px\">")
              .Append("<span style=\"display:inline-block;background:").Append(color).Append(";color:#fff;font-size:11px;font-weight:700;letter-spacing:.08em;padding:5px 12px;border-radius:999px\">")
              .Append(etiqueta).Append("</span>")
              .Append("<h1 style=\"margin:14px 0 6px;font-size:22px;color:#111827\">").Append(e(titulo)).Append("</h1>")
              .Append("<p style=\"margin:0;font-size:15px;color:#374151;line-height:1.5\">Hola <strong>").Append(e(Titulo(d.Huesped)))
              .Append("</strong>, ").Append(e(intro)).Append("</p></td></tr>");

            // Numero de reserva y total
            sb.Append("<tr><td style=\"padding:18px 32px\"><table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"background:#f8f7ff;border:1px solid #e9e5ff;border-radius:12px\"><tr>")
              .Append("<td style=\"padding:16px 18px\"><div style=\"font-size:12px;color:#6b7280\">Número de reserva</div><div style=\"font-size:22px;font-weight:700;color:#3b1d8f\">#").Append(d.IdReserva).Append("</div></td>")
              .Append("<td style=\"padding:16px 18px;text-align:right\"><div style=\"font-size:12px;color:#6b7280\">Total de la estadía</div><div style=\"font-size:22px;font-weight:700;color:#b45309\">RD$ ")
              .Append(d.CostoTotal.ToString("N2", Cultura)).Append("</div></td></tr></table></td></tr>");

            // Detalles
            sb.Append("<tr><td style=\"padding:4px 32px 8px\"><table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\">")
              .Append(fila("Hotel", e(Titulo(d.Hotel))))
              .Append(string.IsNullOrWhiteSpace(d.DireccionHotel) ? "" : fila("Dirección", e(Titulo(d.DireccionHotel))))
              .Append(fila("Habitación", e(d.Habitacion) + (string.IsNullOrWhiteSpace(d.TipoHabitacion) ? "" : " · " + e(Titulo(d.TipoHabitacion)))))
              .Append(string.IsNullOrWhiteSpace(d.Ubicacion) ? "" : fila("Ubicación", e(d.Ubicacion)))
              .Append(fila("Entrada", e(Fecha(d.Entrada))))
              .Append(fila("Salida", e(Fecha(d.Salida))))
              .Append(fila("Noches", d.Noches.ToString()))
              .Append(fila("Huéspedes", d.Huespedes.ToString()))
              .Append(fila("Titular", e(Titulo(d.Huesped)) + (string.IsNullOrWhiteSpace(d.Documento) ? "" : "<br><span style=\"font-weight:400;color:#6b7280\">" + e(d.Documento) + "</span>")))
              .Append(fila("Estado", "<span style=\"color:" + color + "\">" + e(d.Estado) + "</span>"))
              .Append("</table></td></tr>");

            // Nota final
            var nota = d.Tipo == TipoNotificacion.Cancelacion
                ? "Si no solicitó esta cancelación o desea hacer una nueva reserva, comuníquese con nosotros."
                : "Presente este correo o su documento de identidad al llegar. Si necesita modificar o cancelar su reserva, comuníquese con la recepción del hotel.";
            sb.Append("<tr><td style=\"padding:18px 32px 28px\"><p style=\"margin:0;font-size:13px;color:#6b7280;line-height:1.5\">")
              .Append(e(nota)).Append("</p></td></tr>");

            // Pie
            sb.Append("<tr><td style=\"background:#f8f9fb;padding:16px 32px;font-size:11px;color:#9ca3af;text-align:center\">")
              .Append("Este es un mensaje automático del Sistema de Gestión de Reservas HotelManager. Por favor no responda a este correo.")
              .Append("</td></tr>");

            sb.Append("</table></td></tr></table></body></html>");
            return sb.ToString();
        }

        private static string Texto(DatosCorreoReserva d, string titulo, string intro)
        {
            var sb = new StringBuilder();
            sb.AppendLine("HotelManager - " + titulo);
            sb.AppendLine();
            sb.AppendLine("Hola " + Titulo(d.Huesped) + ", " + intro);
            sb.AppendLine();
            sb.AppendLine("Reserva:     #" + d.IdReserva);
            sb.AppendLine("Hotel:       " + Titulo(d.Hotel));
            if (!string.IsNullOrWhiteSpace(d.DireccionHotel)) sb.AppendLine("Dirección:   " + Titulo(d.DireccionHotel));
            sb.AppendLine("Habitación:  " + d.Habitacion + (string.IsNullOrWhiteSpace(d.TipoHabitacion) ? "" : " (" + Titulo(d.TipoHabitacion) + ")"));
            sb.AppendLine("Entrada:     " + Fecha(d.Entrada));
            sb.AppendLine("Salida:      " + Fecha(d.Salida));
            sb.AppendLine("Noches:      " + d.Noches);
            sb.AppendLine("Huéspedes:   " + d.Huespedes);
            sb.AppendLine("Total:       RD$ " + d.CostoTotal.ToString("N2", Cultura));
            sb.AppendLine("Estado:      " + d.Estado);
            sb.AppendLine();
            sb.AppendLine("Este es un mensaje automático. Por favor no responda a este correo.");
            return sb.ToString();
        }

        private static string Fecha(DateTime f)
        {
            var texto = f.ToString("dddd d 'de' MMMM 'de' yyyy", Cultura);
            return char.ToUpper(texto[0]) + texto.Substring(1);
        }

        // Los datos se guardan en minuscula en la base: "hotel caribe" -> "Hotel Caribe"
        private static string Titulo(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? s : Cultura.TextInfo.ToTitleCase(s.Trim());
        }
    }
}
