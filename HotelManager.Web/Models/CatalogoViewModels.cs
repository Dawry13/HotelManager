using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HotelManager.Business.Dtos;

namespace HotelManager.Web.Models
{
    // =========================================================
    // Catalogo publico (huespedes e invitados, sin registro)
    // =========================================================

    /// <summary>Tarjeta de un hotel en el catalogo.</summary>
    public class HotelTarjetaViewModel
    {
        public int IdHotel { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string Descripcion { get; set; }
        public int Categoria { get; set; }
        public List<string> Caracteristicas { get; set; } = new List<string>();
        public int TotalBloques { get; set; }
        public int TotalPisos { get; set; }
        public int TotalHabitaciones { get; set; }
        public int CapacidadMaxima { get; set; }
        /// <summary>Tarifa mas baja de sus habitaciones disponibles (null si no tiene).</summary>
        public decimal? PrecioDesde { get; set; }
    }

    public class CatalogoIndexViewModel
    {
        public string Buscar { get; set; }
        public int? Categoria { get; set; }
        public List<HotelTarjetaViewModel> Hoteles { get; set; } = new List<HotelTarjetaViewModel>();
        /// <summary>Cantidad de hoteles activos por categoria (3, 4, 5) para las pestañas.</summary>
        public Dictionary<int, int> PorCategoria { get; set; } = new Dictionary<int, int>();
        public int Total { get; set; }
    }

    public class HabitacionCatalogoViewModel
    {
        public int IdHabitacion { get; set; }
        public string Numero { get; set; }
        public string Tipo { get; set; }
        public string DescripcionTipo { get; set; }
        public int Capacidad { get; set; }
        public decimal Tarifa { get; set; }
        public List<string> Caracteristicas { get; set; } = new List<string>();

        // Solo cuando se consulto disponibilidad
        public bool? DisponibleEnFechas { get; set; }
        public decimal? CostoTotal { get; set; }
        public decimal? TarifaPromedio { get; set; }
    }

    public class PisoCatalogoViewModel
    {
        public int IdPiso { get; set; }
        public int Numero { get; set; }
        public List<HabitacionCatalogoViewModel> Habitaciones { get; set; } = new List<HabitacionCatalogoViewModel>();
    }

    public class BloqueCatalogoViewModel
    {
        public int? IdBloque { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public List<PisoCatalogoViewModel> Pisos { get; set; } = new List<PisoCatalogoViewModel>();
        public int TotalHabitaciones { get { return Pisos.Sum(p => p.Habitaciones.Count); } }
    }

    public class CatalogoHotelViewModel
    {
        public HotelTarjetaViewModel Hotel { get; set; }
        public List<BloqueCatalogoViewModel> Bloques { get; set; } = new List<BloqueCatalogoViewModel>();

        public DateTime Entrada { get; set; }
        public DateTime Salida { get; set; }
        public int Huespedes { get; set; }
        public bool Consultado { get; set; }
        public string Error { get; set; }
        public int Noches { get { return Math.Max(0, (Salida.Date - Entrada.Date).Days); } }
        public int Disponibles { get; set; }
    }

    /// <summary>Resumen antes de enviar la solicitud de reserva del huesped.</summary>
    public class ConfirmarReservaViewModel
    {
        public int IdHotel { get; set; }
        public string Hotel { get; set; }
        public int Categoria { get; set; }
        public string Direccion { get; set; }
        public int IdHabitacion { get; set; }
        public string Habitacion { get; set; }
        public string Tipo { get; set; }
        public string Ubicacion { get; set; }
        public List<string> Caracteristicas { get; set; } = new List<string>();
        public DateTime Entrada { get; set; }
        public DateTime Salida { get; set; }
        public int Huespedes { get; set; }
        public int Noches { get { return Math.Max(0, (Salida.Date - Entrada.Date).Days); } }
        public decimal TarifaPromedio { get; set; }
        public decimal CostoTotal { get; set; }
        public HuespedDto Huesped { get; set; }
    }

    /// <summary>Casillas de caracteristicas en los formularios de hotel y habitacion.</summary>
    public class CaracteristicasSelectorViewModel
    {
        public string Nombre { get; set; } = "Caracteristicas";
        public string NombreOtras { get; set; } = "OtrasCaracteristicas";
        public IReadOnlyDictionary<string, string> Opciones { get; set; }
        public IEnumerable<string> Seleccionadas { get; set; } = new List<string>();
        public string Otras { get; set; }
        public string Prefijo { get; set; } = "car";
    }

    // =========================================================
    // Centro de gestion del hotel (administrador / recepcion)
    // =========================================================

    public class HotelGestionViewModel
    {
        public HotelViewModel Hotel { get; set; }
        public List<BloqueDto> Bloques { get; set; } = new List<BloqueDto>();
        public List<PisoDto> Pisos { get; set; } = new List<PisoDto>();
        public Dictionary<int, BloqueDto> BloquesPorId { get; set; } = new Dictionary<int, BloqueDto>();
        public Dictionary<int, List<HabitacionDto>> HabitacionesPorPiso { get; set; } = new Dictionary<int, List<HabitacionDto>>();
        public Dictionary<int, string> EtiquetaPiso { get; set; } = new Dictionary<int, string>();
        public List<TipoHabitacionDto> Tipos { get; set; } = new List<TipoHabitacionDto>();
        public List<SelectListItem> PisosActivos { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ListaHuespedes { get; set; } = new List<SelectListItem>();
        public List<ReservaItemViewModel> Reservas { get; set; } = new List<ReservaItemViewModel>();

        public string Tab { get; set; }

        // Disponibilidad dentro del hotel
        public DateTime Entrada { get; set; }
        public DateTime Salida { get; set; }
        public int Huespedes { get; set; }
        public bool Consultado { get; set; }
        public string ErrorDisponibilidad { get; set; }
        public int Noches { get { return Math.Max(0, (Salida.Date - Entrada.Date).Days); } }
        public List<HabitacionDisponibleViewModel> Disponibles { get; set; } = new List<HabitacionDisponibleViewModel>();

        public int PasoActual
        {
            get
            {
                if (Pisos.Count == 0) return Bloques.Count == 0 ? 1 : 2;
                if (!HabitacionesPorPiso.Any()) return 3;
                return 4;
            }
        }

        public string NombreTipo(int? idTipo)
        {
            if (!idTipo.HasValue) return "—";
            var t = Tipos.FirstOrDefault(x => x.IdTipoHabitacion == idTipo.Value);
            return t != null ? Helpers.TextoHelper.Titulo(t.Nombre) : "—";
        }
    }
}
