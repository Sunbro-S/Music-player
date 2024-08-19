using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Services.Services;

namespace Services;

public static class ServiceStartUp
{
    public static IServiceCollection TryAddService(this IServiceCollection services)
    {
        services.TryAddScoped<IMusicService, MusicService>();
        services.TryAddScoped<IPlaylistService, PlaylistService>();
        services.TryAddScoped<IMusicOrderService, MusicOrderService>();
        return services;
    }
}