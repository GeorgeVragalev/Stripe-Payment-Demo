using System.Text.Json.Serialization;

namespace PaymentDemo.Services.Credentials;

/// <summary>
/// Represents Stripe API credentials.
/// Used for both configuration-based and session-based credentials.
/// </summary>
public class StripeCredentials
{
    public string SecretKey { get; init; } = string.Empty;
    public string PublicKey { get; init; } = string.Empty;
    public string WebhookSecret { get; init; } = string.Empty;

    /// <summary>
    /// When these credentials were configured (for session credentials)
    /// </summary>
    public DateTime? ConfiguredAt { get; init; }

    /// <summary>
    /// Check if credentials are in test mode
    /// </summary>
    [JsonIgnore]
    public bool IsTestMode => SecretKey?.StartsWith("sk_test_", StringComparison.OrdinalIgnoreCase) == true;

    /// <summary>
    /// Check if credentials are in live mode (production)
    /// </summary>
    [JsonIgnore]
    public bool IsLiveMode => SecretKey?.StartsWith("sk_live_", StringComparison.OrdinalIgnoreCase) == true;

    /// <summary>
    /// Check if all required credentials are present
    /// </summary>
    [JsonIgnore]
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(SecretKey) &&
        !string.IsNullOrWhiteSpace(PublicKey) &&
        (SecretKey.StartsWith("sk_test_") || SecretKey.StartsWith("sk_live_")) &&
        (PublicKey.StartsWith("pk_test_") || PublicKey.StartsWith("pk_live_"));

    /// <summary>
    /// Check if webhook secret is configured
    /// </summary>
    [JsonIgnore]
    public bool HasWebhookSecret => !string.IsNullOrWhiteSpace(WebhookSecret);

    /// <summary>
    /// Get masked version of secret key for display (sk_test_****abcd)
    /// </summary>
    [JsonIgnore]
    public string MaskedSecretKey => MaskKey(SecretKey);

    /// <summary>
    /// Get masked version of public key for display
    /// </summary>
    [JsonIgnore]
    public string MaskedPublicKey => MaskKey(PublicKey);

    /// <summary>
    /// Get masked version of webhook secret for display
    /// </summary>
    [JsonIgnore]
    public string MaskedWebhookSecret => MaskKey(WebhookSecret);

    private static string MaskKey(string? key)
    {
        if (string.IsNullOrEmpty(key))
            return "(not set)";

        if (key.Length <= 12)
            return "****";

        // Show prefix and last 4 characters: sk_test_****abcd
        return $"{key[..8]}****{key[^4..]}";
    }
}

/// <summary>
/// Indicates the source of the current credentials
/// </summary>
public enum CredentialSource
{
    /// <summary>
    /// No credentials are configured
    /// </summary>
    None,

    /// <summary>
    /// Credentials loaded from appsettings.json or environment variables
    /// </summary>
    Configuration,

    /// <summary>
    /// Credentials provided by user and stored in session
    /// </summary>
    Session
}
