namespace LudereAI.Domain.Models.Configs;

public class StripeConfig
{
    public string SecretKey { get; set; }
    public string PublishableKey { get; set; }
    public string WebhookSecret { get; set; }
    public string ProMonthlyId { get; set; }
    public string ProYearlyId { get; set; }
    public string UltimateMonthlyId { get; set; }
    public string UltimateYearlyId { get; set; }
    public string SuccessUrl { get; set; }
    public string CancelUrl { get; set; }
}