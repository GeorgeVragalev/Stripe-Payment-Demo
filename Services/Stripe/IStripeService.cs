using PaymentDemo.Models;
using Stripe;
using Stripe.Checkout;

namespace PaymentDemo.Services.Stripe;

/// <summary>
/// Service for interacting with Stripe API
/// </summary>
public interface IStripeService
{
    /// <summary>
    /// Create a Stripe Checkout Session
    /// </summary>
    Task<Session> CreateCheckoutSessionAsync(Order order, string successUrl, string cancelUrl);

    /// <summary>
    /// Retrieve a Checkout Session by ID
    /// </summary>
    Task<Session?> GetSessionAsync(string sessionId);

    /// <summary>
    /// Verify webhook signature and construct event
    /// </summary>
    Event ConstructWebhookEvent(string payload, string signature);

    /// <summary>
    /// Check if credentials are configured
    /// </summary>
    bool HasValidCredentials();

    /// <summary>
    /// Get the public key for client-side use
    /// </summary>
    string? GetPublicKey();
}
