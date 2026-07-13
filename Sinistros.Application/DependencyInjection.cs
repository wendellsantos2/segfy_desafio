using Microsoft.Extensions.DependencyInjection;
using Sinistros.Domain.Servicos;

namespace Sinistros.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
            services.AddTransient<AberturaDeSinistroService>();
            return services;
        }
    }
}
