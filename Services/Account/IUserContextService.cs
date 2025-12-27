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
}
