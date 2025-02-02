using System.Diagnostics;
using LudereAI.Shared.DTOs;
using LudereAI.Shared.Enums;
using LudereAI.WPF.Interfaces;
using Microsoft.Extensions.Logging;

namespace LudereAI.WPF.Services;

public class SubscriptionService(ILogger<ISubscriptionService> logger,
    IAPIClient apiClient) : ISubscriptionService
{
    public async Task Subscribe(SubscriptionPlan subscriptionPlan)
    {
        var request = new SubscriptionRequestDTO(){ SubscriptionPlan = subscriptionPlan };
        
        var result = await apiClient.PostAsync<string>("Subscription/Subscribe", request);
        if (result is { IsSuccess: true, Data: not null })
        {
            var sessionUrl = result.Data;
            logger.LogInformation("Redirecting to {sessionUrl}", sessionUrl);
            
            Process.Start (new ProcessStartInfo(sessionUrl) { UseShellExecute = true });
        }
        else
        {
            logger.LogWarning("Subscription Failed: {message}", result?.Message);
        }
    }

    public async Task CancelSubscription()
    {
        throw new NotImplementedException();
    }
}