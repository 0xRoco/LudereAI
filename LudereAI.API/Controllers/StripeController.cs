using System.Net;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models;
using LudereAI.Domain.Models.Configs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using Subscription = Stripe.Subscription;

namespace LudereAI.API.Controllers;


[ApiController, Route("api/[controller]")]
public class StripeController : ControllerBase
{
    private readonly ILogger<StripeController> _logger;
    private readonly ISubscriptionService _subscriptionService;
    private readonly SubscriptionService _stripeSubscriptionService;
    private readonly IOptions<StripeConfig> _stripeConfig;

    public StripeController(ILogger<StripeController> logger,
        ISubscriptionService subscriptionService,
        SubscriptionService stripeSubscriptionService,
        IOptions<StripeConfig> stripeConfig)
    {
        _logger = logger;
        _subscriptionService = subscriptionService;
        _stripeSubscriptionService = stripeSubscriptionService;
        _stripeConfig = stripeConfig;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> HandleWebhook()
    {
        try
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                _stripeConfig.Value.WebhookSecret);

            _logger.LogInformation("Processing Stripe webhook {EventType}", stripeEvent.Type);

            await ProcessStripeEvents(stripeEvent);

            return Ok();
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook");
            return BadRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error processing Stripe webhook");
            return StatusCode((int) HttpStatusCode.InternalServerError);
        }
    }

    private async Task ProcessStripeEvents(Event stripeEvent)
    {
        switch (stripeEvent.Type)
        {
            case EventTypes.CheckoutSessionCompleted:
                await HandleCheckoutSessionCompleted(stripeEvent);
                break;
            case EventTypes.InvoicePaymentSucceeded:
                await HandleInvoicePaymentSucceeded(stripeEvent);
                break;
            case EventTypes.InvoicePaymentFailed:
                await HandleInvoicePaymentFailed(stripeEvent);
                break;
            
            case EventTypes.CustomerSubscriptionUpdated:
                await HandleCustomerSubscriptionUpdated(stripeEvent);
                break;
            
            case EventTypes.CustomerSubscriptionDeleted:
                await HandleCustomerSubscriptionDeleted(stripeEvent);
                break;
            
            default:
                _logger.LogWarning("Unhandled event type {EventType}", stripeEvent.Type);
                break;
        }
    }
    
    private async Task HandleCheckoutSessionCompleted(Event stripeEvent)
    {
        var session = stripeEvent.Data.Object as Session ?? throw new InvalidOperationException("Invalid session object");
        
        if (session.PaymentStatus != "paid")
        {
            _logger.LogWarning("Invalid payment status {PaymentStatus}", session.PaymentStatus);
            return;
        }

        var subscription = await _stripeSubscriptionService.GetAsync(session.SubscriptionId);
        
        var priceId = subscription.Items.Data.FirstOrDefault()?.Price.Id ?? "";
        
        
        
        await _subscriptionService.ActivateSubscription(
            session.ClientReferenceId, 
            session.SubscriptionId, 
            priceId, 
            subscription.CurrentPeriodStart, 
            subscription.CurrentPeriodEnd,
            subscription.Status);
    }
    
    private async Task HandleInvoicePaymentSucceeded(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice ?? throw new InvalidOperationException("Invalid invoice object");
        
        var subscription = await _stripeSubscriptionService.GetAsync(invoice.SubscriptionId);
        
        var priceId = subscription.Items.Data.FirstOrDefault()?.Price.Id ?? "";
        
        await _subscriptionService.RenewSubscription(
            subscription.Id, 
            subscription.CurrentPeriodEnd,
            priceId,
            subscription.Status);
    }
    
    private async Task HandleInvoicePaymentFailed(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice ?? throw new InvalidOperationException("Invalid invoice object");
        
        await _subscriptionService.HandleFailedPayment(invoice.SubscriptionId, invoice.PeriodEnd);
    }
    
    private async Task HandleCustomerSubscriptionUpdated(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Subscription ?? throw new InvalidOperationException("Invalid subscription object");
        
        var priceId = subscription.Items.Data.FirstOrDefault()?.Price.Id ?? "";
        
        await _subscriptionService.UpdateSubscription(
            subscription.Id, 
            subscription.Status, 
            priceId, 
            subscription.CurrentPeriodEnd);
    }
    
    private async Task HandleCustomerSubscriptionDeleted(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Subscription ?? throw new InvalidOperationException("Invalid subscription object");
        
        await _subscriptionService.CancelSubscription(subscription.Id, subscription.EndedAt ?? DateTime.UtcNow);
    }

}