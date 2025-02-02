namespace LudereAI.Application.Interfaces.Services;

public interface IFeatureFlagsService
{
    bool IsFeatureEnabled(string featurePath);
    void SetFeatureState(string featurePath, bool enabled);
    bool IsMaintenanceModeEnabled();
    void SetMaintenanceModeState(bool enabled);
}