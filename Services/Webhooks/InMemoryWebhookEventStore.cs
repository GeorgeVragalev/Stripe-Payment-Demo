using System.Collections.Concurrent;
using PaymentDemo.Models;

namespace PaymentDemo.Services.Webhooks;

/// <summary>
/// In-memory webhook event store for demo purposes.
/// In production, replace with a proper database.
/// </summary>
public class InMemoryWebhookEventStore : IWebhookEventStore
{
    private readonly ConcurrentDictionary<string, WebhookEvent> _events = new();
    private readonly ConcurrentDictionary<string, bool> _processedStripeEventIds = new();
    private readonly ILogger<InMemoryWebhookEventStore> _logger;
    private int _eventCounter = 0;

    public InMemoryWebhookEventStore(ILogger<InMemoryWebhookEventStore> logger)
    {
        _logger = logger;
        SeedDemoEvents();
    }

    private void SeedDemoEvents()
    {
        var demoEvents = new[]
        {
            new WebhookEvent
            {
                Id = "evt_local_0001",
                StripeEventId = "evt_demo_abc123",
                EventType = "checkout.session.completed",
                RelatedObjectId = "cs_demo_abc123",
                Status = WebhookEventStatus.Processed,
                ReceivedAt = DateTime.UtcNow.AddDays(-5),
                ProcessedAt = DateTime.UtcNow.AddDays(-5).AddMilliseconds(150)
            },
            new WebhookEvent
            {
                Id = "evt_local_0002",
                StripeEventId = "evt_demo_def456",
                EventType = "checkout.session.completed",
                RelatedObjectId = "cs_demo_def456",
                Status = WebhookEventStatus.Processed,
                ReceivedAt = DateTime.UtcNow.AddDays(-3),
                ProcessedAt = DateTime.UtcNow.AddDays(-3).AddMilliseconds(120)
            },
            new WebhookEvent
            {
                Id = "evt_local_0003",
                StripeEventId = "evt_demo_ghi789",
                EventType = "payment_intent.succeeded",
                RelatedObjectId = "pi_demo_xyz789",
                Status = WebhookEventStatus.Processed,
                ReceivedAt = DateTime.UtcNow.AddDays(-5).AddMinutes(1),
                ProcessedAt = DateTime.UtcNow.AddDays(-5).AddMinutes(1).AddMilliseconds(80)
            },
            new WebhookEvent
            {
                Id = "evt_local_0004",
                StripeEventId = "evt_demo_jkl012",
                EventType = "charge.succeeded",
                RelatedObjectId = "ch_demo_abc123",
                Status = WebhookEventStatus.Skipped,
                ReceivedAt = DateTime.UtcNow.AddDays(-3).AddMinutes(1),
                ProcessedAt = DateTime.UtcNow.AddDays(-3).AddMinutes(1).AddMilliseconds(50)
            },
            new WebhookEvent
            {
                Id = "evt_local_0005",
                StripeEventId = "evt_demo_failed",
                EventType = "checkout.session.completed",
                RelatedObjectId = "cs_demo_failed",
                Status = WebhookEventStatus.Failed,
                ErrorMessage = "Order not found for session",
                ReceivedAt = DateTime.UtcNow.AddDays(-2),
                ProcessedAt = DateTime.UtcNow.AddDays(-2).AddMilliseconds(200)
            }
        };

        foreach (var evt in demoEvents)
        {
            _events[evt.Id] = evt;
            if (evt.Status == WebhookEventStatus.Processed)
            {
                _processedStripeEventIds[evt.StripeEventId] = true;
            }
        }

        _eventCounter = 5;
        _logger.LogInformation("Seeded {Count} demo webhook events", demoEvents.Length);
    }

    public bool HasBeenProcessed(string stripeEventId)
    {
        return _processedStripeEventIds.ContainsKey(stripeEventId);
    }

    public WebhookEvent RecordEvent(string stripeEventId, string eventType, string? relatedObjectId, string? rawPayload)
    {
        var eventNumber = Interlocked.Increment(ref _eventCounter);
        var eventId = $"evt_local_{eventNumber:D4}";

        var webhookEvent = new WebhookEvent
        {
            Id = eventId,
            StripeEventId = stripeEventId,
            EventType = eventType,
            RelatedObjectId = relatedObjectId,
            RawPayload = rawPayload,
            Status = WebhookEventStatus.Received,
            ReceivedAt = DateTime.UtcNow
        };

        _events[eventId] = webhookEvent;
        _logger.LogInformation("Recorded webhook event {EventId} (Stripe: {StripeEventId}, Type: {EventType})",
            eventId, stripeEventId, eventType);

        return webhookEvent;
    }

    public void MarkAsProcessed(string eventId)
    {
        if (_events.TryGetValue(eventId, out var evt))
        {
            evt.Status = WebhookEventStatus.Processed;
            evt.ProcessedAt = DateTime.UtcNow;
            _processedStripeEventIds[evt.StripeEventId] = true;
            _logger.LogInformation("Marked event {EventId} as processed", eventId);
        }
    }

    public void MarkAsFailed(string eventId, string errorMessage)
    {
        if (_events.TryGetValue(eventId, out var evt))
        {
            evt.Status = WebhookEventStatus.Failed;
            evt.ErrorMessage = errorMessage;
            evt.ProcessedAt = DateTime.UtcNow;
            _logger.LogWarning("Marked event {EventId} as failed: {Error}", eventId, errorMessage);
        }
    }

    public void MarkAsSkipped(string eventId, string reason)
    {
        if (_events.TryGetValue(eventId, out var evt))
        {
            evt.Status = WebhookEventStatus.Skipped;
            evt.ErrorMessage = reason;
            evt.ProcessedAt = DateTime.UtcNow;
            _logger.LogInformation("Skipped event {EventId}: {Reason}", eventId, reason);
        }
    }

    public IEnumerable<WebhookEvent> GetAll()
    {
        return _events.Values.OrderByDescending(e => e.ReceivedAt);
    }

    public WebhookEvent? GetById(string eventId)
    {
        _events.TryGetValue(eventId, out var evt);
        return evt;
    }

    public WebhookEvent? GetByStripeEventId(string stripeEventId)
    {
        return _events.Values.FirstOrDefault(e => e.StripeEventId == stripeEventId);
    }

    public IEnumerable<WebhookEvent> GetRecent(int count = 50)
    {
        return _events.Values
            .OrderByDescending(e => e.ReceivedAt)
            .Take(count);
    }

    public WebhookStats GetStats()
    {
        var events = _events.Values.ToList();
        return new WebhookStats
        {
            TotalEvents = events.Count,
            ProcessedEvents = events.Count(e => e.Status == WebhookEventStatus.Processed),
            FailedEvents = events.Count(e => e.Status == WebhookEventStatus.Failed),
            SkippedEvents = events.Count(e => e.Status == WebhookEventStatus.Skipped),
            LastEventAt = events.OrderByDescending(e => e.ReceivedAt).FirstOrDefault()?.ReceivedAt
        };
    }
}
