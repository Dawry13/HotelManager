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
    public class ReservaRepository : BaseRepository<Reserva>, IReservaRepository
    {
        protected override string TableName => "reservas";
        protected override string KeyColumn => "id_reserva";

        protected override Reserva Map(IDataRecord r) => new Reserva
        {
            IdReserva = (int)r["id_reserva"],
            Entrada = (DateTime)r["fecha_entrada"],
            Salida = (DateTime)r["fecha_salida"],
            Huespedes = (int)r["cantidad_huespedes"],
            CostoTotal = (decimal)r["costo_total"],
            Estado = ParseEstado((string)r["estado"]),
            IdHabitacion = (int)r["id_habitacion"],
            IdHuesped = (int)r["id_huesped"]
        };

        private static EstadoReserva ParseEstado(string texto)
            => (EstadoReserva)Enum.Parse(typeof(EstadoReserva), texto, true);

        private static string EstadoTexto(EstadoReserva e) => e.ToString().ToLower();

        public override int Add(Reserva e)
            => Convert.ToInt32(Scalar(
                @"INSERT INTO reservas
                    (fecha_entrada, fecha_salida, cantidad_huespedes, costo_total, estado, id_habitacion, id_huesped)
                  OUTPUT INSERTED.id_reserva
                  VALUES (@en, @sa, @g, @c, @e, @h, @hu)",
                new SqlParameter("@en", e.Entrada.Date),
                new SqlParameter("@sa", e.Salida.Date),
                new SqlParameter("@g", e.Huespedes),
                new SqlParameter("@c", e.CostoTotal),
                new SqlParameter("@e", EstadoTexto(e.Estado)),
                new SqlParameter("@h", e.IdHabitacion),
                new SqlParameter("@hu", e.IdHuesped)));

        public override bool Update(Reserva e)
            => Execute(
                @"UPDATE reservas
                  SET fecha_entrada=@en, fecha_salida=@sa, cantidad_huespedes=@g, costo_total=@c,
                      estado=@e, id_habitacion=@h, id_huesped=@hu
                  WHERE id_reserva=@id",
                new SqlParameter("@en", e.Entrada.Date),
                new SqlParameter("@sa", e.Salida.Date),
                new SqlParameter("@g", e.Huespedes),
                new SqlParameter("@c", e.CostoTotal),
                new SqlParameter("@e", EstadoTexto(e.Estado)),
                new SqlParameter("@h", e.IdHabitacion),
                new SqlParameter("@hu", e.IdHuesped),
                new SqlParameter("@id", e.IdReserva)) > 0;

        // RN-010 y RN-012: solo bloquean las reservas activas (misma regla que el trigger).
        public bool HaySolapamiento(int idHabitacion, DateTime entrada, DateTime salida, int? excluirIdReserva = null)
            => Convert.ToInt32(Scalar(
                @"SELECT COUNT(1) FROM reservas
                  WHERE id_habitacion=@h
                    AND estado IN ('pendiente','confirmada','modificada')
                    AND fecha_entrada < @sa AND fecha_salida > @en
                    AND (@x IS NULL OR id_reserva<>@x)",
                new SqlParameter("@h", idHabitacion),
                new SqlParameter("@en", entrada.Date),
                new SqlParameter("@sa", salida.Date),
                new SqlParameter("@x", SqlDbType.Int) { Value = DbVal(excluirIdReserva) })) > 0;

        public IEnumerable<Reserva> GetByHuesped(int idHuesped)
            => Query("SELECT * FROM reservas WHERE id_huesped=@h ORDER BY fecha_entrada DESC",
                     new SqlParameter("@h", idHuesped));

        public IEnumerable<Reserva> GetByHabitacion(int idHabitacion)
            => Query("SELECT * FROM reservas WHERE id_habitacion=@h ORDER BY fecha_entrada DESC",
                     new SqlParameter("@h", idHabitacion));

        public bool CambiarEstado(int idReserva, EstadoReserva nuevoEstado)
            => Execute("UPDATE reservas SET estado=@e WHERE id_reserva=@id",
                new SqlParameter("@e", EstadoTexto(nuevoEstado)),
                new SqlParameter("@id", idReserva)) > 0;
    }
}