using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using HotelManager.Business.Interfaces;
using HotelManager.Data.Base;
using HotelManager.HotelManager.Domain.Entities;

namespace HotelManager.Data.Repositories
{
    public class HotelRepository : BaseRepository<Hotel>, IHotelResository
    {
        protected override string TableName => "Hoteles";
        protected override string KeyColumn => "IdHotel";

        protected override Hotel Map(IDataRecord r) => new Hotel
        {
            IdHotel = (int)r["IdHotel"],
            Nombre = (string)r["Nombre"],
            Direccion = (string)r["Direccion"],
            Estado = (bool)r["Estado"]
        };

        public override int Add(Hotel e)
            => (int)Scalar(@"INSERT INTO Hoteles (Nombre, Direccion, Estado)
                             OUTPUT INSERTED.IdHotel
                             VALUES (@n, @d, @e)",
                new SqlParameter("@n", e.Nombre),
                new SqlParameter("@d", e.Direccion),
                new SqlParameter("@e", e.Estado));

        public override bool Update(Hotel e)
            => Execute(@"UPDATE Hoteles SET Nombre=@n, Direccion=@d, Estado=@e WHERE IdHotel=@id",
                new SqlParameter("@n", e.Nombre),
                new SqlParameter("@d", e.Direccion),
                new SqlParameter("@e", e.Estado),
                new SqlParameter("@id", e.IdHotel)) > 0;

        public IEnumerable<Hotel> GetActivos()
            => Query("SELECT * FROM Hoteles WHERE Estado = 1");

        public IEnumerable<Piso> GetPisos(int idHotel)
        {
            var list = new List<Piso>();
            using (var cn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand("SELECT * FROM Pisos WHERE IdHotel = @h", cn))
            {
                cmd.Parameters.AddWithValue("@h", idHotel);
                cn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new Piso
                        {
                            IdPiso = (int)r["IdPiso"],
                            Numero = (int)r["Numero"],
                            Estado = (bool)r["Estado"],
                            IdHotel = (int)r["IdHotel"]
                        });
            }
            return list;
        }

        public bool ExistePisoActivo(int idHotel, int numero, int? excluirIdPiso = null)
            => (int)Scalar(@"SELECT COUNT(1) FROM Pisos
                             WHERE IdHotel=@h AND Numero=@n AND Estado=1
                               AND (@x IS NULL OR IdPiso<>@x)",
                new SqlParameter("@h", idHotel),
                new SqlParameter("@n", numero),
                new SqlParameter("@x", DbVal(excluirIdPiso))) > 0;
    }
}
