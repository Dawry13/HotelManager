using System.Configuration;
using System.Data.SqlClient;

namespace HotelManager.Data.Base
{
    public static class Conexion
    {
        public const string Nombre = "HotelManagerDB";
        public const string BaseDatos = "HotelManagerDB";

        private const string CadenaPorDefecto =
            @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=HotelManagerDB;Integrated Security=True;Connect Timeout=60";

        public static string Cadena
        {
            get
            {
                var config = ConfigurationManager.ConnectionStrings[Nombre];
                return config != null && !string.IsNullOrWhiteSpace(config.ConnectionString)
                    ? config.ConnectionString
                    : CadenaPorDefecto;
            }
        }

        /// <summary>Misma conexion pero apuntando a master (para crear o adjuntar la base).</summary>
        public static string CadenaMaster
        {
            get
            {
                var builder = new SqlConnectionStringBuilder(Cadena) { InitialCatalog = "master" };
                return builder.ConnectionString;
            }
        }

        public static SqlConnection Crear()
        {
            return new SqlConnection(Cadena);
        }
    }
}
