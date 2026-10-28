using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using HotelManager.Data.Base;
using HotelManager.Data.Interfaces;
using HotelManager.Domain.Entities;

namespace HotelManager.Data.Repositories
{
    public class HuespedRepository : BaseRepository<Huesped>, IHuespedRepository
    {
        protected override string TableName => "huespedes";
        protected override string KeyColumn => "id_huesped";

        protected override Huesped Map(IDataRecord r) => new Huesped
        {
            IdHuesped = (int)r["id_huesped"],
            Nombre = (string)r["nombre"],
            Documento = (string)r["documento"],
            Telefono = r["telefono"] as string,   // DBNull -> null
            Correo = r["correo"] as string
        };

        public override int Add(Huesped e)
            => Convert.ToInt32(Scalar(
                @"INSERT INTO huespedes (nombre, documento, telefono, correo)
                  OUTPUT INSERTED.id_huesped
                  VALUES (@n, @d, @t, @c)",
                new SqlParameter("@n", e.Nombre),
                new SqlParameter("@d", e.Documento),
                new SqlParameter("@t", DbVal(e.Telefono)),
                new SqlParameter("@c", DbVal(e.Correo))));

        public override bool Update(Huesped e)
            => Execute(
                @"UPDATE huespedes
                  SET nombre=@n, documento=@d, telefono=@t, correo=@c
                  WHERE id_huesped=@id",
                new SqlParameter("@n", e.Nombre),
                new SqlParameter("@d", e.Documento),
                new SqlParameter("@t", DbVal(e.Telefono)),
                new SqlParameter("@c", DbVal(e.Correo)),
                new SqlParameter("@id", e.IdHuesped)) > 0;

        // Busca por nombre o documento (coincidencia parcial).
        public IEnumerable<Huesped> Buscar(string texto)
            => Query(
                @"SELECT * FROM huespedes
                  WHERE nombre LIKE @q OR documento LIKE @q
                  ORDER BY nombre",
                new SqlParameter("@q", "%" + (texto ?? string.Empty).Trim() + "%"));
    }
}