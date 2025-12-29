using PaymentDemo.Constants;
using PaymentDemo.Models;

namespace PaymentDemo.Services.Account;

public interface IUserContextService
{
    /// <summary>
    /// Get the full user context
    /// </summary>
    UserContextModel GetUserContext();

    /// <summary>
    /// Set the user's preferred currency
    /// </summary>
    void SetCurrency(CurrencyType currency);

    /// <summary>
    /// Get the user's preferred currency
    /// </summary>
    CurrencyType GetCurrency();

    /// <summary>
    /// Get the currency symbol for the current currency
    /// </summary>
    string GetCurrencySymbol();

    /// <summary>
    /// Get the Stripe currency code (lowercase, e.g., "usd")
    /// </summary>
    string GetCurrencyCode();

    /// <summary>
    /// Format a price with the current currency symbol
    /// </summary>
    string FormatPrice(decimal amount);

    /// <summary>
    /// Check if separate authorize and capture is enabled (two-step payment)
    /// </summary>
    bool IsSeparateAuthorizeAndCapture();

    /// <summary>
    /// Set the payment capture mode
    /// </summary>
    /// <param name="separateAuthorizeAndCapture">True for two-step (authorize then capture), false for automatic capture</param>
    void SetSeparateAuthorizeAndCapture(bool separateAuthorizeAndCapture);
}
