using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models;
using LudereAI.Domain.Models.Features;
using Microsoft.Extensions.Options;

namespace LudereAI.Infrastructure.Services;

public class FeatureFlagsService : IFeatureFlagsService
{
    private readonly FeatureFlagsConfig _config;

    public FeatureFlagsService(IOptions<FeatureFlagsConfig> featureFlagsConfig)
    {
        _config = featureFlagsConfig.Value;
    }

    public bool IsFeatureEnabled(string featurePath)
    {
        if (_config.MaintenanceMode && featurePath != "MaintenanceMode")
        {
            return false;
        }
        
        var pathParts = featurePath.Split('.');
        var currentObject = (object)_config;

        foreach (var part in pathParts)
        {
            var property = currentObject.GetType().GetProperty(part);
            if (property == null)
            {
                return false;
            }
            
            currentObject = property.GetValue(currentObject);
            if (currentObject == null)
            {
                return false;
            }
        }

        return currentObject is bool value && value;
    }

    public void SetFeatureState(string featurePath, bool enabled)
    {
        var pathParts = featurePath.Split('.');
        var currentObject = (object)_config;

        for (var i = 0; i < pathParts.Length - 1; i++)
        {
            var property = currentObject.GetType().GetProperty(pathParts[i]);
            if (property == null) throw new ArgumentException("Invalid feature path: ", featurePath);
            
            currentObject = property.GetValue(currentObject);
            if (currentObject == null) throw new ArgumentException("Invalid feature path: ", featurePath);
        }
        
        var lastProperty = currentObject.GetType().GetProperty(pathParts[^1]);
        if (lastProperty?.PropertyType != typeof(bool)) throw new ArgumentException("Invalid feature path: ", featurePath);
        
        lastProperty.SetValue(currentObject, enabled);
    }

    public bool IsMaintenanceModeEnabled() => _config.MaintenanceMode;

    public void SetMaintenanceModeState(bool enabled) => _config.MaintenanceMode = enabled;
}