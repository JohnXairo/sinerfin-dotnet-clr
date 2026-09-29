# sinerfin-dotnet-clr

Clon de **sinerfin-dotnet** en **.NET Framework 4.8 (CLR)** para pruebas de instrumentación con **IBM Instana**.

> Replica la aplicación MVC completa (login, dashboard, transacciones, consultas, usuarios)
> usando **ASP.NET MVC 5 + OWIN self-host + TopShelf Windows Service**, sin IIS ni Visual Studio.

---

## Estructura

```
sinerfin-dotnet-clr/
├── App_Start/
│   ├── MvcConfig.cs             # Rutas MVC 5 (equiv. MapControllerRoute)
│   ├── OwinStartup.cs           # Bootstrap: MVC + WebAPI + DI + CORS
│   └── WebApiConfig.cs          # Config Web API 2 (endpoint /cliente JSON)
├── Config/
│   ├── DbConnectionFactory.cs   # Fábrica multi-motor (lee de App.config)
│   ├── DbSettings.cs            # Configuración tipada de conexiones
│   ├── DbUtil.cs                # T() — nombre de tabla por motor
│   └── PasswordUtil.cs          # BCrypt hash/verify ($2a$/$2b$/$2y$)
├── Controllers/
│   ├── ClienteController.cs     # GET /cliente?cedula= (Web API 2, JSON)
│   ├── ConsultaController.cs    # GET /consulta
│   ├── LoginController.cs       # GET+POST /login, GET /logout
│   ├── MovimientoController.cs  # POST /movimiento
│   ├── PagesController.cs       # GET /dashboard, GET /movimiento
│   └── UsuarioController.cs     # GET+POST /usuarios
├── Infrastructure/
│   └── SimpleDependencyResolver.cs  # DI para MVC 5 y Web API 2
├── Models/
│   ├── DbProvider.cs            # Enum: POSTGRES, SQLSERVER, MYSQL, ORACLE, DB2
│   └── Movimiento.cs            # Modelo de movimiento bancario
├── Repositories/
│   └── MovimientoRepository.cs  # Dapper — cuentas y movimientos
├── Services/
│   └── AppService.cs            # Start/Stop del WebApp OWIN (TopShelf)
├── Views/
│   ├── Consulta/Index.cshtml
│   ├── Dashboard/Index.cshtml
│   ├── Login/Index.cshtml
│   ├── Movimiento/Index.cshtml
│   ├── Shared/_Layout.cshtml
│   ├── Usuarios/Index.cshtml
│   └── Web.config               # Razor engine config + sessionState
├── Program.cs                   # Entry point TopShelf
├── App.config                   # Configuración (equiv. appsettings.json)
├── packages.config
├── sinerfin-dotnet-clr.csproj
└── build.ps1
```

---

## Diferencias vs sinerfin-dotnet (.NET 10)

| Aspecto | .NET 10 (original) | .NET Framework 4.8 (CLR) |
|---|---|---|
| Host | `WebApplication.CreateBuilder()` | OWIN `WebApp.Start<OwinStartup>()` |
| Windows Service | `UseWindowsService()` | **TopShelf** 4.3 |
| MVC | ASP.NET Core MVC | **ASP.NET MVC 5** |
| Sesiones | `AddSession()` / `HttpContext.Session` | `System.Web.SessionState` / `Session[]` |
| DI | Built-in `IServiceCollection` | `SimpleDependencyResolver` manual |
| Config | `appsettings.json` / `IOptions<T>` | `App.config` / `ConfigurationManager` |
| Razor Views | `@Context.Session.GetString()` | `@Session["key"]` |
| PostgreSQL | `Npgsql 8.x` | **Npgsql 4.1.x** (última compatible con .NET FW) |
| SQL Server | `Microsoft.Data.SqlClient` | `System.Data.SqlClient` (nativo .NET FW) |
| MySQL | `MySqlConnector` | `MySql.Data` (Oracle connector oficial) |
| Oracle | `Oracle.ManagedDataAccess.Core` | `Oracle.ManagedDataAccess` (versión FW) |
| DB2 | `Net.IBM.Data.Db2` | `IBM.Data.DB2.LUW` |
| Dapper | 2.1.35 | **Mismo NuGet** |
| BCrypt | 4.0.3 | **Mismo NuGet** |

---

## Build sin Visual Studio

```powershell
git clone https://github.com/JohnXairo/sinerfin-dotnet-clr.git
cd sinerfin-dotnet-clr

# Compilar (descarga nuget.exe si no existe, restaura paquetes, compila)
.\build.ps1
```

---

## Desplegar como Windows Service

```powershell
# Como Administrador
cd bin\Release

# Permitir el puerto
netsh http add urlacl url=http://+:8080/ user="NT AUTHORITY\SYSTEM"

# Instalar y arrancar
.\sinerfin-dotnet-clr.exe install
.\sinerfin-dotnet-clr.exe start

# Verificar
Invoke-RestMethod http://localhost:8080/login
```

---

## Configuración (App.config)

```xml
<appSettings>
  <add key="App:Url"                value="http://+:8080" />
  <add key="Db:Postgres:Host"       value="192.168.1.192" />
  <add key="Db:Postgres:Password"   value="TU_PASSWORD" />
  <add key="Db:SqlServer:ConnectionString" value="Server=...;Database=SINERFIN;..." />
  <add key="Db:MySql:ConnectionString"     value="Server=...;Database=sinerfin;..." />
  <add key="Db:Oracle:ConnectionString"    value="User Id=...;Data Source=...;" />
  <add key="Db:Db2:ConnectionString"       value="Server=...;Database=SINERFIN;..." />
</appSettings>
```

---

## Instrumentación Instana (CLR Profiler)

```powershell
[System.Environment]::SetEnvironmentVariable("COR_ENABLE_PROFILING", "1", "Machine")
[System.Environment]::SetEnvironmentVariable("COR_PROFILER", "{FA8F1D88-0E79-422F-8CBF-C9E2D5C0780F}", "Machine")
[System.Environment]::SetEnvironmentVariable("COR_PROFILER_PATH",
    "C:\Program Files\Instana\agent\bin\Instana.CLRProfiler.dll", "Machine")
[System.Environment]::SetEnvironmentVariable("INSTANA_AGENT_HOST", "localhost", "Machine")
[System.Environment]::SetEnvironmentVariable("INSTANA_AGENT_PORT", "42699", "Machine")

Restart-Service SinerfinDotnetCLR
```

### Spans capturados automáticamente

| Span | Tipo | Tecnología |
|---|---|---|
| `POST /login` | **entry** | ASP.NET MVC 5 (OWIN) |
| `POST /movimiento` | **entry** | ASP.NET MVC 5 |
| `GET /cliente` | **entry** | Web API 2 |
| Queries SQL (Dapper) | **exit** | ADO.NET (SqlClient, Npgsql, MySql, Oracle, DB2) |

---

## Rutas disponibles

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/login` | Pantalla de login |
| `POST` | `/login` | Autenticar usuario |
| `GET` | `/logout` | Cerrar sesión |
| `GET` | `/dashboard` | Panel principal |
| `GET` | `/movimiento` | Formulario de transacción |
| `POST` | `/movimiento` | Procesar depósito/retiro |
| `GET` | `/consulta?cedula=X` | Consultar movimientos |
| `GET` | `/usuarios` | Gestión de usuarios |
| `POST` | `/usuarios` | Crear/eliminar usuario |
| `GET` | `/cliente?cedula=X` | JSON — datos del cliente (autocompletar) |
