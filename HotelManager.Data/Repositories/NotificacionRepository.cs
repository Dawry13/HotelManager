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
    public class NotificacionRepository : BaseRepository<Notificacion>, INotificacionRepository
    {
        protected override string TableName => "notificaciones";
        protected override string KeyColumn => "id_notificacion";

        protected override Notificacion Map(IDataRecord r) => new Notificacion
        {
            IdNotificacion = (int)r["id_notificacion"],
            IdReserva = (int)r["id_reserva"],
            Tipo = (TipoNotificacion)Enum.Parse(typeof(TipoNotificacion), (string)r["tipo"], true),
            Fecha = (DateTime)r["fecha"],
            Estado = (EstadoNotificacion)Enum.Parse(typeof(EstadoNotificacion), (string)r["estado"], true),
            DetalleError = Texto(r, "detalle_error")
        };

        // ck_notificaciones_tipo: 'confirmacion','modificacion','cancelacion'
        private static string TipoTexto(TipoNotificacion t) => t.ToString().ToLower();

        // ck_notificaciones_estado: 'pendiente','enviada','fallida'
        private static string EstadoTexto(EstadoNotificacion e) => e.ToString().ToLower();

        public override IEnumerable<Notificacion> GetAll()
            => Query("select * from notificaciones order by fecha desc");

        public IEnumerable<Notificacion> GetByReserva(int idReserva)
            => Query("select * from notificaciones where id_reserva = @r order by fecha desc",
                new SqlParameter("@r", idReserva));

        public IEnumerable<Notificacion> GetPorEstado(EstadoNotificacion estado)
            => Query("select * from notificaciones where estado = @e order by fecha",
                new SqlParameter("@e", EstadoTexto(estado)));

        public override int Add(Notificacion e)
            => Convert.ToInt32(Scalar(@"insert into notificaciones (tipo, fecha, estado, id_reserva, detalle_error)
                                       output inserted.id_notificacion
                                       values (@t, @f, @e, @r, @d)",
                new SqlParameter("@t", TipoTexto(e.Tipo)),
                new SqlParameter("@f", SqlDbType.DateTime2) { Value = e.Fecha == default(DateTime) ? DateTime.Now : e.Fecha },
                new SqlParameter("@e", EstadoTexto(e.Estado)),
                new SqlParameter("@r", e.IdReserva),
                new SqlParameter("@d", SqlDbType.VarChar, 500) { Value = DbVal(e.DetalleError) }));

        public override bool Update(Notificacion e)
            => Execute(@"update notificaciones
                         set tipo = @t, fecha = @f, estado = @e, id_reserva = @r, detalle_error = @d
                         where id_notificacion = @id",
                new SqlParameter("@t", TipoTexto(e.Tipo)),
                new SqlParameter("@f", SqlDbType.DateTime2) { Value = e.Fecha },
                new SqlParameter("@e", EstadoTexto(e.Estado)),
                new SqlParameter("@r", e.IdReserva),
                new SqlParameter("@d", SqlDbType.VarChar, 500) { Value = DbVal(e.DetalleError) },
                new SqlParameter("@id", e.IdNotificacion)) > 0;

        public bool RegistrarResultado(int idNotificacion, EstadoNotificacion estado, string detalleError)
            => Execute(@"update notificaciones set estado = @e, detalle_error = @d where id_notificacion = @id",
                new SqlParameter("@e", EstadoTexto(estado)),
                new SqlParameter("@d", SqlDbType.VarChar, 500) { Value = DbVal(detalleError) },
                new SqlParameter("@id", idNotificacion)) > 0;
    }
}
