using PaymentDemo.Models;
using PaymentDemo.Services.Credentials;
using Stripe;
using Stripe.Checkout;

namespace PaymentDemo.Services.Stripe;

/// <summary>
/// Stripe service implementation that uses session-aware credentials
/// </summary>
public class StripeService : IStripeService
{
    private readonly IStripeCredentialProvider _credentialProvider;
    private readonly ILogger<StripeService> _logger;

    public StripeService(
        IStripeCredentialProvider credentialProvider,
        ILogger<StripeService> logger)
    {
        _credentialProvider = credentialProvider;
        _logger = logger;
    }

    public async Task<Session> CreateCheckoutSessionAsync(Order order, string successUrl, string cancelUrl)
    {
        var credentials = GetCredentialsOrThrow();
        var client = new StripeClient(credentials.SecretKey);

        var lineItems = order.Items.Select(item => new SessionLineItemOptions
        {
            PriceData = new SessionLineItemPriceDataOptions
            {
                Currency = order.Currency,
                UnitAmount = (long)(item.UnitPrice * 100), // Convert to cents
                ProductData = new SessionLineItemPriceDataProductDataOptions
                {
                    Name = item.ProductName,
                }
            },
            Quantity = item.Quantity
        }).ToList();

        // Add tax as a separate line item
        if (order.Tax > 0)
        {
            lineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = order.Currency,
                    UnitAmount = (long)(order.Tax * 100),
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = "Tax"
                    }
                },
                Quantity = 1
            });
        }

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = lineItems,
            Mode = "payment",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            CustomerEmail = order.CustomerEmail,
            Metadata = new Dictionary<string, string>
            {
                { "order_id", order.Id }
            },
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    { "order_id", order.Id }
                }
            }
        };

        var service = new SessionService(client);

        _logger.LogInformation("Creating Stripe checkout session for order {OrderId}", order.Id);

        var session = await service.CreateAsync(options);

        _logger.LogInformation(
            "Created Stripe session {SessionId} for order {OrderId}",
            session.Id,
            order.Id);

        return session;
    }

    public async Task<Session?> GetSessionAsync(string sessionId)
    {
        var credentials = GetCredentialsOrThrow();
        var client = new StripeClient(credentials.SecretKey);

        var service = new SessionService(client);

        try
        {
            var session = await service.GetAsync(sessionId, new SessionGetOptions
            {
                Expand = new List<string> { "payment_intent" }
            });
            return session;
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve session {SessionId}", sessionId);
            return null;
        }
    }

    public Event ConstructWebhookEvent(string payload, string signature)
    {
        var credentials = GetCredentialsOrThrow();

        if (string.IsNullOrEmpty(credentials.WebhookSecret))
        {
            throw new InvalidOperationException("Webhook secret is not configured");
        }

        return EventUtility.ConstructEvent(payload, signature, credentials.WebhookSecret);
    }

    public bool HasValidCredentials()
    {
        return _credentialProvider.HasValidCredentials();
    }

    public string? GetPublicKey()
    {
        return _credentialProvider.GetPublicKey();
    }

    private StripeCredentials GetCredentialsOrThrow()
    {
        var credentials = _credentialProvider.GetCredentials();

        if (credentials == null || !credentials.IsValid)
        {
            throw new InvalidOperationException(
                "Stripe credentials are not configured. Please configure credentials in the admin panel.");
        }

        return credentials;
    }
}
