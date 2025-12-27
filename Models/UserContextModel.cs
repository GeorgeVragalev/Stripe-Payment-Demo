using PaymentDemo.Constants;

namespace PaymentDemo.Models;

/// <summary>
/// User session context for preferences like currency and language
/// </summary>
public class UserContextModel
{
    public CurrencyType Currency { get; set; } = CurrencyType.USD;

    // Future: Add language, theme, etc.
    // public string Language { get; set; } = "en";
}
