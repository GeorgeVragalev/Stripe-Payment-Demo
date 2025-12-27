namespace PaymentDemo.Constants;

/// <summary>
/// Supported currencies for the payment demo
/// </summary>
public enum CurrencyType
{
    USD,
    EUR,
    GBP,
    CAD
}

/// <summary>
/// Currency information and helper methods
/// </summary>
public static class CurrencyInfo
{
    private static readonly Dictionary<CurrencyType, (string Symbol, string Name, string Code)> CurrencyData = new()
    {
        { CurrencyType.USD, ("$", "US Dollar", "usd") },
        { CurrencyType.EUR, ("€", "Euro", "eur") },
        { CurrencyType.GBP, ("£", "British Pound", "gbp") },
        { CurrencyType.CAD, ("C$", "Canadian Dollar", "cad") }
    };

    /// <summary>
    /// Get the currency symbol (e.g., $, €, £)
    /// </summary>
    public static string GetSymbol(this CurrencyType currency)
    {
        return CurrencyData.TryGetValue(currency, out var data) ? data.Symbol : "$";
    }

    /// <summary>
    /// Get the display name (e.g., "US Dollar")
    /// </summary>
    public static string GetDisplayName(this CurrencyType currency)
    {
        return CurrencyData.TryGetValue(currency, out var data) ? data.Name : "US Dollar";
    }

    /// <summary>
    /// Get the Stripe currency code (e.g., "usd", "eur")
    /// </summary>
    public static string GetCode(this CurrencyType currency)
    {
        return CurrencyData.TryGetValue(currency, out var data) ? data.Code : "usd";
    }

    /// <summary>
    /// Get display text for dropdown (e.g., "$ USD")
    /// </summary>
    public static string GetDropdownDisplay(this CurrencyType currency)
    {
        return $"{currency.GetSymbol()} {currency}";
    }

    /// <summary>
    /// Parse currency code string to CurrencyType
    /// </summary>
    public static CurrencyType FromCode(string? code)
    {
        if (string.IsNullOrEmpty(code))
            return CurrencyType.USD;

        return code.ToLowerInvariant() switch
        {
            "usd" => CurrencyType.USD,
            "eur" => CurrencyType.EUR,
            "gbp" => CurrencyType.GBP,
            "cad" => CurrencyType.CAD,
            _ => Enum.TryParse<CurrencyType>(code, true, out var result) ? result : CurrencyType.USD
        };
    }

    /// <summary>
    /// Get all available currencies
    /// </summary>
    public static IEnumerable<CurrencyType> GetAll()
    {
        return Enum.GetValues<CurrencyType>();
    }

    /// <summary>
    /// Format a price with the currency symbol
    /// </summary>
    public static string FormatPrice(this CurrencyType currency, decimal amount)
    {
        return $"{currency.GetSymbol()}{amount:N2}";
    }

    /// <summary>
    /// Get the symbol from a currency code string (e.g., "usd" -> "$")
    /// </summary>
    public static string GetSymbolFromCode(string? currencyCode)
    {
        return FromCode(currencyCode).GetSymbol();
    }

    /// <summary>
    /// Format a price with the currency symbol from a code string
    /// </summary>
    public static string FormatPriceFromCode(string? currencyCode, decimal amount)
    {
        return FromCode(currencyCode).FormatPrice(amount);
    }
}
