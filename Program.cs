using System;
using Sinerfin.Services;
using Topshelf;

namespace Sinerfin
{
    /// <summary>
    /// Punto de entrada — TopShelf registra el proceso como Windows Service.
    ///
    /// Uso:
    ///   sinerfin-dotnet-clr.exe           -> consola (debug)
    ///   sinerfin-dotnet-clr.exe install   -> instala el servicio
    ///   sinerfin-dotnet-clr.exe start     -> arranca el servicio
    ///   sinerfin-dotnet-clr.exe stop      -> detiene el servicio
    ///   sinerfin-dotnet-clr.exe uninstall -> desinstala el servicio
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var exitCode = HostFactory.Run(host =>
            {
                host.Service<AppService>(svc =>
                {
                    svc.ConstructUsing(() => new AppService());
                    svc.WhenStarted(s => s.Start());
                    svc.WhenStopped(s => s.Stop());
                });

                host.RunAsLocalSystem();
                host.StartAutomatically();

                host.SetServiceName("SinerfinDotnetCLR");
                host.SetDisplayName("Sinerfin Dotnet CLR (.NET 4.8)");
                host.SetDescription(
                    "Core bancario demo .NET Framework 4.8 para pruebas de instrumentacion Instana");
            });

            int code = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
            Environment.Exit(code);
        }
    }
}
