namespace LudereAI.Domain.Models.Tiers;

public class BaseTierConfig
{
    public int DailyMessageLimit { get; set; }
    public int DailyScreenshotLimit { get; set; }
    public int ConversationHistoryLimit { get; set; }
    public bool AutomaticScreenshots { get; set; }
    public bool HasTTS { get; set; }
    public bool HasPremiumTTS { get; set; }
}