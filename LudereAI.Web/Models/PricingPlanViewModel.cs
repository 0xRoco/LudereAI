namespace LudereAI.Web.Models;

public class PricingPlanViewModel
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Price { get; set; }
    public string PriceInterval { get; set; }
    public List<string> Features { get; set; }
    public string CtaText { get; set; }
    public string CtaLink { get; set; }
    public string Badge { get; set; }
    public string BadgeClass { get; set; }
    public bool IsShine { get; set; }
    public bool IsPulse { get; set; }
    public int AnimationDelay { get; set; }
}