namespace PaymentDemo.Services.Credentials;

/// <summary>
/// Provides Stripe credentials using a layered resolution strategy:
/// 1. Session credentials (user override) - highest priority
/// 2. Configuration credentials (appsettings/env vars) - fallback
/// </summary>
public interface IStripeCredentialProvider
{
    /// <summary>
    /// Get the current active credentials.
    /// Returns session credentials if set, otherwise falls back to configuration.
    /// </summary>
    /// <returns>Credentials or null if none configured</returns>
    StripeCredentials? GetCredentials();

    /// <summary>
    /// Get the source of the current credentials
    /// </summary>
    CredentialSource GetCurrentSource();

    /// <summary>
    /// Check if valid credentials are available from any source
    /// </summary>
    bool HasValidCredentials();

    /// <summary>
    /// Get the public key for client-side use.
    /// Safe to expose to the browser.
    /// </summary>
    string? GetPublicKey();

    /// <summary>
    /// Check if currently using test mode credentials
    /// </summary>
    bool IsTestMode();
}
