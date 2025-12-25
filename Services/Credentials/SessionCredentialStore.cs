using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;

namespace PaymentDemo.Services.Credentials;

/// <summary>
/// Stores Stripe credentials in the user's session with encryption.
/// Credentials are automatically cleared when the session expires.
/// </summary>
public class SessionCredentialStore : ISessionCredentialStore
{
    private const string SessionKey = "StripeCredentials";
    private const string ProtectorPurpose = "StripeCredentialProtection";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDataProtector _protector;
    private readonly ILogger<SessionCredentialStore> _logger;

    public SessionCredentialStore(
        IHttpContextAccessor httpContextAccessor,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<SessionCredentialStore> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);
        _logger = logger;
    }

    public void SetCredentials(StripeCredentials credentials)
    {
        var session = GetSession();
        if (session == null)
        {
            _logger.LogWarning("Cannot store credentials: no session available");
            return;
        }

        try
        {
            // Add timestamp
            var credentialsWithTimestamp = new StripeCredentials
            {
                SecretKey = credentials.SecretKey,
                PublicKey = credentials.PublicKey,
                WebhookSecret = credentials.WebhookSecret,
                ConfiguredAt = DateTime.UtcNow
            };

            // Serialize and encrypt
            var json = JsonSerializer.Serialize(credentialsWithTimestamp);
            var encrypted = _protector.Protect(json);

            session.SetString(SessionKey, encrypted);

            _logger.LogInformation(
                "Stored session credentials (test mode: {IsTestMode})",
                credentials.IsTestMode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store credentials in session");
            throw;
        }
    }

    public StripeCredentials? GetCredentials()
    {
        var session = GetSession();
        if (session == null)
            return null;

        try
        {
            var encrypted = session.GetString(SessionKey);
            if (string.IsNullOrEmpty(encrypted))
                return null;

            // Decrypt and deserialize
            var json = _protector.Unprotect(encrypted);
            return JsonSerializer.Deserialize<StripeCredentials>(json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve credentials from session, clearing invalid data");
            ClearCredentials();
            return null;
        }
    }

    public void ClearCredentials()
    {
        var session = GetSession();
        session?.Remove(SessionKey);

        _logger.LogInformation("Cleared session credentials");
    }

    public bool HasCredentials()
    {
        var session = GetSession();
        if (session == null)
            return false;

        return !string.IsNullOrEmpty(session.GetString(SessionKey));
    }

    private ISession? GetSession()
    {
        return _httpContextAccessor.HttpContext?.Session;
    }
}
