using PaymentDemo.Constants;

namespace PaymentDemo.Models;

/// <summary>
/// User session context for preferences like currency, payment mode, etc.
/// </summary>
public class UserContextModel
{
    public CurrencyType Currency { get; set; } = CurrencyType.USD;

    /// <summary>
    /// When true, payments use two-step process: Authorize first, then Capture on confirmation.
    /// When false, payments are captured automatically during checkout.
    /// Default is true (two-step process).
    /// </summary>
    public bool SeparateAuthorizeAndCapture { get; set; } = true;

    // Future: Add language, theme, etc.
    // public string Language { get; set; } = "en";
}
