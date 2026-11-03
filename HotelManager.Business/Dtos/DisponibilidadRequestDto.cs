using System;

namespace HotelManager.Business.Dtos
{
    public class DisponibilidadRequestDto
    {
        public DateTime Entrada { get; set; }
        public DateTime Salida { get; set; }
        public int CantidadHuespedes { get; set; }
        public int? IdHotel { get; set; }  
    }

    public class HabitacionDisponibleDto
    {
        public int IdHabitacion { get; set; }
        public int IdHotel { get; set; }
        public int IdPiso { get; set; }
        public int Numero { get; set; }
        public int Capacidad { get; set; }
        public decimal TarifaPorNoche { get; set; }
        public int Noches { get; set; }
        public decimal CostoEstimado { get; set; }
    }
}