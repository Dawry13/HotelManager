using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using HotelManager.Data.Base;
using HotelManager.Data.Interfaces;
using HotelManager.Domain.Entities;

namespace HotelManager.Data.Repositories
{
    public class HotelRepository : BaseRepository<Hotel>, IHotelRepository
    {
        protected override string TableName => "hoteles";
        protected override string KeyColumn => "id_hotel";

        protected override Hotel Map(IDataRecord r) => new Hotel
        {
            IdHotel = (int)r["id_hotel"],
            Nombre = (string)r["nombre"],
            Direccion = (string)r["direccion"],
            Estado = EsActivo(r["estado"]),
            Categoria = r["categoria"] == DBNull.Value ? 3 : Convert.ToInt32(r["categoria"]),
            Caracteristicas = Texto(r, "caracteristicas"),
            Descripcion = Texto(r, "descripcion")
        };

        public override IEnumerable<Hotel> GetAll()
            => Query("select * from hoteles order by nombre");

        public override int Add(Hotel e)
            => (int)Scalar(@"insert into hoteles (nombre, direccion, estado, categoria, caracteristicas, descripcion)
                             output inserted.id_hotel
                             values (@n, @d, @e, @cat, @car, @des)",
                new SqlParameter("@n", e.Nombre),
                new SqlParameter("@d", e.Direccion),
                new SqlParameter("@e", EstadoTexto(e.Estado)),
                new SqlParameter("@cat", e.Categoria),
                new SqlParameter("@car", SqlDbType.VarChar, 500) { Value = DbVal(e.Caracteristicas) },
                new SqlParameter("@des", SqlDbType.VarChar, 500) { Value = DbVal(e.Descripcion) });

        public override bool Update(Hotel e)
            => Execute(@"update hoteles set nombre = @n, direccion = @d, estado = @e,
                                categoria = @cat, caracteristicas = @car, descripcion = @des
                         where id_hotel = @id",
                new SqlParameter("@n", e.Nombre),
                new SqlParameter("@d", e.Direccion),
                new SqlParameter("@e", EstadoTexto(e.Estado)),
                new SqlParameter("@cat", e.Categoria),
                new SqlParameter("@car", SqlDbType.VarChar, 500) { Value = DbVal(e.Caracteristicas) },
                new SqlParameter("@des", SqlDbType.VarChar, 500) { Value = DbVal(e.Descripcion) },
                new SqlParameter("@id", e.IdHotel)) > 0;

        public IEnumerable<Hotel> GetActivos()
            => Query("select * from hoteles where estado = 'activo' order by nombre");

        public IEnumerable<Piso> GetPisos(int idHotel)
        {
            var list = new List<Piso>();
            using (var cn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand("select * from pisos where id_hotel = @h order by numero", cn))
            {
                cmd.Parameters.AddWithValue("@h", idHotel);
                cn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new Piso
                        {
                            IdPiso = (int)r["id_piso"],
                            Numero = (int)r["numero"],
                            Estado = EsActivo(r["estado"]),
                            IdHotel = (int)r["id_hotel"],
                            IdBloque = EnteroNulo(r, "id_bloque")
                        });
            }
            return list;
        }

        public Hotel GetConPisos(int idHotel)
        {
            var hotel = GetById(idHotel);
            if (hotel == null) return null;
            hotel.Pisos = GetPisos(idHotel).ToList();
            return hotel;
        }

        public bool ExisteNombre(string nombre, int? excluirIdHotel = null)
            => Convert.ToInt32(Scalar(
                @"select count(1) from hoteles
                  where nombre = @n and (@x is null or id_hotel <> @x)",
                new SqlParameter("@n", nombre),
                new SqlParameter("@x", SqlDbType.Int) { Value = DbVal(excluirIdHotel) })) > 0;
    }
}
