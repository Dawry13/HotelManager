using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace HotelManager.Data.Base
{
    public static class InicializadorBaseDatos
    {
        public const string ScriptEstructura = "HotelManager.Data.Database.01_HotelManagerDB.sql";
        public const string ScriptExtension = "HotelManager.Data.Database.02_bloques_tipos_habitacion.sql";
        public const string ScriptUsuarios = "HotelManager.Data.Database.03_usuarios_roles.sql";

        private static readonly object Bloqueo = new object();
        private static readonly Regex SeparadorGo =
            new Regex(@"^\s*go\s*(?:--.*)?\r?$", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        public static IList<string> UltimasAcciones { get; private set; } = new List<string>();

        public static IList<string> Inicializar()
        {
            lock (Bloqueo)
            {
                var acciones = new List<string>();
                var nombre = Conexion.BaseDatos;

                using (var master = new SqlConnection(Conexion.CadenaMaster))
                {
                    master.Open();

                    if (!ExisteBase(master, nombre))
                    {
                        string mdf, ldf;
                        if (BuscarArchivos(master, nombre, out mdf, out ldf))
                        {
                            Adjuntar(master, nombre, mdf, ldf);
                            acciones.Add("Base de datos adjuntada desde " + mdf);
                        }
                        else
                        {
                            EjecutarScript(master, LeerScript(ScriptEstructura), omitirCreacion: false);
                            acciones.Add("Base de datos creada con 01_HotelManagerDB.sql");
                        }
                    }
                }

                using (var db = Conexion.Crear())
                {
                    db.Open();

                    if (!ExisteTabla(db, "hoteles"))
                    {
                        // La base existe pero esta vacia: se crean las tablas sin volver a crear la base.
                        EjecutarScript(db, LeerScript(ScriptEstructura), omitirCreacion: true);
                        acciones.Add("Tablas creadas con 01_HotelManagerDB.sql");
                    }

                    if (!ExisteTabla(db, "bloques") || !ExisteTabla(db, "tipos_habitacion") || !ExisteColumna(db, "pisos", "id_bloque"))
                    {
                        EjecutarScript(db, LeerScript(ScriptExtension), omitirCreacion: true);
                        acciones.Add("Extension aplicada con 02_bloques_tipos_habitacion.sql");
                    }

                    if (!ExisteTabla(db, "usuarios"))
                    {
                        EjecutarScript(db, LeerScript(ScriptUsuarios), omitirCreacion: true);
                        acciones.Add("Tabla de usuarios creada con 03_usuarios_roles.sql");
                    }
                }

                UltimasAcciones = acciones;
                return acciones;
            }
        }

        private static bool ExisteBase(SqlConnection master, string nombre)
        {
            using (var cmd = new SqlCommand("select db_id(@n)", master))
            {
                cmd.Parameters.AddWithValue("@n", nombre);
                return cmd.ExecuteScalar() != DBNull.Value;
            }
        }

        private static bool ExisteTabla(SqlConnection db, string tabla)
        {
            using (var cmd = new SqlCommand("select object_id(@t, 'U')", db))
            {
                cmd.Parameters.AddWithValue("@t", "dbo." + tabla);
                return cmd.ExecuteScalar() != DBNull.Value;
            }
        }

        private static bool ExisteColumna(SqlConnection db, string tabla, string columna)
        {
            using (var cmd = new SqlCommand("select col_length(@t, @c)", db))
            {
                cmd.Parameters.AddWithValue("@t", "dbo." + tabla);
                cmd.Parameters.AddWithValue("@c", columna);
                return cmd.ExecuteScalar() != DBNull.Value;
            }
        }

        /// <summary>Busca HotelManagerDB.mdf / _log.ldf en la carpeta de datos por defecto del servidor.</summary>
        private static bool BuscarArchivos(SqlConnection master, string nombre, out string mdf, out string ldf)
        {
            mdf = ldf = null;
            using (var cmd = new SqlCommand("select cast(serverproperty('InstanceDefaultDataPath') as nvarchar(400))", master))
            {
                var carpeta = cmd.ExecuteScalar() as string;
                if (string.IsNullOrEmpty(carpeta)) return false;

                mdf = Path.Combine(carpeta, nombre + ".mdf");
                ldf = Path.Combine(carpeta, nombre + "_log.ldf");
                return File.Exists(mdf);
            }
        }

        private static void Adjuntar(SqlConnection master, string nombre, string mdf, string ldf)
        {
            Func<string, string> lit = s => "N'" + s.Replace("'", "''") + "'";
            var sql = File.Exists(ldf)
                ? "create database [" + nombre + "] on (filename = " + lit(mdf) + "), (filename = " + lit(ldf) + ") for attach"
                : "create database [" + nombre + "] on (filename = " + lit(mdf) + ") for attach_rebuild_log";

            using (var cmd = new SqlCommand(sql, master) { CommandTimeout = 120 })
                cmd.ExecuteNonQuery();
        }

        private static string LeerScript(string recurso)
        {
            var ensamblado = Assembly.GetExecutingAssembly();
            using (var stream = ensamblado.GetManifestResourceStream(recurso))
            {
                if (stream == null)
                    throw new InvalidOperationException("No se encontró el script incrustado " + recurso + ".");
                using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8, true))
                    return reader.ReadToEnd();
            }
        }

        /// <summary>Ejecuta un script con separadores GO, lote por lote, en la misma conexion.</summary>
        private static void EjecutarScript(SqlConnection conexion, string script, bool omitirCreacion)
        {
            var lotes = SeparadorGo.Split(script)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0 && !SoloComentarios(s));

            foreach (var lote in lotes)
            {
                if (omitirCreacion && lote.IndexOf("create database", StringComparison.OrdinalIgnoreCase) >= 0)
                    continue;

                using (var cmd = new SqlCommand(lote, conexion) { CommandTimeout = 120 })
                    cmd.ExecuteNonQuery();
            }
        }

        private static bool SoloComentarios(string lote)
        {
            return lote.Split('\n').All(l => l.Trim().Length == 0 || l.Trim().StartsWith("--"));
        }
    }
}
