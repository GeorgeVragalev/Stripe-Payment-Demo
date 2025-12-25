namespace PaymentDemo.Configuration;

/// <summary>
/// Stripe configuration settings loaded from appsettings.json or environment variables.
/// These serve as the default/demo credentials.
/// </summary>
public class StripeSettings
{
    /// <summary>
    /// Stripe Secret API Key (sk_test_xxx or sk_live_xxx)
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Stripe Publishable API Key (pk_test_xxx or pk_live_xxx)
    /// </summary>
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>
    /// Stripe Webhook Signing Secret (whsec_xxx)
    /// </summary>
    public string WebhookSecret { get; set; } = string.Empty;

    /// <summary>
    /// Check if all required credentials are configured
    /// </summary>
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(SecretKey) &&
        !string.IsNullOrWhiteSpace(PublicKey);

    /// <summary>
    /// Check if using test mode credentials
    /// </summary>
    public bool IsTestMode =>
        SecretKey?.StartsWith("sk_test_", StringComparison.OrdinalIgnoreCase) == true;
}
