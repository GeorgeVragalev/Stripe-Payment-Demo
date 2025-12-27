namespace PaymentDemo.Models;

/// <summary>
/// Represents a Stripe webhook event received by the application
/// </summary>
public class WebhookEvent
{
    public string Id { get; set; } = string.Empty;
    public string StripeEventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string? RelatedObjectId { get; set; }
    public WebhookEventStatus Status { get; set; } = WebhookEventStatus.Received;
    public string? ErrorMessage { get; set; }
    public string? RawPayload { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }

    public string StatusDisplay => Status switch
    {
        WebhookEventStatus.Received => "Received",
        WebhookEventStatus.Processing => "Processing",
        WebhookEventStatus.Processed => "Processed",
        WebhookEventStatus.Failed => "Failed",
        WebhookEventStatus.Skipped => "Skipped",
        _ => "Unknown"
    };

    public string StatusBadgeClass => Status switch
    {
        WebhookEventStatus.Received => "badge-info",
        WebhookEventStatus.Processing => "badge-warning",
        WebhookEventStatus.Processed => "badge-success",
        WebhookEventStatus.Failed => "badge-error",
        WebhookEventStatus.Skipped => "badge-warning",
        _ => "badge-info"
    };

    public string EventTypeDisplay => EventType switch
    {
        "checkout.session.completed" => "Checkout Completed",
        "checkout.session.expired" => "Checkout Expired",
        "payment_intent.succeeded" => "Payment Succeeded",
        "payment_intent.payment_failed" => "Payment Failed",
        "charge.succeeded" => "Charge Succeeded",
        "charge.failed" => "Charge Failed",
        "charge.refunded" => "Charge Refunded",
        _ => EventType
    };
}

public enum WebhookEventStatus
{
    Received,
    Processing,
    Processed,
    Failed,
    Skipped
}
