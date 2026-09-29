using System.Data;
using System.Configuration;
using Npgsql;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using IBM.Data.DB2;
using Sinerfin.Models;

namespace Sinerfin.Config
{
    /// <summary>
    /// Fabrica de conexiones multi-motor.
    /// Equivalente a DbConnectionFactory.cs de .NET Core,
    /// pero sin IOptions — lee directamente de ConfigurationManager (App.config).
    /// </summary>
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
                    conn = new DB2Connection(_settings.Db2.ConnectionString);
                    break;
                default:
                    throw new System.NotSupportedException("Motor no soportado: " + provider);
            }
            conn.Open();
            return conn;
        }
    }
}
