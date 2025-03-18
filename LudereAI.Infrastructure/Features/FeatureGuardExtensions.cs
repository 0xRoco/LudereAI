using System.Reflection;
using LudereAI.Application.Interfaces.Services;

namespace LudereAI.Infrastructure.Features;

public static class FeatureGuardExtensions
{
    public static void GuardFeature(this IFeatureFlagsService featureFlags, MethodBase method)
    {
        var attribute = method.GetCustomAttribute<FeatureGuardAttribute>();
        
        if (attribute != null && !featureFlags.IsFeatureEnabled(attribute.FeaturePath))
        {
            throw new FeatureDisabledException($"Feature {attribute.FeaturePath} is currently disabled");
        }
    }
}