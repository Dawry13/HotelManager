using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using HotelManager.Business.Interfaces;

namespace HotelManager.Data.Base
{
    /// <summary>
    /// Repositorio base (ADO.NET). Los repositorios concretos definen tabla, llave y mapeo.
    /// Cadena de conexión: App.config -> connectionStrings -> "HotelManagerDb".
    /// </summary>
    public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly string ConnectionString =
            ConfigurationManager.ConnectionStrings["HotelManagerDB"].ConnectionString;

        protected abstract string TableName { get; }
        protected abstract string KeyColumn { get; }
        protected abstract T Map(IDataRecord r);

        public abstract int Add(T entity);
        public abstract bool Update(T entity);

        public virtual IEnumerable<T> GetAll()
            => Query($"SELECT * FROM {TableName}");

        public virtual T GetById(int id)
        {
            foreach (var e in Query($"SELECT * FROM {TableName} WHERE {KeyColumn} = @id",
                                    new SqlParameter("@id", id)))
                return e;
            return null;
        }

        public virtual bool Delete(int id)
            => Execute($"DELETE FROM {TableName} WHERE {KeyColumn} = @id",
                       new SqlParameter("@id", id)) > 0;

        // ---------- helpers ----------
        protected List<T> Query(string sql, params SqlParameter[] parameters)
        {
            var list = new List<T>();
            using (var cn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddRange(parameters);
                cn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            return list;
        }

        protected int Execute(string sql, params SqlParameter[] parameters)
        {
            using (var cn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddRange(parameters);
                cn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        protected object Scalar(string sql, params SqlParameter[] parameters)
        {
            using (var cn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddRange(parameters);
                cn.Open();
                return cmd.ExecuteScalar();
            }
        }

        protected static object DbVal(object v) => v ?? DBNull.Value;
    }
}
