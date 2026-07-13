using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sinistros.Domain.Apolices;
using Sinistros.Domain.Clientes;
using Sinistros.Domain.SeedWork;
using Sinistros.Domain.Sinistros;
using Sinistros.Infrastructure.Persistence;
using Sinistros.Infrastructure.Persistence.Repositories;

namespace Sinistros.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());

            services.AddScoped<IClienteRepository, ClienteRepository>();
            services.AddScoped<IApoliceRepository, ApoliceRepository>();
            services.AddScoped<ISinistroRepository, SinistroRepository>();

            return services;
        }
    }
}
