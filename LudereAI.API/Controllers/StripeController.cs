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


[ApiController, Route("[controller]")]
public class StripeController : ControllerBase
{
    private readonly ILogger<StripeController> _logger;
    private readonly ISubscriptionService _subscriptionService;
    private readonly SubscriptionService _stripeSubscriptionService;
    private readonly IOptions<StripeConfig> _stripeConfig;

    public StripeController(ILogger<StripeController> logger,
        ISubscriptionService subscriptionService,
        IOptions<StripeConfig> stripeConfig, SubscriptionService stripeSubscriptionService)
    {
        _logger = logger;
        _subscriptionService = subscriptionService;
        _stripeConfig = stripeConfig;
        _stripeSubscriptionService = stripeSubscriptionService;
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
                    if (stripeEvent.Data.Object is Session session)
                    {
                        if (session.PaymentStatus != "paid") return;
                        var sub = await _stripeSubscriptionService.GetAsync(session.SubscriptionId);
                        await _subscriptionService.CreateSubscription(sub);
                    }
                    break;
            
                case EventTypes.CustomerSubscriptionCreated:
                    //if (stripeEvent.Data.Object is Subscription newSubscription) await _subscriptionService.CreateSubscription(newSubscription);
                    break;
                case EventTypes.CustomerSubscriptionUpdated:
                    if (stripeEvent.Data.Object is Subscription updatedSubscription) await _subscriptionService.UpdateSubscription(updatedSubscription);
                    break;

                case EventTypes.CustomerSubscriptionDeleted:
                    if (stripeEvent.Data.Object is Subscription deletedSubscription)
                        await _subscriptionService.HandleSubscriptionCanceled(deletedSubscription);
                    break;

                // Payment failure handling
                case EventTypes.InvoicePaymentFailed:
                    if (stripeEvent.Data.Object is Invoice failedInvoice) await _subscriptionService.HandlePaymentFailure(failedInvoice);
                    break;

                // Trial ending notification (optional but common)
                case EventTypes.CustomerSubscriptionTrialWillEnd:
                    if (stripeEvent.Data.Object is Subscription trialEndingSub) await _subscriptionService.HandleTrialEnding(trialEndingSub);
                    break;
            
            default:
                _logger.LogWarning("Unhandled event type {EventType}", stripeEvent.Type);
                break;
        }
    }
}