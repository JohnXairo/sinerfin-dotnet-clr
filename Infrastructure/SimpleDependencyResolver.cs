using System;
using System.Collections.Generic;
using Sinerfin.Config;
using Sinerfin.Controllers;
using Sinerfin.Repositories;

namespace Sinerfin.Infrastructure
{
    /// <summary>
    /// DI resolver para MVC 5 controllers.
    /// Implementa System.Web.Mvc.IDependencyResolver (interfaz de MVC).
    /// </summary>
    public class SimpleMvcDependencyResolver : System.Web.Mvc.IDependencyResolver
    {
        private readonly DbConnectionFactory  _factory;
        private readonly MovimientoRepository _repo;

        public SimpleMvcDependencyResolver(DbConnectionFactory factory, MovimientoRepository repo)
        {
            _factory = factory;
            _repo    = repo;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(LoginController))      return new LoginController(_factory);
            if (serviceType == typeof(UsuarioController))    return new UsuarioController(_factory);
            if (serviceType == typeof(ConsultaController))   return new ConsultaController(_repo);
            if (serviceType == typeof(MovimientoController)) return new MovimientoController(_repo);
            if (serviceType == typeof(PagesController))      return new PagesController();
            return null;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return new List<object>();
        }
    }

    /// <summary>
    /// DI resolver para Web API 2 (ClienteController JSON endpoint).
    /// Implementa System.Web.Http.Dependencies.IDependencyResolver (interfaz de Web API).
    /// </summary>
    public class SimpleApiDependencyResolver : System.Web.Http.Dependencies.IDependencyResolver
    {
        private readonly DbConnectionFactory  _factory;
        private readonly MovimientoRepository _repo;

        public SimpleApiDependencyResolver(DbConnectionFactory factory, MovimientoRepository repo)
        {
            _factory = factory;
            _repo    = repo;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(ClienteController)) return new ClienteController(_repo);
            return null;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return new List<object>();
        }

        public System.Web.Http.Dependencies.IDependencyScope BeginScope()
        {
            return this;
        }

        public void Dispose() { }
    }
}
