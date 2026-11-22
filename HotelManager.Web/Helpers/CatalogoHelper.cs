using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using HotelManager.Business.Catalogos;

namespace HotelManager.Web.Helpers
{
    /// <summary>Estrellas de categoria y chips de caracteristicas para las vistas.</summary>
    public static class CatalogoHelper
    {
        public static IReadOnlyDictionary<string, string> CaracteristicasHotel { get { return Caracteristicas.DeHotel; } }
        public static IReadOnlyDictionary<string, string> CaracteristicasHabitacion { get { return Caracteristicas.DeHabitacion; } }

        public static string NombreCategoria(int categoria)
        {
            return Caracteristicas.NombreCategoria(categoria);
        }

        /// <summary>Cinco estrellas: las de la categoria rellenas, el resto vacias.</summary>
        public static IHtmlString Estrellas(int categoria, bool soloLlenas = true)
        {
            var sb = new StringBuilder("<span class=\"hm-stars\" title=\"" + HttpUtility.HtmlAttributeEncode(NombreCategoria(categoria)) + "\" aria-label=\"" + categoria + " estrellas\">");
            var total = soloLlenas ? categoria : 5;
            for (var i = 1; i <= total; i++)
                sb.Append(i <= categoria ? "<i class=\"bi bi-star-fill\"></i>" : "<i class=\"bi bi-star\"></i>");
            sb.Append("</span>");
            return new MvcHtmlString(sb.ToString());
        }

        public static string Icono(string caracteristica)
        {
            return Caracteristicas.Icono(caracteristica);
        }

        /// <summary>Lista de chips con icono; vacia si no hay caracteristicas.</summary>
        public static IHtmlString Chips(IEnumerable<string> caracteristicas, int maximo = int.MaxValue)
        {
            var lista = (caracteristicas ?? Enumerable.Empty<string>()).ToList();
            if (lista.Count == 0) return MvcHtmlString.Empty;

            var sb = new StringBuilder("<ul class=\"hm-feature-list\">");
            foreach (var c in lista.Take(maximo))
                sb.Append("<li><i class=\"bi " + Icono(c) + "\"></i>" + HttpUtility.HtmlEncode(c) + "</li>");
            if (lista.Count > maximo)
                sb.Append("<li class=\"mas\">+" + (lista.Count - maximo) + " más</li>");
            sb.Append("</ul>");
            return new MvcHtmlString(sb.ToString());
        }

        /// <summary>Une las casillas marcadas con las escritas a mano ("Terraza, Karaoke").</summary>
        public static List<string> Combinar(IEnumerable<string> marcadas, string otras)
        {
            var lista = new List<string>(marcadas ?? Enumerable.Empty<string>());
            if (!string.IsNullOrWhiteSpace(otras))
                lista.AddRange(otras.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(o => o.Trim()));
            return lista.Where(c => c.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        /// <summary>Las que no estan en el catalogo (para el campo "Otras").</summary>
        public static string Otras(IEnumerable<string> caracteristicas, IReadOnlyDictionary<string, string> catalogo)
        {
            return string.Join(", ", (caracteristicas ?? Enumerable.Empty<string>()).Where(c => !catalogo.ContainsKey(c)));
        }
    }
}
