using System;
using System.Collections.Generic;

namespace HotelManager.Web.Models
{
    public class DashboardViewModel
    {
        public int Hoteles { get; set; }
        public int HotelesActivos { get; set; }
        public int Bloques { get; set; }
        public int Pisos { get; set; }
        public int Habitaciones { get; set; }
        public int TiposHabitacion { get; set; }
        public int ReservasActivas { get; set; }
        public int ReservasHoy { get; set; }
        public int Huespedes { get; set; }

        // Disponibilidad de habitaciones hoy
        public int Ocupadas { get; set; }
        public int Disponibles { get; set; }
        public int Mantenimiento { get; set; }
        public int FueraDeServicio { get; set; }
        public int Inactivas { get; set; }

        public int PorcentajeOcupacion
        {
            get
            {
                var operativas = Ocupadas + Disponibles;
                return operativas == 0 ? 0 : (int)Math.Round(Ocupadas * 100.0 / operativas);
            }
        }

        public List<ReservaProximaViewModel> ProximasReservas { get; set; } = new List<ReservaProximaViewModel>();
        public List<HabitacionesPorHotelViewModel> PorHotel { get; set; } = new List<HabitacionesPorHotelViewModel>();
    }

    public class ReservaProximaViewModel
    {
        public string Huesped { get; set; }
        public string Habitacion { get; set; }
        public string Hotel { get; set; }
        public DateTime Entrada { get; set; }
        public DateTime Salida { get; set; }
        public string Estado { get; set; }
    }

    public class HabitacionesPorHotelViewModel
    {
        public int IdHotel { get; set; }
        public string Hotel { get; set; }
        public int Total { get; set; }
        public int Disponibles { get; set; }
    }

    public class BusquedaViewModel
    {
        public string Texto { get; set; }
        public List<ResultadoBusqueda> Resultados { get; set; } = new List<ResultadoBusqueda>();
    }

    public class ResultadoBusqueda
    {
        public string Modulo { get; set; }
        public string Icono { get; set; }
        public string Titulo { get; set; }
        public string Detalle { get; set; }
        public string Url { get; set; }
    }
}
