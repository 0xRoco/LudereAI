using System.Net;
using System.Security.Claims;
using LudereAI.API.Core;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models.Configs;
using LudereAI.Shared;
using LudereAI.Shared.DTOs;
using LudereAI.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;

namespace LudereAI.API.Controllers;


[ApiController, Authorize, Route("api/[controller]")]
public class SubscriptionController(ILogger<SubscriptionController> logger,
    IAccountService accountService,
    IStripeService stripeService,
    ISubscriptionService subscriptionService,
    IOptions<StripeConfig> stripeConfig) : ControllerBase
{
    
    [HttpGet("me")]
    public async Task<ActionResult<SubscriptionDTO>> GetCurrentSubscription()
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(accountId))
        {
            logger.LogWarning("Invalid token received for subscription attempt");
            return BadRequest(APIResult<SubscriptionDTO>.Error(HttpStatusCode.BadRequest, "Invalid token"));
        }

        var account = await accountService.GetAccount(accountId);
        if (account == null)
        {
            logger.LogWarning("Account not found for ID {AccountId}", accountId);
            return NotFound(APIResult<SubscriptionDTO>.Error(HttpStatusCode.NotFound, "Account not found"));
        }
        
        var subscription = await subscriptionService.GetSubscriptionByAccountId(account.Id);

        return Ok(APIResult<SubscriptionDTO>.Success(data: subscription));
    }
    
    [RequireFeature("Subscription.NewSubscriptionsEnabled")]
    [HttpPost("Subscribe")]
    public async Task<ActionResult<APIResult<string>>> Subscribe([FromBody] SubscriptionRequestDTO dto)
    {
        try
        {
            var account = await ValidateAndGetAccount();
            if (account.Result != null) return account.Result;

            var validationResult = await ValidateSubscriptionEligibility(account.Account!);
            if (validationResult != null) return validationResult;

            var priceId = GetPriceIdForPlan(dto.SubscriptionPlan);
            if (string.IsNullOrWhiteSpace(priceId))
            {
                logger.LogWarning("Invalid plan type received {PlanType}", dto.SubscriptionPlan);
                return BadRequest(APIResult<string>.Error(HttpStatusCode.BadRequest, "Invalid plan type"));
            }

            return await CreateStripeCheckoutSession(account.Account!, priceId, dto.SubscriptionPlan);
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe error occurred while creating subscription session");
            return StatusCode((int)HttpStatusCode.ServiceUnavailable,
                APIResult<string>.Error(HttpStatusCode.ServiceUnavailable, "Payment service unavailable"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while processing subscription");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                APIResult<string>.Error(HttpStatusCode.InternalServerError, "An unexpected error occurred"));
        }
    }

    private async Task<(AccountDTO? Account, ActionResult<APIResult<string>>? Result)> ValidateAndGetAccount()
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(accountId))
        {
            logger.LogWarning("Invalid token received for subscription attempt");
            return (null, BadRequest(APIResult<string>.Error(HttpStatusCode.BadRequest, "Invalid token")));
        }

        var account = await accountService.GetAccount(accountId);
        if (account == null)
        {
            logger.LogWarning("Account not found for ID {AccountId}", accountId);
            return (null, NotFound(APIResult<string>.Error(HttpStatusCode.NotFound, "Account not found")));
        }

        return (account, null);
    }

    private async Task<ActionResult<APIResult<string>>?> ValidateSubscriptionEligibility(AccountDTO account)
    {
        if (account.Subscription?.Status == SubscriptionStatus.Active || account.IsSubscribed)
        {
            logger.LogWarning("Account {AccountId} already has an active subscription", account.Id);
            return BadRequest(APIResult<string>.Error(HttpStatusCode.BadRequest, "Account already has an active subscription"));
        }

        if (account.Tier == SubscriptionTier.Guest)
        {
            logger.LogWarning("Guest account {AccountId} cannot subscribe to a plan", account.Id);
            return BadRequest(APIResult<string>.Error(HttpStatusCode.BadRequest, "Guest account cannot subscribe to a plan"));
        }

        if (account.Status != AccountStatus.Active)
        {
            logger.LogWarning("Account {AccountId} is not active", account.Id);
            return BadRequest(APIResult<string>.Error(HttpStatusCode.BadRequest, "Account is not active"));
        }

        return null;
    }

    private async Task<ActionResult<APIResult<string>>> CreateStripeCheckoutSession(
        AccountDTO account, string priceId, SubscriptionPlan subscriptionPlan)
    {
        var session = await stripeService.CreateCheckoutSession(account, priceId, subscriptionPlan);
        if (session == null)
        {
            logger.LogWarning("Failed to create subscription session for account: {AccountId}", account.Id);
            return StatusCode((int)HttpStatusCode.InternalServerError,
                APIResult<string>.Error(HttpStatusCode.ServiceUnavailable, "Payment service unavailable"));
        }

        logger.LogInformation("Successfully created subscription session for account: {AccountId}", account.Id);
        return Ok(APIResult<string>.Success(data: session.Url));
    }

    private string GetPriceIdForPlan(SubscriptionPlan subscriptionPlan) => subscriptionPlan switch
    {
        SubscriptionPlan.Pro => stripeConfig.Value.ProMonthlyId,
        SubscriptionPlan.ProYearly => stripeConfig.Value.ProYearlyId,
        SubscriptionPlan.Ultimate => stripeConfig.Value.UltimateMonthlyId,
        SubscriptionPlan.UltimateYearly => stripeConfig.Value.UltimateYearlyId,
        _ => ""
    };
    
}