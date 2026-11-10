using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace HotelManager.Web.Infrastructure
{
    /// <summary>
    /// Conecta el contenedor Microsoft.Extensions.DependencyInjection (usado por la
    /// capa Business) con ASP.NET MVC 5. Crea un scope por cada peticion HTTP.
    /// </summary>
    public class ServiceProviderDependencyResolver : IDependencyResolver
    {
        private const string ScopeKey = "__HotelManager_RequestScope";
        private readonly IServiceProvider _rootProvider;

        public ServiceProviderDependencyResolver(IServiceProvider rootProvider)
        {
            _rootProvider = rootProvider;
        }

        private IServiceProvider Current
        {
            get
            {
                var context = HttpContext.Current;
                if (context == null) return _rootProvider;

                var scope = context.Items[ScopeKey] as IServiceScope;
                if (scope == null)
                {
                    scope = _rootProvider.CreateScope();
                    context.Items[ScopeKey] = scope;
                }
                return scope.ServiceProvider;
            }
        }

        public object GetService(Type serviceType)
        {
            return Current.GetService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return Current.GetServices(serviceType).Where(s => s != null);
        }

        /// <summary>Se llama al final de cada peticion para liberar el scope.</summary>
        public static void DisposeRequestScope(HttpContext context)
        {
            var scope = context?.Items[ScopeKey] as IServiceScope;
            if (scope != null)
            {
                scope.Dispose();
                context.Items.Remove(ScopeKey);
            }
        }
    }
}
