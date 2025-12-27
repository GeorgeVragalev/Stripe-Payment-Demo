using Microsoft.AspNetCore.Mvc;
using PaymentDemo.Models;
using PaymentDemo.Services.Credentials;
using PaymentDemo.Services.Orders;
using PaymentDemo.Services.Webhooks;
using Stripe;
using Stripe.Checkout;

namespace PaymentDemo.Controllers;

[ApiController]
[Route("api/webhooks")]
public class StripeWebhookController : ControllerBase
{
    private readonly IStripeCredentialProvider _credentialProvider;
    private readonly IWebhookEventStore _webhookEventStore;
    private readonly IOrderStore _orderStore;
    private readonly ILogger<StripeWebhookController> _logger;

    public StripeWebhookController(
        IStripeCredentialProvider credentialProvider,
        IWebhookEventStore webhookEventStore,
        IOrderStore orderStore,
        ILogger<StripeWebhookController> logger)
    {
        _credentialProvider = credentialProvider;
        _webhookEventStore = webhookEventStore;
        _orderStore = orderStore;
        _logger = logger;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> HandleWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        // Get webhook secret (uses configuration, not session - webhooks have no session context)
        var webhookSecret = _credentialProvider.GetWebhookSecret();
        if (string.IsNullOrEmpty(webhookSecret))
        {
            _logger.LogWarning("Webhook received but no webhook secret configured");
            return BadRequest("Webhook secret not configured");
        }

        Event stripeEvent;

        try
        {
            // Verify webhook signature
            var signatureHeader = Request.Headers["Stripe-Signature"];
            stripeEvent = EventUtility.ConstructEvent(
                json,
                signatureHeader,
                webhookSecret
            );
        }
        catch (StripeException ex)
        {
            _logger.LogWarning("Webhook signature verification failed: {Message}", ex.Message);
            return BadRequest($"Webhook signature verification failed: {ex.Message}");
        }

        // Check for idempotency - skip if already processed
        if (_webhookEventStore.HasBeenProcessed(stripeEvent.Id))
        {
            _logger.LogInformation("Webhook event {EventId} already processed, skipping", stripeEvent.Id);
            return Ok(new { received = true, status = "already_processed" });
        }

        // Extract related object ID
        string? relatedObjectId = null;
        if (stripeEvent.Data.Object is Session session)
        {
            relatedObjectId = session.Id;
        }
        else if (stripeEvent.Data.Object is PaymentIntent paymentIntent)
        {
            relatedObjectId = paymentIntent.Id;
        }
        else if (stripeEvent.Data.Object is Charge charge)
        {
            relatedObjectId = charge.Id;
        }

        // Record the event
        var webhookEvent = _webhookEventStore.RecordEvent(
            stripeEvent.Id,
            stripeEvent.Type,
            relatedObjectId,
            json
        );

        try
        {
            // Handle specific event types
            switch (stripeEvent.Type)
            {
                case "checkout.session.completed":
                    await HandleCheckoutSessionCompleted(stripeEvent, webhookEvent.Id);
                    break;

                case "checkout.session.expired":
                    await HandleCheckoutSessionExpired(stripeEvent, webhookEvent.Id);
                    break;

                case "payment_intent.succeeded":
                    // Log but don't process - we handle this via checkout.session.completed
                    _webhookEventStore.MarkAsSkipped(webhookEvent.Id, "Handled via checkout.session.completed");
                    break;

                case "payment_intent.payment_failed":
                    await HandlePaymentIntentFailed(stripeEvent, webhookEvent.Id);
                    break;

                default:
                    _webhookEventStore.MarkAsSkipped(webhookEvent.Id, $"Unhandled event type: {stripeEvent.Type}");
                    _logger.LogInformation("Unhandled webhook event type: {Type}", stripeEvent.Type);
                    break;
            }
        }
        catch (Exception ex)
        {
            _webhookEventStore.MarkAsFailed(webhookEvent.Id, ex.Message);
            _logger.LogError(ex, "Error processing webhook event {EventId}", stripeEvent.Id);
            // Return 200 to prevent Stripe from retrying - we've logged the error
            return Ok(new { received = true, status = "error", message = ex.Message });
        }

        return Ok(new { received = true, status = "processed" });
    }

    private Task HandleCheckoutSessionCompleted(Event stripeEvent, string localEventId)
    {
        var session = stripeEvent.Data.Object as Session;
        if (session == null)
        {
            _webhookEventStore.MarkAsFailed(localEventId, "Could not parse session object");
            return Task.CompletedTask;
        }

        _logger.LogInformation("Checkout session completed: {SessionId}", session.Id);

        // Find the order by session ID
        var order = _orderStore.GetByStripeSessionId(session.Id);
        if (order == null)
        {
            _webhookEventStore.MarkAsFailed(localEventId, $"Order not found for session {session.Id}");
            _logger.LogWarning("Order not found for session {SessionId}", session.Id);
            return Task.CompletedTask;
        }

        // Update order status
        order.Status = OrderStatus.Paid;
        order.PaidAt = DateTime.UtcNow;
        order.StripePaymentIntentId = session.PaymentIntentId;
        order.CustomerEmail = session.CustomerDetails?.Email ?? order.CustomerEmail;
        _orderStore.Update(order);

        _webhookEventStore.MarkAsProcessed(localEventId);
        _logger.LogInformation("Order {OrderId} marked as paid via webhook", order.Id);

        return Task.CompletedTask;
    }

    private Task HandleCheckoutSessionExpired(Event stripeEvent, string localEventId)
    {
        var session = stripeEvent.Data.Object as Session;
        if (session == null)
        {
            _webhookEventStore.MarkAsFailed(localEventId, "Could not parse session object");
            return Task.CompletedTask;
        }

        _logger.LogInformation("Checkout session expired: {SessionId}", session.Id);

        var order = _orderStore.GetByStripeSessionId(session.Id);
        if (order != null)
        {
            order.Status = OrderStatus.Cancelled;
            _orderStore.Update(order);
            _logger.LogInformation("Order {OrderId} cancelled due to expired session", order.Id);
        }

        _webhookEventStore.MarkAsProcessed(localEventId);
        return Task.CompletedTask;
    }

    private Task HandlePaymentIntentFailed(Event stripeEvent, string localEventId)
    {
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
        if (paymentIntent == null)
        {
            _webhookEventStore.MarkAsFailed(localEventId, "Could not parse payment intent object");
            return Task.CompletedTask;
        }

        _logger.LogWarning("Payment intent failed: {PaymentIntentId}", paymentIntent.Id);

        // Find orders with this payment intent and mark as failed
        var orders = _orderStore.GetAll()
            .Where(o => o.StripePaymentIntentId == paymentIntent.Id);

        foreach (var order in orders)
        {
            order.Status = OrderStatus.Failed;
            _orderStore.Update(order);
            _logger.LogInformation("Order {OrderId} marked as failed", order.Id);
        }

        _webhookEventStore.MarkAsProcessed(localEventId);
        return Task.CompletedTask;
    }
}
