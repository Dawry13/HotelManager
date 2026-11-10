using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using HotelManager.Data.Base;
using HotelManager.Data.Interfaces;
using HotelManager.Domain.Entities;

namespace HotelManager.Data.Repositories
{
    public class TipoHabitacionRepository : BaseRepository<TipoHabitacion>, ITipoHabitacionRepository
    {
        protected override string TableName => "tipos_habitacion";
        protected override string KeyColumn => "id_tipo_habitacion";

        protected override TipoHabitacion Map(IDataRecord r) => new TipoHabitacion
        {
            IdTipoHabitacion = (int)r["id_tipo_habitacion"],
            Nombre = (string)r["nombre"],
            Descripcion = Texto(r, "descripcion"),
            CapacidadMaxima = (int)r["capacidad_maxima"],
            TarifaBase = (decimal)r["tarifa_base"],
            Estado = EsActivo(r["estado"])
        };

        public override IEnumerable<TipoHabitacion> GetAll()
            => Query("select * from tipos_habitacion order by nombre");

        public bool ExisteNombre(string nombre, int? excluirIdTipo = null)
            => Convert.ToInt32(Scalar(@"select count(1) from tipos_habitacion
                                       where nombre = @n and (@x is null or id_tipo_habitacion <> @x)",
                new SqlParameter("@n", nombre),
                new SqlParameter("@x", SqlDbType.Int) { Value = DbVal(excluirIdTipo) })) > 0;

        public int ContarHabitaciones(int idTipoHabitacion)
            => Convert.ToInt32(Scalar("select count(1) from habitaciones where id_tipo_habitacion = @t",
                new SqlParameter("@t", idTipoHabitacion)));

        public override int Add(TipoHabitacion e)
            => (int)Scalar(@"insert into tipos_habitacion (nombre, descripcion, capacidad_maxima, tarifa_base, estado)
                             output inserted.id_tipo_habitacion
                             values (@n, @d, @c, @t, @e)",
                new SqlParameter("@n", e.Nombre),
                new SqlParameter("@d", SqlDbType.VarChar, 250) { Value = DbVal(e.Descripcion) },
                new SqlParameter("@c", e.CapacidadMaxima),
                new SqlParameter("@t", e.TarifaBase),
                new SqlParameter("@e", EstadoTexto(e.Estado)));

        public override bool Update(TipoHabitacion e)
            => Execute(@"update tipos_habitacion
                         set nombre = @n, descripcion = @d, capacidad_maxima = @c, tarifa_base = @t, estado = @e
                         where id_tipo_habitacion = @id",
                new SqlParameter("@n", e.Nombre),
                new SqlParameter("@d", SqlDbType.VarChar, 250) { Value = DbVal(e.Descripcion) },
                new SqlParameter("@c", e.CapacidadMaxima),
                new SqlParameter("@t", e.TarifaBase),
                new SqlParameter("@e", EstadoTexto(e.Estado)),
                new SqlParameter("@id", e.IdTipoHabitacion)) > 0;
    }
}
