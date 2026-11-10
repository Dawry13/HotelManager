using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using HotelManager.Data.Base;
using HotelManager.Data.Interfaces;
using HotelManager.Domain.Entities;

namespace HotelManager.Data.Repositories
{
    public class BloqueRepository : BaseRepository<Bloque>, IBloqueRepository
    {
        protected override string TableName => "bloques";
        protected override string KeyColumn => "id_bloque";

        protected override Bloque Map(IDataRecord r) => new Bloque
        {
            IdBloque = (int)r["id_bloque"],
            IdHotel = (int)r["id_hotel"],
            Codigo = (string)r["codigo"],
            Nombre = (string)r["nombre"],
            Descripcion = Texto(r, "descripcion"),
            Estado = EsActivo(r["estado"])
        };

        public override IEnumerable<Bloque> GetAll()
            => Query("select * from bloques order by id_hotel, codigo");

        public IEnumerable<Bloque> GetByHotel(int idHotel)
            => Query("select * from bloques where id_hotel = @h order by codigo",
                new SqlParameter("@h", idHotel));

        public bool ExisteCodigo(int idHotel, string codigo, int? excluirIdBloque = null)
            => Convert.ToInt32(Scalar(@"select count(1) from bloques
                                       where id_hotel = @h and codigo = @c
                                         and (@x is null or id_bloque <> @x)",
                new SqlParameter("@h", idHotel),
                new SqlParameter("@c", codigo),
                new SqlParameter("@x", SqlDbType.Int) { Value = DbVal(excluirIdBloque) })) > 0;

        public int ContarPisos(int idBloque)
            => Convert.ToInt32(Scalar("select count(1) from pisos where id_bloque = @b",
                new SqlParameter("@b", idBloque)));

        public override int Add(Bloque e)
            => (int)Scalar(@"insert into bloques (id_hotel, codigo, nombre, descripcion, estado)
                             output inserted.id_bloque
                             values (@h, @c, @n, @d, @e)",
                new SqlParameter("@h", e.IdHotel),
                new SqlParameter("@c", e.Codigo),
                new SqlParameter("@n", e.Nombre),
                new SqlParameter("@d", SqlDbType.VarChar, 250) { Value = DbVal(e.Descripcion) },
                new SqlParameter("@e", EstadoTexto(e.Estado)));

        public override bool Update(Bloque e)
            => Execute(@"update bloques
                         set id_hotel = @h, codigo = @c, nombre = @n, descripcion = @d, estado = @e
                         where id_bloque = @id",
                new SqlParameter("@h", e.IdHotel),
                new SqlParameter("@c", e.Codigo),
                new SqlParameter("@n", e.Nombre),
                new SqlParameter("@d", SqlDbType.VarChar, 250) { Value = DbVal(e.Descripcion) },
                new SqlParameter("@e", EstadoTexto(e.Estado)),
                new SqlParameter("@id", e.IdBloque)) > 0;
    }
}
