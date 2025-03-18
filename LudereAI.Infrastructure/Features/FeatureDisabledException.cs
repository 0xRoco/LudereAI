namespace LudereAI.Infrastructure.Features;

public class FeatureDisabledException : Exception
{
    public FeatureDisabledException(string message) : base(message) { }
}