using System;
using System.Collections.Generic;

namespace HotelManager.Business.Dtos
{
    /// <summary>Resultado del reporte general para un rango de fechas.</summary>
    public class ReporteGeneralDto
    {
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }

        public int TotalReservas { get; set; }
        public int ReservasCanceladas { get; set; }
        public decimal Ingresos { get; set; }
        public decimal IngresoPromedioReserva { get; set; }
        public int NochesVendidas { get; set; }
        public decimal OcupacionPromedio { get; set; }     // 0 - 100
        public int HuespedesAtendidos { get; set; }

        public List<OcupacionHotelDto> PorHotel { get; set; } = new List<OcupacionHotelDto>();
        public List<ConteoDto> PorEstado { get; set; } = new List<ConteoDto>();
        public List<ConteoDto> PorTipoHabitacion { get; set; } = new List<ConteoDto>();
        public List<IngresoMesDto> PorMes { get; set; } = new List<IngresoMesDto>();
        public List<HuespedFrecuenteDto> HuespedesFrecuentes { get; set; } = new List<HuespedFrecuenteDto>();
    }

    public class OcupacionHotelDto
    {
        public int IdHotel { get; set; }
        public string Hotel { get; set; }
        public int Habitaciones { get; set; }
        public int NochesDisponibles { get; set; }
        public int NochesOcupadas { get; set; }
        public decimal Ocupacion { get; set; }            // 0 - 100
        public int Reservas { get; set; }
        public decimal Ingresos { get; set; }
    }

    public class ConteoDto
    {
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
        public decimal Monto { get; set; }
    }

    public class IngresoMesDto
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public int Reservas { get; set; }
        public decimal Ingresos { get; set; }
    }

    public class HuespedFrecuenteDto
    {
        public int IdHuesped { get; set; }
        public string Nombre { get; set; }
        public int Reservas { get; set; }
        public int Noches { get; set; }
        public decimal Total { get; set; }
    }
}
