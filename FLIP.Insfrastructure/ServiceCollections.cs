using FLIP.Application.Interfaces;
using FLIP.Infrastructure.Dapperervice;
using Microsoft.Extensions.DependencyInjection;

namespace FLIP.Infrastructure;

public static class ServiceCollections
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IDapperQueries, DapperQueries>();

        return services;
    }
}
