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


[ApiController, Authorize, Route("[controller]")]
public class SubscriptionController : ControllerBase
{
    private readonly ILogger<SubscriptionController> _logger;
    private readonly IAccountService _accountService;
    private readonly IStripeService _stripeService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly IOptions<StripeConfig> _stripeConfig;

    private readonly string _plansInfoPath;

    public SubscriptionController(ILogger<SubscriptionController> logger,
        IAccountService accountService,
        IStripeService stripeService,
        ISubscriptionService subscriptionService,
        IOptions<StripeConfig> stripeConfig)
    {
        _logger = logger;
        _accountService = accountService;
        _stripeService = stripeService;
        _subscriptionService = subscriptionService;
        _stripeConfig = stripeConfig;
        
        _plansInfoPath = Path.Combine(AppContext.BaseDirectory, "plans-info.json");
    }

    [HttpGet("me")]
    public async Task<ActionResult<APIResult<UserSubscriptionDTO>>> GetCurrentSubscription()
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(accountId))
        {
            _logger.LogWarning("Invalid token received for subscription attempt");
            return BadRequest(APIResult<UserSubscriptionDTO>.Error(HttpStatusCode.BadRequest, "Invalid token"));
        }

        var account = await _accountService.GetAccount(accountId);
        if (account == null)
        {
            _logger.LogWarning("Account not found for ID {AccountId}", accountId);
            return NotFound(APIResult<UserSubscriptionDTO>.Error(HttpStatusCode.NotFound, "Account not found"));
        }
        
        var subscription = await _subscriptionService.GetSubscriptionByAccountId(account.Id);

        return Ok(APIResult<UserSubscriptionDTO>.Success(data: subscription));
    }
    
    [HttpGet("plans"), AllowAnonymous]
    public async Task<ActionResult<APIResult<PlansInfoConfig>>> GetSubscriptionPlans()
    {
        if (!System.IO.File.Exists(_plansInfoPath))
            return Ok(APIResult<PlansInfoConfig>.Error(HttpStatusCode.NotFound, "Plans info not found"));
        
        var plans = (await System.IO.File.ReadAllTextAsync(_plansInfoPath)).FromJson<PlansInfoConfig>();
        
        return Ok(APIResult<PlansInfoConfig>.Success(data: plans));
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
                _logger.LogWarning("Invalid plan type received {PlanType}", dto.SubscriptionPlan);
                return BadRequest(APIResult<string>.Error(HttpStatusCode.BadRequest, "Invalid plan type"));
            }

            return await CreateStripeCheckoutSession(account.Account!, priceId, dto.SubscriptionPlan);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error occurred while creating subscription session");
            return StatusCode((int)HttpStatusCode.ServiceUnavailable,
                APIResult<string>.Error(HttpStatusCode.ServiceUnavailable, "Payment service unavailable"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing subscription");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                APIResult<string>.Error(HttpStatusCode.InternalServerError, "An unexpected error occurred"));
        }
    }
    
    [HttpGet("CustomerPortal")]
    public async Task<ActionResult<APIResult<string>>> CustomerPortal()
    {
        var account = await ValidateAndGetAccount();
        if (account.Result != null) return account.Result;
        
        if (account.Account == null)
        {
            _logger.LogWarning("Account not found for ID {AccountId}", account.Account!.Id);
            return NotFound(APIResult<string>.Error(HttpStatusCode.NotFound, "Account not found"));
        }

        var sessionUrl = await _stripeService.CreateCustomerPortalSession(account.Account.Id);
        if (string.IsNullOrWhiteSpace(sessionUrl))
        {
            _logger.LogWarning("Failed to create customer portal session for account: {AccountId}", account.Account!.Id);
            return StatusCode((int)HttpStatusCode.InternalServerError,
                APIResult<string>.Error(HttpStatusCode.ServiceUnavailable, "Payment service unavailable"));
        }

        _logger.LogInformation("Successfully created customer portal session for account: {AccountId}", account.Account!.Id);
        return Ok(APIResult<string>.Success(data: sessionUrl));
    }

    private async Task<(AccountDTO? Account, ActionResult<APIResult<string>>? Result)> ValidateAndGetAccount()
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(accountId))
        {
            _logger.LogWarning("Invalid token received for subscription attempt");
            return (null, BadRequest(APIResult<string>.Error(HttpStatusCode.BadRequest, "Invalid token")));
        }

        var account = await _accountService.GetAccount(accountId);
        if (account == null)
        {
            _logger.LogWarning("Account not found for ID {AccountId}", accountId);
            return (null, NotFound(APIResult<string>.Error(HttpStatusCode.NotFound, "Account not found")));
        }

        return (account, null);
    }

    private async Task<ActionResult<APIResult<string>>?> ValidateSubscriptionEligibility(AccountDTO account)
    {
        if (account.Subscription?.Status == SubscriptionStatus.Active || account.IsSubscribed)
        {
            _logger.LogWarning("Account {AccountId} already has an active subscription", account.Id);
            return BadRequest(APIResult<string>.Error(HttpStatusCode.BadRequest, "Account already has an active subscription"));
        }

        if (account.Tier == SubscriptionTier.Guest)
        {
            _logger.LogWarning("Guest account {AccountId} cannot subscribe to a plan", account.Id);
            return BadRequest(APIResult<string>.Error(HttpStatusCode.BadRequest, "Guest account cannot subscribe to a plan"));
        }

        if (account.Status != AccountStatus.Active)
        {
            _logger.LogWarning("Account {AccountId} is not active", account.Id);
            return BadRequest(APIResult<string>.Error(HttpStatusCode.BadRequest, "Account is not active"));
        }

        return null;
    }

    private async Task<ActionResult<APIResult<string>>> CreateStripeCheckoutSession(
        AccountDTO account, string priceId, SubscriptionPlan subscriptionPlan)
    {
        var session = await _stripeService.CreateCheckoutSession(account, priceId, subscriptionPlan);
        if (session == null)
        {
            _logger.LogWarning("Failed to create subscription session for account: {AccountId}", account.Id);
            return StatusCode((int)HttpStatusCode.InternalServerError,
                APIResult<string>.Error(HttpStatusCode.ServiceUnavailable, "Payment service unavailable"));
        }

        _logger.LogInformation("Successfully created subscription session for account: {AccountId}", account.Id);
        return Ok(APIResult<string>.Success(data: session.Url));
    }

    private string GetPriceIdForPlan(SubscriptionPlan subscriptionPlan) => subscriptionPlan switch
    {
        SubscriptionPlan.Pro => _stripeConfig.Value.ProMonthlyId,
        SubscriptionPlan.ProYearly => _stripeConfig.Value.ProYearlyId,
        SubscriptionPlan.Ultimate => _stripeConfig.Value.UltimateMonthlyId,
        SubscriptionPlan.UltimateYearly => _stripeConfig.Value.UltimateYearlyId,
        _ => ""
    };
    
}