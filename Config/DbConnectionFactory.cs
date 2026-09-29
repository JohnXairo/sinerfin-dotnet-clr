using System;
using System.Data;
using System.Configuration;
using Npgsql;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using Sinerfin.Models;

namespace Sinerfin.Config
{
    public class DbConnectionFactory
    {
        private readonly DbSettings _settings;

        public DbConnectionFactory(DbSettings settings)
        {
            _settings = settings;
        }

        public IDbConnection GetConnection(DbProvider provider)
        {
            IDbConnection conn;
            switch (provider)
            {
                case DbProvider.POSTGRES:
                    conn = new NpgsqlConnection(_settings.Postgres.ToConnectionString());
                    break;
                case DbProvider.SQLSERVER:
                    conn = new SqlConnection(_settings.SqlServer.ConnectionString);
                    break;
                case DbProvider.SQLSERVER_DEV:
                    conn = new SqlConnection(_settings.SqlServerDev.ConnectionString);
                    break;
                case DbProvider.MYSQL:
                    conn = new MySqlConnection(_settings.MySql.ConnectionString);
                    break;
                case DbProvider.ORACLE:
                    conn = new OracleConnection(_settings.Oracle.ConnectionString);
                    break;
                case DbProvider.DB2:
                    conn = CreateDb2Connection(_settings.Db2.ConnectionString);
                    break;
                default:
                    throw new NotSupportedException("Motor no soportado: " + provider);
            }
            conn.Open();
            return conn;
        }

        /// <summary>
        /// Carga IBM.Data.DB2.dll via reflection para no requerir la DLL en compile-time.
        /// Requiere IBM Data Server Client instalado en:
        /// C:\Program Files\IBM\SQLLIB\BIN\netf40_64\IBM.Data.DB2.dll
        /// </summary>
        private static IDbConnection CreateDb2Connection(string connectionString)
        {
            var db2Path = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "IBM", "SQLLIB", "BIN", "netf40_64", "IBM.Data.DB2.dll");

            if (!System.IO.File.Exists(db2Path))
                throw new NotSupportedException(
                    "IBM DB2 driver no encontrado en: " + db2Path + ".\n" +
                    "Instala IBM Data Server Client para usar DB2.");

            var asm  = System.Reflection.Assembly.LoadFrom(db2Path);
            var type = asm.GetType("IBM.Data.DB2.DB2Connection");
            return (IDbConnection)Activator.CreateInstance(type, connectionString);
        }
    }
}
