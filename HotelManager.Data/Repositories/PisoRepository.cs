using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using HotelManager.Data.Base;
using HotelManager.Data.Interfaces;
using HotelManager.Domain.Entities;

namespace HotelManager.Data.Repositories
{
    public class PisoRepository : BaseRepository<Piso>, IPisoRepository
    {
  
        protected override string TableName => "Piso";
        protected override string KeyColumn => "IdPiso";

     
        public PisoRepository() : base()
        {
        }

        protected override Piso Map(IDataRecord r)
        {
            return new Piso
            {
                IdPiso = r.GetInt32(r.GetOrdinal("IdPiso")),
                IdHotel = r.GetInt32(r.GetOrdinal("IdHotel")),
                Numero = r.GetInt32(r.GetOrdinal("Numero")),
                Estado = r.GetBoolean(r.GetOrdinal("Estado"))
            };
        }

        public IEnumerable<Piso> GetByHotel(int idHotel)
        {
            var lista = new List<Piso>();

            const string sql = @"SELECT IdPiso, IdHotel, Numero, Estado
                                 FROM Piso
                                 WHERE IdHotel = @IdHotel
                                 ORDER BY Numero";

            using (var cn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@IdHotel", idHotel);
                cn.Open();

                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        lista.Add(Map(r));
                }
            }

            return lista;
        }

        public bool ExisteNumeroActivo(int idHotel, int numero, int? excluirIdPiso = null)
        {
            const string sql = @"SELECT COUNT(1)
                                 FROM Piso
                                 WHERE IdHotel = @IdHotel
                                   AND Numero = @Numero
                                   AND Estado = @Estado
                                   AND (@Excluir IS NULL OR IdPiso <> @Excluir)";

            using (var cn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@IdHotel", idHotel);
                cmd.Parameters.AddWithValue("@Numero", numero);
                cmd.Parameters.AddWithValue("@Estado", true);
                cmd.Parameters.AddWithValue("@Excluir", (object)excluirIdPiso ?? DBNull.Value);
                cn.Open();

                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        public override int Add(Piso entity)
        {
            const string sql = @"INSERT INTO Piso (IdHotel, Numero, Estado)
                                 OUTPUT INSERTED.IdPiso
                                 VALUES (@IdHotel, @Numero, @Estado)";

            using (var cn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@IdHotel", entity.IdHotel);
                cmd.Parameters.AddWithValue("@Numero", entity.Numero);
                cmd.Parameters.AddWithValue("@Estado", entity.Estado);
                cn.Open();

                return (int)cmd.ExecuteScalar();
            }
        }

        public override bool Update(Piso entity)
        {
            const string sql = @"UPDATE Piso
                                 SET IdHotel = @IdHotel,
                                     Numero = @Numero,
                                     Estado = @Estado
                                 WHERE IdPiso = @IdPiso";

            using (var cn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@IdPiso", entity.IdPiso);
                cmd.Parameters.AddWithValue("@IdHotel", entity.IdHotel);
                cmd.Parameters.AddWithValue("@Numero", entity.Numero);
                cmd.Parameters.AddWithValue("@Estado", entity.Estado);
                cn.Open();

                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}