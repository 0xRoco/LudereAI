using System.Net;
using System.Security.Claims;
using LudereAI.API.Core;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models;
using LudereAI.Domain.Models.Configs;
using LudereAI.Shared;
using LudereAI.Shared.DTOs;
using LudereAI.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

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
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(accountId))
            {
                logger.LogWarning("Invalid token received for subscription attempt");
                return BadRequest(APIResult<string>.Error(HttpStatusCode.BadRequest, "Invalid token"));
            }

            var account = await accountService.GetAccount(accountId);
            if (account == null)
            {
                logger.LogWarning("Account not found for ID {AccountId}", accountId);
                return NotFound(APIResult<string>.Error(HttpStatusCode.NotFound, "Account not found"));
            }


            var priceId = GetPriceIdForPlan(dto.SubscriptionPlan);
            
            if (string.IsNullOrWhiteSpace(priceId))
            {
                logger.LogWarning("Invalid plan type recieved {PlanType}", dto.SubscriptionPlan);
                return BadRequest(APIResult<string>.Error(HttpStatusCode.BadRequest, "Invalid plan type"));
            }

            var session = await stripeService.CreateCheckoutSession(account, priceId, dto.SubscriptionPlan);
            if (session == null)
            {
                logger.LogWarning("Failed to create subscription session for account: {AccountId}", account.Id);
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    APIResult<string>.Error(HttpStatusCode.ServiceUnavailable, "Payment service unavailable"));
            }

            logger.LogInformation("Successfully created subscription session for account: {AccountId}", account.Id);
            return Ok(APIResult<string>.Success(data: session.Url));
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe error occured while creating subscription session");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                APIResult<string>.Error(HttpStatusCode.ServiceUnavailable, "Payment service unavailable"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occured while processing subscription ");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                APIResult<string>.Error(HttpStatusCode.InternalServerError, "An unexpected error occured"));
        }
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