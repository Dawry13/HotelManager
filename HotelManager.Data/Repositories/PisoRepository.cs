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
        protected override string TableName => "pisos";
        protected override string KeyColumn => "id_piso";

        protected override Piso Map(IDataRecord r) => new Piso
        {
            IdPiso = (int)r["id_piso"],
            IdHotel = (int)r["id_hotel"],
            IdBloque = EnteroNulo(r, "id_bloque"),
            Numero = (int)r["numero"],
            Estado = EsActivo(r["estado"])
        };

        public override IEnumerable<Piso> GetAll()
            => Query("select * from pisos order by id_hotel, id_bloque, numero");

        public IEnumerable<Piso> GetByHotel(int idHotel)
            => Query("select * from pisos where id_hotel = @h order by id_bloque, numero",
                new SqlParameter("@h", idHotel));

        public IEnumerable<Piso> GetByBloque(int idBloque)
            => Query("select * from pisos where id_bloque = @b order by numero",
                new SqlParameter("@b", idBloque));

        /// <summary>
        /// RN-002/RN-003: no pueden existir dos pisos activos con el mismo numero en un hotel
        /// (la base de datos lo garantiza con el indice ux_pisos_hotel_numero_activo).
        /// </summary>
        public bool ExisteNumeroActivo(int idHotel, int numero, int? excluirIdPiso = null)
            => Convert.ToInt32(Scalar(@"select count(1) from pisos
                                       where id_hotel = @h and numero = @n and estado = 'activo'
                                         and (@x is null or id_piso <> @x)",
                new SqlParameter("@h", idHotel),
                new SqlParameter("@n", numero),
                new SqlParameter("@x", SqlDbType.Int) { Value = DbVal(excluirIdPiso) })) > 0;

        public int ContarHabitaciones(int idPiso)
            => Convert.ToInt32(Scalar("select count(1) from habitaciones where id_piso = @p",
                new SqlParameter("@p", idPiso)));

        public override int Add(Piso e)
            => (int)Scalar(@"insert into pisos (numero, estado, id_hotel, id_bloque)
                             output inserted.id_piso
                             values (@n, @e, @h, @b)",
                new SqlParameter("@n", e.Numero),
                new SqlParameter("@e", EstadoTexto(e.Estado)),
                new SqlParameter("@h", e.IdHotel),
                new SqlParameter("@b", SqlDbType.Int) { Value = DbVal(e.IdBloque) });

        public override bool Update(Piso e)
            => Execute(@"update pisos
                         set numero = @n, estado = @e, id_hotel = @h, id_bloque = @b
                         where id_piso = @id",
                new SqlParameter("@n", e.Numero),
                new SqlParameter("@e", EstadoTexto(e.Estado)),
                new SqlParameter("@h", e.IdHotel),
                new SqlParameter("@b", SqlDbType.Int) { Value = DbVal(e.IdBloque) },
                new SqlParameter("@id", e.IdPiso)) > 0;
    }
}
