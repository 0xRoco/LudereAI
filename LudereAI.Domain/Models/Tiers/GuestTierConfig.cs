namespace LudereAI.Domain.Models.Tiers;

public class GuestTierConfig : BaseTierConfig
{
    public GuestTierConfig()
    {
        DailyMessageLimit = 10;
        DailyScreenshotLimit = 3;
        ConversationHistoryLimit = 1;
        AutomaticScreenshots = false;
        HasTTS = false;
        HasPremiumTTS = false;
    }
}