using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using HotelManager.Business.Interfaces;
using HotelManager.Data.Base;
using HotelManager.HotelManager.Domain.Entities;
using HotelManager.HotelManager.Domain.Enums;

namespace HotelManager.Data.Repositories
{
    public class ReservaRepository : BaseRepository<Reserva>, IReservaResository
    {
        protected override string TableName => "Reservas";
        protected override string KeyColumn => "IdReserva";

        protected override Reserva Map(IDataRecord r) => new Reserva
        {
            IdReserva = (int)r["IdReserva"],
            Entrada = (DateTime)r["Entrada"],
            Salida = (DateTime)r["Salida"],
            Huespedes = (int)r["Huespedes"],
            CostoTotal = (decimal)r["CostoTotal"],
            Estado = (EstadoReserva)(int)r["Estado"],
            IdHabitacion = (int)r["IdHabitacion"],
            IdHuesped = (int)r["IdHuesped"]
        };

        public override int Add(Reserva e)
            => (int)Scalar(@"INSERT INTO Reservas
                               (Entrada, Salida, Huespedes, CostoTotal, Estado, IdHabitacion, IdHuesped)
                             OUTPUT INSERTED.IdReserva
                             VALUES (@en, @sa, @g, @c, @e, @h, @hu)",
                new SqlParameter("@en", e.Entrada),
                new SqlParameter("@sa", e.Salida),
                new SqlParameter("@g", e.Huespedes),
                new SqlParameter("@c", e.CostoTotal),
                new SqlParameter("@e", (int)e.Estado),
                new SqlParameter("@h", e.IdHabitacion),
                new SqlParameter("@hu", e.IdHuesped));

        public override bool Update(Reserva e)
            => Execute(@"UPDATE Reservas
                         SET Entrada=@en, Salida=@sa, Huespedes=@g, CostoTotal=@c,
                             Estado=@e, IdHabitacion=@h, IdHuesped=@hu
                         WHERE IdReserva=@id",
                new SqlParameter("@en", e.Entrada),
                new SqlParameter("@sa", e.Salida),
                new SqlParameter("@g", e.Huespedes),
                new SqlParameter("@c", e.CostoTotal),
                new SqlParameter("@e", (int)e.Estado),
                new SqlParameter("@h", e.IdHabitacion),
                new SqlParameter("@hu", e.IdHuesped),
                new SqlParameter("@id", e.IdReserva)) > 0;

        // RN-010 y RN-012: las canceladas no bloquean
        public bool HaySolapamiento(int idHabitacion, DateTime entrada, DateTime salida, int? excluirIdReserva = null)
            => (int)Scalar(@"SELECT COUNT(1) FROM Reservas
                             WHERE IdHabitacion=@h AND Estado<>@cancelada
                               AND Entrada < @sa AND Salida > @en
                               AND (@x IS NULL OR IdReserva<>@x)",
                new SqlParameter("@h", idHabitacion),
                new SqlParameter("@cancelada", (int)EstadoReserva.Cancelada),
                new SqlParameter("@en", entrada),
                new SqlParameter("@sa", salida),
                new SqlParameter("@x", DbVal(excluirIdReserva))) > 0;

        public IEnumerable<Reserva> GetByHuesped(int idHuesped)
            => Query("SELECT * FROM Reservas WHERE IdHuesped=@h ORDER BY Entrada DESC",
                     new SqlParameter("@h", idHuesped));

        public IEnumerable<Reserva> GetByHabitacion(int idHabitacion)
            => Query("SELECT * FROM Reservas WHERE IdHabitacion=@h ORDER BY Entrada DESC",
                     new SqlParameter("@h", idHabitacion));

        public bool CambiarEstado(int idReserva, EstadoReserva nuevoEstado)
            => Execute("UPDATE Reservas SET Estado=@e WHERE IdReserva=@id",
                new SqlParameter("@e", (int)nuevoEstado),
                new SqlParameter("@id", idReserva)) > 0;
    }
}
