using LudereAI.Application.Interfaces.Repositories;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models;
using LudereAI.Domain.Models.Configs;
using LudereAI.Shared.DTOs;
using LudereAI.Shared.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using Account = LudereAI.Domain.Models.Account.Account;

namespace LudereAI.Infrastructure.Services;

public class StripeService : IStripeService
{
    private readonly ILogger<IStripeService> _logger;
    private readonly IAccountRepository _accountRepository;
    private readonly CustomerService _customerService;
    private readonly SessionService _sessionService;
    private readonly StripeConfig _stripeConfig;

    public StripeService(ILogger<IStripeService> logger, IAccountRepository accountRepository,
        CustomerService customerService, IOptions<StripeConfig> stripeConfig, SessionService sessionService)
    {
        _logger = logger;
        _accountRepository = accountRepository;
        _customerService = customerService;
        _sessionService = sessionService;
        _stripeConfig = stripeConfig.Value;
    }

    public async Task<Session?> CreateCheckoutSession(AccountDTO dto, string priceId, SubscriptionPlan subscriptionPlan)
    {
        var account = await _accountRepository.Get(dto.Id);
        if (account == null)
        {
            _logger.LogWarning("Account not found for ID {AccountId}", dto.Id);
            return null;
        }

        var customerId = await CreateOrSyncAccount(account);

        var options = new SessionCreateOptions
        {
            AllowPromotionCodes = true,
            Customer = customerId,
            Mode = "subscription",
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    Price = priceId,
                    Quantity = 1
                }
            },
            SuccessUrl = $"{_stripeConfig.SuccessUrl}?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = _stripeConfig.CancelUrl,
            ClientReferenceId = account.Id,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CustomerUpdate = new SessionCustomerUpdateOptions
            {
                Address = "auto",
                Shipping = "auto"
            },
            Metadata = new Dictionary<string, string>
            {
                { "AccountId", account.Id },
                { "Username", account.Username },
            },
            SubscriptionData = new SessionSubscriptionDataOptions()
            {
                Metadata = new Dictionary<string, string>
                {
                    { "AccountId", account.Id },
                    { "Username", account.Username }
                }
            }
        };

        var session = await _sessionService.CreateAsync(options);
        return session;
    }

    public async Task<string> CreateCustomerPortalSession(string customerId)
    {
        var options = new Stripe.BillingPortal.SessionCreateOptions()
        {
            Customer = customerId,
            ReturnUrl = "https://ludereai.com/account",
        };

        var service = new Stripe.BillingPortal.SessionService();
        var sessions = await service.CreateAsync(options);
        
        return sessions.Url;
    }

    public async Task SyncAllAccounts()
    {
        var accounts = await _accountRepository.GetAll();

        foreach (var account in accounts)
        {
            try
            {
                await CreateOrSyncAccount(account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync account {AccountId}", account.Id);
            }
        }
    }

    public async Task<string> CreateOrSyncAccount(Account account)
    {
        try
        {
            var existingCustomer = await _customerService.ListAsync(new CustomerListOptions
            {
                Email = account.Email,
                Limit = 1
            });

            var customer = existingCustomer.FirstOrDefault();

            if (customer == null)
            {
                var customerOptions = new CustomerCreateOptions
                {
                    Email = account.Email,
                    Name = account.FullName,
                    Metadata = new Dictionary<string, string>
                    {
                        { "AccountId", account.Id },
                        { "Username", account.Username }
                    }
                };

                customer = await _customerService.CreateAsync(customerOptions);
                _logger.LogInformation("Created new Stripe customer {CustomerId} for account {AccountId}", customer.Id,
                    account.Id);
            }
            else
            {
                var updateOptions = new CustomerUpdateOptions
                {
                    Name = account.FullName,
                    Metadata = new Dictionary<string, string>
                    {
                        { "AccountId", account.Id },
                        { "Username", account.Username },
                    }
                };

                customer = await _customerService.UpdateAsync(customer.Id, updateOptions);
                _logger.LogInformation("Updated Stripe customer {CustomerId} for account {AccountId}", customer.Id,
                    account.Id);
            }
            
            account.StripeCustomerId = customer.Id;
            await _accountRepository.Update(account);

            return customer.Id;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error while syncing account {AccountId}", account.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while syncing account {AccountId}", account.Id);
            throw;
        }
    }
}