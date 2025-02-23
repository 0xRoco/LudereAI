using System.Collections.Concurrent;
using System.Reflection;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LudereAI.Infrastructure.Services;


public class FeatureFlagsService : IFeatureFlagsService
{
    private readonly ILogger<IFeatureFlagsService> _logger;
    private readonly IOptionsMonitor<FeatureFlagsConfig> _optionsMonitor;
    
    private readonly ConcurrentDictionary<string, PropertyInfo[]> _propertyCache = new();

    public FeatureFlagsService(IOptionsMonitor<FeatureFlagsConfig> optionsMonitor, ILogger<IFeatureFlagsService> logger)
    {
        _logger = logger;
        _optionsMonitor = optionsMonitor;
    }

    public bool IsFeatureEnabled(string featurePath)
    {
        try
        {
            if (Config.MaintenanceMode && featurePath != "MaintenanceMode")
            {
                return false;
            }

            var properties = GetProperties(featurePath);
            var currentObject = (object)Config;

            foreach (var property in properties)
            {
                currentObject = property.GetValue(currentObject);
                if (currentObject == null)
                {
                    _logger.LogWarning("Feature path is invalid: {FeaturePath}", featurePath);
                    return false;
                }
            }

            return currentObject is bool value && value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking feature state for path: {FeaturePath}", featurePath);
            return false;
        }
    }

    public void SetFeatureState(string featurePath, bool enabled)
    {
        try
        {
            var properties = GetProperties(featurePath);
            var currentObject = (object)Config;

            for (var i = 0; i < properties.Length - 1; i++)
            {
                currentObject = properties[i].GetValue(currentObject);
                if (currentObject == null) throw new ArgumentException("Invalid feature path: ", featurePath);
            }
            
            var lastProperty = properties[^1];
            if (lastProperty.PropertyType != typeof(bool)) throw new ArgumentException("Invalid feature path: ", featurePath);
            
            lastProperty.SetValue(currentObject, enabled);
            _logger.LogInformation("Feature state set for path: {FeaturePath} to {Enabled}", featurePath, enabled);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting feature state for path: {FeaturePath}", featurePath);
        }
    }

    public bool IsMaintenanceModeEnabled() => Config.MaintenanceMode;

    public void SetMaintenanceModeState(bool enabled) => Config.MaintenanceMode = enabled;
    
    private PropertyInfo[] GetProperties(string featurePath)
    {
        return _propertyCache.GetOrAdd(featurePath, path =>
        {
            var pathParts = path.Split('.');
            var properties = new PropertyInfo[pathParts.Length];
            var currentType = typeof(FeatureFlagsConfig);

            for (var i = 0; i < pathParts.Length; i++)
            {
                properties[i] = currentType.GetProperty(pathParts[i]) ?? throw new ArgumentException("Invalid feature path: ", path);
                if (properties[i] == null) throw new ArgumentException("Invalid feature path: ", path);
                currentType = properties[i].PropertyType;
            }

            return properties;
        });
    }
    
    private FeatureFlagsConfig Config => _optionsMonitor.CurrentValue;
}