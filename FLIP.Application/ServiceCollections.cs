using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FLIP.Application;

public static class ServiceCollections
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(Assembly.GetExecutingAssembly());

        return services;
    }
}
