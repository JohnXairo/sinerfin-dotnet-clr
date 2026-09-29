using System;
using System.Configuration;
using Microsoft.Owin.Hosting;

namespace Sinerfin.Services
{
    public class AppService
    {
        private IDisposable _webApp;

        public bool Start()
        {
            var url = ConfigurationManager.AppSettings["App:Url"] ?? "http://+:8080";
            System.Diagnostics.Trace.TraceInformation("[AppService] Iniciando en {0}", url);
            try
            {
                _webApp = WebApp.Start<OwinStartup>(url);
                System.Diagnostics.Trace.TraceInformation("[AppService] Escuchando en {0}", url);
                return true;
            }
            catch (Exception ex)
            {
                // Imprimir cadena completa de InnerException
                Console.WriteLine("=== ERROR INICIO ===");
                var e = ex; int n = 0;
                while (e != null)
                {
                    Console.WriteLine("[{0}] {1}: {2}", n, e.GetType().FullName, e.Message);
                    Console.WriteLine(e.StackTrace);
                    e = e.InnerException; n++;
                }
                Console.WriteLine("=== FIN ERROR ===");
                return false;
            }
        }

        public bool Stop()
        {
            _webApp?.Dispose();
            return true;
        }
    }
}
