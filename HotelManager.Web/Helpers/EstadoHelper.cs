using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HotelManager.Domain.Enums;

namespace HotelManager.Web.Helpers
{
    /// <summary>Formato de textos para mostrar (los datos se guardan en minuscula).</summary>
    public static class TextoHelper
    {
        private static readonly System.Globalization.TextInfo Info =
            new System.Globalization.CultureInfo("es-DO").TextInfo;

        /// <summary>"hotel caribe santo domingo" -> "Hotel Caribe Santo Domingo".</summary>
        public static string Titulo(string texto)
        {
            return string.IsNullOrWhiteSpace(texto) ? texto : Info.ToTitleCase(texto.Trim());
        }
    }

    /// <summary>Textos y colores para mostrar estados en las vistas.</summary>
    public static class EstadoHelper
    {
        public static string Texto(EstadoHabitacion estado)
        {
            switch (estado)
            {
                case EstadoHabitacion.Disponible: return "Disponible";
                case EstadoHabitacion.Mantenimiento: return "En mantenimiento";
                case EstadoHabitacion.FueraDeServicio: return "Fuera de servicio";
                case EstadoHabitacion.Inactiva: return "Inactiva";
                default: return estado.ToString();
            }
        }

        public static string Css(EstadoHabitacion estado)
        {
            switch (estado)
            {
                case EstadoHabitacion.Disponible: return "ok";
                case EstadoHabitacion.Mantenimiento: return "warn";
                case EstadoHabitacion.FueraDeServicio: return "bad";
                default: return "off";
            }
        }

        /// <summary>Clase de badge (hm-badge) para estados Activo/Inactivo.</summary>
        public static string CssActivo(string estado)
        {
            return string.Equals(estado, "Activo", StringComparison.OrdinalIgnoreCase) ? "ok" : "off";
        }

        /// <summary>Clase de badge (hm-badge) para estados de reserva.</summary>
        public static string CssReserva(string estado)
        {
            switch ((estado ?? string.Empty).ToLower())
            {
                case "confirmada": return "ok";
                case "pendiente": return "info";
                case "modificada": return "purple";
                case "cancelada": return "bad";
                case "finalizada": return "gold";
                default: return "off";
            }
        }

        /// <summary>Clase de badge (hm-badge) para estados de notificacion.</summary>
        public static string CssNotificacion(string estado)
        {
            switch ((estado ?? string.Empty).ToLower())
            {
                case "enviada": return "ok";
                case "pendiente": return "info";
                case "fallida": return "bad";
                default: return "off";
            }
        }

        public static EstadoHabitacion Parse(string estado)
        {
            EstadoHabitacion valor;
            return Enum.TryParse(estado, out valor) ? valor : EstadoHabitacion.Inactiva;
        }

        public static IEnumerable<SelectListItem> OpcionesHabitacion(EstadoHabitacion? seleccionado)
        {
            return Enum.GetValues(typeof(EstadoHabitacion)).Cast<EstadoHabitacion>()
                .Select(e => new SelectListItem
                {
                    Value = e.ToString(),
                    Text = Texto(e),
                    Selected = seleccionado.HasValue && seleccionado.Value == e
                });
        }
    }
}
