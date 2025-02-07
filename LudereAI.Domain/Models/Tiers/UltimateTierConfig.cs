namespace LudereAI.Domain.Models.Tiers;

public class UltimateTierConfig : BaseTierConfig
{
    public UltimateTierConfig()
    {
        DailyMessageLimit = 0;
        DailyScreenshotLimit = 0;
        ConversationHistoryLimit = 0;
        AutomaticScreenshots = true;
        HasTTS = true;
        HasPremiumTTS = true;
    }
}