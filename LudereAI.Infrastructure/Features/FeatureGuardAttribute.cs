namespace LudereAI.Infrastructure.Features;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class FeatureGuardAttribute : Attribute
{
    public string FeaturePath { get; }

    public FeatureGuardAttribute(string featurePath)
    {
        FeaturePath = featurePath;
    }
}