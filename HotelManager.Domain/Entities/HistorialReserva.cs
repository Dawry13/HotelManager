using System;
using HotelManager.HotelManager.Domain.Enums;

namespace HotelManager.HotelManager.Domain.Entities
{
    /// <summary>
    /// Registra cada cambio relevante realizado sobre una reserva
    /// (creación, modificación, cambio de estado, cancelación, etc.).
    /// </summary>
    public class HistorialReserva
    {
        public int IdHistorial { get; set; }
        public int IdReserva { get; set; }
        public Reserva Reserva { get; set; }
        public DateTime FechaCambio { get; set; } = DateTime.Now;
        public string Accion { get; set; } = string.Empty;
        public EstadoReserva? EstadoAnterior { get; set; }
        public EstadoReserva EstadoNuevo { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Observaciones { get; set; }

        public HistorialReserva() { }

        public HistorialReserva(
            int idReserva,
            string accion,
            EstadoReserva? estadoAnterior,
            EstadoReserva estadoNuevo,
            string usuario,
            string observaciones = null)
        {
            IdReserva = idReserva;
            Accion = accion;
            EstadoAnterior = estadoAnterior;
            EstadoNuevo = estadoNuevo;
            Usuario = usuario;
            Observaciones = observaciones;
            FechaCambio = DateTime.Now;
        }
    }
}