namespace LudereAI.Domain.Models.Tiers;

public class ProTierConfig : BaseTierConfig
{
    public ProTierConfig()
    {
        DailyMessageLimit = 1000;
        DailyScreenshotLimit = 100;
        ConversationHistoryLimit = 30;
        AutomaticScreenshots = true;
        HasTTS = true;
        HasPremiumTTS = false;
    }
}