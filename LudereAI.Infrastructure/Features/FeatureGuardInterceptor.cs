using LudereAI.Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Reflection;

namespace LudereAI.Infrastructure.Features;

public class FeatureGuardInterceptor : IInterceptor
{
    private readonly IFeatureFlagsService _featureFlags;

    public FeatureGuardInterceptor(IFeatureFlagsService featureFlags)
    {
        _featureFlags = featureFlags;
    }
    
}