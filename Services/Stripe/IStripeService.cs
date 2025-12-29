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
    /// <param name="order">The order to create a session for</param>
    /// <param name="successUrl">URL to redirect to on success</param>
    /// <param name="cancelUrl">URL to redirect to on cancel</param>
    /// <param name="manualCapture">If true, authorize only (manual capture later). If false, capture automatically.</param>
    Task<Session> CreateCheckoutSessionAsync(Order order, string successUrl, string cancelUrl, bool manualCapture = false);

    /// <summary>
    /// Retrieve a Checkout Session by ID
    /// </summary>
    Task<Session?> GetSessionAsync(string sessionId);

    /// <summary>
    /// Capture an authorized payment intent
    /// </summary>
    /// <param name="paymentIntentId">The payment intent ID to capture</param>
    /// <returns>The captured payment intent</returns>
    Task<PaymentIntent> CapturePaymentIntentAsync(string paymentIntentId);

    /// <summary>
    /// Get a payment intent by ID
    /// </summary>
    Task<PaymentIntent?> GetPaymentIntentAsync(string paymentIntentId);

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
