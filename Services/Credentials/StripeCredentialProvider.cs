using Microsoft.Extensions.Options;
using PaymentDemo.Configuration;

namespace PaymentDemo.Services.Credentials;

/// <summary>
/// Provides Stripe credentials with layered resolution:
/// Session credentials override configuration credentials.
/// </summary>
public class StripeCredentialProvider : IStripeCredentialProvider
{
    private readonly ISessionCredentialStore _sessionStore;
    private readonly StripeSettings _configSettings;
    private readonly ILogger<StripeCredentialProvider> _logger;

    public StripeCredentialProvider(
        ISessionCredentialStore sessionStore,
        IOptions<StripeSettings> configOptions,
        ILogger<StripeCredentialProvider> logger)
    {
        _sessionStore = sessionStore;
        _configSettings = configOptions.Value;
        _logger = logger;
    }

    public StripeCredentials? GetCredentials()
    {
        // Priority 1: Session credentials (user override)
        var sessionCredentials = _sessionStore.GetCredentials();
        if (sessionCredentials?.IsValid == true)
        {
            _logger.LogDebug("Using session credentials");
            return sessionCredentials;
        }

        // Priority 2: Configuration credentials (appsettings / env vars)
        if (_configSettings.IsConfigured)
        {
            _logger.LogDebug("Using configuration credentials");
            return new StripeCredentials
            {
                SecretKey = _configSettings.SecretKey,
                PublicKey = _configSettings.PublicKey,
                WebhookSecret = _configSettings.WebhookSecret
            };
        }

        // No credentials available
        _logger.LogDebug("No credentials available");
        return null;
    }

    public CredentialSource GetCurrentSource()
    {
        // Check session first
        var sessionCredentials = _sessionStore.GetCredentials();
        if (sessionCredentials?.IsValid == true)
        {
            return CredentialSource.Session;
        }

        // Check configuration
        if (_configSettings.IsConfigured)
        {
            return CredentialSource.Configuration;
        }

        return CredentialSource.None;
    }

    public bool HasValidCredentials()
    {
        var credentials = GetCredentials();
        return credentials?.IsValid == true;
    }

    public string? GetPublicKey()
    {
        return GetCredentials()?.PublicKey;
    }

    public bool IsTestMode()
    {
        var credentials = GetCredentials();
        return credentials?.IsTestMode ?? true; // Default to test mode assumption if unknown
    }
}
