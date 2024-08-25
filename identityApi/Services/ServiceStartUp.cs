using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RPC;
using Services.Services;
using Services.Services.Interfaces;
namespace Services;

public static class ServiceStartUp
{
    public static IServiceCollection TryAddService(this IServiceCollection services)
    {
        services.AddHostedService<RPCListener>();
        services.TryAddScoped<IAuthService, AuthService>();
        return services;
    }
}