using System;
using HotelManager.Domain.Enums;

namespace HotelManager.Domain.Entities
{
    public class HistorialReserva
    {
        public int IdHistorial { get; set; }
        public int IdReserva { get; set; }
        public Reserva Reserva { get; set; }
        public DateTime FechaCambio { get; set; }
        public string Accion { get; set; } = string.Empty;
        public EstadoReserva? EstadoAnterior { get; set; }
        public EstadoReserva EstadoNuevo { get; set; }
    }
}