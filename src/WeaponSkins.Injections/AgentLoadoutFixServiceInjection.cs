using Microsoft.Extensions.DependencyInjection;

using WeaponSkins.Services;

namespace WeaponSkins.Injections;

public static class AgentLoadoutFixServiceInjection
{
    public static IServiceCollection AddAgentLoadoutFixService(this IServiceCollection services)
    {
        return services.AddSingleton<AgentLoadoutFixService>();
    }

    public static IServiceProvider UseAgentLoadoutFixService(this IServiceProvider provider)
    {
        provider.GetRequiredService<AgentLoadoutFixService>();
        return provider;
    }
}
