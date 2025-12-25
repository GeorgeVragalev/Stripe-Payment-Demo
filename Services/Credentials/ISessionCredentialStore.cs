namespace PaymentDemo.Services.Credentials;

/// <summary>
/// Interface for storing and retrieving user-provided Stripe credentials in the session.
/// Credentials stored here take precedence over configuration-based credentials.
/// </summary>
public interface ISessionCredentialStore
{
    /// <summary>
    /// Store credentials in the session
    /// </summary>
    void SetCredentials(StripeCredentials credentials);

    /// <summary>
    /// Retrieve credentials from the session
    /// </summary>
    /// <returns>Credentials if set, null otherwise</returns>
    StripeCredentials? GetCredentials();

    /// <summary>
    /// Remove credentials from the session
    /// </summary>
    void ClearCredentials();

    /// <summary>
    /// Check if session has credentials stored
    /// </summary>
    bool HasCredentials();
}
