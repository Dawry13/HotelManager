using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using HotelManager.Data.Base;
using HotelManager.Data.Interfaces;
using HotelManager.Domain.Entities;
using HotelManager.Domain.Enums;

namespace HotelManager.Data.Repositories
{
    public class HistorialReservaRepository : BaseRepository<HistorialReserva>, IHistorialReservaRepository
    {
        protected override string TableName => "historial_reservas";
        protected override string KeyColumn => "id_historial";

        protected override HistorialReserva Map(IDataRecord r) => new HistorialReserva
        {
            IdHistorial = (int)r["id_historial"],
            IdReserva = (int)r["id_reserva"],
            FechaCambio = (DateTime)r["fecha"],
            Accion = (string)r["accion"],
            EstadoAnterior = r["estado_anterior"] is DBNull
                ? (EstadoReserva?)null
                : ParseEstado((string)r["estado_anterior"]),
            EstadoNuevo = ParseEstado((string)r["estado_nuevo"])
        };

        private static EstadoReserva ParseEstado(string texto)
            => (EstadoReserva)Enum.Parse(typeof(EstadoReserva), texto, true);

        public IEnumerable<HistorialReserva> GetByReserva(int idReserva)
        {
            return Query(
                @"SELECT * FROM historial_reservas
                  WHERE id_reserva = @idReserva
                  ORDER BY fecha DESC, id_historial DESC",
                new SqlParameter("@idReserva", idReserva));
        }

        // El historial lo escribe el trigger tr_reservas_historial; es inmutable desde C#.
        public override int Add(HistorialReserva entity)
            => throw new NotSupportedException("El historial se registra automaticamente (trigger).");

        public override bool Update(HistorialReserva entity)
            => throw new NotSupportedException("El historial no se puede modificar.");

        public override bool Delete(int id)
            => throw new NotSupportedException("El historial no se puede eliminar.");
    }
}