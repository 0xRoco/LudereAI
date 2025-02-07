namespace LudereAI.Domain.Models.Tiers;

public class FreeTierConfig : BaseTierConfig
{
    public FreeTierConfig()
    {
        DailyMessageLimit = 100;
        DailyScreenshotLimit = 10;
        ConversationHistoryLimit = 7;
        AutomaticScreenshots = false;
        HasTTS = false;
        HasPremiumTTS = false;
    }
}