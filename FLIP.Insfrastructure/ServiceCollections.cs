using FLIP.Application.Interfaces;
using FLIP.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FLIP.Infrastructure;

public static class ServiceCollections
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IDapperQueries, DapperQueries>();
        services.AddScoped<INotifyMessages, NotifyMessages>();
        services.AddScoped<IAPIIntegeration, APIIntegeration>();
        return services;
    }
}
