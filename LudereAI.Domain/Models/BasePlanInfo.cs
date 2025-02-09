namespace LudereAI.Domain.Models;

public class BasePlanInfo
{
    public string Name { get; set; } = "undefined";
    public string Description { get; set; } = "undefined";
    public decimal MonthlyPrice { get; set; }
    public decimal AnnualPrice { get; set; }
    public bool HasTrial { get; set; }
    public int TrialDuration { get; set; }
    public string Badge { get; set; } = "";
    public bool Shine { get; set; }
    public bool CallToAction { get; set; }
    public List<string> Features { get; set; } = [];
}