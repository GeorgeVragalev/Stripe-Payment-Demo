using PaymentDemo.Models;

namespace PaymentDemo.Services.Webhooks;

/// <summary>
/// Store for webhook events - provides logging and idempotency
/// </summary>
public interface IWebhookEventStore
{
    /// <summary>
    /// Check if an event has already been processed (for idempotency)
    /// </summary>
    bool HasBeenProcessed(string stripeEventId);

    /// <summary>
    /// Record a new webhook event
    /// </summary>
    WebhookEvent RecordEvent(string stripeEventId, string eventType, string? relatedObjectId, string? rawPayload);

    /// <summary>
    /// Mark an event as successfully processed
    /// </summary>
    void MarkAsProcessed(string eventId);

    /// <summary>
    /// Mark an event as failed
    /// </summary>
    void MarkAsFailed(string eventId, string errorMessage);

    /// <summary>
    /// Mark an event as skipped (e.g., unhandled event type)
    /// </summary>
    void MarkAsSkipped(string eventId, string reason);

    /// <summary>
    /// Get all webhook events
    /// </summary>
    IEnumerable<WebhookEvent> GetAll();

    /// <summary>
    /// Get a specific event by ID
    /// </summary>
    WebhookEvent? GetById(string eventId);

    /// <summary>
    /// Get events by Stripe event ID
    /// </summary>
    WebhookEvent? GetByStripeEventId(string stripeEventId);

    /// <summary>
    /// Get recent events (for dashboard)
    /// </summary>
    IEnumerable<WebhookEvent> GetRecent(int count = 50);

    /// <summary>
    /// Get event statistics
    /// </summary>
    WebhookStats GetStats();
}

public class WebhookStats
{
    public int TotalEvents { get; set; }
    public int ProcessedEvents { get; set; }
    public int FailedEvents { get; set; }
    public int SkippedEvents { get; set; }
    public DateTime? LastEventAt { get; set; }
}
