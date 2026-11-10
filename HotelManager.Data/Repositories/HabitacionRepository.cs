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
    public class HabitacionRepository : BaseRepository<Habitacion>, IHabitacionRepository
    {
        protected override string TableName => "habitaciones";
        protected override string KeyColumn => "id_habitacion";

        protected override Habitacion Map(IDataRecord r) => new Habitacion
        {
            IdHabitacion = (int)r["id_habitacion"],
            Numero = (string)r["numero"],
            Capacidad = (int)r["capacidad"],
            Tarifa = (decimal)r["tarifa"],
            Estado = EstadoDesdeTexto((string)r["estado"]),
            IdPiso = (int)r["id_piso"],
            IdTipoHabitacion = EnteroNulo(r, "id_tipo_habitacion")
        };

        // ck_habitaciones_estado: 'disponible','mantenimiento','fuera_de_servicio','inactiva'
        public static string EstadoTexto(EstadoHabitacion estado)
        {
            switch (estado)
            {
                case EstadoHabitacion.Disponible: return "disponible";
                case EstadoHabitacion.Mantenimiento: return "mantenimiento";
                case EstadoHabitacion.FueraDeServicio: return "fuera_de_servicio";
                default: return "inactiva";
            }
        }

        public static EstadoHabitacion EstadoDesdeTexto(string texto)
        {
            switch ((texto ?? string.Empty).Trim().ToLower())
            {
                case "disponible": return EstadoHabitacion.Disponible;
                case "mantenimiento": return EstadoHabitacion.Mantenimiento;
                case "fuera_de_servicio": return EstadoHabitacion.FueraDeServicio;
                default: return EstadoHabitacion.Inactiva;
            }
        }

        public override IEnumerable<Habitacion> GetAll()
            => Query("select * from habitaciones order by id_hotel, numero");

        // id_hotel se toma del piso: la fk compuesta (id_piso, id_hotel) exige que coincidan.
        // Se usa scope_identity() porque la tabla tiene el trigger tr_habitaciones_piso_activo
        // (SQL Server no permite OUTPUT sin INTO en tablas con triggers).
        public override int Add(Habitacion e)
            => Convert.ToInt32(Scalar(@"insert into habitaciones (numero, capacidad, tarifa, estado, id_piso, id_hotel, id_tipo_habitacion)
                             select @n, @c, @t, @e, p.id_piso, p.id_hotel, @tipo
                             from pisos p where p.id_piso = @p;
                             select cast(scope_identity() as int);",
                new SqlParameter("@n", e.Numero),
                new SqlParameter("@c", e.Capacidad),
                new SqlParameter("@t", e.Tarifa),
                new SqlParameter("@e", EstadoTexto(e.Estado)),
                new SqlParameter("@p", e.IdPiso),
                new SqlParameter("@tipo", SqlDbType.Int) { Value = DbVal(e.IdTipoHabitacion) }));

        public override bool Update(Habitacion e)
            => Execute(@"update h
                         set numero = @n, capacidad = @c, tarifa = @t, estado = @e,
                             id_piso = p.id_piso, id_hotel = p.id_hotel, id_tipo_habitacion = @tipo
                         from habitaciones h
                         join pisos p on p.id_piso = @p
                         where h.id_habitacion = @id",
                new SqlParameter("@n", e.Numero),
                new SqlParameter("@c", e.Capacidad),
                new SqlParameter("@t", e.Tarifa),
                new SqlParameter("@e", EstadoTexto(e.Estado)),
                new SqlParameter("@p", e.IdPiso),
                new SqlParameter("@tipo", SqlDbType.Int) { Value = DbVal(e.IdTipoHabitacion) },
                new SqlParameter("@id", e.IdHabitacion)) > 0;

        public IEnumerable<Habitacion> GetByPiso(int idPiso)
            => Query("select * from habitaciones where id_piso = @p order by numero", new SqlParameter("@p", idPiso));

        public IEnumerable<Habitacion> GetByHotel(int idHotel)
            => Query("select * from habitaciones where id_hotel = @h order by numero", new SqlParameter("@h", idHotel));

        /// <summary>RN-005: uq_habitaciones_hotel_numero.</summary>
        public bool ExisteNumeroEnHotel(int idHotel, string numero, int? excluirIdHabitacion = null)
            => Convert.ToInt32(Scalar(@"select count(1) from habitaciones
                                       where id_hotel = @h and numero = @n
                                         and (@x is null or id_habitacion <> @x)",
                new SqlParameter("@h", idHotel),
                new SqlParameter("@n", numero),
                new SqlParameter("@x", SqlDbType.Int) { Value = DbVal(excluirIdHabitacion) })) > 0;

        public IEnumerable<Habitacion> GetDisponibles(DateTime entrada, DateTime salida, int huespedes, int? idHotel = null)
            => Query(@"select h.* from habitaciones h
                       join pisos p on p.id_piso = h.id_piso
                       join hoteles ho on ho.id_hotel = p.id_hotel
                       where h.estado = 'disponible'          -- rn-007
                         and p.estado = 'activo'
                         and ho.estado = 'activo'
                         and h.capacidad >= @g                -- rn-011
                         and (@hotel is null or h.id_hotel = @hotel)
                         and not exists (                     -- rn-010 / rn-012
                             select 1 from reservas r
                             where r.id_habitacion = h.id_habitacion
                               and r.estado in ('pendiente','confirmada','modificada')
                               and r.fecha_entrada < @salida and r.fecha_salida > @entrada)",
                new SqlParameter("@g", huespedes),
                new SqlParameter("@hotel", SqlDbType.Int) { Value = DbVal(idHotel) },
                new SqlParameter("@entrada", SqlDbType.Date) { Value = entrada.Date },
                new SqlParameter("@salida", SqlDbType.Date) { Value = salida.Date });

        public bool CambiarEstado(int idHabitacion, EstadoHabitacion nuevoEstado)
            => Execute("update habitaciones set estado = @e where id_habitacion = @id",
                new SqlParameter("@e", EstadoTexto(nuevoEstado)),
                new SqlParameter("@id", idHabitacion)) > 0;
    }
}
