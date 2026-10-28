using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using HotelManager.Data.Base;
using HotelManager.Data.Interfaces;
using HotelManager.Domain.Entities;

namespace HotelManager.Data.Repositories
{
    public class NotificacionRepository : BaseRepository<Notificacion>, INotificacionRepository
    {
        protected override string TableName => "Notificacion";
        protected override string KeyColumn => "IdNotificacion";

        public NotificacionRepository() : base()
        {
        }

        protected override Notificacion Map(IDataRecord r)
        {
            return new Notificacion
            {
                IdNotificacion = r.GetInt32(r.GetOrdinal("IdNotificacion")),
                IdReserva = r.GetInt32(r.GetOrdinal("IdReserva")),
                Mensaje = r.GetString(r.GetOrdinal("Mensaje")),
                Fecha = r.GetDateTime(r.GetOrdinal("Fecha")),
                Leida = r.GetBoolean(r.GetOrdinal("Leida"))
            };
        }

        public IEnumerable<Notificacion> GetByReserva(int idReserva)
        {
            var lista = new List<Notificacion>();

            const string sql = @"SELECT IdNotificacion, IdReserva, Mensaje, Fecha, Leida
                                 FROM Notificacion
                                 WHERE IdReserva = @IdReserva
                                 ORDER BY Fecha DESC";

            using (var cn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@IdReserva", idReserva);
                cn.Open();

                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        lista.Add(Map(r));
                }
            }

            return lista;
        }

        public IEnumerable<Notificacion> GetPorLeida(bool leida)
        {
            var lista = new List<Notificacion>();

            const string sql = @"SELECT IdNotificacion, IdReserva, Mensaje, Fecha, Leida
                                 FROM Notificacion
                                 WHERE Leida = @Leida
                                 ORDER BY Fecha";

            using (var cn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@Leida", leida);
                cn.Open();

                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        lista.Add(Map(r));
                }
            }

            return lista;
        }

        public override int Add(Notificacion entity)
        {
            const string sql = @"INSERT INTO Notificacion (IdReserva, Mensaje, Fecha, Leida)
                                 OUTPUT INSERTED.IdNotificacion
                                 VALUES (@IdReserva, @Mensaje, @Fecha, @Leida)";

            using (var cn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@IdReserva", entity.IdReserva);
                cmd.Parameters.AddWithValue("@Mensaje", entity.Mensaje);
                cmd.Parameters.AddWithValue("@Fecha", entity.Fecha);
                cmd.Parameters.AddWithValue("@Leida", entity.Leida);
                cn.Open();

                return (int)cmd.ExecuteScalar();
            }
        }

        public override bool Update(Notificacion entity)
        {
            const string sql = @"UPDATE Notificacion
                                 SET IdReserva = @IdReserva,
                                     Mensaje = @Mensaje,
                                     Fecha = @Fecha,
                                     Leida = @Leida
                                 WHERE IdNotificacion = @IdNotificacion";

            using (var cn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@IdNotificacion", entity.IdNotificacion);
                cmd.Parameters.AddWithValue("@IdReserva", entity.IdReserva);
                cmd.Parameters.AddWithValue("@Mensaje", entity.Mensaje);
                cmd.Parameters.AddWithValue("@Fecha", entity.Fecha);
                cmd.Parameters.AddWithValue("@Leida", entity.Leida);
                cn.Open();

                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}