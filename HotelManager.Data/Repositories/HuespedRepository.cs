using System.Data;
using System.Data.SqlClient;
using HotelManager.Business.Interfaces;
using HotelManager.Data.Base;
using HotelManager.HotelManager.Domain.Entities;

namespace HotelManager.Data.Repositories
{
    public class HuespedRepository : BaseRepository<Huesped>, IHuespedResository
    {
        protected override string TableName => "Huespedes";
        protected override string KeyColumn => "IdHuesped";

        protected override Huesped Map(IDataRecord r) => new Huesped
        {
            IdHuesped = (int)r["IdHuesped"],
            Nombre = (string)r["Nombre"],
            Documento = (string)r["Documento"],
            Telefono = r["Telefono"] as string,
            Correo = r["Correo"] as string
        };

        public override int Add(Huesped e)
            => (int)Scalar(@"INSERT INTO Huespedes (Nombre, Documento, Telefono, Correo)
                             OUTPUT INSERTED.IdHuesped
                             VALUES (@n, @d, @t, @c)",
                new SqlParameter("@n", e.Nombre),
                new SqlParameter("@d", e.Documento),
                new SqlParameter("@t", DbVal(e.Telefono)),
                new SqlParameter("@c", DbVal(e.Correo)));

        public override bool Update(Huesped e)
            => Execute(@"UPDATE Huespedes SET Nombre=@n, Documento=@d, Telefono=@t, Correo=@c
                         WHERE IdHuesped=@id",
                new SqlParameter("@n", e.Nombre),
                new SqlParameter("@d", e.Documento),
                new SqlParameter("@t", DbVal(e.Telefono)),
                new SqlParameter("@c", DbVal(e.Correo)),
                new SqlParameter("@id", e.IdHuesped)) > 0;

        public Huesped GetByDocumento(string documento)
        {
            foreach (var h in Query("SELECT * FROM Huespedes WHERE Documento=@d",
                                    new SqlParameter("@d", documento)))
                return h;
            return null;
        }

        public bool ExisteDocumento(string documento, int? excluirIdHuesped = null)
            => (int)Scalar(@"SELECT COUNT(1) FROM Huespedes
                             WHERE Documento=@d AND (@x IS NULL OR IdHuesped<>@x)",
                new SqlParameter("@d", documento),
                new SqlParameter("@x", DbVal(excluirIdHuesped))) > 0;
    }
}
