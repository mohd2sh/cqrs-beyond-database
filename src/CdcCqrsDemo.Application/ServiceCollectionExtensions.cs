using CleanArchitecture.Core.Application;
using Microsoft.Extensions.DependencyInjection;

namespace CdcCqrsDemo.Application
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {

            services.AddApplicationHandlers(typeof(ServiceCollectionExtensions).Assembly);

            return services;
        }
    }
}
