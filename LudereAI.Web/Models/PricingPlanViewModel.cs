namespace LudereAI.Web.Models;

public class PricingPlanViewModel
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Price { get; set; }
    public string PriceInterval { get; set; }
    public List<string> Features { get; set; }
    public string CtaText { get; set; }
    
    // Link properties
    public string CtaLink { get; set; }
    public bool IsDisabled { get; set; }
    
    // Form properties
    public bool IsFormSubmit { get; set; }
    public string FormController { get; set; }
    public string FormAction { get; set; }
    public string FormMethod { get; set; } = "post";
    public Dictionary<string, object> FormParameters { get; set; } = new();
    
    // Style properties
    public string Badge { get; set; }
    public string BadgeClass { get; set; }
    public bool IsShine { get; set; }
    public bool IsPulse { get; set; }
    public int AnimationDelay { get; set; }

    public int ColumnWidth { get; set; } = 3;
}