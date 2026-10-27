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
    public class HabitacionRepository : BaseRepository<Habitacion>, IHabitacionesResository
    {
        protected override string TableName => "Habitaciones";
        protected override string KeyColumn => "IdHabitacion";

        protected override Habitacion Map(IDataRecord r) => new Habitacion
        {
            IdHabitacion = (int)r["IdHabitacion"],
            Numero = (int)r["Numero"],
            Capacidad = (int)r["Capacidad"],
            Tarifa = (decimal)r["Tarifa"],
            Estado = (EstadoHabitacion)(int)r["Estado"],
            IdPiso = (int)r["IdPiso"]
        };

        public override int Add(Habitacion e)
            => (int)Scalar(@"INSERT INTO Habitaciones (Numero, Capacidad, Tarifa, Estado, IdPiso)
                             OUTPUT INSERTED.IdHabitacion
                             VALUES (@n, @c, @t, @e, @p)",
                new SqlParameter("@n", e.Numero),
                new SqlParameter("@c", e.Capacidad),
                new SqlParameter("@t", e.Tarifa),
                new SqlParameter("@e", (int)e.Estado),
                new SqlParameter("@p", e.IdPiso));

        public override bool Update(Habitacion e)
            => Execute(@"UPDATE Habitaciones
                         SET Numero=@n, Capacidad=@c, Tarifa=@t, Estado=@e, IdPiso=@p
                         WHERE IdHabitacion=@id",
                new SqlParameter("@n", e.Numero),
                new SqlParameter("@c", e.Capacidad),
                new SqlParameter("@t", e.Tarifa),
                new SqlParameter("@e", (int)e.Estado),
                new SqlParameter("@p", e.IdPiso),
                new SqlParameter("@id", e.IdHabitacion)) > 0;

        public IEnumerable<Habitacion> GetByPiso(int idPiso)
            => Query("SELECT * FROM Habitaciones WHERE IdPiso = @p", new SqlParameter("@p", idPiso));

        public IEnumerable<Habitacion> GetByHotel(int idHotel)
            => Query(@"SELECT h.* FROM Habitaciones h
                       INNER JOIN Pisos p ON p.IdPiso = h.IdPiso
                       WHERE p.IdHotel = @h", new SqlParameter("@h", idHotel));

        public bool ExisteNumeroEnHotel(int idHotel, int numero, int? excluirIdHabitacion = null)
            => (int)Scalar(@"SELECT COUNT(1) FROM Habitaciones h
                             INNER JOIN Pisos p ON p.IdPiso = h.IdPiso
                             WHERE p.IdHotel=@h AND h.Numero=@n
                               AND (@x IS NULL OR h.IdHabitacion<>@x)",
                new SqlParameter("@h", idHotel),
                new SqlParameter("@n", numero),
                new SqlParameter("@x", DbVal(excluirIdHabitacion))) > 0;

        public IEnumerable<Habitacion> GetDisponibles(DateTime entrada, DateTime salida, int huespedes, int? idHotel = null)
            => Query(@"SELECT h.* FROM Habitaciones h
                       INNER JOIN Pisos p ON p.IdPiso = h.IdPiso
                       WHERE h.Estado = @disp                 -- RN-007: solo habitaciones operativas
                         AND p.Estado = 1
                         AND h.Capacidad >= @g                -- RN-011
                         AND (@hotel IS NULL OR p.IdHotel = @hotel)
                         AND NOT EXISTS (                     -- RN-010 / RN-012
                             SELECT 1 FROM Reservas r
                             WHERE r.IdHabitacion = h.IdHabitacion
                               AND r.Estado <> @cancelada
                               AND r.Entrada < @salida AND r.Salida > @entrada)",
                new SqlParameter("@disp", (int)EstadoHabitacion.Disponible),
                new SqlParameter("@cancelada", (int)EstadoReserva.Cancelada),
                new SqlParameter("@g", huespedes),
                new SqlParameter("@hotel", DbVal(idHotel)),
                new SqlParameter("@entrada", entrada),
                new SqlParameter("@salida", salida));

        public bool CambiarEstado(int idHabitacion, EstadoHabitacion nuevoEstado)
            => Execute("UPDATE Habitaciones SET Estado=@e WHERE IdHabitacion=@id",
                new SqlParameter("@e", (int)nuevoEstado),
                new SqlParameter("@id", idHabitacion)) > 0;
    }
}
