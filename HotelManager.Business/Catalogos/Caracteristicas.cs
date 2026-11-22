using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelManager.Business.Catalogos
{
    /// <summary>
    /// Caracteristicas (amenidades) sugeridas para hoteles y habitaciones, y conversion
    /// entre la lista que usa la aplicacion y el texto separado por ';' que guarda la base.
    /// </summary>
    public static class Caracteristicas
    {
        public const int CategoriaMinima = 3;
        public const int CategoriaMaxima = 5;
        private const char Separador = ';';
        private const int LargoMaximo = 500;

        /// <summary>Nombre visible e icono (Bootstrap Icons) de las amenidades de un hotel.</summary>
        public static readonly IReadOnlyDictionary<string, string> DeHotel = new Dictionary<string, string>
        {
            { "Wi-Fi gratis", "bi-wifi" },
            { "Piscina", "bi-water" },
            { "Playa", "bi-umbrella" },
            { "Spa", "bi-flower1" },
            { "Gimnasio", "bi-heart-pulse" },
            { "Restaurante", "bi-egg-fried" },
            { "Bar", "bi-cup-straw" },
            { "Desayuno incluido", "bi-cup-hot" },
            { "Parqueo", "bi-p-square" },
            { "Servicio a la habitación", "bi-bell" },
            { "Recepción 24 horas", "bi-clock" },
            { "Traslado al aeropuerto", "bi-airplane" },
            { "Admite mascotas", "bi-heart" },
            { "Salón de eventos", "bi-easel" },
            { "Club infantil", "bi-balloon" },
            { "Accesible", "bi-universal-access" }
        };

        /// <summary>Nombre visible e icono de las caracteristicas de una habitacion.</summary>
        public static readonly IReadOnlyDictionary<string, string> DeHabitacion = new Dictionary<string, string>
        {
            { "Aire acondicionado", "bi-snow" },
            { "Wi-Fi", "bi-wifi" },
            { "Smart TV", "bi-tv" },
            { "Minibar", "bi-cup-straw" },
            { "Caja fuerte", "bi-safe" },
            { "Balcón", "bi-door-open" },
            { "Vista al mar", "bi-water" },
            { "Jacuzzi", "bi-droplet" },
            { "Cafetera", "bi-cup-hot" },
            { "Escritorio", "bi-laptop" },
            { "Cama king", "bi-moon-stars" },
            { "Sofá cama", "bi-lamp" },
            { "Bañera", "bi-droplet-half" },
            { "No fumadores", "bi-slash-circle" }
        };

        public static string Icono(string caracteristica)
        {
            string icono;
            if (caracteristica == null) return "bi-check2";
            if (DeHotel.TryGetValue(caracteristica, out icono)) return icono;
            if (DeHabitacion.TryGetValue(caracteristica, out icono)) return icono;
            return "bi-check2";
        }

        /// <summary>Convierte el texto guardado ("Wi-Fi;Piscina") en lista.</summary>
        public static List<string> Separar(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return new List<string>();
            return texto.Split(Separador)
                .Select(c => c.Trim())
                .Where(c => c.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>Une la lista en el texto que se guarda (null si esta vacia).</summary>
        public static string Unir(IEnumerable<string> lista)
        {
            if (lista == null) return null;
            var limpias = lista
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.Replace(Separador.ToString(), " ").Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (limpias.Count == 0) return null;

            var texto = string.Join(Separador.ToString(), limpias);
            return texto.Length <= LargoMaximo ? texto : texto.Substring(0, LargoMaximo);
        }

        public static string NombreCategoria(int categoria)
        {
            switch (categoria)
            {
                case 5: return "5 estrellas · Lujo";
                case 4: return "4 estrellas · Superior";
                default: return "3 estrellas · Confort";
            }
        }
    }
}
