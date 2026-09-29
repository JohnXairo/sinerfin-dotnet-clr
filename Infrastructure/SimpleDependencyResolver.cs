using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using System.Web.Mvc;
using Sinerfin.Config;
using Sinerfin.Controllers;
using Sinerfin.Repositories;

namespace Sinerfin.Infrastructure
{
    /// <summary>DI resolver para MVC 5 controllers.</summary>
    public class SimpleMvcDependencyResolver : IDependencyResolver
    {
        private readonly DbConnectionFactory _factory;
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

        public IEnumerable<object> GetServices(Type serviceType) => new List<object>();
        public void ReleaseScope() { }
    }

    /// <summary>DI resolver para Web API 2 (ClienteController).</summary>
    public class SimpleApiDependencyResolver : System.Web.Http.Dependencies.IDependencyResolver
    {
        private readonly DbConnectionFactory _factory;
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

        public IEnumerable<object> GetServices(Type serviceType) => new List<object>();
        public IDependencyScope BeginScope() => this;
        public void Dispose() { }
    }
}
