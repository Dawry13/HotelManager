using System;

namespace HotelManager.Business.Dtos
{
    public class CrearReservaDto
    {
        public int HuespedId { get; set; }
        public int HabitacionId { get; set; }
        public DateTime FechaEntrada { get; set; }
        public DateTime FechaSalida { get; set; }
        public int CantidadHuespedes { get; set; }
    }

    public class ModificarReservaDto
    {
        public int ReservaId { get; set; }
        public int HabitacionId { get; set; }
        public DateTime FechaEntrada { get; set; }
        public DateTime FechaSalida { get; set; }
        public int CantidadHuespedes { get; set; }
    }

    public class ReservaDto
    {
        public int Id { get; set; }
        public int HuespedId { get; set; }
        public int HabitacionId { get; set; }
        public DateTime FechaEntrada { get; set; }
        public DateTime FechaSalida { get; set; }
        public int CantidadHuespedes { get; set; }
        public decimal CostoTotal { get; set; }
        public string Estado { get; set; }
    }
}