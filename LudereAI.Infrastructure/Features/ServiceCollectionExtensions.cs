using Microsoft.Extensions.DependencyInjection;

namespace LudereAI.Infrastructure.Features;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFeatureGuards(this IServiceCollection services)
    {
        services.AddScoped<FeatureGuardInterceptor>();
        return services;
    }
}