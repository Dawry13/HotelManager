using System.Collections.Generic;
using System.Linq;
using HotelManager.Business.Dtos;
using HotelManager.Business.Interfaces;

namespace HotelManager.Web.Helpers
{
    /// <summary>
    /// Diccionarios de apoyo para mostrar nombres legibles (huesped, habitacion, hotel)
    /// en los modulos de reservas, huespedes, tarifas y notificaciones.
    /// </summary>
    public class ContextoHotel
    {
        public Dictionary<int, HotelDto> Hoteles { get; private set; }
        public Dictionary<int, BloqueDto> Bloques { get; private set; }
        public Dictionary<int, PisoDto> Pisos { get; private set; }
        public Dictionary<int, HabitacionDto> Habitaciones { get; private set; }
        public Dictionary<int, TipoHabitacionDto> Tipos { get; private set; }
        public Dictionary<int, HuespedDto> Huespedes { get; private set; }

        public static ContextoHotel Cargar(IHotelService hoteles, IBloqueService bloques, IPisoService pisos,
                                           IHabitacionService habitaciones, ITipoHabitacionService tipos,
                                           IHuespedService huespedes)
        {
            return new ContextoHotel
            {
                Hoteles = hoteles.Listar().ToDictionary(h => h.IdHotel),
                Bloques = bloques.Listar().ToDictionary(b => b.IdBloque),
                Pisos = pisos.Listar().ToDictionary(p => p.IdPiso),
                Habitaciones = habitaciones.Listar().ToDictionary(h => h.IdHabitacion),
                Tipos = tipos.Listar().ToDictionary(t => t.IdTipoHabitacion),
                Huespedes = huespedes != null ? huespedes.Listar().ToDictionary(h => h.IdHuesped) : new Dictionary<int, HuespedDto>()
            };
        }

        public int IdHotelDeHabitacion(int idHabitacion)
        {
            HabitacionDto hab;
            PisoDto piso;
            return Habitaciones.TryGetValue(idHabitacion, out hab) && Pisos.TryGetValue(hab.IdPiso, out piso) ? piso.IdHotel : 0;
        }

        public string NumeroHabitacion(int idHabitacion)
        {
            HabitacionDto hab;
            return Habitaciones.TryGetValue(idHabitacion, out hab) ? hab.Numero : "#" + idHabitacion;
        }

        public string Hotel(int idHotel)
        {
            HotelDto h;
            return Hoteles.TryGetValue(idHotel, out h) ? TextoHelper.Titulo(h.Nombre) : "";
        }

        public string HotelDeHabitacion(int idHabitacion)
        {
            return Hotel(IdHotelDeHabitacion(idHabitacion));
        }

        /// <summary>"Torre A - Piso 1"</summary>
        public string UbicacionHabitacion(int idHabitacion)
        {
            HabitacionDto hab;
            PisoDto piso;
            if (!Habitaciones.TryGetValue(idHabitacion, out hab) || !Pisos.TryGetValue(hab.IdPiso, out piso)) return "";
            BloqueDto bloque;
            return piso.IdBloque.HasValue && Bloques.TryGetValue(piso.IdBloque.Value, out bloque)
                ? TextoHelper.Titulo(bloque.Nombre) + " - Piso " + piso.Numero
                : "Piso " + piso.Numero;
        }

        public string TipoDeHabitacion(int idHabitacion)
        {
            HabitacionDto hab;
            TipoHabitacionDto tipo;
            return Habitaciones.TryGetValue(idHabitacion, out hab) && hab.IdTipoHabitacion.HasValue
                   && Tipos.TryGetValue(hab.IdTipoHabitacion.Value, out tipo)
                ? TextoHelper.Titulo(tipo.Nombre) : "—";
        }

        public string Huesped(int idHuesped)
        {
            HuespedDto h;
            return Huespedes.TryGetValue(idHuesped, out h) ? TextoHelper.Titulo(h.Nombre) : "Huésped #" + idHuesped;
        }

        public static string Iniciales(string nombre)
        {
            return new string((nombre ?? "?").Split(' ').Where(p => p.Length > 0).Take(2)
                .Select(p => char.ToUpper(p[0])).ToArray());
        }
    }
}
