namespace LudereAI.Domain.Models.Tiers;

public class TierLimitsConfig
{
    public BaseTierConfig Guest { get; set; }
    public BaseTierConfig Free { get; set; }
    public BaseTierConfig Pro { get; set; }
    public BaseTierConfig Ultimate { get; set; }
}