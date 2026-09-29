using System.Configuration;

namespace Sinerfin.Config
{
    /// <summary>
    /// Configuracion de conexiones por motor.
    /// Equivalente a DbSettings.cs .NET Core pero lee de App.config
    /// en vez de IOptions / appsettings.json.
    /// </summary>
    public class DbSettings
    {
        public PostgresConfig   Postgres     { get; set; }
        public ConnStringConfig SqlServer    { get; set; }
        public ConnStringConfig SqlServerDev { get; set; }
        public ConnStringConfig MySql        { get; set; }
        public ConnStringConfig Oracle       { get; set; }
        public ConnStringConfig Db2          { get; set; }

        public class PostgresConfig
        {
            public string Host     { get; set; }
            public int    Port     { get; set; }
            public string Database { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }

            public string ToConnectionString() =>
                string.Format("Host={0};Port={1};Database={2};Username={3};Password={4}",
                    Host, Port, Database, Username, Password);
        }

        public class ConnStringConfig
        {
            public string ConnectionString { get; set; }
        }

        /// <summary>Lee todos los valores desde ConfigurationManager.AppSettings.</summary>
        public static DbSettings FromAppConfig()
        {
            var cfg = ConfigurationManager.AppSettings;
            return new DbSettings
            {
                Postgres = new PostgresConfig
                {
                    Host     = cfg["Db:Postgres:Host"]     ?? "localhost",
                    Port     = int.TryParse(cfg["Db:Postgres:Port"], out int p) ? p : 5432,
                    Database = cfg["Db:Postgres:Database"] ?? "sinerfin",
                    Username = cfg["Db:Postgres:Username"] ?? "",
                    Password = cfg["Db:Postgres:Password"] ?? ""
                },
                SqlServer    = new ConnStringConfig { ConnectionString = cfg["Db:SqlServer:ConnectionString"]    ?? "" },
                SqlServerDev = new ConnStringConfig { ConnectionString = cfg["Db:SqlServerDev:ConnectionString"] ?? "" },
                MySql        = new ConnStringConfig { ConnectionString = cfg["Db:MySql:ConnectionString"]        ?? "" },
                Oracle       = new ConnStringConfig { ConnectionString = cfg["Db:Oracle:ConnectionString"]       ?? "" },
                Db2          = new ConnStringConfig { ConnectionString = cfg["Db:Db2:ConnectionString"]          ?? "" }
            };
        }
    }
}
