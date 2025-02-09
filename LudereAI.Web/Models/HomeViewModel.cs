using LudereAI.Domain.Models.Configs;

namespace LudereAI.Web.Models;

public class HomeViewModel
{
    public PlansInfoConfig Plans { get; set; } = new();
}