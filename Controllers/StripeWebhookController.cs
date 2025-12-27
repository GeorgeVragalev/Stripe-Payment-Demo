using Microsoft.AspNetCore.Mvc;
using PaymentDemo.Extensions;
using PaymentDemo.Models;
using PaymentDemo.Services.Credentials;
using PaymentDemo.Services.Orders;
using PaymentDemo.Services.Webhooks;
using Stripe;
using Stripe.Checkout;

namespace PaymentDemo.Controllers;

/// <summary>
/// Webhook controller for Stripe events.
///
/// IMPORTANT: Webhooks are primarily needed for async payment methods (SEPA, BACS, etc.)
/// For card payments, the success page handles payment confirmation directly.
/// Webhooks provide redundancy and handle async payment confirmations.
/// </summary>
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

        // IDEMPOTENCY CHECK: Skip if already processed
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
                // Async payment succeeded (SEPA, BACS, etc.)
                case "checkout.session.async_payment_succeeded":
                    HandleAsyncPaymentSucceeded(stripeEvent, webhookEvent.Id);
                    break;

                // Async payment failed
                case "checkout.session.async_payment_failed":
                    HandleAsyncPaymentFailed(stripeEvent, webhookEvent.Id);
                    break;

                // Standard checkout completed - only process if order not already paid
                // This is a backup for the success page processing
                case "checkout.session.completed":
                    HandleCheckoutSessionCompleted(stripeEvent, webhookEvent.Id);
                    break;

                case "checkout.session.expired":
                    HandleCheckoutSessionExpired(stripeEvent, webhookEvent.Id);
                    break;

                // Payment intent events - log but don't process (handled via session events)
                case "payment_intent.succeeded":
                case "payment_intent.payment_failed":
                case "charge.succeeded":
                case "charge.failed":
                    _webhookEventStore.MarkAsSkipped(webhookEvent.Id, $"Handled via checkout.session events");
                    _logger.LogInformation("Skipped {EventType} - handled via session events", stripeEvent.Type);
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

    /// <summary>
    /// Handle async payment success (SEPA, BACS, etc.)
    /// This is the primary use case for webhooks.
    /// </summary>
    private void HandleAsyncPaymentSucceeded(Event stripeEvent, string localEventId)
    {
        var session = stripeEvent.Data.Object as Session;
        if (session == null)
        {
            _webhookEventStore.MarkAsFailed(localEventId, "Could not parse session object");
            return;
        }

        _logger.LogInformation("Async payment succeeded for session {SessionId}", session.Id);

        var order = _orderStore.GetByStripeSessionId(session.Id);
        if (order == null)
        {
            _webhookEventStore.MarkAsFailed(localEventId, $"Order not found for session {session.Id}");
            _logger.LogWarning("Order not found for session {SessionId}", session.Id);
            return;
        }

        // IDEMPOTENCY: Only update if not already paid
        if (order.Status == OrderStatus.Paid)
        {
            _webhookEventStore.MarkAsSkipped(localEventId, "Order already paid");
            _logger.LogInformation("Order {OrderId} already paid, skipping", order.Id);
            return;
        }

        // Check session is actually paid
        if (!session.IsPaid())
        {
            _webhookEventStore.MarkAsFailed(localEventId, $"Session not paid: {session.PaymentStatus}");
            return;
        }

        // Mark as paid
        order.Status = OrderStatus.Paid;
        order.PaidAt = DateTime.UtcNow;
        order.StripePaymentIntentId = session.PaymentIntentId;
        order.CustomerEmail = session.CustomerDetails?.Email ?? order.CustomerEmail;
        order.UpdatedAt = DateTime.UtcNow;
        _orderStore.Update(order);

        _webhookEventStore.MarkAsProcessed(localEventId);
        _logger.LogInformation("Order {OrderId} marked as paid via async payment webhook", order.Id);
    }

    /// <summary>
    /// Handle async payment failure
    /// </summary>
    private void HandleAsyncPaymentFailed(Event stripeEvent, string localEventId)
    {
        var session = stripeEvent.Data.Object as Session;
        if (session == null)
        {
            _webhookEventStore.MarkAsFailed(localEventId, "Could not parse session object");
            return;
        }

        _logger.LogWarning("Async payment failed for session {SessionId}", session.Id);

        var order = _orderStore.GetByStripeSessionId(session.Id);
        if (order == null)
        {
            _webhookEventStore.MarkAsFailed(localEventId, $"Order not found for session {session.Id}");
            return;
        }

        // IDEMPOTENCY: Only update if not already in a final state
        if (order.Status == OrderStatus.Paid || order.Status == OrderStatus.Failed)
        {
            _webhookEventStore.MarkAsSkipped(localEventId, $"Order already in final state: {order.Status}");
            return;
        }

        order.Status = OrderStatus.Failed;
        order.UpdatedAt = DateTime.UtcNow;
        _orderStore.Update(order);

        _webhookEventStore.MarkAsProcessed(localEventId);
        _logger.LogInformation("Order {OrderId} marked as failed via webhook", order.Id);
    }

    /// <summary>
    /// Handle checkout.session.completed - backup processing for card payments
    /// The success page is the primary handler, this is for redundancy
    /// </summary>
    private void HandleCheckoutSessionCompleted(Event stripeEvent, string localEventId)
    {
        var session = stripeEvent.Data.Object as Session;
        if (session == null)
        {
            _webhookEventStore.MarkAsFailed(localEventId, "Could not parse session object");
            return;
        }

        _logger.LogInformation("Checkout session completed: {SessionId}", session.Id);

        var order = _orderStore.GetByStripeSessionId(session.Id);
        if (order == null)
        {
            _webhookEventStore.MarkAsFailed(localEventId, $"Order not found for session {session.Id}");
            _logger.LogWarning("Order not found for session {SessionId}", session.Id);
            return;
        }

        // IDEMPOTENCY: Only update if not already paid
        // The success page should have already handled this for card payments
        if (order.Status == OrderStatus.Paid)
        {
            _webhookEventStore.MarkAsSkipped(localEventId, "Order already paid (likely by success page)");
            _logger.LogInformation("Order {OrderId} already paid, skipping webhook processing", order.Id);
            return;
        }

        // Check if payment is complete
        if (session.IsPaid())
        {
            order.Status = OrderStatus.Paid;
            order.PaidAt = DateTime.UtcNow;
            order.StripePaymentIntentId = session.PaymentIntentId;
            order.CustomerEmail = session.CustomerDetails?.Email ?? order.CustomerEmail;
            order.UpdatedAt = DateTime.UtcNow;
            _orderStore.Update(order);

            _webhookEventStore.MarkAsProcessed(localEventId);
            _logger.LogInformation("Order {OrderId} marked as paid via webhook (backup)", order.Id);
        }
        else if (session.IsProcessing())
        {
            // Async payment - waiting for bank confirmation
            if (order.Status != OrderStatus.PendingPayment)
            {
                order.Status = OrderStatus.PendingPayment;
                order.StripePaymentIntentId = session.PaymentIntentId;
                order.UpdatedAt = DateTime.UtcNow;
                _orderStore.Update(order);
            }

            _webhookEventStore.MarkAsProcessed(localEventId);
            _logger.LogInformation("Order {OrderId} awaiting async payment confirmation", order.Id);
        }
        else
        {
            _webhookEventStore.MarkAsSkipped(localEventId, $"Unexpected payment status: {session.PaymentStatus}");
        }
    }

    private void HandleCheckoutSessionExpired(Event stripeEvent, string localEventId)
    {
        var session = stripeEvent.Data.Object as Session;
        if (session == null)
        {
            _webhookEventStore.MarkAsFailed(localEventId, "Could not parse session object");
            return;
        }

        _logger.LogInformation("Checkout session expired: {SessionId}", session.Id);

        var order = _orderStore.GetByStripeSessionId(session.Id);
        if (order != null && order.Status == OrderStatus.Pending)
        {
            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;
            _orderStore.Update(order);
            _logger.LogInformation("Order {OrderId} cancelled due to expired session", order.Id);
        }

        _webhookEventStore.MarkAsProcessed(localEventId);
    }
}
