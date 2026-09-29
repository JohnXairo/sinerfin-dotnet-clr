using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Sinerfin.Config;
using Sinerfin.Controllers;
using Sinerfin.Repositories;

namespace Sinerfin.Infrastructure
{
    // Web API 2 DI resolver — only needed for ClienteController now
    public class SimpleApiDependencyResolver : System.Web.Http.Dependencies.IDependencyResolver
    {
        private readonly DbConnectionFactory _factory;
        private readonly MovimientoRepository _repo;

        public SimpleApiDependencyResolver(DbConnectionFactory factory, MovimientoRepository repo)
        {
            _factory = factory;
            _repo    = repo;
        }

        public IDependencyScope BeginScope() => this;

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(ClienteController))
                return new ClienteController(_factory, _repo);
            return null;
        }

        public IEnumerable<object> GetServices(Type serviceType) => new object[0];
        public void Dispose() { }
    }
}
